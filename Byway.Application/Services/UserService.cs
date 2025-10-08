using Azure.Core;
using Byway.Application.DTOs.Auth;
using Byway.Application.Interfaces;
using Byway.Core.DTOs;
using Byway.Core.Entities;
using Byway.Core.Interfaces;
using Org.BouncyCastle.Crypto.Generators;

namespace Byway.Application.Services
{

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        public UserService(IUserRepository userRepo, ITokenService tokenService, IConfiguration config, IEmailService emailService)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
            _config = config;
            _emailService = emailService;
        }

        public async Task ValidateRegister(RegisterUserDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.FirstName)) throw new ArgumentException("First Name cannot be null or empty!");
            if (string.IsNullOrWhiteSpace(dto.LastName)) throw new ArgumentException("Last Name cannot be null or empty!");
            if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Email cannot be null or empty!");
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password cannot be null or empty!");
            if (dto.UserName.Contains('@')) throw new ArgumentException("Invalid username format");
            if (!dto.Email.Contains('@') || !dto.Email.Contains('.')) throw new ArgumentException("Invalid email format");
            if (dto.Password.Length < 6) throw new ArgumentException("Password must be at least 6 characters long!");

            if (await _userRepo.ExistsByEmailAsync(dto.Email))
                throw new InvalidOperationException("Email is already in use!");

            if (await _userRepo.ExistsByUsernameAsync(dto.UserName))
                throw new InvalidOperationException("Username is already in use!");
        }

        public void ValidateLogin(LoginDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Username)) throw new ArgumentException("Username or Email cannot be null or empty!");
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password cannot be null or empty!");
        }


        private UserDto MakeDto(User user) => new UserDto(user.Id, user.Courses.Select(c => c.Id).ToList())
        {
            Name = user.Name.Trim(),
            PictureUrl = user.PictureUrl,
            IsAdmin = user.IsAdmin,
            Email = user.Email.Trim().ToLower(),
            Username = user.Username
        };

        public async Task<UserDto> RegisterAsync(RegisterUserDto dto)
        {

            await ValidateRegister(dto);

            var user = new User
            {
                Name = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Username = dto.UserName,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsAdmin = false
            };


            await _emailService.SendEmailAsync(user.Email, 
                "🎉 Welcome to Byway Learning!", 
                $"🎉 Welcome aboard {user.Name}! Your learning journey starts here. Let’s grow your skills together.");
            await _userRepo.AddAsync(user);
            return MakeDto(user);
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
        {

            ValidateLogin(dto);

            var username = dto.Username.Trim().ToLower();
            var user = username.Contains("@")
                ? await _userRepo.GetByEmailAsync(username)
                : await _userRepo.GetByUsernameAsync(username);


            if (user == null) throw new UnauthorizedAccessException("Invalid Email or Password!");


            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword);
            if (!isPasswordValid) throw new UnauthorizedAccessException("Invalid Email or Password!");

            var token = _tokenService.GenerateToken(user);

            var expirationMinutes = Convert.ToDouble(_config["Jwt:ExpiresInMinutes"]);

            return new TokenResponseDto
            {
                AccessToken = token,
                Expiration = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
        }

    }

}
