using Byway.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace Byway.Core.DTOs
{
    public class UserDto
    {
        public int Id { get; private set; }
        public List<int> CoursesIds { get; private set; } = new List<int>();

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;


        [Required, MaxLength(200)]
        public string Username { get; set; }

        [Required, MaxLength(200)]
        public string Email { get; set; }

        [Required, MaxLength(200)]
        public string Password { get; set; }

        [MaxLength(200)]
        public string? PictureUrl { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        public UserDto(int id, List<int> coursesIds)
        {
            Id = id;
            CoursesIds = coursesIds;
        }

        public UserDto() { }

        public void AddCourse(int courseId) => CoursesIds.Add(courseId);
    }
}
