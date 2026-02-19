USE db33505;
GO

-- Disable foreign key constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Delete all data and reset identity columns
EXEC sp_MSforeachtable 'DELETE FROM ?';
EXEC sp_MSforeachtable 'DBCC CHECKIDENT(''?'', RESEED, 0)';
GO

-- Enable foreign key constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
GO