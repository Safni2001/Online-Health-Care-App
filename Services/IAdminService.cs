using HealthCareApp.Models;

namespace HealthCareApp.Services
{
    public interface IAdminService
    {
        // Doctor Management
        Task<List<Doctor>> GetAllDoctorsAsync();
        Task<Doctor?> GetDoctorByIdAsync(int doctorId);
        Task<bool> AddDoctorAsync(Doctor doctor, string password);
        Task<bool> UpdateDoctorAsync(Doctor doctor);
        Task<bool> DeactivateDoctorAsync(int doctorId);
        Task<bool> ActivateDoctorAsync(int doctorId);
        Task<bool> DeleteDoctorAsync(int doctorId);
        
        // Patient Management
        Task<List<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int patientId);
        Task<bool> DeactivatePatientAsync(int patientId);
        Task<bool> ActivatePatientAsync(int patientId);
        
        // System Statistics
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetTotalDoctorsCountAsync();
        Task<int> GetTotalPatientsCountAsync();
        Task<int> GetTotalAppointmentsCountAsync();
        
        // Specialty and Location Management
        Task<List<Specialty>> GetAllSpecialtiesAsync();
        Task<List<Location>> GetAllLocationsAsync();
        
        // Date-wise Reports
        Task<AppointmentReportData> GetAppointmentReportAsync(DateTime fromDate, DateTime toDate);
        Task<PaymentReportData> GetPaymentReportAsync(DateTime fromDate, DateTime toDate);
        Task<PatientReportData> GetPatientReportAsync(DateTime fromDate, DateTime toDate);
    }
}
