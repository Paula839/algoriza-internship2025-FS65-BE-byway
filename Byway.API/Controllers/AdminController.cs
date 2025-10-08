using Byway.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Byway.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IInstructorRepository _instructorRepo;
        private readonly ICourseRepository _courseRepo;

        public AdminController(IUserRepository userRepository, IInstructorRepository instructorRepository, ICourseRepository courseRepository)
        {
            _userRepo = userRepository;
            _instructorRepo = instructorRepository;
            _courseRepo = courseRepository;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _userRepo.Query().Where(u => !u.IsAdmin).CountAsync();
            var totalInstructors = await _instructorRepo.Query().CountAsync();
            var totalCourses = await _courseRepo.Query().CountAsync();
            var totalCategories = await _courseRepo.Query()
                .Select(c => c.Category)
                .Distinct()
                .CountAsync();

            var totalWallet = await _courseRepo.Query()
                .SumAsync(c => c.Users.Count * c.Price);

            totalWallet = Math.Round(totalWallet, 2);

            var stats = new
            {
                TotalUsers = totalUsers,
                TotalInstructors = totalInstructors,
                TotalCourses = totalCourses,
                TotalCategories = totalCategories,
                TotalWallet = totalWallet
            };

            return Ok(stats);
        }
    }
}
