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
                    .Include(s => s.StudentTeachers)
                        .ThenInclude(st => st.Teacher);

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
                    query = query.Where(s => s.StudentTeachers.Any(st => teacherIdSet.Contains(st.TeacherId)));
                }

                if (role == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null)
                        return Result<PaginatedResponseDto<StudentResponseDto>>.Success(EmptyPaginatedResponse(pageNumber, pageSize));

                    query = query.Where(s => s.StudentTeachers.Any(st => st.TeacherId == teacher.Id));
                }
                else if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null)
                        return Result<PaginatedResponseDto<StudentResponseDto>>.Success(EmptyPaginatedResponse(pageNumber, pageSize));

                    query = query.Where(s => s.StudentTeachers.Any(st => st.Teacher.SupervisorId == supervisor.Id));
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
                    .Include(s => s.StudentTeachers)
                        .ThenInclude(st => st.Teacher)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (student == null)
                    return Result<StudentResponseDto>.Failure("«·ÿ«·» €Ì— „ÊÃÊœ");

                if (role == "Teacher")
                {
                    var teacher = await GetTeacherByUserId(userId);
                    if (teacher == null || !student.StudentTeachers.Any(st => st.TeacherId == teacher.Id))
                        return Result<StudentResponseDto>.Failure("«·ÿ«·» €Ì— „ÊÃÊœ");
                }
                else if (role == "Admin")
                {
                    var supervisor = await GetSupervisorByUserId(userId);
                    if (supervisor == null || !student.StudentTeachers.Any(st => st.Teacher.SupervisorId == supervisor.Id))
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

                // Validate teachers
                if (dto.TeacherIds == null || !dto.TeacherIds.Any())
                {
                    return Result<int>.Failure("ÌÃ»  ÕœÌœ „⁄·„ Ê«Õœ ⁄·Ï «·√ﬁ·");
                }

                var teachers = await _unitOfWork.Repository<Teacher>()
                    .Query()
                    .Where(t => dto.TeacherIds.Contains(t.Id))
                    .ToListAsync();

                if (teachers.Count != dto.TeacherIds.Distinct().Count())
                {
                    return Result<int>.Failure("»⁄÷ «·„⁄·„Ì‰ €Ì— „ÊÃÊœÌ‰");
                }

                // Create student
                var student = new Student
                {
                    StudentName = dto.FullName,
                    FamilyId = dto.FamilyId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _unitOfWork.Repository<Student>().AddAsync(student);
                await _unitOfWork.CompleteAsync();

                // Add student-teacher relationships
                foreach (var teacherId in dto.TeacherIds.Distinct())
                {
                    var studentTeacher = new StudentTeacher
                    {
                        StudentId = student.Id,
                        TeacherId = teacherId,
                        AssignedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<StudentTeacher>().AddAsync(studentTeacher);
                }

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

                    // Validate teachers
                    if (studentDto.TeacherIds == null || !studentDto.TeacherIds.Any())
                    {
                        return Result<IEnumerable<int>>.Failure($"ÌÃ»  ÕœÌœ „⁄·„ Ê«Õœ ⁄·Ï «·√ﬁ· ··ÿ«·» {studentDto.FullName}");
                    }

                    var teachers = await _unitOfWork.Repository<Teacher>()
                        .Query()
                        .Where(t => studentDto.TeacherIds.Contains(t.Id))
                        .ToListAsync();

                    if (teachers.Count != studentDto.TeacherIds.Distinct().Count())
                    {
                        return Result<IEnumerable<int>>.Failure($"»⁄÷ «·„⁄·„Ì‰ €Ì— „ÊÃÊœÌ‰ ··ÿ«·» {studentDto.FullName}");
                    }

                    // Create student
                    var student = new Student
                    {
                        StudentName = studentDto.FullName,
                        FamilyId = studentDto.FamilyId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    await _unitOfWork.Repository<Student>().AddAsync(student);
                    await _unitOfWork.CompleteAsync();

                    // Add student-teacher relationships
                    foreach (var teacherId in studentDto.TeacherIds.Distinct())
                    {
                        var studentTeacher = new StudentTeacher
                        {
                            StudentId = student.Id,
                            TeacherId = teacherId,
                            AssignedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.Repository<StudentTeacher>().AddAsync(studentTeacher);
                    }

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
                // 1?? ÃÌ» «·ÿ«·» (Tracked)
                var student = await _unitOfWork.Repository<Student>()
                    .Query()
                    .Include(s => s.StudentTeachers)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (student == null)
                    return Result<bool>.Failure("«·ÿ«·» €Ì— „ÊÃÊœ");

                // 2??  Õﬁﬁ „‰ «·«”„ (·Ê « €Ì—)
                if (student.StudentName != dto.FullName)
                {
                    var exists = await _unitOfWork.Repository<Student>()
                        .Query()
                        .AnyAsync(s => s.StudentName == dto.FullName && s.Id != id);

                    if (exists)
                        return Result<bool>.Failure("«”„ «·ÿ«·» „ÊÃÊœ »«·›⁄·");
                }

                // 3??  Õﬁﬁ „‰ ÊÃÊœ «·⁄«∆·… (»œÊ‰ Tracking)
                var familyExists = await _unitOfWork.Repository<Family>()
                    .Query()
                    .AsNoTracking()
                    .AnyAsync(f => f.Id == dto.FamilyId);

                if (!familyExists)
                    return Result<bool>.Failure("«·⁄«∆·… €Ì— „ÊÃÊœ…");

                // 4??  Õﬁﬁ „‰ ÊÃÊœ «·„⁄·„Ì‰
                if (dto.TeacherIds == null || !dto.TeacherIds.Any())
                    return Result<bool>.Failure("ÌÃ»  ÕœÌœ „⁄·„ Ê«Õœ ⁄·Ï «·√ﬁ·");

                var distinctTeacherIds = dto.TeacherIds.Distinct().ToList();
                var teacherCount = await _unitOfWork.Repository<Teacher>()
                    .Query()
                    .AsNoTracking()
                    .CountAsync(t => distinctTeacherIds.Contains(t.Id));

                if (teacherCount != distinctTeacherIds.Count)
                    return Result<bool>.Failure("»⁄÷ «·„⁄·„Ì‰ €Ì— „ÊÃÊœÌ‰");

                // 5?? «· ÕœÌÀ
                student.StudentName = dto.FullName;
                student.FamilyId = dto.FamilyId;
                student.UpdatedAt = DateTime.UtcNow;
                student.UpdatedBy = userId;

                // 6??  ÕœÌÀ «·⁄·«ﬁ… „⁄ «·„⁄·„Ì‰ - Õ–› «·⁄·«ﬁ«  «·ﬁœÌ„…
                var existingTeacherIds = student.StudentTeachers.Select(st => st.TeacherId).ToList();
                
                // Õ–› «·⁄·«ﬁ«  «· Ì ·„  ⁄œ „ÊÃÊœ…
                var teachersToRemove = student.StudentTeachers
                    .Where(st => !distinctTeacherIds.Contains(st.TeacherId))
                    .ToList();
                
                foreach (var st in teachersToRemove)
                {
                    _unitOfWork.Repository<StudentTeacher>().Delete(st);
                }

                // ≈÷«›… «·⁄·«ﬁ«  «·ÃœÌœ…
                var teachersToAdd = distinctTeacherIds
                    .Where(tid => !existingTeacherIds.Contains(tid))
                    .ToList();
                
                foreach (var teacherId in teachersToAdd)
                {
                    var studentTeacher = new StudentTeacher
                    {
                        StudentId = student.Id,
                        TeacherId = teacherId,
                        AssignedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<StudentTeacher>().AddAsync(studentTeacher);
                }

                _unitOfWork.Repository<Student>().Update(student);

                // 7?? Save
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
                    .Include(s => s.StudentTeachers)
                        .ThenInclude(st => st.Teacher)
                    .Where(s => s.StudentTeachers.Any(st => st.TeacherId == teacherId));

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
