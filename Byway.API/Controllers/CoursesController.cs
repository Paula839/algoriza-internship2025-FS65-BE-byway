using Byway.Application.DTOs;
using Byway.Application.Services;
using Byway.Application.Services.Enums;
using Byway.Core.DTOs;
using Byway.Core.Entities;
using Byway.Core.Entities.Enums;
using Byway.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Byway.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : GenericController<Course, CourseDto>
    {
        private readonly IInstructorRepository _instructorRepo;
        private readonly FilterService _filterService;

        public CoursesController(ICourseRepository repo, IInstructorRepository instructorRepo, FilterService filterService) : base(repo)
        {
            _instructorRepo = instructorRepo;
            _filterService = filterService;
        }


        protected override  CourseDto MakeDto(Course course)
        {

           var dto = new CourseDto(
          course.Id,
          course.Users?.Select(u => u.Id).ToList() ?? new List<int>()
         )
            {
                Name = course.Name,
                PictureUrl = course.PictureUrl,
                Description = course.Description,
                Certification = course.Certification,
                Rate = course.Rate,
                Price = course.Price,
                InstructorId = course.InstructorId,
                Contents = course.Contents,
                level = course.Level,
                Category = course.Category
           };

            if (course.Instructor != null)
            {
                dto.SetInstructorName(course.Instructor.Name);
            }

            dto.SetTotalHours();

            return dto;
        }


        protected override Course MapToEntity(CourseDto dto) => new Course
        {
            Name = dto.Name,
            PictureUrl = dto.PictureUrl,
            Description = dto.Description,
            Rate = dto.Rate,
            Price = dto.Price,
            InstructorId = dto.InstructorId,
            Contents = dto.Contents,
            Level = dto.level,
            Certification = dto.Certification,
            Category = dto.Category
        };

        protected override void UpdateEntity(Course entity, CourseDto dto)
        {
            entity.Name = dto.Name;
            entity.PictureUrl = dto.PictureUrl;
            entity.Description = dto.Description;
            entity.Rate = dto.Rate;
            entity.Price = dto.Price;
            entity.InstructorId = dto.InstructorId;
        }

        protected override int GetEntityId(Course entity) => entity.Id;

        protected override async Task<bool> Validate(CourseDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 100)
                return false;
            if (dto.Rate < 0 || dto.Rate > 5)
                return false;
            if (dto.Price < 0)
                return false;
            if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description.Length > 1000)
                return false;
            if (!string.IsNullOrWhiteSpace(dto.PictureUrl) && dto.PictureUrl.Length > 200)
                return false;

            var instructor = await _instructorRepo.GetByIdAsync(dto.InstructorId);

            if (instructor == null) return false;


            return true;
        }

        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public override async Task<IActionResult> Put(int id, [FromBody] CourseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto == null) return BadRequest("DTO cannot be null");
            var existingEntity = await _repo.GetByIdAsync(id);
            if (existingEntity == null) return NotFound();

            if (!await Validate(dto)) return BadRequest("Invalid DTO data");
            
            if(existingEntity.Users.Count > 0)
                return Conflict("Cannot change a course with enrolled users.");


            UpdateEntity(existingEntity, dto);
            await _repo.UpdateAsync(existingEntity);
            return Ok(MakeDto(existingEntity));
        }

        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public override async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingEntity = await _repo.GetByIdAsync(id);
            if (existingEntity == null) return NotFound();

            if (existingEntity.Users.Count > 0) return Conflict("Cannot delete course with enrolled users.");

            await _repo.DeleteAsync(existingEntity);
            return NoContent();
        }

        // GET: api/[controller]/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var categories = await _repo.Query()
                .Select(c => c.Category)      
                .Distinct()                   
                .ToListAsync();

            return Ok(categories);          
        }

        // GET: api/[controller]/top-categories
        [HttpGet("top-categories")]
        public async Task<IActionResult> GetTopCategories(int top = 5)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var topCategories = await _repo.Query()
                .GroupBy(c => c.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    AverageRating = g.Average(c => c.Rate),
                    CourseCount = g.Count()
                })
                .OrderByDescending(g => g.AverageRating)
                .Take(top)
                .ToListAsync();

            return Ok(topCategories);
        }

        // GET: api/[controller]/top-courses
        [HttpGet("top-courses")]
        public async Task<IActionResult> GetTopCourses(int top = 5)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var topCourses = await _repo.Query()
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.Rate)
                .Take(top)
                .ToListAsync();

            List<CourseDto> topCourseDtos = topCourses.Select(MakeDto).ToList();

            return Ok(topCourseDtos);
        }

        // GET: api/[controller]/top-courses/{category}
        [HttpGet("top-courses/{category}")]
        public async Task<IActionResult> GetTopCourses(InstructorCategory category , int top = 4)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (top <= 0) top = 4;

            var topCourses = await _repo.Query()
                .Where(c => c.Category == category)
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.Rate)
                .Take(top)
                .ToListAsync();

            List<CourseDto> topCourseDtos = topCourses.Select(MakeDto).ToList();

            return Ok(topCourseDtos);
        }

        // POST: api/[controller]/cart
        [HttpPost("cart")]
        public async Task<IActionResult> GetCart([FromBody] HashSet<int> cartIds)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (cartIds == null || cartIds.Count == 0)
                return BadRequest("Cart IDs cannot be empty.");

            List<Course> courses = await _repo.Query()
                        .Include(c => c.Instructor)
                        .Where(c => cartIds.Contains(c.Id))
                        .ToListAsync();
            List<CourseDto> courseDtos = courses.Select(MakeDto).ToList();

            return Ok(courseDtos);
        }

        // GET: api/[controller]/search
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int top = 7)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be null or empty.");
            try
            {
                var results = await _filterService.Search(query, top);
                var resultDtos = results.Select(MakeDto).ToList();
                return Ok(resultDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during search: " + ex.Message });
            }
        }

        // GET: api/[controller]/search-pagination
        [HttpGet("search-pagination")]
        public async Task<IActionResult> SearchPagination([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 9)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be null or empty.");
            if(pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 9;
            try
            {
                var results = await _filterService.CourseSearch(query, pageNumber, pageSize);
                var resultDtos = results.Items.Select(MakeDto).ToList();
                return Ok(new
                {
                    items = resultDtos,
                    totalCount = results.TotalCount,
                    pageNumber = results.PageNumber,
                    pageSize = results.PageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during search: " + ex.Message });
            }
        }

        // POST: api/[controller]/filter
        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] FilterDto filter)
        {
            
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                if (filter.PageNumber <= 0) filter.PageNumber = 1;
                if (filter.PageSize <= 0) filter.PageSize = 9;
                if (filter.MinimumPrice < 0) filter.MinimumPrice = 0;
                if(filter.MaximumPrice < filter.MinimumPrice) throw new Exception("Maximum Price cannot be less than Minimum Price.");
                if (filter.MaximumPrice <= 0) filter.MaximumPrice = double.MaxValue;

                var result = await _filterService.Filter(filter);
                var resultDtos = result.Items.Select(MakeDto).ToList();
                
                return Ok(new
                {
                    items = resultDtos,
                    totalCount = result.TotalCount,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during filtering: " + ex.Message });
            }
        }

    }
}
