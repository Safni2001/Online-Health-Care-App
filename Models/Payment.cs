using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        
        [StringLength(50)]
        public string? BookingRef { get; set; }
        
        public int PatientID { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending";
        
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        
        // Navigation property
        public Patient Patient { get; set; } = null!;
    }
}
