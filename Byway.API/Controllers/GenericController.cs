using Byway.Core.Interfaces;
using Byway.Core.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Byway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class GenericController<TEntity, TDto> : ControllerBase
        where TEntity : class
        where TDto : class
    {

        protected readonly IRepository<TEntity> _repo;

        protected GenericController(IRepository<TEntity> repo)
        {
            _repo = repo;
        }


        protected abstract TEntity MapToEntity(TDto dto);
        protected abstract void UpdateEntity(TEntity entity, TDto dto);

        protected abstract TDto MakeDto(TEntity entity);

        protected abstract int GetEntityId(TEntity entity);

        protected abstract Task<bool> Validate(TDto dto);


        

        // GET: api/[controller]
        [HttpGet]
        public virtual async Task<IActionResult> GetPages([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            if(pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _repo.Query();
            var totalCount = await query.CountAsync();

            var pagedEntities = await query
                .Skip((pageNumber - 1) * pageSize) 
                .Take(pageSize)
                .ToListAsync();

            var dtos = pagedEntities.Select(e => MakeDto(e)).ToList();

            var res = new
            {
                totalCount = totalCount,
                pageNumber = pageNumber,
                pageSize = pageSize,
                items = dtos
            };

            return Ok(res);
        }

        // GET: api/[controller]/all
        [HttpGet("all"), Authorize(Roles = "Admin")]
        public virtual async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var query = await _repo.GetAllAsync();
            var dtos = query.Select(e => MakeDto(e)).ToList();       
            return Ok(dtos);
        }

        // GET: api/[controller]/{id}
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> Get(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return Ok(MakeDto(entity));
        }



        // POST: api/[controller]
        
        [HttpPost, Authorize(Roles = "Admin")]
        public virtual async Task<IActionResult> Post([FromBody] TDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            if(dto == null) return BadRequest("DTO cannot be null");

            if (!await Validate(dto)) return BadRequest("Invalid DTO data");

            var entity = MapToEntity(dto);
            await _repo.AddAsync(entity);

            return CreatedAtAction(nameof(Get), new { id = GetEntityId(entity) }, MakeDto(entity));
        }

        // PUT: api/[controller]/{id}
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public virtual async Task<IActionResult> Put(int id, [FromBody] TDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if(dto == null) return BadRequest("DTO cannot be null");
            var existingEntity = await _repo.GetByIdAsync(id);
            if (existingEntity == null) return NotFound();

            if (!await Validate(dto)) return BadRequest("Invalid DTO data");


            UpdateEntity(existingEntity, dto);
            await _repo.UpdateAsync(existingEntity);
            return Ok(MakeDto(existingEntity));
        }

        // DELETE: api/[controller]/{id}
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var entity = await _repo.GetByIdAsync(id);
                if (entity == null) return NotFound();
                await _repo.DeleteAsync(entity);
                return Ok(new { message = "Deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Cannot delete entity because of related entity." });
            }
        }
    }
}

