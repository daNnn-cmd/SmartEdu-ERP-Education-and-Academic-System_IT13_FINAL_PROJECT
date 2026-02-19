-- Delete existing data and reset identity for HR tables
DELETE FROM PERFORMANCE_REVIEW;
DBCC CHECKIDENT ('PERFORMANCE_REVIEW', RESEED, 0);

DELETE FROM LEAVE_REQUEST;
DBCC CHECKIDENT ('LEAVE_REQUEST', RESEED, 0);

DELETE FROM EMPLOYEE_ATTENDANCE;
DBCC CHECKIDENT ('EMPLOYEE_ATTENDANCE', RESEED, 0);

DELETE FROM APPLICANT;
DBCC CHECKIDENT ('APPLICANT', RESEED, 0);

DELETE FROM EMPLOYEE;
DBCC CHECKIDENT ('EMPLOYEE', RESEED, 0);

DELETE FROM JOB_POSTING;
DBCC CHECKIDENT ('JOB_POSTING', RESEED, 0);

-- Insert 70 records into JOB_POSTING table
INSERT INTO JOB_POSTING (title, department, employment_type, description, posted_date, closing_date) VALUES
('Software Developer', 'IT', 'Full-Time', 'Develop and maintain software applications', '2024-01-15', '2024-02-15'),
('HR Manager', 'Human Resources', 'Full-Time', 'Manage HR operations and employee relations', '2024-01-20', '2024-02-20'),
('Marketing Specialist', 'Marketing', 'Contract', 'Create marketing campaigns and content', '2024-01-25', '2024-02-25'),
('Data Analyst', 'IT', 'Full-Time', 'Analyze business data and generate insights', '2024-02-01', '2024-03-01'),
('Administrative Assistant', 'Administration', 'Part-Time', 'Provide administrative support', '2024-02-05', '2024-03-05'),
('Senior Developer', 'IT', 'Full-Time', 'Lead development teams and projects', '2024-02-10', '2024-03-10'),
('Sales Executive', 'Sales', 'Full-Time', 'Generate sales and build client relationships', '2024-02-15', '2024-03-15'),
('Accountant', 'Finance', 'Full-Time', 'Manage financial records and reports', '2024-02-20', '2024-03-20'),
('Customer Support', 'Customer Service', 'Full-Time', 'Assist customers with inquiries', '2024-02-25', '2024-03-25'),
('Project Manager', 'IT', 'Full-Time', 'Manage IT projects and teams', '2024-03-01', '2024-03-31'),
('Network Engineer', 'IT', 'Full-Time', 'Design and maintain network infrastructure', '2024-03-05', '2024-04-05'),
('UI/UX Designer', 'Design', 'Full-Time', 'Create user interfaces and experiences', '2024-03-10', '2024-04-10'),
('DevOps Engineer', 'IT', 'Full-Time', 'Manage deployment and operations', '2024-03-15', '2024-04-15'),
('Quality Assurance', 'IT', 'Full-Time', 'Test software for quality assurance', '2024-03-20', '2024-04-20'),
('Business Analyst', 'Business', 'Full-Time', 'Analyze business processes and requirements', '2024-03-25', '2024-04-25'),
('Legal Counsel', 'Legal', 'Full-Time', 'Provide legal advice and support', '2024-04-01', '2024-04-30'),
('Recruitment Specialist', 'HR', 'Full-Time', 'Recruit and screen candidates', '2024-04-05', '2024-05-05'),
('Training Coordinator', 'HR', 'Full-Time', 'Coordinate employee training programs', '2024-04-10', '2024-05-10'),
('Payroll Specialist', 'Finance', 'Full-Time', 'Process payroll and benefits', '2024-04-15', '2024-05-15'),
('Procurement Officer', 'Operations', 'Full-Time', 'Manage purchasing and vendor relations', '2024-04-20', '2024-05-20'),
('Logistics Manager', 'Operations', 'Full-Time', 'Oversee logistics and distribution', '2024-04-25', '2024-05-25'),
('Warehouse Supervisor', 'Operations', 'Full-Time', 'Supervise warehouse operations', '2024-05-01', '2024-05-31'),
('Maintenance Technician', 'Facilities', 'Full-Time', 'Maintain equipment and facilities', '2024-05-05', '2024-06-05'),
('Security Officer', 'Security', 'Full-Time', 'Ensure safety and security', '2024-05-10', '2024-06-10'),
('Cleaning Staff', 'Facilities', 'Part-Time', 'Clean and maintain facilities', '2024-05-15', '2024-06-15'),
('Executive Assistant', 'Administration', 'Full-Time', 'Support executives with administrative tasks', '2024-05-20', '2024-06-20'),
('Content Writer', 'Marketing', 'Full-Time', 'Create written content for marketing', '2024-05-25', '2024-06-25'),
('Social Media Manager', 'Marketing', 'Full-Time', 'Manage social media accounts and campaigns', '2024-06-01', '2024-06-30'),
('SEO Specialist', 'Marketing', 'Full-Time', 'Optimize website for search engines', '2024-06-05', '2024-07-05'),
('Graphic Designer', 'Design', 'Full-Time', 'Create visual designs and graphics', '2024-06-10', '2024-07-10'),
('Video Editor', 'Marketing', 'Contract', 'Edit and produce video content', '2024-06-15', '2024-07-15'),
('Photographer', 'Marketing', 'Part-Time', 'Take professional photographs', '2024-06-20', '2024-07-20'),
('Event Coordinator', 'Marketing', 'Full-Time', 'Plan and coordinate events', '2024-06-25', '2024-07-25'),
('Public Relations Officer', 'Marketing', 'Full-Time', 'Manage public relations and communications', '2024-07-01', '2024-07-31'),
('Market Researcher', 'Marketing', 'Full-Time', 'Conduct market research and analysis', '2024-07-05', '2024-08-05'),
('Product Manager', 'Product', 'Full-Time', 'Manage product development and strategy', '2024-07-10', '2024-08-10'),
('Technical Writer', 'Documentation', 'Full-Time', 'Create technical documentation', '2024-07-15', '2024-08-15'),
('Database Administrator', 'IT', 'Full-Time', 'Manage and maintain databases', '2024-07-20', '2024-08-20'),
('Systems Analyst', 'IT', 'Full-Time', 'Analyze and design IT systems', '2024-07-25', '2024-08-25'),
('IT Support Specialist', 'IT', 'Full-Time', 'Provide IT support to employees', '2024-08-01', '2024-08-31'),
('Cybersecurity Analyst', 'IT', 'Full-Time', 'Protect systems from cyber threats', '2024-08-05', '2024-09-05'),
('Cloud Engineer', 'IT', 'Full-Time', 'Design and manage cloud infrastructure', '2024-08-10', '2024-09-10'),
('Mobile Developer', 'IT', 'Full-Time', 'Develop mobile applications', '2024-08-15', '2024-09-15'),
('Frontend Developer', 'IT', 'Full-Time', 'Develop user-facing features', '2024-08-20', '2024-09-20'),
('Backend Developer', 'IT', 'Full-Time', 'Develop server-side logic', '2024-08-25', '2024-09-25'),
('Full Stack Developer', 'IT', 'Full-Time', 'Develop both frontend and backend', '2024-09-01', '2024-09-30'),
('Machine Learning Engineer', 'IT', 'Full-Time', 'Develop machine learning models', '2024-09-05', '2024-10-05'),
('Data Scientist', 'IT', 'Full-Time', 'Analyze complex data sets', '2024-09-10', '2024-10-10'),
('AI Specialist', 'IT', 'Full-Time', 'Develop artificial intelligence solutions', '2024-09-15', '2024-10-15'),
('Blockchain Developer', 'IT', 'Full-Time', 'Develop blockchain applications', '2024-09-20', '2024-10-20'),
('Game Developer', 'IT', 'Full-Time', 'Develop video games', '2024-09-25', '2024-10-25'),
('Embedded Systems Engineer', 'IT', 'Full-Time', 'Develop embedded software', '2024-10-01', '2024-10-31'),
('Robotics Engineer', 'Engineering', 'Full-Time', 'Design and build robots', '2024-10-05', '2024-11-05'),
('Electrical Engineer', 'Engineering', 'Full-Time', 'Design electrical systems', '2024-10-10', '2024-11-10'),
('Mechanical Engineer', 'Engineering', 'Full-Time', 'Design mechanical systems', '2024-10-15', '2024-11-15'),
('Civil Engineer', 'Engineering', 'Full-Time', 'Design infrastructure projects', '2024-10-20', '2024-11-20'),
('Architect', 'Design', 'Full-Time', 'Design buildings and structures', '2024-10-25', '2024-11-25'),
('Interior Designer', 'Design', 'Full-Time', 'Design interior spaces', '2024-11-01', '2024-11-30'),
('Landscape Architect', 'Design', 'Full-Time', 'Design outdoor spaces', '2024-11-05', '2024-12-05'),
('Urban Planner', 'Planning', 'Full-Time', 'Plan urban development', '2024-11-10', '2024-12-10'),
('Environmental Specialist', 'Environmental', 'Full-Time', 'Address environmental issues', '2024-11-15', '2024-12-15'),
('Health and Safety Officer', 'Safety', 'Full-Time', 'Ensure workplace safety', '2024-11-20', '2024-12-20'),
('Nutritionist', 'Health', 'Full-Time', 'Provide nutritional advice', '2024-11-25', '2024-12-25'),
('Fitness Trainer', 'Health', 'Part-Time', 'Provide fitness training', '2024-12-01', '2024-12-31'),
('Counselor', 'Health', 'Full-Time', 'Provide counseling services', '2024-12-05', '2025-01-05'),
('Nurse', 'Health', 'Full-Time', 'Provide medical care', '2024-12-10', '2025-01-10'),
('Doctor', 'Health', 'Full-Time', 'Provide medical diagnosis and treatment', '2024-12-15', '2025-01-15'),
('Pharmacist', 'Health', 'Full-Time', 'Dispense medications', '2024-12-20', '2025-01-20'),
('Lab Technician', 'Health', 'Full-Time', 'Conduct laboratory tests', '2024-12-25', '2025-01-25'),
('Research Scientist', 'Research', 'Full-Time', 'Conduct scientific research', '2024-12-30', '2025-01-30');

-- Insert 70 records into EMPLOYEE table
INSERT INTO EMPLOYEE (first_name, last_name, middle_name, email, phone_number, department, position, hire_date, date_of_birth, address) VALUES
('John', 'Smith', 'A', 'john.smith@smartedu.edu', '123-456-7890', 'IT', 'Senior Developer', '2020-03-15', '1985-06-10', '123 Main St, Cityville'),
('Sarah', 'Johnson', 'M', 'sarah.johnson@smartedu.edu', '123-456-7891', 'Human Resources', 'HR Director', '2019-07-22', '1988-11-25', '456 Oak Ave, Townsville'),
('Michael', 'Chen', NULL, 'michael.chen@smartedu.edu', '123-456-7892', 'Marketing', 'Marketing Manager', '2021-01-10', '1990-03-15', '789 Pine Rd, Villageton'),
('Emily', 'Davis', 'R', 'emily.davis@smartedu.edu', '123-456-7893', 'IT', 'Data Analyst', '2022-08-05', '1992-09-18', '321 Elm St, Cityville'),
('Robert', 'Wilson', 'T', 'robert.wilson@smartedu.edu', '123-456-7894', 'Administration', 'Office Manager', '2018-11-30', '1983-12-05', '654 Maple Dr, Townsville'),
('Jessica', 'Brown', 'L', 'jessica.brown@smartedu.edu', '123-456-7895', 'IT', 'Junior Developer', '2023-02-20', '1995-04-22', '987 Cedar Ln, Villageton'),
('David', 'Miller', 'J', 'david.miller@smartedu.edu', '123-456-7896', 'Sales', 'Sales Executive', '2020-05-12', '1987-08-14', '234 Birch St, Cityville'),
('Lisa', 'Taylor', 'K', 'lisa.taylor@smartedu.edu', '123-456-7897', 'Finance', 'Accountant', '2021-09-18', '1991-02-28', '567 Walnut Ave, Townsville'),
('Kevin', 'Anderson', 'P', 'kevin.anderson@smartedu.edu', '123-456-7898', 'Customer Service', 'Support Manager', '2019-11-05', '1986-07-03', '890 Oak Rd, Villageton'),
('Amanda', 'White', 'S', 'amanda.white@smartedu.edu', '123-456-7899', 'IT', 'Project Manager', '2020-08-22', '1989-12-15', '432 Pine St, Cityville'),
('Brian', 'Clark', 'D', 'brian.clark@smartedu.edu', '123-456-7900', 'IT', 'Network Engineer', '2021-03-30', '1993-05-20', '765 Maple Ave, Townsville'),
('Jennifer', 'Lee', 'H', 'jennifer.lee@smartedu.edu', '123-456-7901', 'Design', 'UI/UX Designer', '2022-06-14', '1994-10-08', '198 Cedar Rd, Villageton'),
('Thomas', 'Harris', 'B', 'thomas.harris@smartedu.edu', '123-456-7902', 'IT', 'DevOps Engineer', '2020-12-01', '1984-01-25', '321 Birch Ave, Cityville'),
('Karen', 'Martin', 'N', 'karen.martin@smartedu.edu', '123-456-7903', 'IT', 'QA Tester', '2021-07-19', '1990-09-12', '654 Oak St, Townsville'),
('Steven', 'Thompson', 'W', 'steven.thompson@smartedu.edu', '123-456-7904', 'Business', 'Business Analyst', '2019-04-08', '1988-03-30', '987 Pine Ave, Villageton'),
('Michelle', 'Garcia', 'C', 'michelle.garcia@smartedu.edu', '123-456-7905', 'Legal', 'Legal Counsel', '2018-06-25', '1985-11-17', '234 Elm Rd, Cityville'),
('Christopher', 'Martinez', 'F', 'christopher.martinez@smartedu.edu', '123-456-7906', 'HR', 'Recruitment Specialist', '2022-02-14', '1992-06-05', '567 Maple St, Townsville'),
('Patricia', 'Robinson', 'G', 'patricia.robinson@smartedu.edu', '123-456-7907', 'HR', 'Training Coordinator', '2021-11-30', '1991-04-18', '890 Cedar Ave, Villageton'),
('Daniel', 'Walker', 'E', 'daniel.walker@smartedu.edu', '123-456-7908', 'Finance', 'Payroll Specialist', '2020-09-17', '1987-07-22', '432 Birch Rd, Cityville'),
('Nancy', 'Young', 'I', 'nancy.young@smartedu.edu', '123-456-7909', 'Operations', 'Procurement Officer', '2019-08-03', '1986-02-14', '765 Oak Ave, Townsville'),
('Mark', 'Allen', 'O', 'mark.allen@smartedu.edu', '123-456-7910', 'Operations', 'Logistics Manager', '2018-12-12', '1983-10-29', '198 Pine St, Villageton'),
('Sandra', 'King', 'Q', 'sandra.king@smartedu.edu', '123-456-7911', 'Operations', 'Warehouse Supervisor', '2022-05-26', '1993-01-03', '321 Maple Rd, Cityville'),
('Paul', 'Wright', 'U', 'paul.wright@smartedu.edu', '123-456-7912', 'Facilities', 'Maintenance Tech', '2021-04-11', '1990-08-16', '654 Cedar St, Townsville'),
('Carol', 'Scott', 'V', 'carol.scott@smartedu.edu', '123-456-7913', 'Security', 'Security Officer', '2020-01-28', '1989-05-07', '987 Birch Ave, Villageton'),
('James', 'Adams', 'X', 'james.adams@smartedu.edu', '123-456-7914', 'Facilities', 'Cleaning Staff', '2023-03-15', '1995-12-24', '234 Oak Rd, Cityville'),
('Susan', 'Baker', 'Y', 'susan.baker@smartedu.edu', '123-456-7915', 'Administration', 'Executive Assistant', '2019-10-09', '1984-09-11', '567 Pine Ave, Townsville'),
('Andrew', 'Nelson', 'Z', 'andrew.nelson@smartedu.edu', '123-456-7916', 'Marketing', 'Content Writer', '2022-07-04', '1994-02-26', '890 Maple St, Villageton'),
('Margaret', 'Carter', NULL, 'margaret.carter@smartedu.edu', '123-456-7917', 'Marketing', 'Social Media Manager', '2021-12-21', '1992-11-13', '432 Cedar Rd, Cityville'),
('Joshua', 'Mitchell', 'AA', 'joshua.mitchell@smartedu.edu', '123-456-7918', 'Marketing', 'SEO Specialist', '2020-06-08', '1987-04-02', '765 Birch Ave, Townsville'),
('Betty', 'Perez', 'BB', 'betty.perez@smartedu.edu', '123-456-7919', 'Design', 'Graphic Designer', '2019-03-25', '1986-01-19', '198 Oak St, Villageton'),
('Matthew', 'Roberts', 'CC', 'matthew.roberts@smartedu.edu', '123-456-7920', 'Marketing', 'Video Editor', '2023-01-11', '1995-10-06', '321 Pine Rd, Cityville'),
('Dorothy', 'Turner', 'DD', 'dorothy.turner@smartedu.edu', '123-456-7921', 'Marketing', 'Photographer', '2022-08-27', '1993-07-23', '654 Maple Ave, Townsville'),
('Donald', 'Phillips', 'EE', 'donald.phillips@smartedu.edu', '123-456-7922', 'Marketing', 'Event Coordinator', '2021-05-14', '1991-12-10', '987 Cedar St, Villageton'),
('Barbara', 'Campbell', 'FF', 'barbara.campbell@smartedu.edu', '123-456-7923', 'Marketing', 'PR Officer', '2020-02-01', '1988-09-27', '234 Birch Ave, Cityville'),
('Kenneth', 'Parker', 'GG', 'kenneth.parker@smartedu.edu', '123-456-7924', 'Marketing', 'Market Researcher', '2019-10-18', '1985-06-14', '567 Oak St, Townsville'),
('Elizabeth', 'Evans', 'HH', 'elizabeth.evans@smartedu.edu', '123-456-7925', 'Product', 'Product Manager', '2022-04-05', '1994-03-01', '890 Pine Ave, Villageton'),
('Steven', 'Edwards', 'II', 'steven.edwards@smartedu.edu', '123-456-7926', 'Documentation', 'Technical Writer', '2021-01-22', '1992-11-18', '432 Maple Rd, Cityville'),
('Helen', 'Collins', 'JJ', 'helen.collins@smartedu.edu', '123-456-7927', 'IT', 'Database Admin', '2020-09-09', '1989-08-05', '765 Cedar St, Townsville'),
('Edward', 'Stewart', 'KK', 'edward.stewart@smartedu.edu', '123-456-7928', 'IT', 'Systems Analyst', '2019-06-26', '1987-05-22', '198 Birch Rd, Villageton'),
('Debra', 'Sanchez', 'LL', 'debra.sanchez@smartedu.edu', '123-456-7929', 'IT', 'IT Support', '2023-03-13', '1995-02-09', '321 Oak Ave, Cityville'),
('Brian', 'Morris', 'MM', 'brian.morris@smartedu.edu', '123-456-7930', 'IT', 'Cybersecurity Analyst', '2022-11-30', '1993-10-26', '654 Pine St, Townsville'),
('Ruth', 'Rogers', 'NN', 'ruth.rogers@smartedu.edu', '123-456-7931', 'IT', 'Cloud Engineer', '2021-08-17', '1991-07-13', '987 Maple Ave, Villageton'),
('Scott', 'Reed', 'OO', 'scott.reed@smartedu.edu', '123-456-7932', 'IT', 'Mobile Developer', '2020-05-04', '1988-04-30', '234 Cedar Rd, Cityville'),
('Kimberly', 'Cook', 'PP', 'kimberly.cook@smartedu.edu', '123-456-7933', 'IT', 'Frontend Developer', '2019-12-21', '1986-01-17', '567 Birch Ave, Townsville'),
('Eric', 'Morgan', 'QQ', 'eric.morgan@smartedu.edu', '123-456-7934', 'IT', 'Backend Developer', '2022-09-08', '1994-10-04', '890 Oak St, Villageton'),
('Shirley', 'Bell', 'RR', 'shirley.bell@smartedu.edu', '123-456-7935', 'IT', 'Full Stack Developer', '2021-06-25', '1992-08-21', '432 Pine Rd, Cityville'),
('Jason', 'Murphy', 'SS', 'jason.murphy@smartedu.edu', '123-456-7936', 'IT', 'ML Engineer', '2020-03-12', '1989-06-08', '765 Maple St, Townsville'),
('Anna', 'Bailey', 'TT', 'anna.bailey@smartedu.edu', '123-456-7937', 'IT', 'Data Scientist', '2019-10-29', '1987-03-25', '198 Cedar Ave, Villageton'),
('Ronald', 'Rivera', 'UU', 'ronald.rivera@smartedu.edu', '123-456-7938', 'IT', 'AI Specialist', '2022-07-16', '1995-01-12', '321 Birch St, Cityville'),
('Brenda', 'Cooper', 'VV', 'brenda.cooper@smartedu.edu', '123-456-7939', 'IT', 'Blockchain Developer', '2021-04-03', '1993-11-29', '654 Oak Ave, Townsville'),
('Timothy', 'Richardson', 'WW', 'timothy.richardson@smartedu.edu', '123-456-7940', 'IT', 'Game Developer', '2020-12-20', '1991-09-16', '987 Pine St, Villageton'),
('Pamela', 'Cox', 'XX', 'pamela.cox@smartedu.edu', '123-456-7941', 'IT', 'Embedded Engineer', '2019-09-07', '1988-07-03', '234 Maple Rd, Cityville'),
('Jeffrey', 'Howard', 'YY', 'jeffrey.howard@smartedu.edu', '123-456-7942', 'Engineering', 'Robotics Engineer', '2022-05-24', '1994-04-20', '567 Cedar St, Townsville'),
('Nicole', 'Ward', 'ZZ', 'nicole.ward@smartedu.edu', '123-456-7943', 'Engineering', 'Electrical Engineer', '2021-02-10', '1992-01-07', '890 Birch Ave, Villageton'),
('Gary', 'Torres', 'AAA', 'gary.torres@smartedu.edu', '123-456-7944', 'Engineering', 'Mechanical Engineer', '2020-10-27', '1989-10-24', '432 Oak St, Cityville'),
('Christine', 'Peterson', 'BBB', 'christine.peterson@smartedu.edu', '123-456-7945', 'Engineering', 'Civil Engineer', '2019-07-14', '1987-08-11', '765 Pine Ave, Townsville'),
('Jacob', 'Gray', 'CCC', 'jacob.gray@smartedu.edu', '123-456-7946', 'Design', 'Architect', '2022-04-01', '1994-05-28', '198 Maple Rd, Villageton'),
('Catherine', 'Ramirez', 'DDD', 'catherine.ramirez@smartedu.edu', '123-456-7947', 'Design', 'Interior Designer', '2021-01-18', '1992-02-15', '321 Cedar Ave, Cityville'),
('Larry', 'James', 'EEE', 'larry.james@smartedu.edu', '123-456-7948', 'Design', 'Landscape Architect', '2020-09-05', '1989-11-02', '654 Birch St, Townsville'),
('Megan', 'Watson', 'FFF', 'megan.watson@smartedu.edu', '123-456-7949', 'Planning', 'Urban Planner', '2019-05-22', '1987-07-19', '987 Oak Rd, Villageton'),
('Frank', 'Brooks', 'GGG', 'frank.brooks@smartedu.edu', '123-456-7950', 'Environmental', 'Environmental Specialist', '2022-02-08', '1995-04-06', '234 Pine Ave, Cityville'),
('Janet', 'Kelly', 'HHH', 'janet.kelly@smartedu.edu', '123-456-7951', 'Safety', 'Safety Officer', '2021-10-25', '1993-01-23', '567 Maple St, Townsville'),
('Samuel', 'Sanders', 'III', 'samuel.sanders@smartedu.edu', '123-456-7952', 'Health', 'Nutritionist', '2020-07-12', '1990-10-10', '890 Cedar Rd, Villageton'),
('Julia', 'Price', 'JJJ', 'julia.price@smartedu.edu', '123-456-7953', 'Health', 'Fitness Trainer', '2019-04-29', '1988-08-27', '432 Birch Ave, Cityville'),
('Raymond', 'Bennett', 'KKK', 'raymond.bennett@smartedu.edu', '123-456-7954', 'Health', 'Counselor', '2022-01-15', '1995-06-14', '765 Oak St, Townsville'),
('Joyce', 'Wood', 'LLL', 'joyce.wood@smartedu.edu', '123-456-7955', 'Health', 'Nurse', '2021-09-02', '1993-03-31', '198 Pine Rd, Villageton'),
('Peter', 'Barnes', 'MMM', 'peter.barnes@smartedu.edu', '123-456-7956', 'Health', 'Doctor', '2020-05-20', '1990-12-18', '321 Maple Ave, Cityville'),
('Diane', 'Ross', 'NNN', 'diane.ross@smartedu.edu', '123-456-7957', 'Health', 'Pharmacist', '2019-02-06', '1988-09-05', '654 Cedar St, Townsville'),
('Patrick', 'Henderson', 'OOO', 'patrick.henderson@smartedu.edu', '123-456-7958', 'Health', 'Lab Technician', '2022-10-23', '1996-07-22', '987 Birch Rd, Villageton'),
('Alice', 'Coleman', 'PPP', 'alice.coleman@smartedu.edu', '123-456-7959', 'Research', 'Research Scientist', '2021-07-10', '1994-05-09', '234 Oak Ave, Cityville');

-- Insert 70 records into APPLICANT table
INSERT INTO APPLICANT (job_posting_id, full_name, email, phone_number, status, applied_date) VALUES
(1, 'David Miller', 'david.miller@gmail.com', '123-555-0101', 'Interview Scheduled', '2024-01-18'),
(1, 'Lisa Taylor', 'lisa.taylor@yahoo.com', '123-555-0102', 'New', '2024-01-19'),
(2, 'Kevin Anderson', 'kevin.anderson@outlook.com', '123-555-0103', 'Rejected', '2024-01-22'),
(3, 'Amanda White', 'amanda.white@gmail.com', '123-555-0104', 'Hired', '2024-01-26'),
(4, 'Brian Clark', 'brian.clark@yahoo.com', '123-555-0105', 'Under Review', '2024-02-02'),
(5, 'Jennifer Lee', 'jennifer.lee@gmail.com', '123-555-0106', 'Interviewed', '2024-02-03'),
(6, 'Thomas Harris', 'thomas.harris@yahoo.com', '123-555-0107', 'Offer Extended', '2024-02-04'),
(7, 'Karen Martin', 'karen.martin@gmail.com', '123-555-0108', 'New', '2024-02-05'),
(8, 'Steven Thompson', 'steven.thompson@yahoo.com', '123-555-0109', 'Rejected', '2024-02-06'),
(9, 'Michelle Garcia', 'michelle.garcia@gmail.com', '123-555-0110', 'Hired', '2024-02-07'),
(10, 'Christopher Martinez', 'christopher.martinez@yahoo.com', '123-555-0111', 'Under Review', '2024-02-08'),
(11, 'Patricia Robinson', 'patricia.robinson@gmail.com', '123-555-0112', 'Interview Scheduled', '2024-02-09'),
(12, 'Daniel Walker', 'daniel.walker@yahoo.com', '123-555-0113', 'New', '2024-02-10'),
(13, 'Nancy Young', 'nancy.young@gmail.com', '123-555-0114', 'Rejected', '2024-02-11'),
(14, 'Mark Allen', 'mark.allen@yahoo.com', '123-555-0115', 'Hired', '2024-02-12'),
(15, 'Sandra King', 'sandra.king@gmail.com', '123-555-0116', 'Under Review', '2024-02-13'),
(16, 'Paul Wright', 'paul.wright@yahoo.com', '123-555-0117', 'Interviewed', '2024-02-14'),
(17, 'Carol Scott', 'carol.scott@gmail.com', '123-555-0118', 'Offer Extended', '2024-02-15'),
(18, 'James Adams', 'james.adams@yahoo.com', '123-555-0119', 'New', '2024-02-16'),
(19, 'Susan Baker', 'susan.baker@gmail.com', '123-555-0120', 'Rejected', '2024-02-17'),
(20, 'Andrew Nelson', 'andrew.nelson@yahoo.com', '123-555-0121', 'Hired', '2024-02-18'),
(21, 'Margaret Carter', 'margaret.carter@gmail.com', '123-555-0122', 'Under Review', '2024-02-19'),
(22, 'Joshua Mitchell', 'joshua.mitchell@yahoo.com', '123-555-0123', 'Interview Scheduled', '2024-02-20'),
(23, 'Betty Perez', 'betty.perez@gmail.com', '123-555-0124', 'New', '2024-02-21'),
(24, 'Matthew Roberts', 'matthew.roberts@yahoo.com', '123-555-0125', 'Rejected', '2024-02-22'),
(25, 'Dorothy Turner', 'dorothy.turner@gmail.com', '123-555-0126', 'Hired', '2024-02-23'),
(26, 'Donald Phillips', 'donald.phillips@yahoo.com', '123-555-0127', 'Under Review', '2024-02-24'),
(27, 'Barbara Campbell', 'barbara.campbell@gmail.com', '123-555-0128', 'Interviewed', '2024-02-25'),
(28, 'Kenneth Parker', 'kenneth.parker@yahoo.com', '123-555-0129', 'Offer Extended', '2024-02-26'),
(29, 'Elizabeth Evans', 'elizabeth.evans@gmail.com', '123-555-0130', 'New', '2024-02-27'),
(30, 'Steven Edwards', 'steven.edwards@yahoo.com', '123-555-0131', 'Rejected', '2024-02-28'),
(31, 'Helen Collins', 'helen.collins@gmail.com', '123-555-0132', 'Hired', '2024-02-29'),
(32, 'Edward Stewart', 'edward.stewart@yahoo.com', '123-555-0133', 'Under Review', '2024-03-01'),
(33, 'Debra Sanchez', 'debra.sanchez@gmail.com', '123-555-0134', 'Interview Scheduled', '2024-03-02'),
(34, 'Brian Morris', 'brian.morris@yahoo.com', '123-555-0135', 'New', '2024-03-03'),
(35, 'Ruth Rogers', 'ruth.rogers@gmail.com', '123-555-0136', 'Rejected', '2024-03-04'),
(36, 'Scott Reed', 'scott.reed@yahoo.com', '123-555-0137', 'Hired', '2024-03-05'),
(37, 'Kimberly Cook', 'kimberly.cook@gmail.com', '123-555-0138', 'Under Review', '2024-03-06'),
(38, 'Eric Morgan', 'eric.morgan@yahoo.com', '123-555-0139', 'Interviewed', '2024-03-07'),
(39, 'Shirley Bell', 'shirley.bell@gmail.com', '123-555-0140', 'Offer Extended', '2024-03-08'),
(40, 'Jason Murphy', 'jason.murphy@yahoo.com', '123-555-0141', 'New', '2024-03-09'),
(41, 'Anna Bailey', 'anna.bailey@gmail.com', '123-555-0142', 'Rejected', '2024-03-10'),
(42, 'Ronald Rivera', 'ronald.rivera@yahoo.com', '123-555-0143', 'Hired', '2024-03-11'),
(43, 'Brenda Cooper', 'brenda.cooper@gmail.com', '123-555-0144', 'Under Review', '2024-03-12'),
(44, 'Timothy Richardson', 'timothy.richardson@yahoo.com', '123-555-0145', 'Interview Scheduled', '2024-03-13'),
(45, 'Pamela Cox', 'pamela.cox@gmail.com', '123-555-0146', 'New', '2024-03-14'),
(46, 'Jeffrey Howard', 'jeffrey.howard@yahoo.com', '123-555-0147', 'Rejected', '2024-03-15'),
(47, 'Nicole Ward', 'nicole.ward@gmail.com', '123-555-0148', 'Hired', '2024-03-16'),
(48, 'Gary Torres', 'gary.torres@yahoo.com', '123-555-0149', 'Under Review', '2024-03-17'),
(49, 'Christine Peterson', 'christine.peterson@gmail.com', '123-555-0150', 'Interviewed', '2024-03-18'),
(50, 'Jacob Gray', 'jacob.gray@yahoo.com', '123-555-0151', 'Offer Extended', '2024-03-19'),
(51, 'Catherine Ramirez', 'catherine.ramirez@gmail.com', '123-555-0152', 'New', '2024-03-20'),
(52, 'Larry James', 'larry.james@yahoo.com', '123-555-0153', 'Rejected', '2024-03-21'),
(53, 'Megan Watson', 'megan.watson@gmail.com', '123-555-0154', 'Hired', '2024-03-22'),
(54, 'Frank Brooks', 'frank.brooks@yahoo.com', '123-555-0155', 'Under Review', '2024-03-23'),
(55, 'Janet Kelly', 'janet.kelly@gmail.com', '123-555-0156', 'Interview Scheduled', '2024-03-24'),
(56, 'Samuel Sanders', 'samuel.sanders@yahoo.com', '123-555-0157', 'New', '2024-03-25'),
(57, 'Julia Price', 'julia.price@gmail.com', '123-555-0158', 'Rejected', '2024-03-26'),
(58, 'Raymond Bennett', 'raymond.bennett@yahoo.com', '123-555-0159', 'Hired', '2024-03-27'),
(59, 'Joyce Wood', 'joyce.wood@gmail.com', '123-555-0160', 'Under Review', '2024-03-28'),
(60, 'Peter Barnes', 'peter.barnes@yahoo.com', '123-555-0161', 'Interviewed', '2024-03-29'),
(61, 'Diane Ross', 'diane.ross@gmail.com', '123-555-0162', 'Offer Extended', '2024-03-30'),
(62, 'Patrick Henderson', 'patrick.henderson@yahoo.com', '123-555-0163', 'New', '2024-03-31'),
(63, 'Alice Coleman', 'alice.coleman@gmail.com', '123-555-0164', 'Rejected', '2024-04-01'),
(64, 'Walter Powell', 'walter.powell@gmail.com', '123-555-0165', 'Hired', '2024-04-02'),
(65, 'Frances Patterson', 'frances.patterson@yahoo.com', '123-555-0166', 'Under Review', '2024-04-03'),
(66, 'Gregory Hughes', 'gregory.hughes@gmail.com', '123-555-0167', 'Interview Scheduled', '2024-04-04'),
(67, 'Victoria Flores', 'victoria.flores@yahoo.com', '123-555-0168', 'New', '2024-04-05'),
(68, 'Bobby Washington', 'bobby.washington@gmail.com', '123-555-0169', 'Rejected', '2024-04-06'),
(69, 'Evelyn Butler', 'evelyn.butler@yahoo.com', '123-555-0170', 'Hired', '2024-04-07'),
(70, 'Philip Simmons', 'philip.simmons@gmail.com', '123-555-0171', 'Under Review', '2024-04-08');

-- Insert 70 records into EMPLOYEE_ATTENDANCE table
DECLARE @i INT = 1;
DECLARE @emp_id INT;
DECLARE @att_date DATE = '2024-02-01';

WHILE @i <= 70
BEGIN
    SET @emp_id = CASE WHEN @i <= (SELECT COUNT(*) FROM EMPLOYEE) THEN @i ELSE @i % (SELECT COUNT(*) FROM EMPLOYEE) + 1 END;
    
    INSERT INTO EMPLOYEE_ATTENDANCE (employee_id, [date], time_in, time_out, status, minutes_late, is_absent)
    VALUES (
        @emp_id,
        DATEADD(DAY, (@i-1)%20, @att_date),
        CASE WHEN @i % 10 = 0 THEN NULL ELSE DATEADD(MINUTE, CASE WHEN @i % 5 = 0 THEN 15 ELSE -2 END, CAST('09:00:00' AS TIME)) END,
        CASE WHEN @i % 10 = 0 THEN NULL ELSE DATEADD(MINUTE, CASE WHEN @i % 4 = 0 THEN 30 ELSE -10 END, CAST('17:00:00' AS TIME)) END,
        CASE WHEN @i % 10 = 0 THEN 'Absent' WHEN @i % 5 = 0 THEN 'Late' ELSE 'Present' END,
        CASE WHEN @i % 5 = 0 THEN 15 ELSE 0 END,
        CASE WHEN @i % 10 = 0 THEN 1 ELSE 0 END
    );
    
    SET @i = @i + 1;
END;

-- Insert 70 records into LEAVE_REQUEST table
DECLARE @j INT = 1;
DECLARE @start_date DATE = '2024-03-01';

WHILE @j <= 70
BEGIN
    SET @emp_id = CASE WHEN @j <= (SELECT COUNT(*) FROM EMPLOYEE) THEN @j ELSE @j % (SELECT COUNT(*) FROM EMPLOYEE) + 1 END;
    
    INSERT INTO LEAVE_REQUEST (employee_id, start_date, end_date, leave_type, reason, status, approver_name)
    VALUES (
        @emp_id,
        DATEADD(DAY, (@j-1)*3, @start_date),
        DATEADD(DAY, (@j-1)*3 + CASE WHEN @j % 3 = 0 THEN 3 WHEN @j % 3 = 1 THEN 2 ELSE 1 END, @start_date),
        CASE WHEN @j % 4 = 0 THEN 'Sick Leave' WHEN @j % 4 = 1 THEN 'Vacation' WHEN @j % 4 = 2 THEN 'Personal Leave' ELSE 'Maternity Leave' END,
        CASE WHEN @j % 4 = 0 THEN 'Medical appointment' WHEN @j % 4 = 1 THEN 'Family vacation' WHEN @j % 4 = 2 THEN 'Personal matters' ELSE 'Maternity leave' END,
        CASE WHEN @j % 5 = 0 THEN 'Pending' WHEN @j % 5 = 1 THEN 'Approved' WHEN @j % 5 = 2 THEN 'Rejected' WHEN @j % 5 = 3 THEN 'Approved' ELSE 'Pending' END,
        CASE WHEN @j % 5 IN (1,3) THEN 'Sarah Johnson' WHEN @j % 5 = 0 THEN NULL ELSE 'John Smith' END
    );
    
    SET @j = @j + 1;
END;

-- Insert 70 records into PERFORMANCE_REVIEW table
DECLARE @k INT = 1;
DECLARE @review_date DATE = '2024-01-15';
DECLARE @period_start DATE = '2023-07-01';
DECLARE @period_end DATE = '2023-12-31';

WHILE @k <= 70
BEGIN
    SET @emp_id = CASE WHEN @k <= (SELECT COUNT(*) FROM EMPLOYEE) THEN @k ELSE @k % (SELECT COUNT(*) FROM EMPLOYEE) + 1 END;
    
    INSERT INTO PERFORMANCE_REVIEW (employee_id, review_date, reviewer_name, score, comments, period_start, period_end)
    VALUES (
        @emp_id,
        DATEADD(MONTH, (@k-1)%6, @review_date),
        CASE WHEN @k % 3 = 0 THEN 'Sarah Johnson' WHEN @k % 3 = 1 THEN 'John Smith' ELSE 'Michael Chen' END,
        80 + (@k % 21), -- Scores from 80 to 100
        CASE 
            WHEN @k % 5 = 0 THEN 'Exceptional performance and leadership. Consistently exceeds expectations.'
            WHEN @k % 5 = 1 THEN 'Strong performance with excellent results. Reliable team member.'
            WHEN @k % 5 = 2 THEN 'Good performance meeting all expectations. Solid contributor.'
            WHEN @k % 5 = 3 THEN 'Satisfactory performance with room for improvement in some areas.'
            ELSE 'Performance needs improvement. Requires additional training and supervision.'
        END,
        DATEADD(MONTH, -6, DATEADD(MONTH, (@k-1)%6, @review_date)),
        DATEADD(DAY, -1, DATEADD(MONTH, (@k-1)%6, @review_date))
    );
    
    SET @k = @k + 1;
END;

-- Verify the data
SELECT 'JOB_POSTING' AS TableName, COUNT(*) AS RecordCount FROM JOB_POSTING WHERE IsDeleted = 0
UNION ALL
SELECT 'EMPLOYEE', COUNT(*) FROM EMPLOYEE WHERE IsDeleted = 0
UNION ALL
SELECT 'APPLICANT', COUNT(*) FROM APPLICANT WHERE IsDeleted = 0
UNION ALL
SELECT 'EMPLOYEE_ATTENDANCE', COUNT(*) FROM EMPLOYEE_ATTENDANCE WHERE IsDeleted = 0
UNION ALL
SELECT 'LEAVE_REQUEST', COUNT(*) FROM LEAVE_REQUEST WHERE IsDeleted = 0
UNION ALL
SELECT 'PERFORMANCE_REVIEW', COUNT(*) FROM PERFORMANCE_REVIEW WHERE IsDeleted = 0;



-- Add 10 more records to JOB_POSTING table (from ID 71 to 80)
INSERT INTO JOB_POSTING (title, department, employment_type, description, posted_date, closing_date) VALUES
('Chief Technology Officer', 'IT', 'Full-Time', 'Lead technology strategy and innovation', '2025-01-01', '2025-02-01'),
('Chief Financial Officer', 'Finance', 'Full-Time', 'Oversee financial operations and strategy', '2025-01-05', '2025-02-05'),
('Chief Marketing Officer', 'Marketing', 'Full-Time', 'Lead marketing strategy and brand management', '2025-01-10', '2025-02-10'),
('Head of Operations', 'Operations', 'Full-Time', 'Oversee daily operations and efficiency', '2025-01-15', '2025-02-15'),
('Dean of Academics', 'Academic', 'Full-Time', 'Lead academic programs and curriculum', '2025-01-20', '2025-02-20'),
('Student Advisor', 'Student Services', 'Full-Time', 'Guide students on academic and career paths', '2025-01-25', '2025-02-25'),
('Library Manager', 'Library', 'Full-Time', 'Manage library resources and services', '2025-01-30', '2025-03-02'),
('IT Security Manager', 'IT', 'Full-Time', 'Lead cybersecurity initiatives and team', '2025-02-02', '2025-03-04'),
('Facilities Manager', 'Facilities', 'Full-Time', 'Oversee building maintenance and operations', '2025-02-05', '2025-03-07'),
('Alumni Relations Manager', 'Development', 'Full-Time', 'Manage alumni engagement and fundraising', '2025-02-08', '2025-03-10');

-- Add 10 more records to EMPLOYEE table (from ID 71 to 80)
INSERT INTO EMPLOYEE (first_name, last_name, middle_name, email, phone_number, department, position, hire_date, date_of_birth, address) VALUES
('Henry', 'Foster', 'AAA', 'henry.foster@smartedu.edu', '123-456-7960', 'IT', 'CTO', '2017-04-10', '1975-09-12', '123 Tech Blvd, Cityville'),
('Victoria', 'Jenkins', 'BBB', 'victoria.jenkins@smartedu.edu', '123-456-7961', 'Finance', 'CFO', '2016-08-22', '1978-03-25', '456 Finance Ave, Townsville'),
('Benjamin', 'Powell', 'CCC', 'benjamin.powell@smartedu.edu', '123-456-7962', 'Marketing', 'CMO', '2018-11-15', '1976-07-18', '789 Market St, Villageton'),
('Olivia', 'Long', 'DDD', 'olivia.long@smartedu.edu', '123-456-7963', 'Operations', 'Head of Operations', '2019-02-28', '1980-12-05', '321 Operations Rd, Cityville'),
('George', 'Patterson', 'EEE', 'george.patterson@smartedu.edu', '123-456-7964', 'Academic', 'Dean of Academics', '2015-06-20', '1972-04-30', '654 Academic Dr, Townsville'),
('Sophia', 'Hughes', 'FFF', 'sophia.hughes@smartedu.edu', '123-456-7965', 'Student Services', 'Student Advisor', '2022-09-05', '1990-10-15', '987 Student Ave, Villageton'),
('Alexander', 'Flores', 'GGG', 'alexander.flores@smartedu.edu', '123-456-7966', 'Library', 'Library Manager', '2021-12-10', '1985-06-22', '234 Library St, Cityville'),
('Charlotte', 'Washington', 'HHH', 'charlotte.washington@smartedu.edu', '123-456-7967', 'IT', 'IT Security Manager', '2020-05-18', '1983-02-14', '567 Security Ave, Townsville'),
('William', 'Butler', 'III', 'william.butler@smartedu.edu', '123-456-7968', 'Facilities', 'Facilities Manager', '2018-03-25', '1979-11-08', '890 Facilities Rd, Villageton'),
('Amelia', 'Simmons', 'JJJ', 'amelia.simmons@smartedu.edu', '123-456-7969', 'Development', 'Alumni Relations Manager', '2019-10-12', '1982-08-03', '432 Alumni St, Cityville');

-- Add 10 more records to APPLICANT table (from ID 71 to 80)
INSERT INTO APPLICANT (job_posting_id, full_name, email, phone_number, status, applied_date) VALUES
(71, 'Robert Foster', 'robert.foster@gmail.com', '123-555-0172', 'Interview Scheduled', '2025-01-03'),
(71, 'Emma Jenkins', 'emma.jenkins@yahoo.com', '123-555-0173', 'New', '2025-01-06'),
(72, 'Daniel Powell', 'daniel.powell@outlook.com', '123-555-0174', 'Rejected', '2025-01-08'),
(73, 'Chloe Long', 'chloe.long@gmail.com', '123-555-0175', 'Hired', '2025-01-12'),
(74, 'Joseph Patterson', 'joseph.patterson@yahoo.com', '123-555-0176', 'Under Review', '2025-01-17'),
(75, 'Mia Hughes', 'mia.hughes@gmail.com', '123-555-0177', 'Interviewed', '2025-01-22'),
(76, 'David Flores', 'david.flores@yahoo.com', '123-555-0178', 'Offer Extended', '2025-01-27'),
(77, 'Grace Washington', 'grace.washington@gmail.com', '123-555-0179', 'New', '2025-02-01'),
(78, 'Christopher Butler', 'christopher.butler@yahoo.com', '123-555-0180', 'Rejected', '2025-02-03'),
(79, 'Zoe Simmons', 'zoe.simmons@gmail.com', '123-555-0181', 'Hired', '2025-02-06');

-- Add 10 more records to EMPLOYEE_ATTENDANCE table (for new employees)
INSERT INTO EMPLOYEE_ATTENDANCE (employee_id, [date], time_in, time_out, status, minutes_late, is_absent)
VALUES
-- Henry Foster (ID: 71)
(71, '2025-02-01', '08:55:00', '17:05:00', 'Present', 0, 0),
(71, '2025-02-02', '09:10:00', '17:00:00', 'Late', 10, 0),
(71, '2025-02-03', '08:58:00', '16:55:00', 'Present', 0, 0),

-- Victoria Jenkins (ID: 72)
(72, '2025-02-01', '09:05:00', '17:10:00', 'Late', 5, 0),
(72, '2025-02-02', '08:50:00', '16:50:00', 'Present', 0, 0),
(72, '2025-02-03', '09:15:00', '17:05:00', 'Late', 15, 0),

-- Benjamin Powell (ID: 73)
(73, '2025-02-01', '09:00:00', '17:00:00', 'Present', 0, 0),
(73, '2025-02-02', '09:20:00', NULL, 'Late', 20, 0),
(73, '2025-02-03', NULL, NULL, 'Absent', 0, 1),

-- Olivia Long (ID: 74)
(74, '2025-02-01', '08:45:00', '16:45:00', 'Present', 0, 0),
(74, '2025-02-02', '09:00:00', '17:00:00', 'Present', 0, 0),
(74, '2025-02-03', '08:55:00', '16:58:00', 'Present', 0, 0);

-- Add 10 more records to LEAVE_REQUEST table (for new employees)
INSERT INTO LEAVE_REQUEST (employee_id, start_date, end_date, leave_type, reason, status, approver_name)
VALUES
(71, '2025-03-15', '2025-03-17', 'Vacation', 'Family trip', 'Approved', 'Sarah Johnson'),
(71, '2025-06-10', '2025-06-14', 'Personal Leave', 'Wedding ceremony', 'Pending', NULL),
(72, '2025-04-01', '2025-04-01', 'Sick Leave', 'Medical checkup', 'Approved', 'John Smith'),
(72, '2025-07-20', '2025-07-25', 'Vacation', 'Summer vacation', 'Approved', 'Sarah Johnson'),
(73, '2025-02-15', '2025-02-15', 'Sick Leave', 'Flu', 'Rejected', 'Michael Chen'),
(73, '2025-05-05', '2025-05-09', 'Personal Leave', 'House moving', 'Approved', 'John Smith'),
(74, '2025-03-22', '2025-03-26', 'Vacation', 'Beach holiday', 'Approved', 'Sarah Johnson'),
(74, '2025-08-10', '2025-08-11', 'Sick Leave', 'Dental appointment', 'Pending', NULL),
(75, '2025-04-10', '2025-04-12', 'Personal Leave', 'Family event', 'Approved', 'John Smith'),
(75, '2025-09-01', '2025-09-03', 'Vacation', 'Long weekend getaway', 'Approved', 'Sarah Johnson');

-- Add 10 more records to PERFORMANCE_REVIEW table (for new employees)
INSERT INTO PERFORMANCE_REVIEW (employee_id, review_date, reviewer_name, score, comments, period_start, period_end)
VALUES
(71, '2025-01-15', 'Board of Directors', 95, 'Outstanding leadership in technology transformation. Visionary approach to digital innovation.', '2024-07-01', '2024-12-31'),
(72, '2025-01-20', 'Board of Directors', 92, 'Excellent financial stewardship. Significant improvements in budget efficiency and revenue growth.', '2024-07-01', '2024-12-31'),
(73, '2025-01-25', 'Board of Directors', 90, 'Strong marketing strategy execution. Notable increase in brand awareness and student enrollment.', '2024-07-01', '2024-12-31'),
(74, '2025-02-01', 'Benjamin Powell', 88, 'Effective operational management. Improved processes and team coordination.', '2024-07-01', '2024-12-31'),
(75, '2025-02-05', 'Sarah Johnson', 93, 'Exceptional academic leadership. Curriculum improvements and faculty development initiatives successful.', '2024-07-01', '2024-12-31'),
(76, '2025-02-10', 'George Patterson', 85, 'Good student support and guidance. Positive feedback from students and parents.', '2024-08-01', '2025-01-31'),
(77, '2025-02-15', 'Olivia Long', 87, 'Effective library management. Digital resources expansion well-received by students.', '2024-08-01', '2025-01-31'),
(78, '2025-02-20', 'Henry Foster', 89, 'Strong security implementation. No security breaches reported during review period.', '2024-08-01', '2025-01-31'),
(79, '2025-02-25', 'Olivia Long', 86, 'Good facilities management. Maintenance schedules well-maintained.', '2024-08-01', '2025-01-31'),
(80, '2025-03-01', 'Benjamin Powell', 84, 'Effective alumni engagement. Successful fundraising campaign organized.', '2024-08-01', '2025-01-31');

-- Verify the data (should show 80 for each table now)
SELECT 'JOB_POSTING' AS TableName, COUNT(*) AS RecordCount FROM JOB_POSTING WHERE IsDeleted = 0
UNION ALL
SELECT 'EMPLOYEE', COUNT(*) FROM EMPLOYEE WHERE IsDeleted = 0
UNION ALL
SELECT 'APPLICANT', COUNT(*) FROM APPLICANT WHERE IsDeleted = 0
UNION ALL
SELECT 'EMPLOYEE_ATTENDANCE', COUNT(*) FROM EMPLOYEE_ATTENDANCE WHERE IsDeleted = 0
UNION ALL
SELECT 'LEAVE_REQUEST', COUNT(*) FROM LEAVE_REQUEST WHERE IsDeleted = 0
UNION ALL
SELECT 'PERFORMANCE_REVIEW', COUNT(*) FROM PERFORMANCE_REVIEW WHERE IsDeleted = 0;