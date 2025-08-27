using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models
{
    public class Doctor
    {
        public int DoctorID { get; set; }
        
        public int UserID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public int? LocationID { get; set; }
        
        [StringLength(20)]
        public string? ContactNo { get; set; }
        
        public int? SpecialID { get; set; }
        
        [StringLength(100)]
        public string? AvailableTime { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal Fees { get; set; } = 0.00m;
        
        // Navigation properties
        public User User { get; set; } = null!;
        public Location? Location { get; set; }
        public Specialty? Specialty { get; set; }
        
        // Collections
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
