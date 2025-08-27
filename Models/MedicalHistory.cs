using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models
{
    public class MedicalHistory
    {
        public int HistoryID { get; set; }
        
        public int PatientID { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime RecordDate { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        [StringLength(500)]
        public string? Medicine { get; set; }
        
        public int? DoctorID { get; set; }
        
        // Navigation properties
        public Patient Patient { get; set; } = null!;
        public Doctor? Doctor { get; set; }
    }
}
