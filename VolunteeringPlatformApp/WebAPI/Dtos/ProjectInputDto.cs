using System.ComponentModel.DataAnnotations;

namespace WebAPI.Dtos
{
    public class ProjectInputDto
    {
        [Required(ErrorMessage = "Project title is required.")]
        [StringLength(100, ErrorMessage = "Project title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public int? ProjectTypeId { get; set; }

        public List<int>? SkillIds { get; set; } 

        public List<int>? AppUserIds { get; set; }
    }
}
