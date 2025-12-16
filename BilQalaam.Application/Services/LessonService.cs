using AutoMapper;
using BilQalaam.Application.DTOs.Lessons;
using BilQalaam.Application.Interfaces;
using BilQalaam.Application.UnitOfWork;
using BilQalaam.Domain.Entities;

namespace BilQalaam.Application.Services
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LessonService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LessonResponseDto>> GetAllAsync()
        {
            var lessons = await _unitOfWork
                .Repository<Lesson>()
                .GetAllAsync();

            return _mapper.Map<IEnumerable<LessonResponseDto>>(lessons);
        }

        public async Task<LessonResponseDto?> GetByIdAsync(int id)
        {
            var lesson = await _unitOfWork
                .Repository<Lesson>()
                .GetByIdAsync(id);

            return lesson == null ? null : _mapper.Map<LessonResponseDto>(lesson);
        }

        public async Task<int> CreateAsync(CreateLessonDto dto)
        {
            var lesson = _mapper.Map<Lesson>(dto);

            await _unitOfWork.Repository<Lesson>().AddAsync(lesson);
            await _unitOfWork.CompleteAsync();

            return lesson.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateLessonDto dto)
        {
            var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);
            if (lesson == null) return false;

            _mapper.Map(dto, lesson);
            _unitOfWork.Repository<Lesson>().Update(lesson);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(id);
            if (lesson == null) return false;

            _unitOfWork.Repository<Lesson>().Delete(lesson);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
