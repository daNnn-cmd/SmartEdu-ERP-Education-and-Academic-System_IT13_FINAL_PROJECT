-- Drop HR Module Tables for SmartEduERP (in reverse order of dependencies)
-- Compatible with SQL Server / SSMS 21

-- First, check if tables exist before dropping
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PERFORMANCE_REVIEW')
BEGIN
    DROP TABLE PERFORMANCE_REVIEW;
    PRINT 'Table PERFORMANCE_REVIEW dropped successfully.';
END
ELSE
BEGIN
    PRINT 'Table PERFORMANCE_REVIEW does not exist.';
END;

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LEAVE_REQUEST')
BEGIN
    DROP TABLE LEAVE_REQUEST;
    PRINT 'Table LEAVE_REQUEST dropped successfully.';
END
ELSE
BEGIN
    PRINT 'Table LEAVE_REQUEST does not exist.';
END;

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EMPLOYEE_ATTENDANCE')
BEGIN
    DROP TABLE EMPLOYEE_ATTENDANCE;
    PRINT 'Table EMPLOYEE_ATTENDANCE dropped successfully.';
END
ELSE
BEGIN
    PRINT 'Table EMPLOYEE_ATTENDANCE does not exist.';
END;

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'APPLICANT')
BEGIN
    DROP TABLE APPLICANT;
    PRINT 'Table APPLICANT dropped successfully.';
END
ELSE
BEGIN
    PRINT 'Table APPLICANT does not exist.';
END;

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JOB_POSTING')
BEGIN
    DROP TABLE JOB_POSTING;
    PRINT 'Table JOB_POSTING dropped successfully.';
END
ELSE
BEGIN
    PRINT 'Table JOB_POSTING does not exist.';
END;

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EMPLOYEE')
BEGIN
    DROP TABLE EMPLOYEE;
    PRINT 'Table EMPLOYEE dropped successfully.';
END
ELSE
BEGIN
    PRINT 'Table EMPLOYEE does not exist.';
END;