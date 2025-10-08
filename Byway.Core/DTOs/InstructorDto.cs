using Byway.Core.Entities;
using Byway.Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Byway.Core.DTOs
{
    public class InstructorDto
    {

        public int Id { get; private set; }
        public List<int> CoursesIds { get; private set; } = new List<int>();
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? PictureUrl { get; set; }

        [Required]
        public InstructorCategory Title { get; set; }

        [Range(0, 5)]
        public double Rate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int NumberOfCourses { get; private set; }

        public InstructorDto(int id, List<int> coursesIds)
        {
            Id = id;
            CoursesIds = coursesIds;
        }

        public InstructorDto() {
            NumberOfCourses = CoursesIds.Count;
        }
        public void AddCourse(int courseId)
        {
             CoursesIds.Add(courseId);
             NumberOfCourses = CoursesIds.Count;
        }
    }
}
