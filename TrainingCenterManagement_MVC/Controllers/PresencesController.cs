using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TrainingCenterManagement_MVC.Data;
using TrainingCenterManagement_MVC.Models;
using TrainingCenterManagement_MVC.ViewModels;

namespace TrainingCenterManagement_MVC.Controllers
{
   // [Authorize(Roles = "Trainer")]
    public class PresencesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PresencesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Presences — the trainer's "الحضور" sidebar entry point.
        // Previously missing entirely, so the sidebar link 404'd.
        [Authorize(Roles = "Trainer")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (trainer == null) return Forbid();

            var lectures = await _context.Lectures
                .Where(l => !l.IsDeleted && l.Course.CourseTrainers.Any(ct => ct.TrainerId == trainer.TrainerId))
                .Include(l => l.Course)
                .Include(l => l.Presences)
                .OrderByDescending(l => l.LectureDate)
                .ToListAsync();

            var rows = lectures.Select(l => new PresenceOverviewRow
            {
                LectureId = l.LectureId,
                LectureTitle = l.Title,
                CourseName = l.Course.CourseName,
                LectureDate = l.LectureDate,
                IsUpcoming = l.LectureDate > DateTime.UtcNow,
                IsMarked = l.Presences.Any(),
                PresentCount = l.Presences.Count(p => p.IsPresent),
                TotalMarked = l.Presences.Count
            }).ToList();

            return View(rows);
        }

        public async Task<IActionResult> MarkAttendance(Guid lectureId)
        {
            var lecture = await _context.Lectures
                .Include(l => l.Course)
                    .ThenInclude(c => c.CourseTrainees)
                        .ThenInclude(ct => ct.Trainee)
                        .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(l => l.LectureId == lectureId);

            if (lecture == null)
                return NotFound();
            // مؤقتًا نستخدم قيمة ثابتة
              var duration = 1.5;
            var viewModel = new MarkAttendanceViewModel
            {
                LectureId = lectureId,
                LectureTitle = lecture.Title,
                DurationHours = duration, // ✅ مهم تمريرها هنا
                Trainees = lecture.Course.CourseTrainees.Select(ct => new TraineeAttendanceInput
                {
                    TraineeId = ct.TraineeId,
                    FullName = ct.Trainee.User.FullName,
                    IsPresent = false
                }).ToList()
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(MarkAttendanceViewModel model)
        {
            var lecture = await _context.Lectures.FindAsync(model.LectureId);
            if (lecture == null || lecture.LectureDate < DateTime.Now.AddHours(-model.DurationHours))
            {
                TempData["Error"] = "Cannot mark attendance after lecture time has passed.";
                return RedirectToAction("Index", "Lectures");
            }

            foreach (var trainee in model.Trainees)
            {
                var existing = await _context.Presences
                    .FirstOrDefaultAsync(p => p.LectureId == model.LectureId && p.TraineeId == trainee.TraineeId);

                if (existing == null)
                {
                    _context.Presences.Add(new Presence
                    {
                        PresenceId = Guid.NewGuid(),
                        LectureId = model.LectureId,
                        TraineeId = trainee.TraineeId,
                        IsPresent = trainee.IsPresent,
                        IsDeleted = false
                    });
                }
                else
                {
                    existing.IsPresent = trainee.IsPresent;
                    _context.Presences.Update(existing);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Attendance saved successfully.";
            return RedirectToAction("Details", "Lectures", new { id = model.LectureId });
        }

        public async Task<IActionResult> LectureAttendance(Guid lectureId)
        {
            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .Include(l => l.Presences)
                    .ThenInclude(p => p.Trainee)
                        .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(l => l.LectureId == lectureId);

            if (lecture == null) return NotFound();

            return View(lecture);
        }
        public async Task<IActionResult> TraineeAttendanceInCourse(Guid traineeId, Guid courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Presences)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null) return NotFound();

            var totalLectures = course.Lectures.Count;
            var attended = course.Lectures.Count(l =>
                l.Presences.Any(p => p.TraineeId == traineeId && p.IsPresent));

            ViewBag.Trainee = await _context.Trainees
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TraineeId == traineeId);

            ViewBag.Course = course;
            ViewBag.Attended = attended;
            ViewBag.TotalLectures = totalLectures;

            return View();
        }
        public async Task<IActionResult> ExportLectureAttendancePdf(Guid lectureId)
        {
            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .Include(l => l.Presences)
                    .ThenInclude(p => p.Trainee)
                        .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(l => l.LectureId == lectureId);

            if (lecture == null)
                return NotFound();

            var viewModel = lecture.Presences.Select(p => new LectureAttendanceViewModel
            {
                LectureTitle = lecture.Title,
                LectureDate = lecture.LectureDate,
                TraineeName = p.Trainee.User.FullName,
                IsPresent = p.IsPresent
            }).ToList();

            return new ViewAsPdf("LectureAttendancePdf", viewModel)
            {
                FileName = $"Lecture_Attendance_{lecture.Title}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }

        public ActionResult LectureAttendanceReport(Guid lectureId)
        {
            var lecture = _context.Lectures
                .Include(l => l.Presences)
                .ThenInclude(p => p.Trainee)
                .FirstOrDefault(l => l.LectureId == lectureId);

            if (lecture == null)
            {
                return NotFound();
            }

            var viewModel = lecture.Presences.Select(p => new LectureAttendanceViewModel
            {
                LectureTitle = lecture.Title,
                LectureDate = lecture.LectureDate,
                TraineeName = p.Trainee.User.FullName,
                IsPresent = p.IsPresent
            }).ToList();

            return View(viewModel);
        }



    }
}
