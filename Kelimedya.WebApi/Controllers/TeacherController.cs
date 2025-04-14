using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kelimedya.Core.Entities;
using Kelimedya.WebAPI.Models;
using Kelimedya.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Kelimedya.Core.Enum;
using Kelimedya.Core.IdentityEntities;
using Kelimedya.Core.Models;

namespace Kelimedya.WebAPI.Controllers
{
    [Route("api/teacher")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly KelimedyaDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public TeacherController(KelimedyaDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            var studentRole = await _context.Roles.SingleOrDefaultAsync(r => r.Name == RoleNames.Student);
            if (studentRole == null)
            {
                return NotFound("Student rolü bulunamadı.");
            }

            var students = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == studentRole.Id))
                .ToListAsync();
            int totalStudents = students.Count;
            int newStudents = students.Count(u => u.CreatedAt.Value.Date == DateTime.UtcNow.Date);

            var studentReports = new List<StudentReportViewModel>();

            foreach (var student in students)
            {
                var progresses = await _context.StudentLessonProgresses
                    .Include(p => p.Lesson)
                        .ThenInclude(l => l.Course)
                    .Where(p => p.StudentId == student.Id.ToString())
                    .ToListAsync();

                var completedLessons = progresses
                    .Where(p => p.IsCompleted)
                    .Select(p => new LessonReportDto
                    {
                        LessonId = p.LessonId,
                        Title = p.Lesson.Title,
                        CourseTitle = p.Lesson.Course.Title,
                        CompletionPercentage = p.CompletionPercentage,
                        StartDate = p.StartDate,
                        LastAccessDate = p.LastAccessDate
                    })
                    .ToList();

                studentReports.Add(new StudentReportViewModel
                {
                    StudentId = student.Id.ToString(),
                    FullName = $"{student.Name} {student.Surname}",
                    Email = student.Email,
                    CreatedAt = student.CreatedAt ?? DateTime.MinValue,
                    CompletedLessons = completedLessons
                });
            }

            decimal averageProgress = 0;
            if (studentReports.Any())
            {
                averageProgress = (decimal)studentReports.Average(r =>
                    r.CompletedLessons.Any() ? r.CompletedLessons.Average(l => (double)l.CompletionPercentage) : 0);
            }

            var teacherName = "Ayşe Öğretmen";

            var dashboard = new TeacherDashboardViewModel
            {
                TeacherName = teacherName,
                TotalStudents = totalStudents,
                NewStudents = newStudents,
                AverageProgress = averageProgress,
                StudentReports = studentReports
            };

            return Ok(dashboard);
        }
        // GET: api/teacher/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var studentRole = await _context.Roles.SingleOrDefaultAsync(r => r.Name == RoleNames.Student);
            var students = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == studentRole.Id))
                .ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in students)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";
                userDtos.Add(new UserDto
                {
                    Id = user.Id.ToString(),
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = $"{user.Name} {user.Surname}",
                    Role = role,
                    CreatedAt = user.CreatedAt
                });
            }

            return Ok(userDtos);
        }
    }
}
