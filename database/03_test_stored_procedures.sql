USE DRMS;
GO

PRINT 'SP 1 - sp_LoginUser';
EXEC sp_LoginUser @Email = 'dev@lnt.com', @PasswordHash = 'PLACEHOLDER';
GO

PRINT 'SP 2 - sp_UpdateLastLogin';
EXEC sp_UpdateLastLogin @UserId = 1;
SELECT UserId, Email, LastLoginDate FROM Users WHERE UserId = 1;
GO

PRINT 'SP 3 - sp_CreateDeploymentRequest';
DECLARE @Created TABLE (DeploymentRequestId INT, RequestNumber VARCHAR(30));
INSERT INTO @Created
EXEC sp_CreateDeploymentRequest
    @ProjectId = 1,
    @RequestedBy = 1,
    @SourceBranch = 'feature/workflow-test',
    @TargetEnvironmentId = 2,
    @DeploymentTypeId = 1,
    @BuildVersion = '1.0.0-test',
    @ChangeSummary = 'Stored procedure workflow test request',
    @RollbackPlan = 'Rollback to previous build',
    @RequestedDeploymentDate = '2026-06-23T10:00:00';
SELECT * FROM @Created;
GO

PRINT 'SP 4 - sp_GetRequestsByUser';
EXEC sp_GetRequestsByUser @UserId = 1;
GO

PRINT 'SP 5 - sp_GetRequestById';
DECLARE @RequestIdForDetails INT = (SELECT MIN(DeploymentRequestId) FROM DeploymentRequests WHERE SourceBranch = 'feature/workflow-test');
EXEC sp_GetRequestById @DeploymentRequestId = @RequestIdForDetails;
GO

PRINT 'SP 6 - sp_CancelRequest';
DECLARE @CancelCreated TABLE (DeploymentRequestId INT, RequestNumber VARCHAR(30));
INSERT INTO @CancelCreated
EXEC sp_CreateDeploymentRequest
    @ProjectId = 1,
    @RequestedBy = 1,
    @SourceBranch = 'feature/cancel-test',
    @TargetEnvironmentId = 2,
    @DeploymentTypeId = 2,
    @BuildVersion = '1.0.1-cancel-test',
    @ChangeSummary = 'Cancel stored procedure test request',
    @RollbackPlan = 'No-op rollback',
    @RequestedDeploymentDate = '2026-06-24T10:00:00';
DECLARE @CancelRequestId INT = (SELECT DeploymentRequestId FROM @CancelCreated);
EXEC sp_CancelRequest @DeploymentRequestId = @CancelRequestId, @RequestedBy = 1;
GO

PRINT 'SP 7 - sp_GetPendingApprovalsForUser';
EXEC sp_GetPendingApprovalsForUser @ApproverId = 2;
GO

PRINT 'SP 8 - sp_ProcessApproval';
DECLARE @ApprovalRequestId INT = (SELECT MIN(DeploymentRequestId) FROM DeploymentRequests WHERE SourceBranch = 'feature/workflow-test');
EXEC sp_ProcessApproval @DeploymentRequestId = @ApprovalRequestId, @ApproverId = 2, @ActionTaken = 'Approved', @Remarks = 'TechLead SQL test approval';
GO

PRINT 'SP 9 - sp_GetAllRequests';
EXEC sp_GetAllRequests @StatusId = NULL, @ProjectId = NULL;
GO

PRINT 'SP 10 - sp_GetDeploymentHistory';
DECLARE @HistoryRequestId INT = (SELECT MIN(DeploymentRequestId) FROM DeploymentRequests WHERE SourceBranch = 'feature/workflow-test');
EXEC sp_GetDeploymentHistory @DeploymentRequestId = @HistoryRequestId;
GO

PRINT 'SP 11 - sp_GetDashboardSummary';
EXEC sp_GetDashboardSummary @UserId = 1, @RoleId = 1;
EXEC sp_GetDashboardSummary @UserId = 5, @RoleId = 5;
GO

PRINT 'SP 12 - sp_GetProjects';
EXEC sp_GetProjects;
GO

PRINT 'SP 13 - sp_GetEnvironments';
EXEC sp_GetEnvironments;
GO

PRINT 'SP 14 - sp_GetDeploymentTypes';
EXEC sp_GetDeploymentTypes;
GO

PRINT 'SP 15 - sp_GetStatuses';
EXEC sp_GetStatuses;
GO

PRINT 'Verify All SPs';
SELECT name AS StoredProcedure FROM sys.objects WHERE type = 'P' ORDER BY name;
GO
