
-- TeacherIncome: per-teacher earnings by period
CREATE TABLE TeacherIncome (
    TeacherIncomeId     INT IDENTITY(1,1) PRIMARY KEY,
    TeacherId           INT NOT NULL,
    TeacherName         NVARCHAR(200) NOT NULL,
    PeriodStartDate     DATE NOT NULL,
    PeriodEndDate       DATE NOT NULL,
    BasicSalary         DECIMAL(18,2) NOT NULL,
    OvertimePay         DECIMAL(18,2) NOT NULL DEFAULT 0,
    OtherIncome         DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2 NULL,
    IsDeleted           BIT NOT NULL DEFAULT 0,
    DeletedAt           DATETIME2 NULL
);
GO

-- Tax: simple tax rules (percentage or fixed)
CREATE TABLE Tax (
    TaxId               INT IDENTITY(1,1) PRIMARY KEY,
    Name                NVARCHAR(100) NOT NULL,
    Description         NVARCHAR(255) NULL,
    Rate                DECIMAL(18,4) NOT NULL, -- percent or fixed amount
    IsPercentage        BIT NOT NULL DEFAULT 1, -- 1: percentage, 0: fixed amount
    IsActive            BIT NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2 NULL,
    IsDeleted           BIT NOT NULL DEFAULT 0,
    DeletedAt           DATETIME2 NULL
);
GO

-- Allowance: global allowance definitions
CREATE TABLE Allowance (
    AllowanceId         INT IDENTITY(1,1) PRIMARY KEY,
    Name                NVARCHAR(100) NOT NULL,
    Description         NVARCHAR(255) NULL,
    Amount              DECIMAL(18,2) NOT NULL, -- percent or fixed
    IsPercentage        BIT NOT NULL DEFAULT 0, -- 1: percentage of income, 0: fixed amount
    IsTaxable           BIT NOT NULL DEFAULT 1, -- 1: included in taxable base
    IsActive            BIT NOT NULL DEFAULT 1,
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2 NULL,
    IsDeleted           BIT NOT NULL DEFAULT 0,
    DeletedAt           DATETIME2 NULL
);
GO

-- AccountingEntry: basic double-entry lines
CREATE TABLE AccountingEntry (
    AccountingEntryId   INT IDENTITY(1,1) PRIMARY KEY,
    TeacherIncomeId     INT NULL,
    EntryDate           DATE NOT NULL,
    Description         NVARCHAR(255) NOT NULL,
    DebitAccount        NVARCHAR(100) NOT NULL,
    CreditAccount       NVARCHAR(100) NOT NULL,
    Amount              DECIMAL(18,2) NOT NULL,
    EntryType           NVARCHAR(50) NULL, -- e.g., Salary, Tax, Allowance
    CreatedAt           DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2 NULL,
    IsDeleted           BIT NOT NULL DEFAULT 0,
    DeletedAt           DATETIME2 NULL,
    CONSTRAINT FK_AccountingEntry_TeacherIncome
        FOREIGN KEY (TeacherIncomeId) REFERENCES TeacherIncome(TeacherIncomeId)
);
GO
