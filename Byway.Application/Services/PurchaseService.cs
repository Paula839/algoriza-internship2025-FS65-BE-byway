using Byway.Application.DTOs;
using Byway.Application.Interfaces;
using Byway.Core.DTOs;
using Byway.Core.Entities;
using Byway.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace Byway.Application.Services
{
    public class PurchaseService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IEmailService _emailService;
        public PurchaseService(IRepository<User> userRepo, IRepository<Course> courseRepo, IEmailService emailService)
        {
            _userRepo = userRepo;
            _courseRepo = courseRepo;
            _emailService = emailService;
        }

        private double Tax(double total, int per = 15)
        {
            return total * per / 100.0;
        }

        private double Discount(double total, int per = 0)
        {
            return total - total * per / 100.0;
        }

        private ReceiptDto MakeDto(List<Course> courses)
        {
            var receipt = new ReceiptDto();

            receipt.Courses = courses.Select(c => new CourseReceiptItemDto
            {
                Id = c.Id,
                Course = c.Name,
                Price = c.Price
            }).ToList();

            double totalPrice = courses.Sum(c => c.Price);

            receipt.TotalPrice = totalPrice + Tax(totalPrice);
            return receipt;
        }


        public async Task<ReceiptDto> PurchaseCourses(int userId, HashSet<int> courseIds)
        {
            if (courseIds == null || !courseIds.Any()) throw new ArgumentException("Course IDs cannot be null or empty!");
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found.");

            var courses = await _courseRepo.GetAllByIdAsync(courseIds);

            if (courses.Count != courseIds.Count) throw new KeyNotFoundException("One or more courses not found!");

            var ownedCourses = user.Courses.Select(c => c.Id).ToHashSet();
            var duplicateCourses = courses.Where(c => ownedCourses.Contains(c.Id)).ToList();

            if (duplicateCourses.Any())
            {
                throw new InvalidOperationException($"User already owns course(s): {string.Join(", ", duplicateCourses.Select(c => c.Id))}");
            }

            double totalPrice = 0.0;

            foreach (var course in courses)
            {
                user.Courses.Add(course);
                totalPrice += course.Price;
            }


            await _userRepo.SaveAsync();
            
            await _emailService.SendEmailAsync(user.Email,
                "🎉 Purchase Confirmation - Byway Learning!",
                $"🎉 Thank you for your purchase. Your courses are now available in your dashboard. Best of luck on your learning journey.");


            return MakeDto(courses);
        }

    }
}
