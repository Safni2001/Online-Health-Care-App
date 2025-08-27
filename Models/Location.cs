using System.ComponentModel.DataAnnotations;

namespace HealthCareApp.Models
{
    public class Location
    {
        public int LocationID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string LocationName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        // Collections
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
