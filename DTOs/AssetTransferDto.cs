// DTOs/AssetTransferDto.cs
using System.ComponentModel.DataAnnotations;

public class AssetTransferDto
{
    public int Id { get; set; }
    public string RequestorId { get; set; } = string.Empty;
    public string RequestorName { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string AssetDescription { get; set; } = string.Empty;
    public string AssetTagNumber { get; set; } = string.Empty;
    public string CurrentLocation { get; set; } = string.Empty;
    public string CurrentCostCenter { get; set; } = string.Empty;
    public string CurrentOwner { get; set; } = string.Empty;
    public string NewLocation { get; set; } = string.Empty;
    public string NewCostCenter { get; set; } = string.Empty;
    public string NewOwner { get; set; } = string.Empty;
    public string PurposeOfTransfer { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public decimal? Cost { get; set; }
    public decimal? NetBookValue { get; set; }
    public DateTime? AccountantSignOffDate { get; set; }
    public string AccountantId { get; set; } = string.Empty;
    public string AccountantName { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public TransferStatus Status { get; set; }
    public DateTime? ManagerApprovalDate { get; set; }
    public DateTime? RecipientApprovalDate { get; set; }
    public DateTime? RecipientManagerApprovalDate { get; set; }
    public DateTime? FinanceControllerApprovalDate { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public string RejectedById { get; set; } = string.Empty;
    public string RejectedByName { get; set; } = string.Empty;
}

public class CreateAssetTransferDto
{
    [Required]
    public string AssetDescription { get; set; } = string.Empty;
    
    [Required]
    public string AssetTagNumber { get; set; } = string.Empty;
    
    [Required]
    public string CurrentLocation { get; set; } = string.Empty;
    
    [Required]
    public string CurrentCostCenter { get; set; } = string.Empty;
    
    [Required]
    public string CurrentOwner { get; set; } = string.Empty;
    
    [Required]
    public string NewLocation { get; set; } = string.Empty;
    
    [Required]
    public string NewCostCenter { get; set; } = string.Empty;
    
    [Required]
    public string NewOwner { get; set; } = string.Empty;
    
    [Required]
    public string PurposeOfTransfer { get; set; } = string.Empty;
    
    [Required]
    public string Justification { get; set; } = string.Empty;
    
    public string RecipientId { get; set; } = string.Empty;
}

public class UpdateFinancialDetailsDto
{
    [Required]
    public decimal Cost { get; set; }
    
    [Required]
    public decimal NetBookValue { get; set; }
}

public class ApproveRejectDto
{
    public bool IsApproved { get; set; }
    public string Reason { get; set; } = string.Empty;
}