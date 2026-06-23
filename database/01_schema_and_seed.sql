SELECT @@VERSION;
GO

USE master;
GO

DROP DATABASE IF EXISTS DRMS;
GO

CREATE DATABASE DRMS;
GO

USE DRMS;
GO

/*
MASTER TABLES
*/

--ROLES
CREATE TABLE Roles (
    RoleId      INT PRIMARY KEY IDENTITY(1,1),
    RoleName    VARCHAR(50) NOT NULL UNIQUE,
    IsActive    BIT DEFAULT 1
);

--USERS
CREATE TABLE Users (
    UserId          INT PRIMARY KEY IDENTITY(1,1),
    EmployeeCode    VARCHAR(20) NOT NULL UNIQUE,
    FirstName       VARCHAR(50) NOT NULL,
    LastName        VARCHAR(50) NOT NULL,
    Email           VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash    VARCHAR(255) NOT NULL,
    RoleId          INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleId),
    IsActive        BIT DEFAULT 1,
    CreatedDate     DATETIME DEFAULT GETDATE(),
    LastLoginDate   DATETIME NULL
);

--PROJECTS
CREATE TABLE Projects (
    ProjectId           INT PRIMARY KEY IDENTITY(1,1),
    ProjectCode         VARCHAR(20) NOT NULL UNIQUE,
    ProjectName         VARCHAR(100) NOT NULL,
    ProjectDescription  VARCHAR(500) NULL,
    TechLeadId          INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    IsActive            BIT DEFAULT 1,
    CreatedDate         DATETIME DEFAULT GETDATE()
);

--ENVIRONMENTS
CREATE TABLE Environments (
    EnvironmentId   INT PRIMARY KEY IDENTITY(1,1),
    EnvironmentName VARCHAR(50) NOT NULL,
    SequenceOrder   INT NOT NULL,
    IsActive        BIT DEFAULT 1
);

--DEPLOYMENT TYPES
CREATE TABLE DeploymentTypes (
    DeploymentTypeId    INT PRIMARY KEY IDENTITY(1,1),
    DeploymentTypeName  VARCHAR(50) NOT NULL,
    IsActive            BIT DEFAULT 1
);



/*
STATUS TABLES
*/
CREATE TABLE StatusMaster (
    StatusId    INT PRIMARY KEY IDENTITY(1,1),
    StatusName  VARCHAR(100) NOT NULL,
    ModuleName  VARCHAR(50) NOT NULL,
    IsActive    BIT DEFAULT 1
);

CREATE TABLE ApprovalWorkflow (
    WorkflowId      INT PRIMARY KEY IDENTITY(1,1),
    RoleId          INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleId),
    ApprovalOrder   INT NOT NULL,
    IsActive        BIT DEFAULT 1
);


/*
TRANSACTION TABLES
*/

CREATE TABLE DeploymentRequests (
    DeploymentRequestId         INT PRIMARY KEY IDENTITY(1,1),
    RequestNumber               VARCHAR(30) NOT NULL UNIQUE,
    ProjectId                   INT NOT NULL FOREIGN KEY REFERENCES Projects(ProjectId),
    RequestedBy                 INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    SourceBranch                VARCHAR(100) NOT NULL,
    TargetEnvironmentId         INT NOT NULL FOREIGN KEY REFERENCES Environments(EnvironmentId),
    DeploymentTypeId            INT NOT NULL FOREIGN KEY REFERENCES DeploymentTypes(DeploymentTypeId),
    BuildVersion                VARCHAR(50) NULL,
    ChangeSummary               VARCHAR(MAX) NOT NULL,
    RollbackPlan                VARCHAR(MAX) NOT NULL,
    RequestedDeploymentDate     DATETIME NOT NULL,
    RequestedDate               DATETIME DEFAULT GETDATE(),
    CurrentStatusId             INT NOT NULL FOREIGN KEY REFERENCES StatusMaster(StatusId),
    CurrentApprovalLevel        INT DEFAULT 0,
    IsActive                    BIT DEFAULT 1
);

CREATE TABLE DeploymentApprovals (
    ApprovalId              INT PRIMARY KEY IDENTITY(1,1),
    DeploymentRequestId     INT NOT NULL FOREIGN KEY REFERENCES DeploymentRequests(DeploymentRequestId),
    ApproverId              INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    ApprovalLevel           INT NOT NULL,
    ActionTaken             VARCHAR(50) NOT NULL,
    Remarks                 VARCHAR(500) NULL,
    ActionDate              DATETIME DEFAULT GETDATE()
);

CREATE TABLE DeploymentHistory (
    HistoryId               INT PRIMARY KEY IDENTITY(1,1),
    DeploymentRequestId     INT NOT NULL FOREIGN KEY REFERENCES DeploymentRequests(DeploymentRequestId),
    OldStatusId             INT NOT NULL FOREIGN KEY REFERENCES StatusMaster(StatusId),
    NewStatusId             INT NOT NULL FOREIGN KEY REFERENCES StatusMaster(StatusId),
    ActionBy                INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    ActionDate              DATETIME DEFAULT GETDATE(),
    Remarks                 VARCHAR(500) NULL
);


-- Roles
INSERT INTO Roles (RoleName) VALUES
('Developer'), ('TechLead'), ('QA'), ('DevOps'), ('Admin');

-- Environments
INSERT INTO Environments (EnvironmentName, SequenceOrder) VALUES
('Development', 1), ('Staging', 2), ('Production', 3);

-- DeploymentTypes
INSERT INTO DeploymentTypes (DeploymentTypeName) VALUES
('Feature Release'), ('Bug Fix'), ('Hotfix'), ('Patch');

-- StatusMaster
INSERT INTO StatusMaster (StatusName, ModuleName) VALUES
('Draft',                   'DeploymentRequest'),
('Submitted',               'DeploymentRequest'),
('TechLead Approved',       'DeploymentRequest'),
('QA Approved',             'DeploymentRequest'),
('Deployed',                'DeploymentRequest'),
('Rejected',                'DeploymentRequest'),
('Returned For Changes',    'DeploymentRequest'),
('Rolled Back',             'DeploymentRequest'),
('Cancelled',               'DeploymentRequest');

-- ApprovalWorkflow
INSERT INTO ApprovalWorkflow (RoleId, ApprovalOrder) VALUES
(2, 1),  -- TechLead
(3, 2),  -- QA
(4, 3);  -- DevOps

-- Test users. Replace HASH_HERE with a locally generated password hash before use.
INSERT INTO Users (EmployeeCode, FirstName, LastName, Email, PasswordHash, RoleId) VALUES
('EMP001', 'Oreo',  'Dev',     'dev@lnt.com',     'HASH_HERE', 1),
('EMP002', 'Arjun', 'TL',      'tl@lnt.com',      'HASH_HERE', 2),
('EMP003', 'Priya', 'QA',      'qa@lnt.com',      'HASH_HERE', 3),
('EMP004', 'Rahul', 'DevOps',  'devops@lnt.com',  'HASH_HERE', 4),
('EMP005', 'Admin', 'User',    'admin@lnt.com',   'HASH_HERE', 5);

-- Test Project
INSERT INTO Projects (ProjectCode, ProjectName, ProjectDescription, TechLeadId) VALUES
('DRMS', 'Deployment Request Management System', 'Capstone internship project', 2),
('ETMS', 'Employee Transfer Management System',  'Reference project',           2);


--check if all tables exist
USE DRMS;
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;


--check seed data
SELECT * FROM Roles;
SELECT * FROM Environments;
SELECT * FROM DeploymentTypes;
SELECT * FROM StatusMaster;
SELECT * FROM ApprovalWorkflow;
SELECT * FROM Users;
SELECT * FROM Projects;

--check if all foreign keys are wired correctly
SELECT 
    fk.name AS ForeignKey,
    tp.name AS ParentTable,
    tr.name AS ReferencedTable
FROM sys.foreign_keys fk
JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
ORDER BY ParentTable;
