using WebAPI.Models;

namespace WebAPI.Dtos
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProjectTypeName { get; set; }
        public List<string>? SkillNames { get; set; }
        public List<string>? AppUserNames { get; set; }
    }
}
