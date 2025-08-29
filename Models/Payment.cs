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
        
        // Banking fields for online payment
        [StringLength(100)]
        public string? BankName { get; set; }
        
        [StringLength(20)]
        public string? CardNumber { get; set; }
        
        [StringLength(7)]
        public string? ExpiryDate { get; set; }
        
        [StringLength(4)]
        public string? CVN { get; set; }
        
        // Navigation property
        public Patient Patient { get; set; } = null!;
    }
}
