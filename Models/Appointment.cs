using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models
{
    public class Appointment
    {
        public int AppointmentID { get; set; }
        
        public int PatientID { get; set; }
        
        public int? LocationID { get; set; }
        
        public int DoctorID { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }
        
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }
        
        public bool IsCancelled { get; set; } = false;
        
        // Navigation properties
        public Patient Patient { get; set; } = null!;
        public Location? Location { get; set; }
        public Doctor Doctor { get; set; } = null!;
        
        // Collections
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
