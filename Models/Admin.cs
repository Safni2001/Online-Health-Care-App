using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models
{
    public class Admin
    {
        public int AdminID { get; set; }
        
        public int UserID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
        
        [StringLength(20)]
        public string? Contact { get; set; }
        
        // Navigation property
        public User User { get; set; } = null!;
    }
}
