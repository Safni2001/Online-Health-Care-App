using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using HealthCareApp.Attributes;
using HealthCareApp.Services;
using HealthCareApp.Models;
using HealthCareApp.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCareApp.Controllers
{
    [AuthorizeUserType("Patient")]
    public class PatientController : Controller
    {
        private readonly ILogger<PatientController> _logger;
        private readonly IAppointmentService _appointmentService;
        private readonly HealthCareApp.Data.HealthCareDbContext _context;

        public PatientController(ILogger<PatientController> logger, IAppointmentService appointmentService, HealthCareApp.Data.HealthCareDbContext context)
        {
            _logger = logger;
            _appointmentService = appointmentService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Patient Dashboard";
            
            // Get patient ID from session
            var userId = HttpContext.Session.GetString("UserID");
            if (int.TryParse(userId, out int patientId))
            {
                // Get patient statistics
                ViewBag.UpcomingAppointments = await _appointmentService.GetUpcomingAppointmentsCountAsync(patientId);
                ViewBag.TotalAppointments = await _appointmentService.GetTotalAppointmentsCountAsync(patientId);
                ViewBag.MedicalRecords = 0; // Placeholder for now
                ViewBag.PendingPayments = 0; // Placeholder for now
            }
            
            return View();
        }

        public async Task<IActionResult> FindDoctors(string? specialty, string? location, string? search)
        {
            ViewData["Title"] = "Find Doctors";
            
            var doctors = await _appointmentService.SearchDoctorsAsync(specialty, location, search);
            return View(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> BookAppointment(int? doctorId = null)
        {
            ViewData["Title"] = "Book Appointment";

            // Get all doctors for dropdown
            var doctors = await _appointmentService.SearchDoctorsAsync();
            ViewBag.Doctors = doctors;

            // If doctorId is provided, pre-select the doctor (optional, not implemented here)
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                if (int.TryParse(userId, out int userIdInt))
                {
                    // Fetch the PatientID using the UserID from the session
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userIdInt);
                    if (patient != null)
                    {
                        int patientId = patient.PatientID;

                        // Fetch selected doctor to get fee
                        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorID == model.DoctorID);
                        var doctorFee = doctor?.Fees ?? 0m;

                        var appointment = new Appointment
                        {
                            PatientID = patientId,
                            DoctorID = model.DoctorID,
                            LocationID = model.LocationID,
                            AppointmentDate = model.AppointmentDate,
                            AppointmentTime = model.AppointmentTime,
                            IsCancelled = false
                        };

                        var result = await _appointmentService.CreateAppointmentAsync(appointment);

                        if (result)
                        {
                            // Create payment records based on amount paid now
                            decimal amountPaidNow = model.PayNowAmount;
                            decimal remaining = Math.Max(0m, doctorFee - amountPaidNow);

                            if (amountPaidNow > 0)
                            {
                                var paidPayment = new Payment
                                {
                                    PatientID = patientId,
                                    Amount = amountPaidNow,
                                    PaymentStatus = "Completed",
                                    PaymentDate = DateTime.Now
                                };
                                await _context.Payments.AddAsync(paidPayment);
                            }

                            if (remaining > 0)
                            {
                                var pendingPayment = new Payment
                                {
                                    PatientID = patientId,
                                    Amount = remaining,
                                    PaymentStatus = "Pending",
                                    PaymentDate = DateTime.Now
                                };
                                await _context.Payments.AddAsync(pendingPayment);
                            }

                            await _context.SaveChangesAsync();

                            if (remaining == 0m)
                            {
                                TempData["SuccessMessage"] = "Appointment booked and payment completed!";
                            }
                            else if (amountPaidNow > 0m)
                            {
                                TempData["SuccessMessage"] = $"Appointment booked. Paid ${amountPaidNow}. Remaining ${remaining}.";
                            }
                            else
                            {
                                TempData["SuccessMessage"] = "Appointment booked. Payment pending.";
                            }

                            return RedirectToAction("MyAppointments");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to book appointment.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Patient record not found for this user.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking appointment");
                ModelState.AddModelError("", "An error occurred while booking the appointment.");
            }

            // Repopulate doctors for the view
            var doctors = await _appointmentService.SearchDoctorsAsync();
            ViewBag.Doctors = doctors;
            return View(model);
        }

        public async Task<IActionResult> MyAppointments()
        {
            ViewData["Title"] = "My Appointments";

            var userId = HttpContext.Session.GetString("UserID");
            if (int.TryParse(userId, out int userIdInt))
            {
                // Fetch the PatientID using the UserID from the session
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userIdInt);
                if (patient != null)
                {
                    int patientId = patient.PatientID;
                    var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);
                    return View(appointments);
                }
            }

            return View(new List<Appointment>());
        }

        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                var result = await _appointmentService.CancelAppointmentAsync(appointmentId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Appointment cancelled successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel appointment.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment");
                TempData["ErrorMessage"] = "An error occurred while cancelling the appointment.";
            }
            
            return RedirectToAction("MyAppointments");
        }

        public async Task<IActionResult> MedicalRecords()
        {
            ViewData["Title"] = "Medical Records";
            var userId = HttpContext.Session.GetString("UserID");
            if (!int.TryParse(userId, out int userIdInt))
                return RedirectToAction("Index");

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userIdInt);
            if (patient == null) return RedirectToAction("Index");

            var records = await _context.MedicalHistories
                .Include(m => m.Doctor)
                .Where(m => m.PatientID == patient.PatientID)
                .OrderByDescending(m => m.RecordDate)
                .ToListAsync();

            return View(records);
        }

        public IActionResult Profile()
        {
            ViewData["Title"] = "Patient Profile";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PayFees()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (!int.TryParse(userId, out int userIdInt))
                return RedirectToAction("Index");

            // Resolve PatientID from UserID
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userIdInt);
            if (patient == null)
                return RedirectToAction("Index");

            var payments = await _context.Payments
                .Where(p => p.PatientID == patient.PatientID)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var totalDue = payments.Where(p => p.PaymentStatus == "Pending").Sum(p => p.Amount);
            var totalPaid = payments.Where(p => p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid").Sum(p => p.Amount);

            ViewBag.TotalDue = totalDue;
            ViewBag.TotalPaid = totalPaid;
            return View(payments);
        }

        [HttpPost]
        public async Task<IActionResult> PayFees(int paymentId, decimal amount)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (!int.TryParse(userId, out int userIdInt))
                return RedirectToAction("Index");

            // Resolve PatientID from UserID
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userIdInt);
            if (patient == null)
                return RedirectToAction("Index");

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentID == paymentId && p.PatientID == patient.PatientID);
            if (payment != null && payment.PaymentStatus == "Pending")
            {
                if (amount >= payment.Amount)
                {
                    // Cap to pending amount and complete
                    payment.Amount = payment.Amount; // keep recorded due amount
                    payment.PaymentStatus = "Completed";
                    payment.PaymentDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Payment completed.";
                }
                else if (amount > 0)
                {
                    // Partial payment: reduce remaining and keep pending
                    payment.Amount = payment.Amount - amount;
                    payment.PaymentDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Partial payment recorded. Remaining ${payment.Amount}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Enter a valid amount greater than 0.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid payment or already paid.";
            }
            return RedirectToAction("PayFees");
        }

        [HttpGet]
        public async Task<IActionResult> Feedback()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (!int.TryParse(userId, out int userIdInt))
                return RedirectToAction("Index");

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userIdInt);
            if (patient == null) return RedirectToAction("Index");

            var feedbacks = await _context.Feedbacks
                .Where(f => f.PatientID == patient.PatientID)
                .OrderByDescending(f => f.FeedbackDate)
                .ToListAsync();

            return View(feedbacks);
        }

        [HttpPost]
        public async Task<IActionResult> Feedback(int? doctorId, string message, int rating, int? appointmentId)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (!int.TryParse(userId, out int userIdInt))
                return RedirectToAction("Index");

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userIdInt);
            if (patient == null) return RedirectToAction("Index");

            // Clamp rating to [1,5]
            if (rating < 1) rating = 1;
            if (rating > 5) rating = 5;

            // If doctorId not provided, infer from appointment
            int? resolvedDoctorId = doctorId;
            if ((!resolvedDoctorId.HasValue || resolvedDoctorId.Value == 0) && appointmentId.HasValue)
            {
                var appt = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentID == appointmentId.Value && a.PatientID == patient.PatientID);
                if (appt != null)
                {
                    resolvedDoctorId = appt.DoctorID;
                }
            }

            var feedback = new Feedback
            {
                PatientID = patient.PatientID,
                DoctorID = resolvedDoctorId,
                AppointmentID = appointmentId,
                Rating = rating,
                Comments = message,
                FeedbackDate = DateTime.Now
            };

            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Feedback submitted!";
            return RedirectToAction("MyAppointments");
        }
    }

    // View Models
    public class BookAppointmentViewModel
    {
        [Required]
        [Display(Name = "Doctor")]
        public int DoctorID { get; set; }

        [Display(Name = "Location")]
        public int? LocationID { get; set; }

        // Removed [Required] for AppointmentDate and AppointmentTime
        [DataType(DataType.Date)]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [DataType(DataType.Time)]
        [Display(Name = "Appointment Time")]
        public TimeSpan AppointmentTime { get; set; } = new TimeSpan(9, 0, 0);

        [Display(Name = "Pay Now Amount")]
        [Range(0, double.MaxValue)]
        public decimal PayNowAmount { get; set; } = 0m;
    }
}