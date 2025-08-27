using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        
        public int PatientID { get; set; }
        
        public int? DoctorID { get; set; }
        
        public int? AppointmentID { get; set; }
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [StringLength(500)]
        public string? Comments { get; set; }
        
        public DateTime FeedbackDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public Patient Patient { get; set; } = null!;
        public Doctor? Doctor { get; set; }
        public Appointment? Appointment { get; set; }
    }
}
