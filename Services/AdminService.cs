using HealthCareApp.Data;
using HealthCareApp.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace HealthCareApp.Services
{
    public class AdminService : IAdminService
    {
        private readonly HealthCareDbContext _context;
        private readonly IUserService _userService;

        public AdminService(HealthCareDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        #region Doctor Management

        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            return await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Include(d => d.Location)
                // .Where(d => d.User.IsActive) // REMOVE THIS FILTER
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int doctorId)
        {
            return await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Include(d => d.Location)
                .FirstOrDefaultAsync(d => d.DoctorID == doctorId);
        }

        public async Task<bool> AddDoctorAsync(Doctor doctor, string password)
        {
            try
            {
                // Create user account first
                var user = new User
                {
                    Username = doctor.User.Username,
                    Password = password,
                    UserType = "Doctor",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                var userCreated = await _userService.CreateUserAsync(user, "Doctor");
                if (!userCreated) return false;

                // Update doctor with the created user ID
                doctor.UserID = user.UserID;
                doctor.User = user;

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateDoctorAsync(Doctor doctor)
        {
            try
            {
                _context.Doctors.Update(doctor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeactivateDoctorAsync(int doctorId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorID == doctorId);
                
                if (doctor != null)
                {
                    doctor.User.IsActive = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActivateDoctorAsync(int doctorId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorID == doctorId);
                
                if (doctor != null)
                {
                    doctor.User.IsActive = true;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteDoctorAsync(int doctorId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorID == doctorId);

                if (doctor == null)
                {
                    return false;
                }

                // Delete related records first to avoid foreign key constraint violations
                
                // Delete feedback records
                var feedbackRecords = await _context.Feedbacks
                    .Where(f => f.DoctorID == doctorId)
                    .ToListAsync();
                _context.Feedbacks.RemoveRange(feedbackRecords);

                // Delete medical history records
                var medicalHistoryRecords = await _context.MedicalHistories
                    .Where(mh => mh.DoctorID == doctorId)
                    .ToListAsync();
                _context.MedicalHistories.RemoveRange(medicalHistoryRecords);

                // Delete appointments
                var appointments = await _context.Appointments
                    .Where(a => a.DoctorID == doctorId)
                    .ToListAsync();
                _context.Appointments.RemoveRange(appointments);

                // Finally, delete the doctor and user
                _context.Doctors.Remove(doctor);
                if (doctor.User != null)
                {
                    _context.Users.Remove(doctor.User);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        #endregion

        #region Patient Management

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _context.Patients
                .Include(p => p.User)
                .Where(p => p.User.IsActive)
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int patientId)
        {
            return await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientID == patientId);
        }

        public async Task<bool> DeactivatePatientAsync(int patientId)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PatientID == patientId);
                
                if (patient != null)
                {
                    patient.User.IsActive = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActivatePatientAsync(int patientId)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PatientID == patientId);
                
                if (patient != null)
                {
                    patient.User.IsActive = true;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region System Statistics

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.IsActive);
        }

        public async Task<int> GetTotalDoctorsCountAsync()
        {
            return await _context.Doctors.CountAsync(d => d.User.IsActive);
        }

        public async Task<int> GetTotalPatientsCountAsync()
        {
            return await _context.Patients.CountAsync(p => p.User.IsActive);
        }

        public async Task<int> GetTotalAppointmentsCountAsync()
        {
            return await _context.Appointments.CountAsync();
        }

        #endregion

        #region Specialty and Location Management

        public async Task<List<Specialty>> GetAllSpecialtiesAsync()
        {
            return await _context.Specialties.ToListAsync();
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return await _context.Locations.ToListAsync();
        }

        #endregion
    }
}
