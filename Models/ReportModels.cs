using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models
{
    public class DateRangeReportRequest
    {
        [Required]
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; } = DateTime.Now.AddDays(-30);

        [Required]
        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; } = DateTime.Now;

        public string ReportType { get; set; } = "Appointments";
    }

    public class AppointmentReportData
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public List<AppointmentReportItem> AppointmentDetails { get; set; } = new();
        public List<SpecialtyAppointmentCount> AppointmentsBySpecialty { get; set; } = new();
    }

    public class PaymentReportData
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CompletedRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public int CompletedPayments { get; set; }
        public int PendingPayments { get; set; }
        public List<PaymentReportItem> PaymentDetails { get; set; } = new();
        public decimal AverageTransactionValue { get; set; }
    }

    public class PatientReportData
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalPatients { get; set; }
        public int NewPatients { get; set; }
        public int ActivePatients { get; set; }
        public int InactivePatients { get; set; }
        public List<PatientReportItem> PatientDetails { get; set; } = new();
    }

    public class AppointmentReportItem
    {
        public int AppointmentID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsCancelled { get; set; }
    }

    public class PaymentReportItem
    {
        public int PaymentID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string BookingRef { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    public class PatientReportItem
    {
        public int PatientID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalAppointments { get; set; }
    }

    public class SpecialtyAppointmentCount
    {
        public string SpecialtyName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ReportsIndexViewModel
    {
        public DateRangeReportRequest Request { get; set; } = new();
        public AppointmentReportData? AppointmentReport { get; set; }
        public PaymentReportData? PaymentReport { get; set; }
        public PatientReportData? PatientReport { get; set; }
    }
}