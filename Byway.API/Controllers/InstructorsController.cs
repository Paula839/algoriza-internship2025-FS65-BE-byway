using Byway.Application.Services;
using Byway.Core.DTOs;
using Byway.Core.Entities;
using Byway.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Byway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstructorsController : GenericController<Instructor, InstructorDto>
    {
        private readonly FilterService _filterService;

        public InstructorsController(IInstructorRepository repo, FilterService filterService) : base(repo)
        {
            _filterService = filterService;
        }


        protected override InstructorDto MakeDto(Instructor entity) => new InstructorDto(
            entity.Id,
            entity.Courses.Select(c => c.Id).ToList()
        )
        {
            Name = entity.Name,
            PictureUrl = entity.PictureUrl,
            Title = entity.Title,
            Rate = entity.Rate,
            Description = entity.Description
        };

        protected override Instructor MapToEntity(InstructorDto dto) => new Instructor
        {
            Name = dto.Name,
            PictureUrl = dto.PictureUrl,
            Title = dto.Title,
            Rate = dto.Rate,
            Description = dto.Description
        };

        protected override void UpdateEntity(Instructor entity, InstructorDto dto)
        {
            entity.Name = dto.Name;
            entity.PictureUrl = dto.PictureUrl;
            entity.Title = dto.Title;
            entity.Rate = dto.Rate;
            entity.Description = dto.Description;
        }


        protected override int GetEntityId(Instructor entity) => entity.Id;

        protected override async Task<bool> Validate(InstructorDto dto)
        {
            if (dto == null) { return false; }
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 100)
                return false;
            if (dto.PictureUrl != null && dto.PictureUrl.Length > 200)
                return false;
            if (dto.Rate < 0 || dto.Rate > 5)
                return false;
            if (dto.Description != null && dto.Description.Length > 1000)
                return false;

            return true;
        }

        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public override async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingEntity = await _repo.GetByIdAsync(id);
            if (existingEntity == null) return NotFound();

            if(existingEntity.Courses.Count > 0) return Conflict("Cannot delete instructor with associated courses.");

            await _repo.DeleteAsync(existingEntity);
            return NoContent();
        }

        // GET: api/[controller]/top
        [HttpGet("top")]
        public async Task<IActionResult> GetTopInstructors(int top = 10)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var topInstructors = await _repo.Query()
                .Include(i => i.Courses)
                    .ThenInclude(c => c.Users)
                .OrderByDescending(i => i.Rate)
                .Take(top)
                .ToListAsync();

            var topInstructorsDtos = topInstructors.Select(i =>
            {
                var dto = MakeDto(i); 
                return new
                {
                    dto.Name,
                    dto.PictureUrl,
                    dto.Title,
                    dto.Rate,
                    dto.Description,
                    NumberOfStudents = i.Courses.Sum(c => c.Users.Count)
                };
            }).ToList();

            return Ok(topInstructorsDtos);
        }

        // GET: api/[controller]
        [HttpGet("name")]
        public async Task<IActionResult> GetByName(string instructorName)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var instructor = await _repo.Query()
           .SingleOrDefaultAsync(c => c.Name.ToLower() == instructorName.ToLower());

            if (instructor == null)
                return NotFound();

            return Ok(MakeDto(instructor));
        }

        // GET: api/[controller]/search-pagination
        [HttpGet("search-pagination")]
        public async Task<IActionResult> SearchPagination([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 8)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be null or empty.");
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 9;
            try
            {
                var results = await _filterService.InstructorSearch(query, pageNumber, pageSize);
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

    }
}
