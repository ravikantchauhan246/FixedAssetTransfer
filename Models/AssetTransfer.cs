// Models/AssetTransfer.cs
public enum TransferStatus
{
    Draft,
    PendingManagerApproval,
    PendingAccountantUpdate,
    PendingRecipientApproval,
    PendingRecipientManagerApproval,
    PendingFinanceControllerApproval,
    Approved,
    Rejected
}

public class AssetTransfer
{
    public int Id { get; set; }
    public string RequestorId { get; set; } = string.Empty;
    public ApplicationUser Requestor { get; set; } = null!;
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public string AssetDescription { get; set; } = string.Empty;
    public string AssetTagNumber { get; set; } = string.Empty;
    public string CurrentLocation { get; set; } = string.Empty;
    public string CurrentCostCenter { get; set; } = string.Empty;
    public string CurrentOwner { get; set; } = string.Empty;
    
    // Transfer details
    public string NewLocation { get; set; } = string.Empty;
    public string NewCostCenter { get; set; } = string.Empty;
    public string NewOwner { get; set; } = string.Empty;
    public string PurposeOfTransfer { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    
    // Financial details (filled by accountant)
    public decimal? Cost { get; set; }
    public decimal? NetBookValue { get; set; }
    public DateTime? AccountantSignOffDate { get; set; }
    public string? AccountantId { get; set; }
    public ApplicationUser? Accountant { get; set; }
    
    // Recipient details
    public string? RecipientId { get; set; }
    public ApplicationUser? Recipient { get; set; }
    
    // Approval workflow
    public TransferStatus Status { get; set; } = TransferStatus.Draft;
    public DateTime? ManagerApprovalDate { get; set; }
    public DateTime? RecipientApprovalDate { get; set; }
    public DateTime? RecipientManagerApprovalDate { get; set; }
    public DateTime? FinanceControllerApprovalDate { get; set; }
    
    public string? RejectionReason { get; set; }
    public string? RejectedById { get; set; }
    public ApplicationUser? RejectedBy { get; set; }
}