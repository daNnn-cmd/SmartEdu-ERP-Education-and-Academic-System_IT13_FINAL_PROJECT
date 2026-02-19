
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EMPLOYEE')
BEGIN
    CREATE TABLE EMPLOYEE (
        employee_id        INT IDENTITY(1,1) PRIMARY KEY,
        first_name         NVARCHAR(100)  NOT NULL,
        last_name          NVARCHAR(100)  NOT NULL,
        middle_name        NVARCHAR(100)  NULL,
        email              NVARCHAR(255)  NULL,
        phone_number       NVARCHAR(50)   NULL,
        department         NVARCHAR(100)  NULL,
        position           NVARCHAR(100)  NULL,
        hire_date          DATETIME2      NOT NULL DEFAULT (SYSUTCDATETIME()),
        date_of_birth      DATETIME2      NULL,
        address            NVARCHAR(255)  NULL,
        employment_status  NVARCHAR(50)   NOT NULL DEFAULT ('Active'),
        resume_path        NVARCHAR(255)  NULL,
        contract_path      NVARCHAR(255)  NULL,
        id_document_path   NVARCHAR(255)  NULL,
        IsDeleted          BIT            NOT NULL DEFAULT (0),
        DeletedAt          DATETIME2      NULL,
        CreatedAt          DATETIME2      NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt          DATETIME2      NOT NULL DEFAULT (SYSUTCDATETIME())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'JOB_POSTING')
BEGIN
    CREATE TABLE JOB_POSTING (
        job_posting_id INT IDENTITY(1,1) PRIMARY KEY,
        title          NVARCHAR(150) NOT NULL,
        department     NVARCHAR(100) NULL,
        employment_type NVARCHAR(50) NULL,
        description    NVARCHAR(MAX) NULL,
        posted_date    DATETIME2     NOT NULL DEFAULT (SYSUTCDATETIME()),
        closing_date   DATETIME2     NULL,
        is_active      BIT           NOT NULL DEFAULT (1),
        IsDeleted      BIT           NOT NULL DEFAULT (0),
        DeletedAt      DATETIME2     NULL,
        CreatedAt      DATETIME2     NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt      DATETIME2     NOT NULL DEFAULT (SYSUTCDATETIME())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'APPLICANT')
BEGIN
    CREATE TABLE APPLICANT (
        applicant_id   INT IDENTITY(1,1) PRIMARY KEY,
        job_posting_id INT           NOT NULL,
        full_name      NVARCHAR(200) NOT NULL,
        email          NVARCHAR(255) NULL,
        phone_number   NVARCHAR(50)  NULL,
        resume_path    NVARCHAR(255) NULL,
        status         NVARCHAR(50)  NOT NULL DEFAULT ('New'),
        applied_date   DATETIME2     NOT NULL DEFAULT (SYSUTCDATETIME()),
        notes          NVARCHAR(MAX) NULL,
        IsDeleted      BIT           NOT NULL DEFAULT (0),
        DeletedAt      DATETIME2     NULL,
        CreatedAt      DATETIME2     NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt      DATETIME2     NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_APPLICANT_JOB_POSTING FOREIGN KEY (job_posting_id)
            REFERENCES JOB_POSTING(job_posting_id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EMPLOYEE_ATTENDANCE')
BEGIN
    CREATE TABLE EMPLOYEE_ATTENDANCE (
        attendance_id  INT IDENTITY(1,1) PRIMARY KEY,
        employee_id    INT          NOT NULL,
        [date]         DATE         NOT NULL,
        time_in        TIME(0)      NULL,
        time_out       TIME(0)      NULL,
        status         NVARCHAR(20) NOT NULL DEFAULT ('Present'),
        minutes_late   INT          NOT NULL DEFAULT (0),
        is_absent      BIT          NOT NULL DEFAULT (0),
        remarks        NVARCHAR(MAX) NULL,
        IsDeleted      BIT          NOT NULL DEFAULT (0),
        DeletedAt      DATETIME2    NULL,
        CreatedAt      DATETIME2    NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt      DATETIME2    NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_EMPLOYEE_ATTENDANCE_EMPLOYEE FOREIGN KEY (employee_id)
            REFERENCES EMPLOYEE(employee_id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LEAVE_REQUEST')
BEGIN
    CREATE TABLE LEAVE_REQUEST (
        leave_request_id INT IDENTITY(1,1) PRIMARY KEY,
        employee_id      INT          NOT NULL,
        start_date       DATE         NOT NULL,
        end_date         DATE         NOT NULL,
        leave_type       NVARCHAR(50) NOT NULL,
        reason           NVARCHAR(MAX) NULL,
        status           NVARCHAR(20) NOT NULL DEFAULT ('Pending'),
        approver_name    NVARCHAR(200) NULL,
        response_notes   NVARCHAR(MAX) NULL,
        requested_at     DATETIME2    NOT NULL DEFAULT (SYSUTCDATETIME()),
        IsDeleted        BIT          NOT NULL DEFAULT (0),
        DeletedAt        DATETIME2    NULL,
        CreatedAt        DATETIME2    NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt        DATETIME2    NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_LEAVE_REQUEST_EMPLOYEE FOREIGN KEY (employee_id)
            REFERENCES EMPLOYEE(employee_id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PERFORMANCE_REVIEW')
BEGIN
    CREATE TABLE PERFORMANCE_REVIEW (
        performance_review_id INT IDENTITY(1,1) PRIMARY KEY,
        employee_id           INT          NOT NULL,
        review_date           DATETIME2    NOT NULL DEFAULT (SYSUTCDATETIME()),
        reviewer_name         NVARCHAR(200) NULL,
        score                 INT          NOT NULL,
        comments              NVARCHAR(MAX) NULL,
        period_start          DATE         NULL,
        period_end            DATE         NULL,
        IsDeleted             BIT          NOT NULL DEFAULT (0),
        DeletedAt             DATETIME2    NULL,
        CreatedAt             DATETIME2    NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt             DATETIME2    NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_PERFORMANCE_REVIEW_EMPLOYEE FOREIGN KEY (employee_id)
            REFERENCES EMPLOYEE(employee_id) ON DELETE CASCADE
    );
END;
