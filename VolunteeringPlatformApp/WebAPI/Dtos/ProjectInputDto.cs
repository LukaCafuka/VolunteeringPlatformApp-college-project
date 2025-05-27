namespace WebAPI.Dtos
{
    public class ProjectInputDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public int? ProjectTypeId { get; set; }
        public List<int>? SkillIds { get; set; } 
        public List<int>? AppUserIds { get; set; }
    }
}
