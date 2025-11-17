-- employees_seed.sql
-- 12 demo employees across 4 departments for demos, filtering and metrics
-- Adjust column names/types if your schema differs. Assumes table `employees` exists with columns:
-- (Id AUTO_INCREMENT PK), Name, Department, Role, Address, Phone, Salary, CreatedAt

INSERT INTO employees (Name, Department, Role, Address, Phone, Salary, CreatedAt) VALUES
('Aisha Mahmoud',     'Engineering', 'Senior Software Engineer', '1200 Elm St, Apt 4B, Springfield', '555-0101', 115000.00, '2023-08-01 09:15:00'),
('Daniel Kim',        'Engineering', 'DevOps Engineer',          '22 River Rd, Suite 3, Springfield', '555-0102', 98000.00,  '2024-01-18 11:30:00'),
('Priya Patel',       'Engineering', 'Software Engineer',        '45 Market St, Springfield', '555-0103', 87000.00,  '2024-03-05 08:45:00'),

('Maria González',    'HR',          'HR Manager',               '300 Oak Ave, Springfield', '555-0201', 78000.00,  '2022-11-12 10:00:00'),
('Liam O''Connor',    'HR',          'Recruiter',                '78 Pine St, Springfield', '555-0202', 56000.00,  '2024-06-20 09:00:00'),
('Chen Wei',          'HR',          'HR Coordinator',           '9 Willow Ln, Springfield', '555-0203', 52000.00,  '2023-05-02 14:20:00'),

('Olivia Park',       'Sales',       'Sales Director',           '150 Commerce Blvd, Springfield', '555-0301', 129000.00, '2021-09-03 08:00:00'),
('Noah Johnson',      'Sales',       'Account Executive',        '4 Capital Way, Springfield', '555-0302', 72000.00,  '2023-12-01 12:10:00'),
('Sofia Russo',       'Sales',       'Account Manager',          '51 Lakeview Dr, Springfield', '555-0303', 68000.00,  '2024-02-27 15:40:00'),

('Ethan Brown',       'Finance',     'Finance Manager',          '88 Bank St, Springfield', '555-0401', 98000.00,  '2022-02-14 09:25:00'),
('Isabella Rossi',    'Finance',     'Financial Analyst',        '16 Hill Rd, Springfield', '555-0402', 72000.00,  '2023-07-09 10:50:00'),
('Mateo Alvarez',     'Finance',     'Payroll Specialist',       '2 Sunset Blvd, Springfield', '555-0403', 59000.00, '2024-04-11 11:05:00');

-- Helpful queries:
-- HEADCOUNT BY DEPT:
-- SELECT Department, COUNT(*) AS Headcount FROM employees GROUP BY Department ORDER BY Headcount DESC;
-- AVG/MIN/MAX SALARY BY DEPT:
-- SELECT Department, COUNT(*) AS Headcount, ROUND(AVG(Salary),2) AS AvgSalary, MIN(Salary) AS MinSalary, MAX(Salary) AS MaxSalary FROM employees GROUP BY Department;
-- TOP 5 HIGHEST PAID:
-- SELECT Name, Department, Role, Salary FROM employees ORDER BY Salary DESC LIMIT 5;
-- ENGINEERING SALARY > 90k:
-- SELECT Name, Role, Salary FROM employees WHERE Department = 'Engineering' AND Salary > 90000 ORDER BY Salary DESC;
-- NEW HIRES LAST 12 MONTHS:
-- SELECT Department, COUNT(*) AS NewHires, ROUND(AVG(Salary),2) AS AvgSalaryNew FROM employees WHERE CreatedAt >= DATE_SUB(CURDATE(), INTERVAL 12 MONTH) GROUP BY Department;
