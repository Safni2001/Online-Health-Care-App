using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models
{
    public class Specialty
    {
        public int SpecialID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SpecialtyName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // Collections
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
