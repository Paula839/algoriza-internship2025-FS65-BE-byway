using Byway.Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Byway.Core.Entities
{
    public class Course
    {
        public int Id { get; set; }

        
        [Required, MaxLength(100)] 
        public string Name { get; set; }
        
        public string? PictureUrl { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Certification { get; set; }

        [Required]
        public InstructorCategory Category { get; set; }

        [Required]
        public Level Level { get; set; }

        public double Rate { get; set; } = 0.0;

        [Required, Range(0, double.MaxValue)]
        public double Price { get; set; }

        [Required, Range(0, double.MaxValue)]
        public double TotalHours { get; set; }

        [Required]
        public int InstructorId { get; set; }

        [Required]
        public virtual Instructor Instructor { get; set; }
        [Required]
        public virtual ICollection<Content> Contents { get; set; } = new List<Content>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    }
}
