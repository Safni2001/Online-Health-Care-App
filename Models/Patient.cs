using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models
{
    public class Patient
    {
        public int PatientID { get; set; }
        
        public int UserID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(300)]
        public string? Address { get; set; }
        
        [StringLength(20)]
        public string? PhoneNo { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
        
        // Collections
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
