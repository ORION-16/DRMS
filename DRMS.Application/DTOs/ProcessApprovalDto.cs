using System.ComponentModel.DataAnnotations;

namespace DRMS.Application.DTOs;

public class ProcessApprovalDto
{
    [Required]
    public int DeploymentRequestId { get; set; }

    [Required(ErrorMessage = "Action is required")]
    public string ActionTaken { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Remarks { get; set; }
}
