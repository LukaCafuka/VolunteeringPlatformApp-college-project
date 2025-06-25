using System.ComponentModel.DataAnnotations;

namespace WebAPI.Dtos
{
    public class ProjectTypeDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(70)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public List<string> ProjectTitles { get; set; } = new List<string>();
    }
    
    public class ProjectTypeInputDto
    {
        [Required]
        [StringLength(70)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
    }
} 