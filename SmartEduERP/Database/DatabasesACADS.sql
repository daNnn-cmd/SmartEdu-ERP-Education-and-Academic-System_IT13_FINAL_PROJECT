
USE master;
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'SmartEduERP')
BEGIN
    ALTER DATABASE SmartEduERP SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SmartEduERP;
END
GO

-- Create the database
CREATE DATABASE SmartEduERP;
GO

USE SmartEduERP;
GO

-- Create USER_ACCOUNT table with soft delete
CREATE TABLE [USER_ACCOUNT] (
    [user_id] INT IDENTITY(1,1) PRIMARY KEY,
    [first_name] NVARCHAR(100) NOT NULL,
    [last_name] NVARCHAR(100) NOT NULL,
    [username] NVARCHAR(50) UNIQUE NOT NULL,
    [password] NVARCHAR(255) NOT NULL,
    [role] NVARCHAR(50) NOT NULL,
    [reference_id] INT NULL,
    [IsDeleted] BIT DEFAULT 0 NOT NULL,
    [DeletedAt] DATETIME NULL,
    [is_email_confirmed] BIT DEFAULT 0 NOT NULL,
    [email_confirmation_token] NVARCHAR(255) NULL,
    [email] NVARCHAR(255) NOT NULL,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE()
);
GO

-- Create STUDENT table with soft delete
CREATE TABLE [STUDENT] (
    [student_id] INT IDENTITY(1,1) PRIMARY KEY,
    [first_name] NVARCHAR(100) NOT NULL,
    [last_name] NVARCHAR(100) NOT NULL,
    [email] NVARCHAR(255) NOT NULL,
    [contact_number] NVARCHAR(20) NULL,
    [address] NVARCHAR(500) NULL,
    [date_of_birth] DATE NULL,
    [enrollment_status] NVARCHAR(50) DEFAULT 'Active',
    [IsDeleted] BIT DEFAULT 0 NOT NULL,
    [DeletedAt] DATETIME NULL,
    [middle_name] NVARCHAR(100) NULL,
    [suffix] NVARCHAR(10) NULL,
    [gender] NVARCHAR(10) NULL,
    [form137_file_path] NVARCHAR(500) NULL,
    [psa_birth_cert_file_path] NVARCHAR(500) NULL,
    [good_moral_cert_file_path] NVARCHAR(500) NULL,
    [registration_date] DATETIME DEFAULT GETDATE(),
    [is_email_confirmed] BIT DEFAULT 0 NOT NULL,
    [email_confirmation_token] NVARCHAR(255) NULL,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE()
);
GO

-- Create TEACHER table with soft delete and registration date
CREATE TABLE [TEACHER] (
    [teacher_id] INT IDENTITY(1,1) PRIMARY KEY,
    [first_name] NVARCHAR(100) NOT NULL,
    [last_name] NVARCHAR(100) NOT NULL,
    [email] NVARCHAR(255) NOT NULL,
    [department] NVARCHAR(100) NOT NULL,
    [position] NVARCHAR(100) NULL,
    [registration_date] DATETIME DEFAULT GETDATE(), -- Added registration date
    [IsDeleted] BIT DEFAULT 0 NOT NULL,
    [DeletedAt] DATETIME NULL,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE()
);
GO

-- Create SUBJECT table with soft delete - removed units, added grade_level and section
CREATE TABLE [SUBJECT] (
    [subject_id] INT IDENTITY(1,1) PRIMARY KEY,
    [teacher_id] INT NULL,
    [subject_code] NVARCHAR(50) UNIQUE NOT NULL,
    [subject_name] NVARCHAR(100) NOT NULL,
    [grade_level] NVARCHAR(50) NOT NULL, -- Replaced units with grade_level
    [section] NVARCHAR(50) NOT NULL, -- Added section
    [academic_year] NVARCHAR(20) NOT NULL DEFAULT '2024-2025',
    -- Proposal / approval workflow
    [subject_status] NVARCHAR(20) NOT NULL DEFAULT 'Proposed',
    [proposed_by_user_id] INT NULL,
    [proposed_at] DATETIME NULL,
    [noted_by_user_id] INT NULL,
    [noted_at] DATETIME NULL,
    [approved_by_user_id] INT NULL,
    [approved_at] DATETIME NULL,
    [IsDeleted] BIT DEFAULT 0 NOT NULL,
    [DeletedAt] DATETIME NULL,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    FOREIGN KEY ([teacher_id]) REFERENCES [TEACHER]([teacher_id])
);
GO

-- Create ENROLLMENT table with soft delete
CREATE TABLE [ENROLLMENT] (
    [enrollment_id] INT IDENTITY(1,1) PRIMARY KEY,
    [student_id] INT NOT NULL,
    [subject_id] INT NOT NULL,
    [academic_year] NVARCHAR(20) NOT NULL,
    [semester] NVARCHAR(20) NOT NULL,
    [enrollment_date] DATETIME DEFAULT GETDATE(),
    [IsDeleted] BIT DEFAULT 0 NOT NULL,
    [DeletedAt] DATETIME NULL,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    FOREIGN KEY ([student_id]) REFERENCES [STUDENT]([student_id]),
    FOREIGN KEY ([subject_id]) REFERENCES [SUBJECT]([subject_id])
);
GO

-- Create GRADES table with soft delete
CREATE TABLE [GRADES] (
    [grade_id] INT IDENTITY(1,1) PRIMARY KEY,
    [enrollment_id] INT NOT NULL,
    [student_id] INT NOT NULL,
    [teacher_id] INT NOT NULL,
    [subject_id] INT NOT NULL,
    [grade_value] DECIMAL(5,2) NULL,
    [remarks] NVARCHAR(100) NULL,
    [IsDeleted] BIT DEFAULT 0 NOT NULL,
    [DeletedAt] DATETIME NULL,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    FOREIGN KEY ([enrollment_id]) REFERENCES [ENROLLMENT]([enrollment_id]),
    FOREIGN KEY ([student_id]) REFERENCES [STUDENT]([student_id]),
    FOREIGN KEY ([teacher_id]) REFERENCES [TEACHER]([teacher_id]),
    FOREIGN KEY ([subject_id]) REFERENCES [SUBJECT]([subject_id])
);
GO

-- Create PAYMENT table with soft delete
CREATE TABLE [PAYMENT] (
    [payment_id] INT IDENTITY(1,1) PRIMARY KEY,
    [student_id] INT NOT NULL,
    [amount] DECIMAL(10,2) NOT NULL,
    [payment_date] DATETIME DEFAULT GETDATE(),
    [payment_method] NVARCHAR(50) NOT NULL,
    [payment_status] NVARCHAR(50) DEFAULT 'Paid',
    [IsDeleted] BIT DEFAULT 0 NOT NULL,
    [DeletedAt] DATETIME NULL,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    FOREIGN KEY ([student_id]) REFERENCES [STUDENT]([student_id])
);
GO

-- Create AUDIT_LOGS table (no soft delete needed for audit logs)
CREATE TABLE [AUDIT_LOGS] (
    [audit_id] INT IDENTITY(1,1) PRIMARY KEY,
    [user_id] INT NULL,
    [action] NVARCHAR(50) NOT NULL,
    [table_name] NVARCHAR(100) NOT NULL,
    [record_id] INT NOT NULL,
    [old_values] NVARCHAR(MAX) NULL,
    [new_values] NVARCHAR(MAX) NULL,
    [created_at] DATETIME DEFAULT GETDATE(),
    FOREIGN KEY ([user_id]) REFERENCES [USER_ACCOUNT]([user_id])
);
GO

-- Insert sample data into USER_ACCOUNT with 12-character passwords
INSERT INTO [USER_ACCOUNT] ([first_name], [last_name], [username], [password], [role], [reference_id], [email])
VALUES 
('Daniel Kent', 'Pelpinosas', 'dandykent', '123456789123', 'Admin', NULL, 'danielpelpinosas5@gmail.com');
GO