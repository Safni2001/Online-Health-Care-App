USE HealthCareManagementDb;

-- Create Users Table (Centralized user management)
CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    -- Should be hashed
    UserType NVARCHAR(20) NOT NULL CHECK (UserType IN ('Admin', 'Doctor', 'Patient')),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- Create Specialties Table
CREATE TABLE Specialties
(
    SpecialID INT IDENTITY(1,1) PRIMARY KEY,
    SpecialtyName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500)
);

-- Create Location Table
CREATE TABLE Locations
(
    LocationID INT IDENTITY(1,1) PRIMARY KEY,
    LocationName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(200)
);

-- Create Admin Table
CREATE TABLE Admins
(
    AdminID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Contact NVARCHAR(20),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- Create Doctors Table
CREATE TABLE Doctors
(
    DoctorID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    LocationID INT,
    ContactNo NVARCHAR(20),
    SpecialID INT,
    AvailableTime NVARCHAR(100),
    -- Simple time format like "9 AM - 5 PM"
    Fees DECIMAL(10,2) DEFAULT 0.00,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (LocationID) REFERENCES Locations(LocationID),
    FOREIGN KEY (SpecialID) REFERENCES Specialties(SpecialID)
);

-- Create Patients Table
CREATE TABLE Patients
(
    PatientID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(300),
    PhoneNo NVARCHAR(20),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- Create Appointments Table
CREATE TABLE Appointments
(
    AppointmentID INT IDENTITY(1,1) PRIMARY KEY,
    PatientID INT NOT NULL,
    LocationID INT,
    DoctorID INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    AppointmentTime TIME NOT NULL,
    IsCancelled BIT DEFAULT 0,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (LocationID) REFERENCES Locations(LocationID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID)
);

-- Create Payment Table
CREATE TABLE Payments
(
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,
    BookingRef NVARCHAR(50),
    -- Booking reference number
    PatientID INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentStatus NVARCHAR(20) DEFAULT 'Pending' CHECK (PaymentStatus IN ('Pending', 'Completed', 'Failed')),
    PaymentDate DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
);

-- Create Medical History Table
CREATE TABLE MedicalHistory
(
    HistoryID INT IDENTITY(1,1) PRIMARY KEY,
    PatientID INT NOT NULL,
    RecordDate DATE NOT NULL,
    Notes NVARCHAR(1000),
    Medicine NVARCHAR(500),
    DoctorID INT,
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID)
);

-- Create Feedback Table
CREATE TABLE Feedback
(
    FeedbackID INT IDENTITY(1,1) PRIMARY KEY,
    PatientID INT NOT NULL,
    DoctorID INT,
    AppointmentID INT,
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    Comments NVARCHAR(500),
    FeedbackDate DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID)
);

-- Insert sample data
INSERT INTO Specialties
    (SpecialtyName, Description)
VALUES
    ('General Medicine', 'General medical consultation'),
    ('Cardiology', 'Heart specialists'),
    ('Dermatology', 'Skin specialists'),
    ('Pediatrics', 'Child care specialists'),
    ('Orthopedics', 'Bone and joint specialists');

INSERT INTO Locations
    (LocationName, Description)
VALUES
    ('Main Clinic', 'Primary healthcare facility'),
    ('Branch Office', 'Secondary location'),
    ('Emergency Center', 'Emergency services');

-- Create simple views for easy data retrieval
GO
CREATE VIEW vw_DoctorList
AS
    SELECT
        d.DoctorID,
        d.Name AS DoctorName,
        s.SpecialtyName,
        l.LocationName,
        d.ContactNo,
        d.AvailableTime,
        d.Fees
    FROM Doctors d
        LEFT JOIN Specialties s ON d.SpecialID = s.SpecialID
        LEFT JOIN Locations l ON d.LocationID = l.LocationID
        INNER JOIN Users u ON d.UserID = u.UserID
    WHERE u.IsActive = 1;
GO

CREATE VIEW vw_AppointmentList
AS
    SELECT
        a.AppointmentID,
        p.Name AS PatientName,
        d.Name AS DoctorName,
        s.SpecialtyName,
        l.LocationName,
        a.AppointmentDate,
        a.AppointmentTime,
        a.IsCancelled
    FROM Appointments a
        INNER JOIN Patients p ON a.PatientID = p.PatientID
        INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
        LEFT JOIN Specialties s ON d.SpecialID = s.SpecialID
        LEFT JOIN Locations l ON a.LocationID = l.LocationID;
GO

-- Create simple stored procedures
CREATE PROCEDURE sp_UserLogin
    @Username NVARCHAR(50),
    @Password NVARCHAR(255)
AS
BEGIN
    SELECT UserID, Username, UserType, IsActive
    FROM Users
    WHERE Username = @Username
        AND Password = @Password
        AND IsActive = 1;
END;
GO

CREATE PROCEDURE sp_GetDoctorsBySpecialty
    @SpecialtyName NVARCHAR(100) = NULL
AS
BEGIN
    SELECT *
    FROM vw_DoctorList
    WHERE (@SpecialtyName IS NULL OR SpecialtyName = @SpecialtyName)
    ORDER BY DoctorName;
END;
GO

CREATE PROCEDURE sp_BookAppointment
    @PatientID INT,
    @DoctorID INT,
    @LocationID INT,
    @AppointmentDate DATE,
    @AppointmentTime TIME
AS
BEGIN
    INSERT INTO Appointments
        (PatientID, DoctorID, LocationID, AppointmentDate, AppointmentTime)
    VALUES
        (@PatientID, @DoctorID, @LocationID, @AppointmentDate, @AppointmentTime);

    SELECT SCOPE_IDENTITY() AS AppointmentID;
END;
GO

-- Create indexes for performance
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_UserType ON Users(UserType);
CREATE INDEX IX_Doctors_SpecialID ON Doctors(SpecialID);
CREATE INDEX IX_Appointments_PatientID ON Appointments(PatientID);
CREATE INDEX IX_Appointments_DoctorID ON Appointments(DoctorID);
CREATE INDEX IX_Appointments_Date ON Appointments(AppointmentDate);

-- =====================================================
-- DATA SEEDER - Sample Users and Data
-- =====================================================

PRINT 'Starting data seeding...';

-- Insert sample Users (Admin, Doctor, Patient)
INSERT INTO Users
    (Username, Password, UserType)
VALUES
    -- Admin Users
    ('admin1', 'admin123', 'Admin'),
    ('admin2', 'admin456', 'Admin'),

    -- Doctor Users  
    ('dr.smith', 'doc123', 'Doctor'),
    ('dr.johnson', 'doc456', 'Doctor'),
    ('dr.williams', 'doc789', 'Doctor'),
    ('dr.brown', 'doc101', 'Doctor'),
    ('dr.davis', 'doc202', 'Doctor'),

    -- Patient Users
    ('john.doe', 'pat123', 'Patient'),
    ('jane.smith', 'pat456', 'Patient'),
    ('mike.wilson', 'pat789', 'Patient'),
    ('sarah.johnson', 'pat101', 'Patient'),
    ('david.lee', 'pat202', 'Patient'),
    ('emma.taylor', 'pat303', 'Patient'),
    ('james.white', 'pat404', 'Patient'),
    ('lisa.anderson', 'pat505', 'Patient');

-- Insert Admin details
INSERT INTO Admins
    (UserID, Name, Email, Contact)
VALUES
    ((SELECT UserID
        FROM Users
        WHERE Username = 'admin1'), 'John Administrator', 'admin1@healthclinic.com', '+1-555-0101'),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'admin2'), 'Sarah Manager', 'admin2@healthclinic.com', '+1-555-0102');

-- Insert Doctor details
INSERT INTO Doctors
    (UserID, Name, LocationID, ContactNo, SpecialID, AvailableTime, Fees)
VALUES
    ((SELECT UserID
        FROM Users
        WHERE Username = 'dr.smith'), 'Dr. Michael Smith', 1, '+1-555-1001', 1, '9:00 AM - 5:00 PM', 150.00),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'dr.johnson'), 'Dr. Emily Johnson', 1, '+1-555-1002', 2, '10:00 AM - 6:00 PM', 200.00),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'dr.williams'), 'Dr. Robert Williams', 2, '+1-555-1003', 3, '8:00 AM - 4:00 PM', 180.00),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'dr.brown'), 'Dr. Jennifer Brown', 1, '+1-555-1004', 4, '9:00 AM - 5:00 PM', 160.00),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'dr.davis'), 'Dr. Christopher Davis', 3, '+1-555-1005', 5, '11:00 AM - 7:00 PM', 220.00);

-- Insert Patient details
INSERT INTO Patients
    (UserID, Name, Address, PhoneNo)
VALUES
    ((SELECT UserID
        FROM Users
        WHERE Username = 'john.doe'), 'John Doe', '123 Main Street, Anytown, ST 12345', '+1-555-2001'),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'jane.smith'), 'Jane Smith', '456 Oak Avenue, Somewhere, ST 12346', '+1-555-2002'),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'mike.wilson'), 'Mike Wilson', '789 Pine Road, Elsewhere, ST 12347', '+1-555-2003'),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'sarah.johnson'), 'Sarah Johnson', '321 Elm Street, Nowhere, ST 12348', '+1-555-2004'),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'david.lee'), 'David Lee', '654 Maple Lane, Anywhere, ST 12349', '+1-555-2005'),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'emma.taylor'), 'Emma Taylor', '987 Cedar Court, Someplace, ST 12350', '+1-555-2006'),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'james.white'), 'James White', '147 Birch Boulevard, Anyplace, ST 12351', '+1-555-2007'),
    ((SELECT UserID
        FROM Users
        WHERE Username = 'lisa.anderson'), 'Lisa Anderson', '258 Spruce Street, Everyplace, ST 12352', '+1-555-2008');

-- Insert sample Appointments
INSERT INTO Appointments
    (PatientID, LocationID, DoctorID, AppointmentDate, AppointmentTime, IsCancelled)
VALUES
    -- John Doe's appointments
    (1, 1, 1, '2025-09-01', '10:00:00', 0),
    (1, 1, 2, '2025-09-15', '11:00:00', 0),

    -- Jane Smith's appointments
    (2, 1, 1, '2025-09-02', '09:30:00', 0),
    (2, 2, 3, '2025-09-10', '02:00:00', 0),

    -- Mike Wilson's appointments
    (3, 1, 4, '2025-09-03', '10:30:00', 0),
    (3, 3, 5, '2025-09-20', '03:00:00', 0),

    -- Sarah Johnson's appointments
    (4, 1, 2, '2025-09-05', '11:30:00', 0),

    -- David Lee's appointments
    (5, 2, 3, '2025-09-08', '09:00:00', 0),
    (5, 1, 1, '2025-09-25', '04:00:00', 1),
    -- Cancelled appointment

    -- Emma Taylor's appointments
    (6, 1, 4, '2025-09-12', '02:30:00', 0),

    -- James White's appointments
    (7, 3, 5, '2025-09-18', '01:00:00', 0),

    -- Lisa Anderson's appointments
    (8, 1, 1, '2025-09-22', '10:00:00', 0);

-- Insert sample Payments
INSERT INTO Payments
    (BookingRef, PatientID, Amount, PaymentStatus, PaymentDate)
VALUES
    ('BK001', 1, 150.00, 'Completed', '2025-08-25 09:30:00'),
    ('BK002', 1, 200.00, 'Pending', '2025-08-26 10:15:00'),
    ('BK003', 2, 150.00, 'Completed', '2025-08-27 11:20:00'),
    ('BK004', 2, 180.00, 'Completed', '2025-08-28 02:45:00'),
    ('BK005', 3, 160.00, 'Completed', '2025-08-29 03:10:00'),
    ('BK006', 3, 220.00, 'Pending', '2025-08-30 04:30:00'),
    ('BK007', 4, 200.00, 'Completed', '2025-09-01 12:00:00'),
    ('BK008', 5, 180.00, 'Failed', '2025-09-02 09:15:00'),
    ('BK009', 6, 160.00, 'Completed', '2025-09-05 01:45:00'),
    ('BK010', 7, 220.00, 'Pending', '2025-09-08 02:20:00');

-- Insert sample Medical History
INSERT INTO MedicalHistory
    (PatientID, RecordDate, Notes, Medicine, DoctorID)
VALUES
    (1, '2025-08-15', 'General checkup - all vitals normal', 'Vitamin D supplement', 1),
    (1, '2025-07-20', 'Mild hypertension detected', 'Lisinopril 10mg daily', 2),
    (2, '2025-08-10', 'Annual physical exam', 'No medication required', 1),
    (2, '2025-06-15', 'Skin rash treatment', 'Hydrocortisone cream', 3),
    (3, '2025-08-05', 'Sports injury consultation', 'Ibuprofen 400mg as needed', 5),
    (4, '2025-07-25', 'Cardiac screening', 'Continue current heart medication', 2),
    (5, '2025-08-01', 'Dermatology consultation for acne', 'Tretinoin cream 0.05%', 3),
    (6, '2025-07-30', 'Pediatric wellness check', 'Age-appropriate vitamins', 4),
    (7, '2025-08-12', 'Orthopedic consultation for back pain', 'Physical therapy recommended', 5),
    (8, '2025-08-18', 'Routine checkup', 'Blood pressure monitoring', 1);

-- Insert sample Feedback
INSERT INTO Feedback
    (PatientID, DoctorID, AppointmentID, Rating, Comments, FeedbackDate)
VALUES
    (1, 1, 1, 5, 'Excellent doctor, very thorough and caring!', '2025-08-16 10:30:00'),
    (2, 1, 3, 4, 'Good consultation, but had to wait a bit longer', '2025-08-11 11:00:00'),
    (1, 2, 2, 5, 'Dr. Johnson was very professional and explained everything clearly', '2025-07-21 12:15:00'),
    (2, 3, 4, 3, 'Treatment was effective but clinic was crowded', '2025-06-16 03:30:00'),
    (3, 4, 5, 5, 'Great with children, highly recommend!', '2025-08-06 11:45:00'),
    (4, 2, 7, 4, 'Knowledgeable cardiologist, felt confident in care', '2025-07-26 02:20:00'),
    (5, 3, 8, 4, 'Skin condition improved significantly after treatment', '2025-08-02 10:15:00'),
    (6, 4, 10, 5, 'Wonderful pediatrician, made my child feel comfortable', '2025-07-31 09:30:00'),
    (7, 5, 11, 3, 'Helpful advice for back pain, could use better scheduling', '2025-08-13 04:45:00'),
    (8, 1, 12, 5, 'Always reliable and trustworthy, been seeing for years', '2025-08-19 11:20:00');

-- Display summary of seeded data
PRINT '=== DATA SEEDING COMPLETED ===';
PRINT '';

-- Count and display seeded records
DECLARE @AdminCount INT, @DoctorCount INT, @PatientCount INT;
DECLARE @AppointmentCount INT, @PaymentCount INT, @HistoryCount INT, @FeedbackCount INT;

SELECT @AdminCount = COUNT(*)
FROM Admins;
SELECT @DoctorCount = COUNT(*)
FROM Doctors;
SELECT @PatientCount = COUNT(*)
FROM Patients;
SELECT @AppointmentCount = COUNT(*)
FROM Appointments;
SELECT @PaymentCount = COUNT(*)
FROM Payments;
SELECT @HistoryCount = COUNT(*)
FROM MedicalHistory;
SELECT @FeedbackCount = COUNT(*)
FROM Feedback;

PRINT 'SEEDED DATA SUMMARY:';
PRINT '-------------------';
PRINT 'Admins: ' + CAST(@AdminCount AS VARCHAR(10));
PRINT 'Doctors: ' + CAST(@DoctorCount AS VARCHAR(10));
PRINT 'Patients: ' + CAST(@PatientCount AS VARCHAR(10));
PRINT 'Appointments: ' + CAST(@AppointmentCount AS VARCHAR(10));
PRINT 'Payments: ' + CAST(@PaymentCount AS VARCHAR(10));
PRINT 'Medical History Records: ' + CAST(@HistoryCount AS VARCHAR(10));
PRINT 'Feedback Records: ' + CAST(@FeedbackCount AS VARCHAR(10));
PRINT '';

-- Display sample login credentials
PRINT 'SAMPLE LOGIN CREDENTIALS:';
PRINT '------------------------';
PRINT 'ADMIN USERS:';
PRINT 'Username: admin1, Password: admin123';
PRINT 'Username: admin2, Password: admin456';
PRINT '';
PRINT 'DOCTOR USERS:';
PRINT 'Username: dr.smith, Password: doc123 (General Medicine)';
PRINT 'Username: dr.johnson, Password: doc456 (Cardiology)';
PRINT 'Username: dr.williams, Password: doc789 (Dermatology)';
PRINT 'Username: dr.brown, Password: doc101 (Pediatrics)';
PRINT 'Username: dr.davis, Password: doc202 (Orthopedics)';
PRINT '';
PRINT 'PATIENT USERS:';
PRINT 'Username: john.doe, Password: pat123';
PRINT 'Username: jane.smith, Password: pat456';
PRINT 'Username: mike.wilson, Password: pat789';
PRINT 'Username: sarah.johnson, Password: pat101';
PRINT 'Username: david.lee, Password: pat202';
PRINT 'Username: emma.taylor, Password: pat303';
PRINT 'Username: james.white, Password: pat404';
PRINT 'Username: lisa.anderson, Password: pat505';
PRINT '';

PRINT 'Simple HealthCare Management System database created and seeded successfully!';
PRINT 'Database is ready for testing your ASP.NET application!';