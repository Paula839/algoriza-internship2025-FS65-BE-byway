using Byway.Application.Interfaces;
using Byway.Application.Services;
using Byway.Core.DTOs;
using Byway.Core.Entities;
using Byway.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Byway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : GenericController<User, UserDto>
    {

        private readonly PurchaseService _purchaseService;


        public UsersController(IRepository<User> repo, PurchaseService purchaseService) : base(repo)
        {
            _purchaseService = purchaseService;
        }

        protected override UserDto MakeDto(User entity) => new UserDto
        (
            entity.Id,
            entity.Courses?.Select(c => c.Id).ToList() ?? new List<int>()
        )
        {
            Name = entity.Name,
            
            PictureUrl = entity.PictureUrl,
            IsAdmin = entity.IsAdmin,
            Email = entity.Email ?? entity.Username,

        };

        protected override User MapToEntity(UserDto dto) => new User
        {
            Name = dto.Name,
            PictureUrl = dto.PictureUrl,
            IsAdmin = dto.IsAdmin,
            Email = dto.Email,
            Username = dto.Username,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        protected override void UpdateEntity(User entity, UserDto dto)
        {
            entity.Name = dto.Name;
            entity.PictureUrl = dto.PictureUrl;
            entity.IsAdmin = dto.IsAdmin;
        }

        protected override int GetEntityId(User entity) => entity.Id;

        protected override async Task<bool> Validate(UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) return false;
            if (string.IsNullOrWhiteSpace(dto.Email)) return false;
            if (string.IsNullOrWhiteSpace(dto.Username)) return false;
            if (dto.Name.Length > 100) return false;
            if (dto.Email.Length > 200) return false;
            if (dto.Username.Length > 200) return false;
            if (dto.PictureUrl != null && dto.PictureUrl.Length > 200) return false;

            return true;
        }

        // POST: api/users/purchase
        [HttpPost("purchase"), Authorize]
        public async Task<IActionResult> PurchaseCourses([FromBody] HashSet<int> ids)
        {

            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                    return Unauthorized("User ID not found in token.");

                if (!int.TryParse(userIdClaim, out var id))
                    return Unauthorized("Invalid user ID in token.");

                var receipt = await _purchaseService.PurchaseCourses(id, ids);
                return Ok(new
                {
                    Message = "Courses purchased successfully.",
                    Receipt = receipt
                });
            }

            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = "Bad request: " + ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = "Not found: " + ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = "Conflict: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred: " + ex.Message });
            }
        }

        //[HttpGet("me"), Authorize]
        //public IActionResult GetProfile()
        //{
        //    var username = User.Identity?.Name;
        //    if(username == null) throw new UnauthorizedAccessException("User is not authenticated.");
        //    return Ok(new {User.Identity.Name,  Username = username });
        //}

        [Authorize(Roles = "Admin")]
        public override async Task<IActionResult> GetPages([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return await base.GetPages(pageNumber, pageSize);
        }

        [Authorize(Roles = "Admin")]
        public override async Task<IActionResult> Get(int id)
        {
            return await base.Get(id);
        
        }

        // GET: api/[controller]/username
        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username or email is required.");

            User? user = await _repo.Query().SingleOrDefaultAsync(u => EF.Functions.Like(u.Username, username));
            if (user == null)
                return NotFound("User not found.");
            return Ok(MakeDto(user));
        }


        // GET: api/[controller]/email
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Username or email is required.");

            if(!email.Contains('@') || !email.Contains('.'))
                return BadRequest("Invalid email format.");

            User? user = await _repo.Query().SingleOrDefaultAsync(u => EF.Functions.Like(u.Email, email));
            if (user == null)
                return NotFound("User not found.");
            return Ok(MakeDto(user));
        }

        // GET: api/users/myCourses
        [HttpGet("myCourses"), Authorize]
        public async Task<IActionResult> GetMyCourses()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                    return Unauthorized("User ID not found in token.");

                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid user ID in token.");

                var courseIds = await _repo.Query()
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.Courses) 
                    .Select(c => c.Id)          
                    .ToListAsync();

                return Ok(courseIds); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred: " + ex.Message });
            }
        }


    }
}
