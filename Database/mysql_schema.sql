USE HealthCareManagementDb;

-- Create Users Table (Centralized user management)
CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
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

-- Create indexes for performance
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_UserType ON Users(UserType);
CREATE INDEX IX_Doctors_SpecialID ON Doctors(SpecialID);
CREATE INDEX IX_Appointments_PatientID ON Appointments(PatientID);
CREATE INDEX IX_Appointments_DoctorID ON Appointments(DoctorID);
CREATE INDEX IX_Appointments_Date ON Appointments(AppointmentDate);