USE DRMS;
GO

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

CREATE PROCEDURE sp_UpdateLastLogin
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET LastLoginDate = GETDATE() WHERE UserId = @UserId;
END
GO

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

-- =============================================
-- Procedure: sp_GetMaxDeployedEnvironmentSequence
-- Returns the highest SequenceOrder of Environments
-- where this project has a successfully Deployed request.
-- Returns 0 if no deployments have ever reached Deployed status.
-- Used by the service layer to enforce Dev -> Staging -> Production promotion order.
-- =============================================
CREATE PROCEDURE sp_GetMaxDeployedEnvironmentSequence
    @ProjectId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ISNULL(MAX(e.SequenceOrder), 0) AS MaxSequenceOrder
    FROM DeploymentRequests dr
    INNER JOIN Environments e ON dr.TargetEnvironmentId = e.EnvironmentId
    INNER JOIN StatusMaster sm ON dr.CurrentStatusId = sm.StatusId
    WHERE dr.ProjectId = @ProjectId
      AND dr.IsActive = 1
      AND sm.StatusName = 'Deployed';
END
GO

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

