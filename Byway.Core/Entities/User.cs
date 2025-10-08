using System.ComponentModel.DataAnnotations;

namespace Byway.Core.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }
        [Required, MaxLength(200)]
        public string HashedPassword { get; set; }

        public string? PictureUrl { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

        [Required]
        public bool IsAdmin { get; set; }

    }
}
