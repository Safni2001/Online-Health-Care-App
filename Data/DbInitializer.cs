using HealthCareApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCareApp.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(HealthCareDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Users.AnyAsync())
            {
                return; // Data already seeded
            }

            // Seed Specialties
            var specialties = new List<Specialty>
            {
                new Specialty { SpecialtyName = "General Medicine", Description = "General medical consultation" },
                new Specialty { SpecialtyName = "Cardiology", Description = "Heart specialists" },
                new Specialty { SpecialtyName = "Dermatology", Description = "Skin specialists" },
                new Specialty { SpecialtyName = "Pediatrics", Description = "Child care specialists" },
                new Specialty { SpecialtyName = "Orthopedics", Description = "Bone and joint specialists" }
            };
            context.Specialties.AddRange(specialties);
            await context.SaveChangesAsync();

            // Seed Locations
            var locations = new List<Location>
            {
                new Location { LocationName = "Main Clinic", Description = "Primary healthcare facility" },
                new Location { LocationName = "Branch Office", Description = "Secondary location" },
                new Location { LocationName = "Emergency Center", Description = "Emergency services" }
            };
            context.Locations.AddRange(locations);
            await context.SaveChangesAsync();

            // Seed Users and their profiles
            var users = new List<User>
            {
                // Admin Users
                new User { Username = "admin1", Password = "admin123", UserType = "Admin", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "admin2", Password = "admin456", UserType = "Admin", IsActive = true, CreatedDate = DateTime.Now },
                
                // Doctor Users
                new User { Username = "dr.smith", Password = "doc123", UserType = "Doctor", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "dr.johnson", Password = "doc456", UserType = "Doctor", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "dr.williams", Password = "doc789", UserType = "Doctor", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "dr.brown", Password = "doc101", UserType = "Doctor", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "dr.davis", Password = "doc202", UserType = "Doctor", IsActive = true, CreatedDate = DateTime.Now },
                
                // Patient Users
                new User { Username = "john.doe", Password = "pat123", UserType = "Patient", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "jane.smith", Password = "pat456", UserType = "Patient", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "mike.wilson", Password = "pat789", UserType = "Patient", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "sarah.johnson", Password = "pat101", UserType = "Patient", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "david.lee", Password = "pat202", UserType = "Patient", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "emma.taylor", Password = "pat303", UserType = "Patient", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "james.white", Password = "pat404", UserType = "Patient", IsActive = true, CreatedDate = DateTime.Now },
                new User { Username = "lisa.anderson", Password = "pat505", UserType = "Patient", IsActive = true, CreatedDate = DateTime.Now }
            };
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // Seed Admin profiles
            var admins = new List<Admin>
            {
                new Admin { UserID = users[0].UserID, Name = "John Administrator", Email = "admin1@healthclinic.com", Contact = "+1-555-0101" },
                new Admin { UserID = users[1].UserID, Name = "Sarah Manager", Email = "admin2@healthclinic.com", Contact = "+1-555-0102" }
            };
            context.Admins.AddRange(admins);

            // Seed Doctor profiles
            var doctors = new List<Doctor>
            {
                new Doctor { UserID = users[2].UserID, Name = "Dr. Michael Smith", LocationID = 1, ContactNo = "+1-555-1001", SpecialID = 1, AvailableTime = "9:00 AM - 5:00 PM", Fees = 150.00m },
                new Doctor { UserID = users[3].UserID, Name = "Dr. Emily Johnson", LocationID = 1, ContactNo = "+1-555-1002", SpecialID = 2, AvailableTime = "10:00 AM - 6:00 PM", Fees = 200.00m },
                new Doctor { UserID = users[4].UserID, Name = "Dr. Robert Williams", LocationID = 2, ContactNo = "+1-555-1003", SpecialID = 3, AvailableTime = "8:00 AM - 4:00 PM", Fees = 180.00m },
                new Doctor { UserID = users[5].UserID, Name = "Dr. Jennifer Brown", LocationID = 1, ContactNo = "+1-555-1004", SpecialID = 4, AvailableTime = "9:00 AM - 5:00 PM", Fees = 160.00m },
                new Doctor { UserID = users[6].UserID, Name = "Dr. Christopher Davis", LocationID = 3, ContactNo = "+1-555-1005", SpecialID = 5, AvailableTime = "11:00 AM - 7:00 PM", Fees = 220.00m }
            };
            context.Doctors.AddRange(doctors);

            // Seed Patient profiles
            var patients = new List<Patient>
            {
                new Patient { UserID = users[7].UserID, Name = "John Doe", Address = "123 Main Street, Anytown, ST 12345", PhoneNo = "+1-555-2001" },
                new Patient { UserID = users[8].UserID, Name = "Jane Smith", Address = "456 Oak Avenue, Somewhere, ST 12346", PhoneNo = "+1-555-2002" },
                new Patient { UserID = users[9].UserID, Name = "Mike Wilson", Address = "789 Pine Road, Elsewhere, ST 12347", PhoneNo = "+1-555-2003" },
                new Patient { UserID = users[10].UserID, Name = "Sarah Johnson", Address = "321 Elm Street, Nowhere, ST 12348", PhoneNo = "+1-555-2004" },
                new Patient { UserID = users[11].UserID, Name = "David Lee", Address = "654 Maple Lane, Anywhere, ST 12349", PhoneNo = "+1-555-2005" },
                new Patient { UserID = users[12].UserID, Name = "Emma Taylor", Address = "987 Cedar Court, Someplace, ST 12350", PhoneNo = "+1-555-2006" },
                new Patient { UserID = users[13].UserID, Name = "James White", Address = "147 Birch Boulevard, Anyplace, ST 12351", PhoneNo = "+1-555-2007" },
                new Patient { UserID = users[14].UserID, Name = "Lisa Anderson", Address = "258 Spruce Street, Everyplace, ST 12352", PhoneNo = "+1-555-2008" }
            };
            context.Patients.AddRange(patients);

            await context.SaveChangesAsync();
        }
    }
}
