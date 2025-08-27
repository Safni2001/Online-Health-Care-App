using HealthCareApp.Models;

namespace HealthCareApp.Services
{
    public interface IAppointmentService
    {
        // Appointment Management
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
        Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId);
        Task<List<Appointment>> GetAppointmentsByDateAsync(DateTime date);
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
        Task<bool> CreateAppointmentAsync(Appointment appointment);
        Task<bool> UpdateAppointmentAsync(Appointment appointment);
        Task<bool> CancelAppointmentAsync(int appointmentId);
        Task<bool> ConfirmAppointmentAsync(int appointmentId);
        
        // Doctor Search and Availability
        Task<List<Doctor>> SearchDoctorsAsync(string? specialty = null, string? location = null, string? searchTerm = null);
        Task<List<Doctor>> GetAvailableDoctorsAsync(DateTime date, TimeSpan time);
        Task<bool> IsDoctorAvailableAsync(int doctorId, DateTime date, TimeSpan time);
        
        // Patient Dashboard Data
        Task<int> GetUpcomingAppointmentsCountAsync(int patientId);
        Task<int> GetTotalAppointmentsCountAsync(int patientId);
        Task<List<Appointment>> GetUpcomingAppointmentsAsync(int patientId);
        
        // Doctor Dashboard Data
        Task<int> GetTodayAppointmentsCountAsync(int doctorId);
        Task<int> GetPendingAppointmentsCountAsync(int doctorId);
        Task<List<Appointment>> GetTodayAppointmentsAsync(int doctorId);
        Task<List<Appointment>> GetPendingAppointmentsAsync(int doctorId);
        
        // System Statistics
        Task<int> GetTotalAppointmentsCountAsync();
        Task<int> GetAppointmentsByStatusAsync(string status);
    }
}
