
---

You are helping me build a **Deployment Request Management System (DRMS)** — an enterprise-grade internal web application built with **ASP.NET Core MVC (.NET 8), Dapper, and SQL Server**. The architecture follows **Onion Architecture** with 4 layers: Domain, Application, Infrastructure, and Web.

---

## GROUND RULES

1. Never skip steps — complete each one fully before moving to the next
2. Never assume — if something is unclear, ask before proceeding
3. Never generate code for a layer until the previous layer is verified working
4. Always use stored procedures — never write raw SQL inside C# code
5. Always use Dapper — never use Entity Framework
6. Every stored procedure must be tested in SQL before being called from C#
7. Use soft deletes only — never hard delete any record
8. Apply role-based authorization from day one — never as an afterthought
9. Lock the database before writing any C# — no schema changes after Step 2
10. Commit to Git after every working feature

---

## TECH STACK

- Language: C#
- Framework: ASP.NET Core MVC (.NET 8)
- ORM: Dapper
- Database: SQL Server (running in Docker on Mac)
- Architecture: Onion Architecture
- Auth: ASP.NET Core Cookie Authentication
- IDE: VS Code on Mac
- Git: Yes

---

## DATABASE (Already designed and approved — do not change schema)

**10 Tables:**

Master Tables: Roles, Users, Projects, Environments, DeploymentTypes

Support Tables: StatusMaster, ApprovalWorkflow

Transaction Tables: DeploymentRequests, DeploymentApprovals, DeploymentHistory

**Key design decisions:**
- `CurrentApprovalLevel` in DeploymentRequests tracks where in the chain a request sits (0=Submitted, 1=TechLead done, 2=QA done, 3=DevOps done)
- `DeploymentHistory` is immutable — insert only, never update or delete
- All FKs use NO ACTION — no cascade deletes
- Soft deletes via IsActive flag

**Roles and their approval levels (from ApprovalWorkflow table):**
- TechLead = ApprovalOrder 1
- QA = ApprovalOrder 2
- DevOps = ApprovalOrder 3

**Status flow:**
Draft → Submitted → TechLead Approved → QA Approved → Deployed → (optionally) Rolled Back
At any point: Rejected or Returned For Changes

---

## STEP 1 — DATABASE SETUP

Create the database and run all scripts in this exact order:

### 1A — Create Database
```sql
CREATE DATABASE DRMS;
GO
USE DRMS;
GO
```

### 1B — Master Tables
```sql
CREATE TABLE Roles (
    RoleId      INT PRIMARY KEY IDENTITY(1,1),
    RoleName    VARCHAR(50) NOT NULL UNIQUE,
    IsActive    BIT DEFAULT 1
);

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

CREATE TABLE Projects (
    ProjectId           INT PRIMARY KEY IDENTITY(1,1),
    ProjectCode         VARCHAR(20) NOT NULL UNIQUE,
    ProjectName         VARCHAR(100) NOT NULL,
    ProjectDescription  VARCHAR(500) NULL,
    TechLeadId          INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    IsActive            BIT DEFAULT 1,
    CreatedDate         DATETIME DEFAULT GETDATE()
);

CREATE TABLE Environments (
    EnvironmentId   INT PRIMARY KEY IDENTITY(1,1),
    EnvironmentName VARCHAR(50) NOT NULL,
    SequenceOrder   INT NOT NULL,
    IsActive        BIT DEFAULT 1
);

CREATE TABLE DeploymentTypes (
    DeploymentTypeId    INT PRIMARY KEY IDENTITY(1,1),
    DeploymentTypeName  VARCHAR(50) NOT NULL,
    IsActive            BIT DEFAULT 1
);
```

### 1C — Support Tables
```sql
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
```

### 1D — Transaction Tables
```sql
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
```

### 1E — Seed Data
```sql
INSERT INTO Roles (RoleName) VALUES
('Developer'), ('TechLead'), ('QA'), ('DevOps'), ('Admin');

INSERT INTO Environments (EnvironmentName, SequenceOrder) VALUES
('Development', 1), ('Staging', 2), ('Production', 3);

INSERT INTO DeploymentTypes (DeploymentTypeName) VALUES
('Feature Release'), ('Bug Fix'), ('Hotfix'), ('Patch');

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

INSERT INTO ApprovalWorkflow (RoleId, ApprovalOrder) VALUES
(2, 1),
(3, 2),
(4, 3);

INSERT INTO Users (EmployeeCode, FirstName, LastName, Email, PasswordHash, RoleId) VALUES
('EMP001', 'Dev',    'User',   'dev@lnt.com',    'PLACEHOLDER', 1),
('EMP002', 'Tech',   'Lead',   'tl@lnt.com',     'PLACEHOLDER', 2),
('EMP003', 'QA',     'User',   'qa@lnt.com',     'PLACEHOLDER', 3),
('EMP004', 'DevOps', 'User',   'devops@lnt.com', 'PLACEHOLDER', 4),
('EMP005', 'Admin',  'User',   'admin@lnt.com',  'PLACEHOLDER', 5);

INSERT INTO Projects (ProjectCode, ProjectName, ProjectDescription, TechLeadId) VALUES
('DRMS', 'Deployment Request Management System', 'Capstone internship project', 2),
('ETMS', 'Employee Transfer Management System',  'Reference project', 2);
```

### 1F — Verify Database
```sql
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;
SELECT * FROM Roles;
SELECT * FROM StatusMaster;
SELECT * FROM ApprovalWorkflow;
SELECT * FROM Users;
```

**STOP. Do not proceed until all 10 tables exist and seed data is confirmed.**

---

## STEP 2 — STORED PROCEDURES

Run all stored procedures in this exact order. Test each one after creating it.

### SP 1 — Login
```sql
CREATE PROCEDURE sp_LoginUser
    @Email          VARCHAR(100),
    @PasswordHash   VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        u.UserId, u.EmployeeCode, u.FirstName, u.LastName,
        u.Email, u.RoleId, r.RoleName, u.IsActive
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE u.Email = @Email
      AND u.PasswordHash = @PasswordHash
      AND u.IsActive = 1;
END
GO
```

### SP 2 — Update Last Login
```sql
CREATE PROCEDURE sp_UpdateLastLogin
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET LastLoginDate = GETDATE() WHERE UserId = @UserId;
END
GO
```

### SP 3 — Create Deployment Request
```sql
CREATE PROCEDURE sp_CreateDeploymentRequest
    @ProjectId                  INT,
    @RequestedBy                INT,
    @SourceBranch               VARCHAR(100),
    @TargetEnvironmentId        INT,
    @DeploymentTypeId           INT,
    @BuildVersion               VARCHAR(50),
    @ChangeSummary              VARCHAR(MAX),
    @RollbackPlan               VARCHAR(MAX),
    @RequestedDeploymentDate    DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RequestNumber VARCHAR(30);
    DECLARE @TodayCount INT;

    SELECT @TodayCount = COUNT(*) + 1
    FROM DeploymentRequests
    WHERE CAST(RequestedDate AS DATE) = CAST(GETDATE() AS DATE);

    SET @RequestNumber = 'DRMS-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + RIGHT('0000' + CAST(@TodayCount AS VARCHAR), 4);

    DECLARE @StatusId INT;
    SELECT @StatusId = StatusId FROM StatusMaster WHERE StatusName = 'Submitted';

    INSERT INTO DeploymentRequests (
        RequestNumber, ProjectId, RequestedBy, SourceBranch,
        TargetEnvironmentId, DeploymentTypeId, BuildVersion,
        ChangeSummary, RollbackPlan, RequestedDeploymentDate,
        CurrentStatusId, CurrentApprovalLevel
    )
    VALUES (
        @RequestNumber, @ProjectId, @RequestedBy, @SourceBranch,
        @TargetEnvironmentId, @DeploymentTypeId, @BuildVersion,
        @ChangeSummary, @RollbackPlan, @RequestedDeploymentDate,
        @StatusId, 0
    );

    DECLARE @NewRequestId INT = SCOPE_IDENTITY();

    INSERT INTO DeploymentHistory (DeploymentRequestId, OldStatusId, NewStatusId, ActionBy, Remarks)
    VALUES (@NewRequestId, @StatusId, @StatusId, @RequestedBy, 'Request submitted');

    SELECT @NewRequestId AS DeploymentRequestId, @RequestNumber AS RequestNumber;
END
GO
```

### SP 4 — Get Requests By User
```sql
CREATE PROCEDURE sp_GetRequestsByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        dr.DeploymentRequestId, dr.RequestNumber,
        p.ProjectName, p.ProjectCode,
        e.EnvironmentName, dt.DeploymentTypeName,
        dr.SourceBranch, dr.BuildVersion,
        dr.RequestedDeploymentDate, dr.RequestedDate,
        sm.StatusName AS CurrentStatus,
        dr.CurrentApprovalLevel, dr.IsActive
    FROM DeploymentRequests dr
    INNER JOIN Projects p       ON dr.ProjectId = p.ProjectId
    INNER JOIN Environments e   ON dr.TargetEnvironmentId = e.EnvironmentId
    INNER JOIN DeploymentTypes dt ON dr.DeploymentTypeId = dt.DeploymentTypeId
    INNER JOIN StatusMaster sm  ON dr.CurrentStatusId = sm.StatusId
    WHERE dr.RequestedBy = @UserId AND dr.IsActive = 1
    ORDER BY dr.RequestedDate DESC;
END
GO
```

### SP 5 — Get Request By Id
```sql
CREATE PROCEDURE sp_GetRequestById
    @DeploymentRequestId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        dr.DeploymentRequestId, dr.RequestNumber,
        dr.SourceBranch, dr.BuildVersion,
        dr.ChangeSummary, dr.RollbackPlan,
        dr.RequestedDeploymentDate, dr.RequestedDate,
        dr.CurrentApprovalLevel,
        p.ProjectName, p.ProjectCode,
        e.EnvironmentName, dt.DeploymentTypeName,
        sm.StatusName AS CurrentStatus,
        u.FirstName + ' ' + u.LastName AS RequestedByName,
        u.EmployeeCode
    FROM DeploymentRequests dr
    INNER JOIN Projects p           ON dr.ProjectId = p.ProjectId
    INNER JOIN Environments e       ON dr.TargetEnvironmentId = e.EnvironmentId
    INNER JOIN DeploymentTypes dt   ON dr.DeploymentTypeId = dt.DeploymentTypeId
    INNER JOIN StatusMaster sm      ON dr.CurrentStatusId = sm.StatusId
    INNER JOIN Users u              ON dr.RequestedBy = u.UserId
    WHERE dr.DeploymentRequestId = @DeploymentRequestId;

    SELECT 
        da.ApprovalLevel, da.ActionTaken, da.Remarks, da.ActionDate,
        u.FirstName + ' ' + u.LastName AS ApproverName,
        r.RoleName AS ApproverRole
    FROM DeploymentApprovals da
    INNER JOIN Users u ON da.ApproverId = u.UserId
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE da.DeploymentRequestId = @DeploymentRequestId
    ORDER BY da.ApprovalLevel ASC;
END
GO
```

### SP 6 — Cancel Request
```sql
CREATE PROCEDURE sp_CancelRequest
    @DeploymentRequestId    INT,
    @RequestedBy            INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentStatus VARCHAR(100);
    DECLARE @CurrentStatusId INT;

    SELECT @CurrentStatus = sm.StatusName, @CurrentStatusId = dr.CurrentStatusId
    FROM DeploymentRequests dr
    INNER JOIN StatusMaster sm ON dr.CurrentStatusId = sm.StatusId
    WHERE dr.DeploymentRequestId = @DeploymentRequestId AND dr.RequestedBy = @RequestedBy;

    IF @CurrentStatus NOT IN ('Submitted', 'Returned For Changes')
    BEGIN
        SELECT 0 AS Success, 'Cannot cancel a request already in approval' AS Message;
        RETURN;
    END

    DECLARE @CancelledStatusId INT;
    SELECT @CancelledStatusId = StatusId FROM StatusMaster WHERE StatusName = 'Cancelled';

    UPDATE DeploymentRequests
    SET CurrentStatusId = @CancelledStatusId, IsActive = 0
    WHERE DeploymentRequestId = @DeploymentRequestId;

    INSERT INTO DeploymentHistory (DeploymentRequestId, OldStatusId, NewStatusId, ActionBy, Remarks)
    VALUES (@DeploymentRequestId, @CurrentStatusId, @CancelledStatusId, @RequestedBy, 'Request cancelled by developer');

    SELECT 1 AS Success, 'Request cancelled successfully' AS Message;
END
GO
```

### SP 7 — Get Pending Approvals For User
```sql
CREATE PROCEDURE sp_GetPendingApprovalsForUser
    @ApproverId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RoleId INT;
    DECLARE @ApprovalLevel INT;

    SELECT @RoleId = u.RoleId FROM Users u WHERE u.UserId = @ApproverId;
    SELECT @ApprovalLevel = ApprovalOrder FROM ApprovalWorkflow WHERE RoleId = @RoleId AND IsActive = 1;

    SELECT 
        dr.DeploymentRequestId, dr.RequestNumber,
        p.ProjectName, p.ProjectCode,
        e.EnvironmentName, dt.DeploymentTypeName,
        dr.SourceBranch, dr.BuildVersion,
        dr.RequestedDeploymentDate, dr.RequestedDate,
        sm.StatusName AS CurrentStatus,
        u.FirstName + ' ' + u.LastName AS RequestedByName,
        dr.CurrentApprovalLevel
    FROM DeploymentRequests dr
    INNER JOIN Projects p           ON dr.ProjectId = p.ProjectId
    INNER JOIN Environments e       ON dr.TargetEnvironmentId = e.EnvironmentId
    INNER JOIN DeploymentTypes dt   ON dr.DeploymentTypeId = dt.DeploymentTypeId
    INNER JOIN StatusMaster sm      ON dr.CurrentStatusId = sm.StatusId
    INNER JOIN Users u              ON dr.RequestedBy = u.UserId
    WHERE dr.CurrentApprovalLevel = @ApprovalLevel - 1
      AND dr.IsActive = 1
      AND sm.StatusName NOT IN ('Deployed', 'Rejected', 'Cancelled', 'Rolled Back')
    ORDER BY dr.RequestedDate ASC;
END
GO
```

### SP 8 — Process Approval
```sql
CREATE PROCEDURE sp_ProcessApproval
    @DeploymentRequestId    INT,
    @ApproverId             INT,
    @ActionTaken            VARCHAR(50),
    @Remarks                VARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @RoleId INT;
        DECLARE @ApprovalLevel INT;

        SELECT @RoleId = u.RoleId FROM Users u WHERE u.UserId = @ApproverId;
        SELECT @ApprovalLevel = ApprovalOrder FROM ApprovalWorkflow WHERE RoleId = @RoleId AND IsActive = 1;

        DECLARE @CurrentStatusId INT;
        DECLARE @CurrentApprovalLevel INT;

        SELECT @CurrentStatusId = CurrentStatusId, @CurrentApprovalLevel = CurrentApprovalLevel
        FROM DeploymentRequests WHERE DeploymentRequestId = @DeploymentRequestId;

        IF @CurrentApprovalLevel != @ApprovalLevel - 1
        BEGIN
            SELECT 0 AS Success, 'Not your turn to approve this request' AS Message;
            ROLLBACK;
            RETURN;
        END

        DECLARE @NewStatusId INT;
        DECLARE @NewApprovalLevel INT = @CurrentApprovalLevel;

        IF @ActionTaken = 'Approved'
        BEGIN
            SET @NewApprovalLevel = @CurrentApprovalLevel + 1;
            IF @ApprovalLevel = 1
                SELECT @NewStatusId = StatusId FROM StatusMaster WHERE StatusName = 'TechLead Approved';
            ELSE IF @ApprovalLevel = 2
                SELECT @NewStatusId = StatusId FROM StatusMaster WHERE StatusName = 'QA Approved';
            ELSE IF @ApprovalLevel = 3
                SELECT @NewStatusId = StatusId FROM StatusMaster WHERE StatusName = 'Deployed';
        END
        ELSE IF @ActionTaken = 'Rejected'
        BEGIN
            SELECT @NewStatusId = StatusId FROM StatusMaster WHERE StatusName = 'Rejected';
        END
        ELSE IF @ActionTaken = 'ReturnedForChanges'
        BEGIN
            SET @NewApprovalLevel = 0;
            SELECT @NewStatusId = StatusId FROM StatusMaster WHERE StatusName = 'Returned For Changes';
        END

        INSERT INTO DeploymentApprovals (DeploymentRequestId, ApproverId, ApprovalLevel, ActionTaken, Remarks)
        VALUES (@DeploymentRequestId, @ApproverId, @ApprovalLevel, @ActionTaken, @Remarks);

        UPDATE DeploymentRequests
        SET CurrentStatusId = @NewStatusId, CurrentApprovalLevel = @NewApprovalLevel
        WHERE DeploymentRequestId = @DeploymentRequestId;

        INSERT INTO DeploymentHistory (DeploymentRequestId, OldStatusId, NewStatusId, ActionBy, Remarks)
        VALUES (@DeploymentRequestId, @CurrentStatusId, @NewStatusId, @ApproverId, @Remarks);

        COMMIT;
        SELECT 1 AS Success, 'Action processed successfully' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO
```

### SP 9 — Get All Requests (Admin)
```sql
CREATE PROCEDURE sp_GetAllRequests
    @StatusId   INT = NULL,
    @ProjectId  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        dr.DeploymentRequestId, dr.RequestNumber,
        p.ProjectName, e.EnvironmentName,
        dt.DeploymentTypeName, dr.SourceBranch,
        dr.RequestedDeploymentDate, dr.RequestedDate,
        sm.StatusName AS CurrentStatus,
        dr.CurrentApprovalLevel,
        u.FirstName + ' ' + u.LastName AS RequestedByName
    FROM DeploymentRequests dr
    INNER JOIN Projects p           ON dr.ProjectId = p.ProjectId
    INNER JOIN Environments e       ON dr.TargetEnvironmentId = e.EnvironmentId
    INNER JOIN DeploymentTypes dt   ON dr.DeploymentTypeId = dt.DeploymentTypeId
    INNER JOIN StatusMaster sm      ON dr.CurrentStatusId = sm.StatusId
    INNER JOIN Users u              ON dr.RequestedBy = u.UserId
    WHERE dr.IsActive = 1
      AND (@StatusId IS NULL OR dr.CurrentStatusId = @StatusId)
      AND (@ProjectId IS NULL OR dr.ProjectId = @ProjectId)
    ORDER BY dr.RequestedDate DESC;
END
GO
```

### SP 10 — Get Deployment History
```sql
CREATE PROCEDURE sp_GetDeploymentHistory
    @DeploymentRequestId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        dh.HistoryId, dh.ActionDate, dh.Remarks,
        sm_old.StatusName AS OldStatus,
        sm_new.StatusName AS NewStatus,
        u.FirstName + ' ' + u.LastName AS ActionByName,
        r.RoleName AS ActionByRole
    FROM DeploymentHistory dh
    INNER JOIN StatusMaster sm_old  ON dh.OldStatusId = sm_old.StatusId
    INNER JOIN StatusMaster sm_new  ON dh.NewStatusId = sm_new.StatusId
    INNER JOIN Users u              ON dh.ActionBy = u.UserId
    INNER JOIN Roles r              ON u.RoleId = r.RoleId
    WHERE dh.DeploymentRequestId = @DeploymentRequestId
    ORDER BY dh.ActionDate ASC;
END
GO
```

### SP 11 — Dashboard Summary
```sql
CREATE PROCEDURE sp_GetDashboardSummary
    @UserId INT,
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        COUNT(CASE WHEN sm.StatusName = 'Submitted'         THEN 1 END) AS PendingCount,
        COUNT(CASE WHEN sm.StatusName = 'TechLead Approved' THEN 1 END) AS TechLeadApprovedCount,
        COUNT(CASE WHEN sm.StatusName = 'QA Approved'       THEN 1 END) AS QAApprovedCount,
        COUNT(CASE WHEN sm.StatusName = 'Deployed'          THEN 1 END) AS DeployedCount,
        COUNT(CASE WHEN sm.StatusName = 'Rejected'          THEN 1 END) AS RejectedCount,
        COUNT(CASE WHEN sm.StatusName = 'Rolled Back'       THEN 1 END) AS RolledBackCount,
        COUNT(*) AS TotalCount
    FROM DeploymentRequests dr
    INNER JOIN StatusMaster sm ON dr.CurrentStatusId = sm.StatusId
    WHERE dr.IsActive = 1
      AND (@RoleId = 5 OR dr.RequestedBy = @UserId);
END
GO
```

### SP 12 to 15 — Master Data Dropdowns
```sql
CREATE PROCEDURE sp_GetProjects
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProjectId, ProjectCode, ProjectName FROM Projects WHERE IsActive = 1 ORDER BY ProjectName;
END
GO

CREATE PROCEDURE sp_GetEnvironments
AS
BEGIN
    SET NOCOUNT ON;
    SELECT EnvironmentId, EnvironmentName, SequenceOrder FROM Environments WHERE IsActive = 1 ORDER BY SequenceOrder;
END
GO

CREATE PROCEDURE sp_GetDeploymentTypes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DeploymentTypeId, DeploymentTypeName FROM DeploymentTypes WHERE IsActive = 1;
END
GO

CREATE PROCEDURE sp_GetStatuses
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StatusId, StatusName FROM StatusMaster WHERE IsActive = 1 ORDER BY StatusId;
END
GO
```

### Verify All SPs
```sql
SELECT name AS StoredProcedure FROM sys.objects WHERE type = 'P' ORDER BY name;
```

**STOP. You must see all 15 stored procedures before proceeding.**

---

## STEP 3 — SCAFFOLD .NET CORE MVC SOLUTION

Create the solution with Onion Architecture — 4 projects:

```bash
mkdir DRMS && cd DRMS
dotnet new sln -n DRMS
dotnet new classlib -n DRMS.Domain
dotnet new classlib -n DRMS.Application
dotnet new classlib -n DRMS.Infrastructure
dotnet new mvc -n DRMS.Web
dotnet sln add DRMS.Domain/DRMS.Domain.csproj
dotnet sln add DRMS.Application/DRMS.Application.csproj
dotnet sln add DRMS.Infrastructure/DRMS.Infrastructure.csproj
dotnet sln add DRMS.Web/DRMS.Web.csproj
dotnet add DRMS.Application/DRMS.Application.csproj reference DRMS.Domain/DRMS.Domain.csproj
dotnet add DRMS.Infrastructure/DRMS.Infrastructure.csproj reference DRMS.Domain/DRMS.Domain.csproj
dotnet add DRMS.Web/DRMS.Web.csproj reference DRMS.Application/DRMS.Application.csproj
dotnet add DRMS.Web/DRMS.Web.csproj reference DRMS.Infrastructure/DRMS.Infrastructure.csproj
```

Install NuGet packages:
```bash
dotnet add DRMS.Infrastructure/DRMS.Infrastructure.csproj package Dapper
dotnet add DRMS.Infrastructure/DRMS.Infrastructure.csproj package Microsoft.Data.SqlClient
dotnet add DRMS.Web/DRMS.Web.csproj package BCrypt.Net-Next
```

**STOP. Run `dotnet build` — fix all errors before proceeding.**

---

## STEP 4 — DOMAIN LAYER

Create these files inside `DRMS.Domain`:

Delete the default `Class1.cs` file first.

Create folder structure:
```
DRMS.Domain/
    Entities/
        Role.cs
        User.cs
        Project.cs
        Environment.cs
        DeploymentType.cs
        StatusMaster.cs
        ApprovalWorkflow.cs
        DeploymentRequest.cs
        DeploymentApproval.cs
        DeploymentHistory.cs
    Interfaces/
        IUserRepository.cs
        IProjectRepository.cs
        IDeploymentRequestRepository.cs
        IDeploymentApprovalRepository.cs
        IDeploymentHistoryRepository.cs
        IMasterDataRepository.cs
```

Each entity must exactly match the database columns. Each interface must define methods that map to the stored procedures.

**STOP. Run `dotnet build` — Domain layer must compile cleanly.**

---

## STEP 5 — APPLICATION LAYER

Create these files inside `DRMS.Application`:

```
DRMS.Application/
    DTOs/
        UserDto.cs
        LoginDto.cs
        ProjectDto.cs
        DeploymentRequestDto.cs
        CreateDeploymentRequestDto.cs
        DeploymentApprovalDto.cs
        ProcessApprovalDto.cs
        DashboardSummaryDto.cs
        DeploymentHistoryDto.cs
    Services/
        Interfaces/
            IAuthService.cs
            IDeploymentRequestService.cs
            IApprovalService.cs
            IMasterDataService.cs
        Implementations/
            AuthService.cs
            DeploymentRequestService.cs
            ApprovalService.cs
            MasterDataService.cs
```

Rules:
- Services depend only on repository interfaces from Domain layer
- DTOs are flat objects — no nested entities
- No SQL, no Dapper, no HttpContext in this layer

**STOP. Run `dotnet build` — Application layer must compile cleanly.**

---

## STEP 6 — INFRASTRUCTURE LAYER

Create these files inside `DRMS.Infrastructure`:

```
DRMS.Infrastructure/
    Data/
        DapperContext.cs
    Repositories/
        UserRepository.cs
        ProjectRepository.cs
        DeploymentRequestRepository.cs
        DeploymentApprovalRepository.cs
        DeploymentHistoryRepository.cs
        MasterDataRepository.cs
```

Rules:
- `DapperContext.cs` creates `IDbConnection` from connection string
- Every repository implements its interface from Domain layer
- Every method calls exactly one stored procedure using Dapper
- Use `CommandType.StoredProcedure` for every call
- Use `QueryMultipleAsync` for SPs that return multiple result sets (sp_GetRequestById)
- Never write inline SQL

Connection string format for `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=DRMS;User Id=sa;Password=YOUR_LOCAL_PASSWORD;TrustServerCertificate=True;"
}
```

**STOP. Run `dotnet build` — Infrastructure layer must compile cleanly.**

---

## STEP 7 — WEB LAYER SETUP

Configure `Program.cs` in `DRMS.Web`:

Register services in this order:
1. DapperContext as singleton
2. All repositories (scoped)
3. All services (scoped)
4. Cookie authentication with login path `/Account/Login`
5. Authorization
6. Session (optional)

Password hashing: use BCrypt for hashing and verifying passwords. Update the placeholder PasswordHash in seed data with a real BCrypt hash before testing login.

Update seed data passwords:
```sql
UPDATE Users SET PasswordHash = '$2a$11$YOUR_BCRYPT_HASH_HERE' WHERE Email = 'dev@lnt.com';
```

Generate the hash in C# first, then update all 5 users.

**STOP. Run `dotnet run` from DRMS.Web — app must start without errors.**

---

## STEP 8 — CONTROLLERS AND VIEWS

Build controllers and views in this exact order. Do not build the next controller until the current one is working end to end.

### 8A — AccountController
- GET /Account/Login → show login form
- POST /Account/Login → authenticate, set cookie, redirect by role
- GET /Account/Logout → clear cookie, redirect to login

Role-based redirect after login:
- Developer → /Dashboard
- TechLead → /Approval/Pending
- QA → /Approval/Pending
- DevOps → /Approval/Pending
- Admin → /Admin/Dashboard

### 8B — DashboardController (Developer)
- GET /Dashboard → show developer's own requests + summary counts
- Authorize: Developer only

### 8C — DeploymentController (Developer)
- GET /Deployment/Create → show new request form with dropdowns
- POST /Deployment/Create → submit request, redirect to dashboard
- GET /Deployment/Details/{id} → show full request detail + approval timeline
- POST /Deployment/Cancel/{id} → cancel request
- Authorize: Developer only

### 8D — ApprovalController (TechLead, QA, DevOps)
- GET /Approval/Pending → show pending requests for logged in approver
- GET /Approval/Details/{id} → show request detail with approve/reject form
- POST /Approval/Process → process approval action
- Authorize: TechLead, QA, DevOps roles

### 8E — AdminController (Admin)
- GET /Admin/Dashboard → all requests with status filter
- GET /Admin/History/{id} → full deployment history for a request
- Authorize: Admin only

**For each controller: build controller first, then its views, then test the full flow before moving to next.**

---

## STEP 9 — VIEWS

For each view use clean Bootstrap 5 layout. No external CSS frameworks other than Bootstrap.

Required layout elements:
- Shared `_Layout.cshtml` with navbar showing logged in user name and role
- Role-aware navbar links (Developer sees different menu than Admin)
- Flash messages for success and error actions
- Table views for request lists with status badges color coded by status

Status badge colors:
- Submitted → blue
- TechLead Approved → cyan
- QA Approved → purple
- Deployed → green
- Rejected → red
- Returned For Changes → orange
- Cancelled → gray
- Rolled Back → dark

---

## STEP 10 — END TO END TESTING

Test the complete workflow in this exact sequence:

1. Login as Developer → raise a new deployment request → verify it appears on dashboard with status Submitted
2. Login as TechLead → verify request appears in pending → approve it → verify status changes to TechLead Approved
3. Login as QA → verify request appears in pending → approve it → verify status changes to QA Approved
4. Login as DevOps → verify request appears in pending → approve it → verify status changes to Deployed
5. Check DeploymentHistory table → verify 4 rows exist for this request
6. Login as Developer → raise another request → login as TechLead → reject it → verify status changes to Rejected
7. Login as Developer → raise another request → login as TechLead → return for changes → verify developer can see it as Returned For Changes
8. Login as Admin → verify all requests visible on admin dashboard
9. Test filter by status on admin dashboard
10. Test cancel flow — developer cancels a Submitted request

**STOP. Every flow above must work before calling the project done.**

---

## WHAT NOT TO BUILD

Do not build any of the following — they are out of scope:
- File upload or attachments
- Email notifications
- Real CI/CD pipeline integration
- GitHub or GitLab integration
- Azure DevOps integration
- Forgot password flow
- User registration (admin creates users via seed data only)
- Dark mode

---

## DEFINITION OF DONE

The project is complete when:
- All 10 tables exist with correct seed data
- All 15 stored procedures work correctly
- All 4 architecture layers compile without errors
- All 5 role-based flows work end to end
- DeploymentHistory logs every status change immutably
- No raw SQL exists anywhere in C# code
- No hardcoded connection strings in source code
- Role-based authorization prevents unauthorized access to any route
- Project runs with `dotnet run` from DRMS.Web with zero errors

---
