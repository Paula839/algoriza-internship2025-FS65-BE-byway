namespace Byway.Core.DTOs
{
    public class RegisterUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string UserName{ get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

    }
}
