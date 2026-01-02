using BilQalaam.Application.DTOs.Teachers;
using BilQalaam.Application.DTOs.Families;
using BilQalaam.Application.DTOs.Students;

namespace BilQalaam.Application.DTOs.Supervisors
{
    public class SupervisorDetailsDto
    {
        public int SupervisorId { get; set; }
        public string SupervisorName { get; set; } = string.Empty;

        // «·„⁄·„Ì‰ «·„— »ÿÌ‰ »«·„‘—›
        public List<TeacherResponseDto> Teachers { get; set; } = new();

        // «·⁄«∆·«  «·„— »ÿ… »«·„‘—›
        public List<FamilyResponseDto> Families { get; set; } = new();

        // «·ÿ·«» «·„— »ÿÌ‰ »«·„⁄·„Ì‰
        public List<StudentResponseDto> Students { get; set; } = new();

        // «·≈Õ’«∆Ì« 
        public int TotalTeachers { get; set; }
        public int TotalFamilies { get; set; }
        public int TotalStudents { get; set; }
    }
}
