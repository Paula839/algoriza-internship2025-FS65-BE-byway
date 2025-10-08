using Byway.Core.Entities;
using Byway.Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Byway.Core.DTOs
{
    public class CourseDto
    {
        public int Id { get; private set; }

        public string InstructorName { get; private set; } = string.Empty;
        public ICollection<int> UserIds { get; private set; } = new List<int>();

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? PictureUrl { get; set; }

        [Required]
        public InstructorCategory Category { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Certification { get; set; }

        public Level level { get; set; } = Level.AllLevels;

        [Required]
        public ICollection<Content> Contents { get; set; } = new List<Content>();

        [Range(0, 5)]
        public double Rate { get; set; }

        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        [Required]
        public int InstructorId { get; set; }

        public float TotalHours { get; private set; } 

        public CourseDto(int id, ICollection<int> userIds)
        {
            Id = id;
            UserIds = userIds;
        }

        public CourseDto() { }
        public void AddUser(int userId) => UserIds.Add(userId);
        public void SetInstructorName(string name) => InstructorName = name;
        public void SetTotalHours() => TotalHours = Contents?.Sum(c => (float)c.Duration) ?? 0;


    }
}
