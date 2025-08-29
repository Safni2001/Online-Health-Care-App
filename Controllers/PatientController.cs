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

        private async Task<string> GenerateBookingIdAsync()
        {
            // Get the last booking reference from the database
            var lastPayment = await _context.Payments
                .Where(p => !string.IsNullOrEmpty(p.BookingRef) && p.BookingRef.StartsWith("BK"))
                .OrderByDescending(p => p.PaymentID)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastPayment != null && !string.IsNullOrEmpty(lastPayment.BookingRef))
            {
                // Extract number from BookingRef (e.g., "BK001" -> 1)
                var numberPart = lastPayment.BookingRef.Substring(2);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            // Format as BK001, BK002, etc.
            return $"BK{nextNumber:D3}";
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber)) return cardNumber;
            
            // Remove any existing formatting
            var cleanNumber = cardNumber.Replace("-", "").Replace(" ", "");
            
            if (cleanNumber.Length < 4)
                return "****";
                
            // Show only last 4 digits
            var lastFour = cleanNumber.Substring(cleanNumber.Length - 4);
            return $"****-****-****-{lastFour}";
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

            // Get all locations for dropdown
            var locations = await _context.Locations.ToListAsync();
            ViewBag.Locations = locations;

            // If doctorId is provided, pre-select the doctor
            var model = new BookAppointmentViewModel();
            if (doctorId.HasValue)
            {
                model.DoctorID = doctorId.Value;
                // Find the selected doctor's location
                var selectedDoctor = doctors.FirstOrDefault(d => d.DoctorID == doctorId.Value);
                if (selectedDoctor?.LocationID.HasValue == true)
                {
                    model.LocationID = selectedDoctor.LocationID.Value;
                }
            }

            return View(model);
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
                            // Generate single booking ID for this appointment
                            var bookingId = await GenerateBookingIdAsync();
                            
                            decimal amountPaidNow = model.PayNowAmount;
                            
                            // Create ONE main payment record for the total doctor fee
                            var mainPayment = new Payment
                            {
                                BookingRef = bookingId,
                                PatientID = patientId,
                                Amount = doctorFee, // Always store the full fee amount
                                PaymentStatus = amountPaidNow >= doctorFee ? "Completed" : "Pending",
                                PaymentDate = DateTime.Now,
                                BankName = amountPaidNow > 0 ? model.BankName : null,
                                CardNumber = amountPaidNow > 0 && !string.IsNullOrEmpty(model.CardNumber) ? MaskCardNumber(model.CardNumber) : null,
                                ExpiryDate = amountPaidNow > 0 ? model.ExpiryDate : null,
                                CVN = amountPaidNow > 0 && !string.IsNullOrEmpty(model.CVN) ? "***" : null
                            };
                            await _context.Payments.AddAsync(mainPayment);
                            
                            // If partial payment was made, create a separate payment record to track the payment history
                            if (amountPaidNow > 0 && amountPaidNow < doctorFee)
                            {
                                var partialPayment = new Payment
                                {
                                    BookingRef = bookingId + "_P1", // Add suffix to indicate partial payment
                                    PatientID = patientId,
                                    Amount = amountPaidNow,
                                    PaymentStatus = "Completed",
                                    PaymentDate = DateTime.Now,
                                    BankName = model.BankName,
                                    CardNumber = !string.IsNullOrEmpty(model.CardNumber) ? MaskCardNumber(model.CardNumber) : null,
                                    ExpiryDate = model.ExpiryDate,
                                    CVN = !string.IsNullOrEmpty(model.CVN) ? "***" : null
                                };
                                await _context.Payments.AddAsync(partialPayment);
                            }

                            await _context.SaveChangesAsync();

                            if (amountPaidNow >= doctorFee)
                            {
                                TempData["SuccessMessage"] = "Appointment booked and payment completed!";
                            }
                            else if (amountPaidNow > 0m)
                            {
                                var remaining = doctorFee - amountPaidNow;
                                TempData["SuccessMessage"] = $"Appointment booked. Paid Rs.{amountPaidNow}. Remaining Rs.{remaining}.";
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

            // Repopulate doctors and locations for the view
            var doctors = await _appointmentService.SearchDoctorsAsync();
            ViewBag.Doctors = doctors;
            var locations = await _context.Locations.ToListAsync();
            ViewBag.Locations = locations;
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

            // Get all payments for this patient
            var allPayments = await _context.Payments
                .Where(p => p.PatientID == patient.PatientID)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Group by main booking reference (without _P suffix)
            var paymentGroups = allPayments
                .GroupBy(p => p.BookingRef.Contains("_P") ? p.BookingRef.Substring(0, p.BookingRef.IndexOf("_P")) : p.BookingRef)
                .ToList();

            var paymentsWithDoctorsCalculated = new List<dynamic>();

            foreach (var group in paymentGroups)
            {
                var mainBookingRef = group.Key;
                var paymentsInGroup = group.ToList();
                
                // Find the main payment (the one without _P suffix)
                var mainPayment = paymentsInGroup.FirstOrDefault(p => p.BookingRef == mainBookingRef);
                if (mainPayment == null) continue;

                // Calculate total paid for this booking
                var totalPaidForBooking = paymentsInGroup
                    .Where(p => p.PaymentStatus == "Completed")
                    .Sum(p => p.Amount);

                var remainingBalance = mainPayment.Amount - totalPaidForBooking;

                // Get doctor information
                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.PatientID == patient.PatientID && a.AppointmentDate.Date == mainPayment.PaymentDate.Date);

                var doctorName = appointment?.Doctor?.Name ?? "General Payment";

                // Create a summary object for display
                paymentsWithDoctorsCalculated.Add(new
                {
                    Payment = new
                    {
                        PaymentID = mainPayment.PaymentID,
                        BookingRef = mainPayment.BookingRef,
                        Amount = remainingBalance > 0 ? remainingBalance : mainPayment.Amount, // Show remaining balance or full amount
                        PaymentStatus = remainingBalance > 0 ? "Pending" : "Completed",
                        PaymentDate = mainPayment.PaymentDate,
                        TotalAmount = mainPayment.Amount, // Store original amount for reference
                        AmountPaid = totalPaidForBooking
                    },
                    DoctorName = doctorName,
                    AppointmentDate = appointment?.AppointmentDate
                });
            }

            ViewBag.PaymentsWithDoctors = paymentsWithDoctorsCalculated;

            var totalDue = paymentsWithDoctorsCalculated.Where(p => p.Payment.PaymentStatus == "Pending").Sum(p => (decimal)p.Payment.Amount);
            var totalPaid = paymentsWithDoctorsCalculated.Sum(p => (decimal)p.Payment.AmountPaid);

            ViewBag.TotalDue = totalDue;
            ViewBag.TotalPaid = totalPaid;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PayFees(PayFeesViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (!int.TryParse(userId, out int userIdInt))
                return RedirectToAction("Index");

            // Resolve PatientID from UserID
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userIdInt);
            if (patient == null)
                return RedirectToAction("Index");

            var mainPayment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentID == model.PaymentID && p.PatientID == patient.PatientID);
            if (mainPayment != null && mainPayment.PaymentStatus == "Pending")
            {
                // Get all payments for this booking to calculate total paid
                var bookingRef = mainPayment.BookingRef;
                var allPaymentsForBooking = await _context.Payments
                    .Where(p => p.PatientID == patient.PatientID && 
                               (p.BookingRef == bookingRef || p.BookingRef.StartsWith(bookingRef + "_P")))
                    .ToListAsync();

                // Calculate total already paid (excluding the main pending payment)
                var totalPaidSoFar = allPaymentsForBooking
                    .Where(p => p.PaymentStatus == "Completed")
                    .Sum(p => p.Amount);

                var totalFeeAmount = mainPayment.Amount; // This is the full doctor fee
                var remainingDue = totalFeeAmount - totalPaidSoFar;
                
                if (model.Amount >= remainingDue)
                {
                    // Full payment of remaining amount
                    // Create a payment record for this transaction
                    var partialCount = allPaymentsForBooking.Count(p => p.BookingRef.Contains("_P"));
                    var newPartialPayment = new Payment
                    {
                        BookingRef = bookingRef + "_P" + (partialCount + 1),
                        PatientID = patient.PatientID,
                        Amount = model.Amount,
                        PaymentStatus = "Completed",
                        PaymentDate = DateTime.Now,
                        BankName = model.BankName,
                        CardNumber = !string.IsNullOrEmpty(model.CardNumber) ? MaskCardNumber(model.CardNumber) : null,
                        ExpiryDate = model.ExpiryDate,
                        CVN = !string.IsNullOrEmpty(model.CVN) ? "***" : null
                    };
                    await _context.Payments.AddAsync(newPartialPayment);

                    // Update main payment status to completed
                    mainPayment.PaymentStatus = "Completed";
                    mainPayment.PaymentDate = DateTime.Now;
                    
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Payment completed successfully! Full amount paid.";
                }
                else if (model.Amount > 0)
                {
                    // Partial payment
                    var partialCount = allPaymentsForBooking.Count(p => p.BookingRef.Contains("_P"));
                    var newPartialPayment = new Payment
                    {
                        BookingRef = bookingRef + "_P" + (partialCount + 1),
                        PatientID = patient.PatientID,
                        Amount = model.Amount,
                        PaymentStatus = "Completed",
                        PaymentDate = DateTime.Now,
                        BankName = model.BankName,
                        CardNumber = !string.IsNullOrEmpty(model.CardNumber) ? MaskCardNumber(model.CardNumber) : null,
                        ExpiryDate = model.ExpiryDate,
                        CVN = !string.IsNullOrEmpty(model.CVN) ? "***" : null
                    };
                    await _context.Payments.AddAsync(newPartialPayment);
                    
                    var newRemainingDue = remainingDue - model.Amount;
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Partial payment of Rs.{model.Amount} completed. Remaining Rs.{newRemainingDue} is still due.";
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

        [HttpGet]
        public async Task<JsonResult> GetDoctorLocations(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Location)
                .FirstOrDefaultAsync(d => d.DoctorID == doctorId);
            
            var locations = new List<object>();
            if (doctor?.Location != null)
            {
                locations.Add(new { 
                    LocationID = doctor.Location.LocationID, 
                    LocationName = doctor.Location.LocationName 
                });
            }
            
            return Json(locations);
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

        public async Task<IActionResult> PaymentHistory()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (!int.TryParse(userId, out int userIdInt))
                return RedirectToAction("Login", "Home");

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userIdInt);
            if (patient == null) return RedirectToAction("Login", "Home");

            // Get all payments for the patient first
            var payments = await _context.Payments
                .Where(p => p.PatientID == patient.PatientID)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Get all appointments with doctors to avoid multiple database calls
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .ToListAsync();

            var paymentHistory = new List<dynamic>();

            foreach (var payment in payments)
            {
                string doctorName = "N/A";
                DateTime? appointmentDate = null;

                if (!string.IsNullOrEmpty(payment.BookingRef))
                {
                    // Extract appointment ID from booking reference
                    if (payment.BookingRef.StartsWith("BK") && payment.BookingRef.Length >= 5)
                    {
                        var idPart = payment.BookingRef.Substring(2);
                        // Handle partial payment suffixes like _P1, _P2
                        if (idPart.Contains("_"))
                        {
                            idPart = idPart.Split('_')[0];
                        }
                        
                        if (int.TryParse(idPart, out int appointmentId))
                        {
                            var appointment = appointments.FirstOrDefault(a => a.AppointmentID == appointmentId);
                            if (appointment != null && appointment.Doctor != null)
                            {
                                doctorName = appointment.Doctor.Name;
                                appointmentDate = appointment.AppointmentDate;
                            }
                        }
                    }
                }

                paymentHistory.Add(new
                {
                    Payment = payment,
                    DoctorName = doctorName,
                    AppointmentDate = appointmentDate
                });
            }

            ViewBag.PatientName = patient.Name;
            return View(paymentHistory);
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

        // Banking fields for online payment
        [Display(Name = "Bank Name")]
        [StringLength(100)]
        public string? BankName { get; set; }

        [Display(Name = "Card Number")]
        [StringLength(20)]
        public string? CardNumber { get; set; }

        [Display(Name = "Expiry Date")]
        [StringLength(7)]
        public string? ExpiryDate { get; set; }

        [Display(Name = "CVN")]
        [StringLength(4)]
        public string? CVN { get; set; }
    }

    // New ViewModel for PayFees with banking fields
    public class PayFeesViewModel
    {
        public int PaymentID { get; set; }
        public decimal Amount { get; set; }
        
        // Banking fields
        [Display(Name = "Bank Name")]
        [StringLength(100)]
        public string? BankName { get; set; }

        [Display(Name = "Card Number")]
        [StringLength(20)]
        public string? CardNumber { get; set; }

        [Display(Name = "Expiry Date")]
        [StringLength(7)]
        public string? ExpiryDate { get; set; }

        [Display(Name = "CVN")]
        [StringLength(4)]
        public string? CVN { get; set; }
    }
}