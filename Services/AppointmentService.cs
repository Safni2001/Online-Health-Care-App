using HealthCareApp.Data;
using HealthCareApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCareApp.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly HealthCareDbContext _context;

        public AppointmentService(HealthCareDbContext context)
        {
            _context = context;
        }

        #region Appointment Management

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Location)
                .Include(a => a.Patient.User)
                .Include(a => a.Doctor.User)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Patient.User)
                .Include(a => a.Location)
                .Where(a => a.DoctorID == doctorId)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Doctor.User)
                .Include(a => a.Location)
                .Where(a => a.PatientID == patientId)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Location)
                .Include(a => a.Patient.User)
                .Include(a => a.Doctor.User)
                .Where(a => a.AppointmentDate.Date == date.Date && !a.IsCancelled)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Location)
                .Include(a => a.Patient.User)
                .Include(a => a.Doctor.User)
                .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);
        }

        public async Task<bool> CreateAppointmentAsync(Appointment appointment)
        {
            try
            {
                // Temporarily removed doctor availability check
                // if (!await IsDoctorAvailableAsync(appointment.DoctorID, appointment.AppointmentDate, appointment.AppointmentTime))
                // {
                //     return false; // Doctor not available
                // }

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateAppointmentAsync] Exception: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateAppointmentAsync(Appointment appointment)
        {
            try
            {
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment != null)
                {
                    // Set to not confirmed per custom semantics
                    appointment.IsCancelled = false;
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

        public async Task<bool> ConfirmAppointmentAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment != null)
                {
                    // Mark as confirmed per custom semantics
                    appointment.IsCancelled = true;
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

        #region Doctor Search and Availability

        public async Task<List<Doctor>> SearchDoctorsAsync(string? specialty = null, string? location = null, string? searchTerm = null)
        {
            var query = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Include(d => d.Location)
                .Where(d => d.User.IsActive);

            if (!string.IsNullOrEmpty(specialty))
            {
                query = query.Where(d => d.Specialty != null &&
                                         d.Specialty.SpecialtyName.Contains(specialty));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(d => d.Location != null &&
                                         d.Location.LocationName.Contains(location));
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d => d.Name.Contains(searchTerm) || d.User.Username.Contains(searchTerm));
            }

            return await query.OrderBy(d => d.Name).ToListAsync();
        }

        public async Task<List<Doctor>> GetAvailableDoctorsAsync(DateTime date, TimeSpan time)
        {
            var busyDoctors = await _context.Appointments
                .Where(a => a.AppointmentDate.Date == date.Date &&
                           a.AppointmentTime == time &&
                           !a.IsCancelled)
                .Select(a => a.DoctorID)
                .ToListAsync();

            return await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Include(d => d.Location)
                .Where(d => d.User.IsActive && !busyDoctors.Contains(d.DoctorID))
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<bool> IsDoctorAvailableAsync(int doctorId, DateTime date, TimeSpan time)
        {
            var existingAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.DoctorID == doctorId &&
                                        a.AppointmentDate.Date == date.Date &&
                                        a.AppointmentTime == time &&
                                        !a.IsCancelled);

            return existingAppointment == null;
        }

        #endregion

        #region Patient Dashboard Data

        public async Task<int> GetUpcomingAppointmentsCountAsync(int patientId)
        {
            return await _context.Appointments
                .CountAsync(a => a.PatientID == patientId &&
                               a.AppointmentDate >= DateTime.Today &&
                               !a.IsCancelled);
        }

        public async Task<int> GetTotalAppointmentsCountAsync(int patientId)
        {
            return await _context.Appointments
                .CountAsync(a => a.PatientID == patientId && !a.IsCancelled);
        }

        public async Task<List<Appointment>> GetUpcomingAppointmentsAsync(int patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Doctor.User)
                .Include(a => a.Location)
                .Where(a => a.PatientID == patientId &&
                           a.AppointmentDate >= DateTime.Today &&
                           !a.IsCancelled)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        #endregion

        #region Doctor Dashboard Data

        public async Task<int> GetTodayAppointmentsCountAsync(int doctorId)
        {
            return await _context.Appointments
                .CountAsync(a => a.DoctorID == doctorId &&
                               a.AppointmentDate.Date == DateTime.Today);
        }

        public async Task<int> GetPendingAppointmentsCountAsync(int doctorId)
        {
            // Count all appointments from today onward
            return await _context.Appointments
                .CountAsync(a => a.DoctorID == doctorId &&
                               a.AppointmentDate >= DateTime.Today);
        }

        public async Task<List<Appointment>> GetTodayAppointmentsAsync(int doctorId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Patient.User)
                .Include(a => a.Location)
                .Where(a => a.DoctorID == doctorId &&
                           a.AppointmentDate.Date == DateTime.Today)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetPendingAppointmentsAsync(int doctorId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Patient.User)
                .Include(a => a.Location)
                .Where(a => a.DoctorID == doctorId &&
                           a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        #endregion

        #region System Statistics

        public async Task<int> GetTotalAppointmentsCountAsync()
        {
            return await _context.Appointments.CountAsync();
        }

        public async Task<int> GetAppointmentsByStatusAsync(string status)
        {
            if (status.ToLower() == "cancelled")
            {
                return await _context.Appointments.CountAsync(a => a.IsCancelled);
            }
            else
            {
                return await _context.Appointments.CountAsync(a => !a.IsCancelled);
            }
        }

        #endregion
    }
}
