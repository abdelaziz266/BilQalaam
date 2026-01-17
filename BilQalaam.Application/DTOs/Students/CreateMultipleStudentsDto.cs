using System.ComponentModel.DataAnnotations;

namespace BilQalaam.Application.DTOs.Students
{
    public class CreateMultipleStudentsDto
    {
        [Required(ErrorMessage = "ﬁ«∆„… «·ÿ·«» „ÿ·Ê»…")]
        [MinLength(1, ErrorMessage = "ÌÃ» ≈÷«›… ÿ«·» Ê«Õœ ⁄·Ï «·√ﬁ·")]
        public IEnumerable<CreateStudentDto> Students { get; set; } = new List<CreateStudentDto>();
    }
}
