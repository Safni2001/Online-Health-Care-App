USE HealthCareManagementDb;

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