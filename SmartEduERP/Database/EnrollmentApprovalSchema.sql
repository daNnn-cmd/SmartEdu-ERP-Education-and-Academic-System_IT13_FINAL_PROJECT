/*
Enrollment Approval Schema Update

Run this in SSMS against your LOCAL SmartEduERP database.
This script is idempotent and uses GO batch separators.
*/

-- 1) Add columns if missing
IF COL_LENGTH('dbo.ENROLLMENT', 'enrollment_status') IS NULL
    ALTER TABLE dbo.ENROLLMENT ADD enrollment_status NVARCHAR(20) NULL;
GO

IF COL_LENGTH('dbo.ENROLLMENT', 'approved_by_user_id') IS NULL
    ALTER TABLE dbo.ENROLLMENT ADD approved_by_user_id INT NULL;
GO

IF COL_LENGTH('dbo.ENROLLMENT', 'approved_by') IS NULL
    ALTER TABLE dbo.ENROLLMENT ADD approved_by NVARCHAR(150) NULL;
GO

IF COL_LENGTH('dbo.ENROLLMENT', 'approved_at') IS NULL
    ALTER TABLE dbo.ENROLLMENT ADD approved_at DATETIME2 NULL;
GO

IF COL_LENGTH('dbo.ENROLLMENT', 'rejected_by_user_id') IS NULL
    ALTER TABLE dbo.ENROLLMENT ADD rejected_by_user_id INT NULL;
GO

IF COL_LENGTH('dbo.ENROLLMENT', 'rejected_by') IS NULL
    ALTER TABLE dbo.ENROLLMENT ADD rejected_by NVARCHAR(150) NULL;
GO

IF COL_LENGTH('dbo.ENROLLMENT', 'rejected_at') IS NULL
    ALTER TABLE dbo.ENROLLMENT ADD rejected_at DATETIME2 NULL;
GO

IF COL_LENGTH('dbo.ENROLLMENT', 'rejection_reason') IS NULL
    ALTER TABLE dbo.ENROLLMENT ADD rejection_reason NVARCHAR(500) NULL;
GO

-- 2) Backfill defaults
UPDATE dbo.ENROLLMENT
SET enrollment_status = 'Pending'
WHERE enrollment_status IS NULL OR LTRIM(RTRIM(enrollment_status)) = '';
GO

-- 3) Add foreign keys (only if not already present)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ENROLLMENT_ApprovedByUser')
BEGIN
    ALTER TABLE dbo.ENROLLMENT WITH NOCHECK
    ADD CONSTRAINT FK_ENROLLMENT_ApprovedByUser
    FOREIGN KEY (approved_by_user_id) REFERENCES dbo.USER_ACCOUNT(user_id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ENROLLMENT_RejectedByUser')
BEGIN
    ALTER TABLE dbo.ENROLLMENT WITH NOCHECK
    ADD CONSTRAINT FK_ENROLLMENT_RejectedByUser
    FOREIGN KEY (rejected_by_user_id) REFERENCES dbo.USER_ACCOUNT(user_id);
END
GO
