using Byway.API.Pagination;
using Byway.Application.DTOs;
using Byway.Application.Services.Enums;
using Byway.Core.Entities;
using Byway.Core.Entities.Enums;
using Byway.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Byway.Application.Services
{
    public class FilterService
    {
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<Instructor> _instructorRepo;

        public FilterService(IRepository<Course> courseRepo, IRepository<Instructor> instructorRepo)
        {
            _courseRepo = courseRepo;
            _instructorRepo = instructorRepo;
        }

        
        public async Task<List<Course>> Search(string query, int top = 7)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    throw new Exception("Query cannot be null or empty!");
                }

                query = query.ToLower();

                var result = await _courseRepo.Query()
                            .Where(c =>
                                c.Name.ToLower().Contains(query) ||
                                (c.Description != null && c.Description.ToLower().Contains(query)) ||
                                c.Instructor.Name.ToLower().Contains(query) ||
                                c.Category.ToString().ToLower().Contains(query)
                                
                            )
                            .Take(top)
                            .ToListAsync();

                return result;
            }

            catch (Exception ex)
            {
                throw new Exception($"Error during search: {ex.Message}");
            }

        }

        public async Task<PagedResult<Instructor>> InstructorSearch(
        string query,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 8)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    throw new ArgumentException("Query cannot be null or empty!");

                query = query.ToLower();

                var queryable = _instructorRepo.Query()
                    .Where(c =>
                        c.Name.ToLower().Contains(query) ||
                        (c.Description != null && c.Description.ToLower().Contains(query)) ||
                        c.Title.ToString().ToLower().Contains(query)
                    );

                var totalCount = await queryable.CountAsync();

                var items = await queryable
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<Instructor>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during instructor search: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<Course>> CourseSearch(
            string query,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 9)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    throw new ArgumentException("Query cannot be null or empty!");

                query = query.ToLower();

                var queryable = _courseRepo.Query()
                    .Where(c =>
                        c.Name.ToLower().Contains(query) ||
                        (c.Description != null && c.Description.ToLower().Contains(query)) ||
                        c.Category.ToString().ToLower().Contains(query)
                    );

                var totalCount = await queryable.CountAsync();

                var items = await queryable
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<Course>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during course search: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<Course>> Filter(FilterDto filterDto)
        {
            try
            {
                var query = _courseRepo.Query().AsQueryable();

                if (filterDto.Categories != null && filterDto.Categories.Any())
                    query = query.Where(c => filterDto.Categories.Contains(c.Category));

                if (filterDto.Rate > 0)
                    query = query.Where(c => c.Rate == filterDto.Rate);

                query = query.Where(c => c.Price >= filterDto.MinimumPrice && c.Price <= filterDto.MaximumPrice);

                if (filterDto.NumOfLecturesOption.HasValue)
                {
                    query = query
                        .Select(c => new
                        {
                            Course = c,
                            TotalLectures = c.Contents.Sum(l => l.NumOfLectures)
                        })
                        .Where(c =>
                            (filterDto.NumOfLecturesOption.Value == NumOfLectures.From1To15 && c.TotalLectures >= 1 && c.TotalLectures <= 15) ||
                            (filterDto.NumOfLecturesOption.Value == NumOfLectures.From16To30 && c.TotalLectures >= 16 && c.TotalLectures <= 30) ||
                            (filterDto.NumOfLecturesOption.Value == NumOfLectures.From31To45 && c.TotalLectures >= 31 && c.TotalLectures <= 45) ||
                            (filterDto.NumOfLecturesOption.Value == NumOfLectures.MoreThan45 && c.TotalLectures > 45)
                        )
                        .Select(c => c.Course);
                }

                query = filterDto.SortBy switch
                {
                    SortBy.TheLatest => query.OrderByDescending(c => c.CreatedAt),
                    SortBy.TheOldest => query.OrderBy(c => c.CreatedAt),
                    SortBy.HighestPrice => query.OrderByDescending(c => c.Price),
                    SortBy.LowestPrice => query.OrderBy(c => c.Price),
                    SortBy.HighestRated => query.OrderByDescending(c => c.Rate),
                    SortBy.LowestRated => query.OrderBy(c => c.Rate),
                    _ => query
                };

                var totalCount = await query.CountAsync();

                var pagedCourses = await query
                    .Skip((filterDto.PageNumber - 1) * filterDto.PageSize)
                    .Take(filterDto.PageSize)
                    .ToListAsync();

                return new PagedResult<Course>
                {
                    Items = pagedCourses,
                    TotalCount = totalCount,
                    PageNumber = filterDto.PageNumber,
                    PageSize = filterDto.PageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during filtering: {ex.Message}");
            }
        }


    }
}
