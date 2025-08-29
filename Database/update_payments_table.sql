USE HealthCareManagementDb;
GO
ALTER TABLE Payments
    ADD 
        BankName NVARCHAR(100) NULL,           
        CardNumber NVARCHAR(20) NULL,         
        ExpiryDate NVARCHAR(7) NULL,        
        CVN NVARCHAR(4) NULL;       
GO