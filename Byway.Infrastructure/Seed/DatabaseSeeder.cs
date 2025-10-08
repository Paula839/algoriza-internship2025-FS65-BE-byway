using Bogus;
using Byway.Core.Entities;
using Byway.Core.Entities.Enums;
using Byway.Infrastructure.Data;
using System;
using System.Linq;

namespace Byway.Infrastructure.Seed
{
    public class DatabaseSeeder
    {
        private readonly BywayDbContext _db;

        public DatabaseSeeder(BywayDbContext db)
        {
            _db = db;
        }

        public void Seed(int userCount = 1000, int courseCount = 1000, int instructorCount = 300)
        {
            var rand = new Random();

            if (_db.Users.Count() < 10)
            {
                var userFaker = new Faker<User>()
                    .RuleFor(u => u.Name, f => f.Name.FullName())
                    .RuleFor(u => u.Username, f => f.Internet.UserName() + f.UniqueIndex)
                    .RuleFor(u => u.Email, f => f.Internet.Email().Replace("@", $"{f.UniqueIndex}@"))
                    .RuleFor(u => u.HashedPassword, f => BCrypt.Net.BCrypt.HashPassword("password"))
                    .RuleFor(u => u.IsAdmin, f => false);

                var users = userFaker.Generate(userCount);
                _db.Users.AddRange(users);
                _db.SaveChanges();
            }

            if (_db.Instructors.Count() < 10)
            {
                var instructorFaker = new Faker<Instructor>()
                    .RuleFor(i => i.Name, f => f.Name.FullName())
                    .RuleFor(i => i.Rate, f => Math.Round(f.Random.Double(3.0, 5.0), 1))
                    .RuleFor(i => i.Description, f => f.Lorem.Paragraph())
                    .RuleFor(i => i.Title, f => f.PickRandom<InstructorCategory>())
                    .RuleFor(i => i.PictureUrl, f => f.Internet.Avatar());

                var instructors = instructorFaker.Generate(instructorCount);
                _db.Instructors.AddRange(instructors);
                _db.SaveChanges();
            }

            if (_db.Courses.Count() < 10)
            {
                var instructorsList = _db.Instructors.ToList();

                var courseFaker = new Faker<Course>()
                    .RuleFor(c => c.Instructor, f => f.PickRandom(instructorsList))
                    .RuleFor(c => c.InstructorId, (f, c) => c.Instructor.Id)
                    .RuleFor(c => c.Category, (f, c) => c.Instructor.Title) 
                    .RuleFor(c => c.Name, (f, c) =>
                    {
                        var skill = c.Instructor.Title.ToString(); 
                        var subjects = skill switch
                        {
                            "FrontendDevelopment" => new[] { "React", "Vue", "Angular", "HTML & CSS", "TypeScript" },
                            "BackendDevelopment" => new[] { "Node.js", "ASP.NET", "Java Spring", "Python Django" },
                            "FullstackDevelopment" => new[] { "MERN Stack", "MEAN Stack", "Fullstack Java" },
                            "UXUIDesign" => new[] { "Figma", "Adobe XD", "UX Research", "Wireframing" },
                            "MobileDevelopment" => new[] { "Flutter", "React Native", "Swift", "Kotlin" },
                            "DevOps" => new[] { "Docker", "Kubernetes", "CI/CD", "AWS" },
                            "DataEngineering" => new[] { "ETL", "Spark", "Airflow", "Data Pipelines" },
                            "DataScience" => new[] { "Machine Learning", "Python for Data Science", "Deep Learning" },
                            "QualityAssurance" => new[] { "Manual Testing", "Selenium", "Cypress", "Test Automation" },
                            "ProductManagement" => new[] { "Agile", "Scrum", "Roadmapping" },
                            "ProjectManagement" => new[] { "Project Planning", "Agile PM", "Risk Management" },
                            "SystemAdministration" => new[] { "Linux Admin", "Windows Server", "Networking" },
                            "SecurityEngineering" => new[] { "Cybersecurity Basics", "Penetration Testing", "Ethical Hacking" },
                            "CloudArchitecture" => new[] { "AWS Cloud", "Azure Fundamentals", "GCP Essentials" },
                            "BusinessAnalysis" => new[] { "Business Analysis", "Requirements Gathering", "Process Mapping" },
                            _ => new[] { "General Course" }
                        };
                        var subject = f.PickRandom(subjects);
                        var levels = new[] { "Beginner", "Intermediate", "Advanced", "Masterclass", "Bootcamp" };
                        var level = f.PickRandom(levels);
                        return $"{level} {subject}"; 
                    }).RuleFor(c => c.Description, f => f.Lorem.Paragraph())
                    .RuleFor(c => c.Certification, f => f.Company.CatchPhrase())
                    .RuleFor(c => c.Level, f => f.PickRandom<Level>())
                    .RuleFor(c => c.Price, f => Math.Round(f.Random.Double(10, 900), 2))
                    .RuleFor(c => c.Rate, f => Math.Round(f.Random.Double(1, 5), 1))
                    .RuleFor(c => c.TotalHours, f => Math.Round(f.Random.Double(1, 50), 1))
                    .RuleFor(c => c.PictureUrl, f => f.Image.PicsumUrl(200, 100))
                    .RuleFor(c => c.Contents, f =>
                    {
                        return new Faker<Content>()
                            .RuleFor(ct => ct.Name, ff => ff.Lorem.Sentence(3))
                            .RuleFor(ct => ct.NumOfLectures, ff => ff.Random.Int(1, 10))
                            .RuleFor(ct => ct.Duration, ff => Math.Round(ff.Random.Double(0.5, 3.0), 1))
                            .Generate(f.Random.Int(3, 8));
                    });

                var courses = courseFaker.Generate(courseCount);
                _db.Courses.AddRange(courses);
                _db.SaveChanges();


                var allUsers = _db.Users.Where(u => !u.IsAdmin).ToList();
                var allCourses = _db.Courses.ToList();

                foreach (var user in allUsers)
                {
                    var purchasedCourses = allCourses.OrderBy(c => rand.Next()).Take(rand.Next(1, 6)).ToList();
                    foreach (var course in purchasedCourses)
                    {
                        if (!user.Courses.Contains(course))
                            user.Courses.Add(course);
                    }
                }

                _db.SaveChanges();
            }
        }
    }
}
