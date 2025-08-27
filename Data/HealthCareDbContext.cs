using Microsoft.EntityFrameworkCore;
using HealthCareApp.Models;

namespace HealthCareApp.Data
{
    public class HealthCareDbContext : DbContext
    {
        public HealthCareDbContext(DbContextOptions<HealthCareDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Password).HasMaxLength(255).IsRequired();
                entity.Property(e => e.UserType).HasMaxLength(20).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Configure Admin entity
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(e => e.AdminID);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Contact).HasMaxLength(20);
                entity.HasOne(e => e.User).WithOne(e => e.Admin).HasForeignKey<Admin>(e => e.UserID);
            });

            // Configure Doctor entity
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.DoctorID);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ContactNo).HasMaxLength(20);
                entity.Property(e => e.AvailableTime).HasMaxLength(100);
                entity.Property(e => e.Fees).HasColumnType("decimal(10,2)");
                entity.HasOne(e => e.User).WithOne(e => e.Doctor).HasForeignKey<Doctor>(e => e.UserID);
                entity.HasOne(e => e.Specialty).WithMany(e => e.Doctors).HasForeignKey(e => e.SpecialID);
                entity.HasOne(e => e.Location).WithMany(e => e.Doctors).HasForeignKey(e => e.LocationID);
            });

            // Configure Patient entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.PatientID);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.PhoneNo).HasMaxLength(20);
                entity.HasOne(e => e.User).WithOne(e => e.Patient).HasForeignKey<Patient>(e => e.UserID);
            });

            // Configure Specialty entity
            modelBuilder.Entity<Specialty>(entity =>
            {
                entity.HasKey(e => e.SpecialID);
                entity.Property(e => e.SpecialtyName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Configure Location entity
            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.LocationID);
                entity.Property(e => e.LocationName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            // Configure Appointment entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.AppointmentID);
                entity.Property(e => e.AppointmentDate).IsRequired();
                entity.Property(e => e.AppointmentTime).IsRequired();
                entity.HasOne(e => e.Patient).WithMany(e => e.Appointments).HasForeignKey(e => e.PatientID);
                entity.HasOne(e => e.Doctor).WithMany(e => e.Appointments).HasForeignKey(e => e.DoctorID);
                entity.HasOne(e => e.Location).WithMany(e => e.Appointments).HasForeignKey(e => e.LocationID);
            });

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentID);
                entity.Property(e => e.BookingRef).HasMaxLength(50);
                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.PaymentStatus).HasMaxLength(20);
                entity.HasOne(e => e.Patient).WithMany(e => e.Payments).HasForeignKey(e => e.PatientID);
            });

            // Configure MedicalHistory entity
            modelBuilder.Entity<MedicalHistory>(entity =>
            {
                entity.ToTable("MedicalHistory");
                entity.HasKey(e => e.HistoryID);
                entity.Property(e => e.RecordDate).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.Medicine).HasMaxLength(500);
                entity.HasOne(e => e.Patient).WithMany(e => e.MedicalHistories).HasForeignKey(e => e.PatientID);
                entity.HasOne(e => e.Doctor).WithMany(e => e.MedicalHistories).HasForeignKey(e => e.DoctorID);
            });

            // Configure Feedback entity
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("Feedback");
                entity.HasKey(e => e.FeedbackID);
                entity.Property(e => e.Comments).HasMaxLength(500);
                entity.HasOne(e => e.Patient).WithMany(e => e.Feedbacks).HasForeignKey(e => e.PatientID);
                entity.HasOne(e => e.Doctor).WithMany(e => e.Feedbacks).HasForeignKey(e => e.DoctorID);
                entity.HasOne(e => e.Appointment).WithMany(e => e.Feedbacks).HasForeignKey(e => e.AppointmentID);
            });
        }
    }
}
