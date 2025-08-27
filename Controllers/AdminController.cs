using Microsoft.AspNetCore.Mvc;
using HealthCareApp.Attributes;
using HealthCareApp.Services;
using HealthCareApp.Models;
using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Controllers
{
    [AuthorizeUserType("Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminService _adminService;

        public AdminController(ILogger<AdminController> logger, IAdminService adminService)
        {
            _logger = logger;
            _adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Admin Dashboard";
            
            // Get system statistics
            ViewBag.TotalUsers = await _adminService.GetTotalUsersCountAsync();
            ViewBag.TotalDoctors = await _adminService.GetTotalDoctorsCountAsync();
            ViewBag.TotalPatients = await _adminService.GetTotalPatientsCountAsync();
            ViewBag.TotalAppointments = await _adminService.GetTotalAppointmentsCountAsync();
            
            return View();
        }

        public async Task<IActionResult> ManageDoctors()
        {
            ViewData["Title"] = "Manage Doctors";
            
            var doctors = await _adminService.GetAllDoctorsAsync();
            return View(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> AddDoctor()
        {
            ViewData["Title"] = "Add New Doctor";
            
            ViewBag.Specialties = await _adminService.GetAllSpecialtiesAsync();
            ViewBag.Locations = await _adminService.GetAllLocationsAsync();
            
            return View(new AddDoctorViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddDoctor(AddDoctorViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var doctor = new Doctor
                    {
                        Name = model.Name,
                        LocationID = model.LocationID,
                        ContactNo = model.ContactNo,
                        SpecialID = model.SpecialID,
                        AvailableTime = model.AvailableTime,
                        Fees = model.Fees,
                        User = new User { Username = model.Username }
                    };

                    var result = await _adminService.AddDoctorAsync(doctor, model.Password);
                    
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Doctor added successfully!";
                        return RedirectToAction("ManageDoctors");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to add doctor. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding doctor");
                    ModelState.AddModelError("", "An error occurred while adding the doctor.");
                }
            }

            ViewBag.Specialties = await _adminService.GetAllSpecialtiesAsync();
            ViewBag.Locations = await _adminService.GetAllLocationsAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateDoctor(int doctorId)
        {
            try
            {
                var result = await _adminService.DeactivateDoctorAsync(doctorId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Doctor deactivated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to deactivate doctor.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating doctor");
                TempData["ErrorMessage"] = "An error occurred while deactivating the doctor.";
            }
            
            return RedirectToAction("ManageDoctors");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateDoctor(int doctorId)
        {
            try
            {
                var result = await _adminService.ActivateDoctorAsync(doctorId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Doctor activated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to activate doctor.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating doctor");
                TempData["ErrorMessage"] = "An error occurred while activating the doctor.";
            }
            
            return RedirectToAction("ManageDoctors");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDoctor(int doctorId)
        {
            try
            {
                var doctor = await _adminService.GetDoctorByIdAsync(doctorId);
                if (doctor != null)
                {
                    // Remove the doctor and their user account
                    var context = HttpContext.RequestServices.GetService(typeof(HealthCareApp.Data.HealthCareDbContext)) as HealthCareApp.Data.HealthCareDbContext;
                    if (context != null)
                    {
                        var user = doctor.User;
                        context.Doctors.Remove(doctor);
                        if (user != null)
                        {
                            context.Users.Remove(user);
                        }
                        await context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Doctor deleted successfully!";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Doctor not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting doctor");
                TempData["ErrorMessage"] = "An error occurred while deleting the doctor.";
            }
            return RedirectToAction("ManageDoctors");
        }

        [HttpGet]
        public async Task<IActionResult> EditDoctor(int doctorId)
        {
            var doctor = await _adminService.GetDoctorByIdAsync(doctorId);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("ManageDoctors");
            }
            ViewBag.Specialties = await _adminService.GetAllSpecialtiesAsync();
            ViewBag.Locations = await _adminService.GetAllLocationsAsync();
            var model = new EditDoctorViewModel
            {
                DoctorID = doctor.DoctorID,
                Name = doctor.Name,
                Username = doctor.User?.Username ?? string.Empty,
                SpecialID = doctor.SpecialID ?? 0,
                LocationID = doctor.LocationID,
                ContactNo = doctor.ContactNo,
                AvailableTime = doctor.AvailableTime,
                Fees = doctor.Fees
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditDoctor(EditDoctorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doctor = await _adminService.GetDoctorByIdAsync(model.DoctorID);
                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Doctor not found.";
                    return RedirectToAction("ManageDoctors");
                }
                doctor.Name = model.Name;
                doctor.SpecialID = model.SpecialID;
                doctor.LocationID = model.LocationID;
                doctor.ContactNo = model.ContactNo;
                doctor.AvailableTime = model.AvailableTime;
                doctor.Fees = model.Fees;
                if (doctor.User != null)
                {
                    doctor.User.Username = model.Username;
                }
                await _adminService.UpdateDoctorAsync(doctor);
                TempData["SuccessMessage"] = "Doctor updated successfully!";
                return RedirectToAction("ManageDoctors");
            }
            ViewBag.Specialties = await _adminService.GetAllSpecialtiesAsync();
            ViewBag.Locations = await _adminService.GetAllLocationsAsync();
            return View(model);
        }

        public class EditDoctorViewModel
        {
            public int DoctorID { get; set; }
            [Required]
            [StringLength(100)]
            [Display(Name = "Doctor Name")]
            public string Name { get; set; } = string.Empty;
            [Required]
            [StringLength(50)]
            [Display(Name = "Username")]
            public string Username { get; set; } = string.Empty;
            [Required]
            [Display(Name = "Specialty")]
            public int SpecialID { get; set; }
            [Display(Name = "Location")]
            public int? LocationID { get; set; }
            [StringLength(20)]
            [Display(Name = "Contact Number")]
            public string? ContactNo { get; set; }
            [StringLength(100)]
            [Display(Name = "Available Time")]
            public string? AvailableTime { get; set; }
            [Range(0, double.MaxValue)]
            [Display(Name = "Consultation Fees")]
            public decimal Fees { get; set; } = 0.00m;
        }

        public async Task<IActionResult> ManagePatients()
        {
            ViewData["Title"] = "Manage Patients";
            
            var patients = await _adminService.GetAllPatientsAsync();
            return View(patients);
        }

        [HttpPost]
        public async Task<IActionResult> DeactivatePatient(int patientId)
        {
            try
            {
                var result = await _adminService.DeactivatePatientAsync(patientId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Patient deactivated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to deactivate patient.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating patient");
                TempData["ErrorMessage"] = "An error occurred while deactivating the patient.";
            }
            
            return RedirectToAction("ManagePatients");
        }

        [HttpPost]
        public async Task<IActionResult> ActivatePatient(int patientId)
        {
            try
            {
                var result = await _adminService.ActivatePatientAsync(patientId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Patient activated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to activate patient.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating patient");
                TempData["ErrorMessage"] = "An error occurred while activating the patient.";
            }
            
            return RedirectToAction("ManagePatients");
        }

        public async Task<IActionResult> ManageSpecialties()
        {
            var specialties = await _adminService.GetAllSpecialtiesAsync();
            return View(specialties);
        }

        [HttpGet]
        public IActionResult AddSpecialty()
        {
            return View(new SpecialtyViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddSpecialty(SpecialtyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var context = HttpContext.RequestServices.GetService(typeof(HealthCareApp.Data.HealthCareDbContext)) as HealthCareApp.Data.HealthCareDbContext;
                if (context != null)
                {
                    var specialty = new Specialty { SpecialtyName = model.SpecialtyName, Description = model.Description };
                    context.Specialties.Add(specialty);
                    await context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Specialty added successfully!";
                    return RedirectToAction("ManageSpecialties");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditSpecialty(int id)
        {
            var context = HttpContext.RequestServices.GetService(typeof(HealthCareApp.Data.HealthCareDbContext)) as HealthCareApp.Data.HealthCareDbContext;
            if (context != null)
            {
                var specialty = await context.Specialties.FindAsync(id);
                if (specialty == null)
                {
                    TempData["ErrorMessage"] = "Specialty not found.";
                    return RedirectToAction("ManageSpecialties");
                }
                return View(new SpecialtyViewModel { SpecialID = specialty.SpecialID, SpecialtyName = specialty.SpecialtyName, Description = specialty.Description });
            }
            TempData["ErrorMessage"] = "Specialty not found.";
            return RedirectToAction("ManageSpecialties");
        }

        [HttpPost]
        public async Task<IActionResult> EditSpecialty(SpecialtyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var context = HttpContext.RequestServices.GetService(typeof(HealthCareApp.Data.HealthCareDbContext)) as HealthCareApp.Data.HealthCareDbContext;
                if (context != null)
                {
                    var specialty = await context.Specialties.FindAsync(model.SpecialID);
                    if (specialty != null)
                    {
                        specialty.SpecialtyName = model.SpecialtyName;
                        specialty.Description = model.Description;
                        await context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Specialty updated successfully!";
                        return RedirectToAction("ManageSpecialties");
                    }
                }
            }
            TempData["ErrorMessage"] = "Specialty not found or update failed.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSpecialty(int id)
        {
            var context = HttpContext.RequestServices.GetService(typeof(HealthCareApp.Data.HealthCareDbContext)) as HealthCareApp.Data.HealthCareDbContext;
            if (context != null)
            {
                var specialty = await context.Specialties.FindAsync(id);
                if (specialty != null)
                {
                    context.Specialties.Remove(specialty);
                    await context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Specialty deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Specialty not found.";
                }
            }
            return RedirectToAction("ManageSpecialties");
        }

        public class SpecialtyViewModel
        {
            public int SpecialID { get; set; }
            [Required]
            [StringLength(100)]
            [Display(Name = "Specialty Name")]
            public string SpecialtyName { get; set; } = string.Empty;
            [StringLength(500)]
            [Display(Name = "Description")]
            public string? Description { get; set; }
        }

        public IActionResult Reports()
        {
            ViewData["Title"] = "Reports & Analytics";
            return View();
        }
    }

    // View Models
    public class AddDoctorViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Doctor Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Specialty")]
        public int SpecialID { get; set; }

        [Display(Name = "Location")]
        public int? LocationID { get; set; }

        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string? ContactNo { get; set; }

        [StringLength(100)]
        [Display(Name = "Available Time")]
        public string? AvailableTime { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Consultation Fees")]
        public decimal Fees { get; set; } = 0.00m;
    }
}
