using Byway.Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Byway.Core.Entities
{
    public class Instructor
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string? PictureUrl { get; set; }

        [Required]
        public InstructorCategory Title { get; set; }

        [Required]
        public double Rate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    }
}
