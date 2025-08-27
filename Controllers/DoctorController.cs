using Microsoft.AspNetCore.Mvc;
using HealthCareApp.Attributes;
using HealthCareApp.Services;
using HealthCareApp.Models;
using HealthCareApp.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCareApp.Controllers
{
    [AuthorizeUserType("Doctor")]
    public class DoctorController : Controller
    {
        private readonly ILogger<DoctorController> _logger;
        private readonly IAppointmentService _appointmentService;
        private readonly HealthCareDbContext _context;

        public DoctorController(ILogger<DoctorController> logger, IAppointmentService appointmentService, HealthCareDbContext context)
        {
            _logger = logger;
            _appointmentService = appointmentService;
            _context = context;
        }

        private async Task<int?> ResolveDoctorIdFromSessionAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (!int.TryParse(userIdStr, out int userIdInt)) return null;
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == userIdInt);
            return doctor?.DoctorID;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Doctor Dashboard";

            var doctorId = await ResolveDoctorIdFromSessionAsync();
            if (doctorId.HasValue)
            {
                ViewBag.TodayAppointments = await _appointmentService.GetTodayAppointmentsCountAsync(doctorId.Value);
                ViewBag.TotalPatients = 0; // Can compute distinct patients if needed
                ViewBag.PendingAppointments = await _appointmentService.GetPendingAppointmentsCountAsync(doctorId.Value);
                ViewBag.AverageRating = 0.0; // Placeholder
            }

            return View();
        }

        public async Task<IActionResult> Appointments()
        {
            ViewData["Title"] = "My Appointments";

            var doctorId = await ResolveDoctorIdFromSessionAsync();
            if (doctorId.HasValue)
            {
                var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId.Value);
                return View(appointments);
            }

            return View(new List<Appointment>());
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int appointmentId)
        {
            try
            {
                var result = await _appointmentService.ConfirmAppointmentAsync(appointmentId);
                TempData[result ? "SuccessMessage" : "ErrorMessage"] = result ? "Appointment confirmed successfully!" : "Failed to confirm appointment.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming appointment");
                TempData["ErrorMessage"] = "An error occurred while confirming the appointment.";
            }
            return RedirectToAction("Appointments");
        }

        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                var result = await _appointmentService.CancelAppointmentAsync(appointmentId);
                TempData[result ? "SuccessMessage" : "ErrorMessage"] = result ? "Appointment cancelled successfully!" : "Failed to cancel appointment.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment");
                TempData["ErrorMessage"] = "An error occurred while cancelling the appointment.";
            }
            return RedirectToAction("Appointments");
        }

        [HttpPost]
        public async Task<IActionResult> RescheduleAppointment(int appointmentId, DateTime newDate, TimeSpan newTime)
        {
            try
            {
                var appt = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                if (appt == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found.";
                    return RedirectToAction("Appointments");
                }
                appt.AppointmentDate = newDate;
                appt.AppointmentTime = newTime;
                var ok = await _appointmentService.UpdateAppointmentAsync(appt);
                TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Appointment rescheduled." : "Failed to reschedule.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling appointment");
                TempData["ErrorMessage"] = "An error occurred while rescheduling the appointment.";
            }
            return RedirectToAction("Appointments");
        }

        public async Task<IActionResult> Patients()
        {
            ViewData["Title"] = "My Patients";

            var doctorId = await ResolveDoctorIdFromSessionAsync();
            if (doctorId.HasValue)
            {
                var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId.Value);
                var patients = appointments.Select(a => a.Patient).DistinctBy(p => p.PatientID).ToList();
                return View(patients);
            }

            return View(new List<Patient>());
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            ViewData["Title"] = "Doctor Profile";
            var doctorId = await ResolveDoctorIdFromSessionAsync();
            if (!doctorId.HasValue) return RedirectToAction("Index");
            var doctor = await _context.Doctors.Include(d => d.Location).Include(d => d.Specialty).FirstOrDefaultAsync(d => d.DoctorID == doctorId.Value);
            return View(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(Doctor model)
        {
            var doctorId = await ResolveDoctorIdFromSessionAsync();
            if (!doctorId.HasValue) return RedirectToAction("Index");
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorID == doctorId.Value);
            if (doctor == null) return RedirectToAction("Index");

            doctor.Name = model.Name;
            doctor.ContactNo = model.ContactNo;
            doctor.AvailableTime = model.AvailableTime;
            doctor.Fees = model.Fees;
            doctor.LocationID = model.LocationID;
            doctor.SpecialID = model.SpecialID;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profile updated.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> Schedule()
        {
            ViewData["Title"] = "Manage Schedule";
            var doctorId = await ResolveDoctorIdFromSessionAsync();
            if (!doctorId.HasValue) return RedirectToAction("Index");
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorID == doctorId.Value);
            var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId.Value);
            ViewBag.Appointments = appointments;
            return View(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> Schedule(string availableTime)
        {
            var doctorId = await ResolveDoctorIdFromSessionAsync();
            if (!doctorId.HasValue) return RedirectToAction("Index");
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorID == doctorId.Value);
            if (doctor == null) return RedirectToAction("Index");
            doctor.AvailableTime = availableTime;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Schedule updated.";
            return RedirectToAction("Schedule");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateConsultationNotes(int appointmentId, string notes, string? medicine)
        {
            try
            {
                var appt = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                if (appt == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found.";
                    return RedirectToAction("Schedule");
                }

                // Enforce DB max lengths to avoid truncation errors
                string? safeNotes = notes;
                if (!string.IsNullOrEmpty(safeNotes) && safeNotes.Length > 1000)
                {
                    safeNotes = safeNotes.Substring(0, 1000);
                }
                string? safeMedicine = medicine;
                if (!string.IsNullOrEmpty(safeMedicine) && safeMedicine.Length > 500)
                {
                    safeMedicine = safeMedicine.Substring(0, 500);
                }

                var record = new MedicalHistory
                {
                    PatientID = appt.PatientID,
                    DoctorID = appt.DoctorID,
                    RecordDate = DateTime.Today,
                    Notes = safeNotes,
                    Medicine = safeMedicine
                };
                await _context.MedicalHistories.AddAsync(record);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Consultation notes saved.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving consultation notes for appointment {AppointmentId}", appointmentId);
                TempData["ErrorMessage"] = "Failed to save consultation notes.";
            }
            return RedirectToAction("Schedule");
        }
    }
}
