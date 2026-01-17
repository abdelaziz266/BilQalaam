using AutoMapper;
using BilQalaam.Application.DTOs.Common;
using BilQalaam.Application.DTOs.Students;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.Results;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BilQalaam.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedResponseDto<StudentResponseDto>>> GetAllAsync(
            int pageNumber,
            int pageSize,
            IEnumerable<int>? familyIds,
            IEnumerable<int>? teacherIds,
            string role,
            string userId,
            string? searchText = null)
        {
            try
            {
                IQueryable<Student> query = _unitOfWork
                    .Repository<Student>()
                    .Query()
                    .Include(s => s.Family)
                    .Include(s => s.Teacher);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var lowerSearchText = searchText.ToLower();
                    query = query.Where(s =>
                        s.StudentName.ToLower().Contains(lowerSearchText));
                }

                if (familyIds?.Any() == true)
                {
                    var familyIdSet = new HashSet<int>(familyIds);
                    query = query.Where(s => s.FamilyId.HasValue && familyIdSet.Contains(s.FamilyId.Value));
                }

                if (teacherIds?.Any() == true)
                {
                    var teacherIdSet = new HashSet<int>(teacherIds);
                    query = query.Where(s => teacherIdSet.Contains(s.TeacherId));
                }

                if (role == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null)
                        return Result<PaginatedResponseDto<StudentResponseDto>>.Success(EmptyPaginatedResponse(pageNumber, pageSize));

                    query = query.Where(s => s.TeacherId == teacher.Id);
                }
                else if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null)
                        return Result<PaginatedResponseDto<StudentResponseDto>>.Success(EmptyPaginatedResponse(pageNumber, pageSize));

                    query = query.Where(s => s.Teacher.SupervisorId == supervisor.Id);
                }

                var totalCount = await query.CountAsync();
                var students = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var pagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Result<PaginatedResponseDto<StudentResponseDto>>.Success(new PaginatedResponseDto<StudentResponseDto>
                {
                    Items = _mapper.Map<IEnumerable<StudentResponseDto>>(students),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    PagesCount = pagesCount
                });
            }
            catch (Exception ex)
            {
                return Result<PaginatedResponseDto<StudentResponseDto>>.Failure($"Œÿ√ ›Ì Ã·» «·ÿ·«»: {ex.Message}");
            }
        }

        public async Task<Result<StudentResponseDto>> GetByIdAsync(int id, string role, string userId)
        {
            try
            {
                var student = await _unitOfWork
                    .Repository<Student>()
                    .Query()
                    .Include(s => s.Family)
                    .Include(s => s.Teacher)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (student == null)
                    return Result<StudentResponseDto>.Failure("«·ÿ«·» €Ì— „ÊÃÊœ");

                if (role == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null || student.TeacherId != teacher.Id)
                        return Result<StudentResponseDto>.Failure("«·ÿ«·» €Ì— „ÊÃÊœ");
                }
                else if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null || student.Teacher.SupervisorId != supervisor.Id)
                        return Result<StudentResponseDto>.Failure("«·ÿ«·» €Ì— „ÊÃÊœ");
                }

                return Result<StudentResponseDto>.Success(_mapper.Map<StudentResponseDto>(student));
            }
            catch (Exception ex)
            {
                return Result<StudentResponseDto>.Failure($"Œÿ√ ›Ì Ã·» «·ÿ«·»: {ex.Message}");
            }
        }

        public async Task<Result<int>> CreateAsync(CreateStudentDto dto, string role, string userId)
        {
            try
            {
                // Validate student name
                var existingStudents = await _unitOfWork.Repository<Student>().FindAsync(s => s.StudentName == dto.FullName);
                if (existingStudents.Any())
                {
                    return Result<int>.Failure("«”„ «·ÿ«·» „ÊÃÊœ »«·›⁄·");
                }

                // Validate family
                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(dto.FamilyId);
                if (family == null)
                {
                    return Result<int>.Failure("«·⁄«∆·… €Ì— „ÊÃÊœ…");
                }

                // Validate teacher
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(dto.TeacherId);
                if (teacher == null)
                {
                    return Result<int>.Failure("«·„⁄·„ €Ì— „ÊÃÊœ");
                }

                // Create student
                var student = new Student
                {
                    StudentName = dto.FullName,
                    FamilyId = dto.FamilyId,
                    TeacherId = dto.TeacherId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _unitOfWork.Repository<Student>().AddAsync(student);
                await _unitOfWork.CompleteAsync();

                return Result<int>.Success(student.Id);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Œÿ√ ›Ì ≈‰‘«¡ «·ÿ«·»: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<int>>> CreateMultipleAsync(CreateMultipleStudentsDto dto, string role, string userId)
        {
            try
            {
                if (dto.Students == null || !dto.Students.Any())
                {
                    return Result<IEnumerable<int>>.Failure("ﬁ«∆„… «·ÿ·«» ›«—€…");
                }

                var createdStudentIds = new List<int>();

                foreach (var studentDto in dto.Students)
                {
                    // Validate student name
                    var existingStudents = await _unitOfWork.Repository<Student>().FindAsync(s => s.StudentName == studentDto.FullName);
                    if (existingStudents.Any())
                    {
                        return Result<IEnumerable<int>>.Failure($"«”„ «·ÿ«·» {studentDto.FullName} „ÊÃÊœ »«·›⁄·");
                    }

                    // Validate family
                    var family = await _unitOfWork.Repository<Family>().GetByIdAsync(studentDto.FamilyId);
                    if (family == null)
                    {
                        return Result<IEnumerable<int>>.Failure($"«·⁄«∆·… €Ì— „ÊÃÊœ… ··ÿ«·» {studentDto.FullName}");
                    }

                    // Validate teacher
                    var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(studentDto.TeacherId);
                    if (teacher == null)
                    {
                        return Result<IEnumerable<int>>.Failure($"«·„⁄·„ €Ì— „ÊÃÊœ ··ÿ«·» {studentDto.FullName}");
                    }

                    // Create student
                    var student = new Student
                    {
                        StudentName = studentDto.FullName,
                        FamilyId = studentDto.FamilyId,
                        TeacherId = studentDto.TeacherId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    await _unitOfWork.Repository<Student>().AddAsync(student);
                    createdStudentIds.Add(student.Id);
                }

                await _unitOfWork.CompleteAsync();

                return Result<IEnumerable<int>>.Success(createdStudentIds);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<int>>.Failure($"Œÿ√ ›Ì ≈‰‘«¡ «·ÿ·«»: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateAsync(int id, UpdateStudentDto dto, string userId)
        {
            try
            {
                var student = await _unitOfWork.Repository<Student>()
                    .Query()
                    .Include(s => s.Family)
                    .FirstOrDefaultAsync(s => s.Id == id);
                
                if (student == null)
                    return Result<bool>.Failure("«·ÿ«·» €Ì— „ÊÃÊœ");

                // Validate student name
                if (student.StudentName != dto.FullName)
                {
                    var existingStudent = await _unitOfWork.Repository<Student>()
                        .FindAsync(s => s.StudentName == dto.FullName && s.Id != id);
                    if (existingStudent.Any())
                        return Result<bool>.Failure("«”„ «·ÿ«·» „ÊÃÊœ »«·›⁄·");
                }

                // Validate family
                var family = await _unitOfWork.Repository<Family>().GetByIdAsync(dto.FamilyId);
                if (family == null)
                    return Result<bool>.Failure("«·⁄«∆·… €Ì— „ÊÃÊœ…");

                // Validate teacher
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(dto.TeacherId);
                if (teacher == null)
                    return Result<bool>.Failure("«·„⁄·„ €Ì— „ÊÃÊœ");

                student.StudentName = dto.FullName;
                student.FamilyId = dto.FamilyId;
                student.TeacherId = dto.TeacherId;
                student.UpdatedAt = DateTime.UtcNow;
                student.UpdatedBy = userId;

                _unitOfWork.Repository<Student>().Update(student);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Œÿ√ ›Ì  ÕœÌÀ «·ÿ«·»: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            try
            {
                var student = await _unitOfWork.Repository<Student>().GetByIdAsync(id);
                if (student == null)
                    return Result<bool>.Failure("«·ÿ«·» €Ì— „ÊÃÊœ");

                var lessons = await _unitOfWork.Repository<Lesson>().FindAsync(l => l.StudentId == id && !l.IsDeleted);
                if (lessons.Any())
                    return Result<bool>.Failure("·« Ì„ﬂ‰ Õ–› «·ÿ«·» ·√‰ Â‰«ﬂ œ—Ê” „— »ÿ… »Â");

                student.IsDeleted = true;
                student.DeletedAt = DateTime.UtcNow;
                student.DeletedBy = id.ToString();

                _unitOfWork.Repository<Student>().Update(student);
                await _unitOfWork.CompleteAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Œÿ√ ›Ì Õ–› «·ÿ«·»: {ex.Message}");
            }
        }

        #region Private Helpers

        private async Task<Teacher?> GetTeacherByUserId(string userId)
        {
            var teachers = await _unitOfWork.Repository<Teacher>().FindAsync(t => t.UserId == userId);
            return teachers.FirstOrDefault();
        }

        private async Task<Supervisor?> GetSupervisorByUserId(string userId)
        {
            var supervisors = await _unitOfWork.Repository<Supervisor>().FindAsync(s => s.UserId == userId);
            return supervisors.FirstOrDefault();
        }

        private static PaginatedResponseDto<StudentResponseDto> EmptyPaginatedResponse(int pageNumber, int pageSize)
        {
            return new PaginatedResponseDto<StudentResponseDto>
            {
                Items = new List<StudentResponseDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0,
                PagesCount = 0
            };
        }

        #endregion

        public async Task<Result<PaginatedResponseDto<StudentResponseDto>>> GetByTeacherIdAsync(
            int teacherId)
        {
            try
            {
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId);
                if (teacher == null)
                    return Result<PaginatedResponseDto<StudentResponseDto>>.Failure("«·„⁄·„ €Ì— „ÊÃÊœ");

                var query = _unitOfWork
                    .Repository<Student>()
                    .Query()
                    .Include(s => s.Family)
                    .Include(s => s.Teacher)
                    .Where(s => s.TeacherId == teacherId);

                var students = await query.ToListAsync();

                return Result<PaginatedResponseDto<StudentResponseDto>>.Success(new PaginatedResponseDto<StudentResponseDto>
                {
                    Items = _mapper.Map<IEnumerable<StudentResponseDto>>(students)
                });
            }
            catch (Exception ex)
            {
                return Result<PaginatedResponseDto<StudentResponseDto>>.Failure($"Œÿ√ ›Ì Ã·» «·ÿ·«»: {ex.Message}");
            }
        }
    }
}
