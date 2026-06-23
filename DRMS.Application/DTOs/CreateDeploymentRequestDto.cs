using System.ComponentModel.DataAnnotations;

namespace DRMS.Application.DTOs;

public class CreateDeploymentRequestDto
{
    [Required(ErrorMessage = "Project is required")]
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "Source branch is required")]
    [StringLength(100)]
    public string SourceBranch { get; set; } = string.Empty;

    [Required(ErrorMessage = "Target environment is required")]
    public int TargetEnvironmentId { get; set; }

    [Required(ErrorMessage = "Deployment type is required")]
    public int DeploymentTypeId { get; set; }

    [StringLength(50)]
    public string? BuildVersion { get; set; }

    [Required(ErrorMessage = "Change summary is required")]
    public string ChangeSummary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rollback plan is required")]
    public string RollbackPlan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Requested deployment date is required")]
    public DateTime RequestedDeploymentDate { get; set; }
}
