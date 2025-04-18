// Services/AssetTransferService.cs
using FixedAssetTransfer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class AssetTransferService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssetTransferService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<AssetTransferDto>> GetAllTransfers()
    {
        return await _context.AssetTransfers
            .Include(at => at.Requestor)
            .Include(at => at.Accountant)
            .Include(at => at.Recipient)
            .Include(at => at.RejectedBy)
            .Select(at => MapToDto(at))
            .ToListAsync();
    }

    public async Task<List<AssetTransferDto>> GetTransfersForUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new List<AssetTransferDto>();
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        IQueryable<AssetTransfer> query = _context.AssetTransfers
            .Include(at => at.Requestor)
            .Include(at => at.Accountant)
            .Include(at => at.Recipient)
            .Include(at => at.RejectedBy);

        // Requestor can see their own transfers
        if (userRoles.Contains("Requestor"))
        {
            query = query.Where(at => at.RequestorId == userId);
        }

        // Manager can see transfers from their department that need approval
        if (userRoles.Contains("Manager"))
        {
            var department = user.Department;
            query = query.Where(at => 
                at.Status == TransferStatus.PendingManagerApproval && 
                at.Requestor.Department == department);
        }

        // Accountant can see transfers that need financial details
        if (userRoles.Contains("Accountant"))
        {
            query = query.Where(at => 
                at.Status == TransferStatus.PendingAccountantUpdate);
        }

        // Recipient can see transfers that need their approval
        if (userRoles.Contains("Recipient"))
        {
            query = query.Where(at => 
                at.Status == TransferStatus.PendingRecipientApproval && 
                at.RecipientId == userId);
        }

        // Recipient Manager can see transfers that need their approval
        if (userRoles.Contains("RecipientManager"))
        {
            var recipientManagerDepartment = user.Department;
            query = query.Where(at => 
                at.Status == TransferStatus.PendingRecipientManagerApproval && 
                at.Recipient != null && 
                at.Recipient.Department == recipientManagerDepartment);
        }

        // Finance Controller can see transfers that need their approval
        if (userRoles.Contains("FinanceController"))
        {
            query = query.Where(at => 
                at.Status == TransferStatus.PendingFinanceControllerApproval);
        }

        return await query.Select(at => MapToDto(at)).ToListAsync();
    }

    public async Task<AssetTransferDto?> GetTransferById(int id)
    {
        var transfer = await _context.AssetTransfers
            .Include(at => at.Requestor)
            .Include(at => at.Accountant)
            .Include(at => at.Recipient)
            .Include(at => at.RejectedBy)
            .FirstOrDefaultAsync(at => at.Id == id);

        if (transfer == null)
        {
            return null;
        }

        return MapToDto(transfer);
    }

    public async Task<AssetTransferDto?> CreateTransfer(CreateAssetTransferDto transferDto, string requestorId)
    {
        // Validate requestor exists
        var requestor = await _userManager.FindByIdAsync(requestorId);
        if (requestor == null)
        {
            return null;
        }

        string? validatedRecipientId = null;
        // Validate recipient if specified
        if (!string.IsNullOrEmpty(transferDto.RecipientId))
        {
            var recipient = await _userManager.FindByIdAsync(transferDto.RecipientId);
            if (recipient == null)
            {
                // Recipient doesn't exist
                return null;
            }
            validatedRecipientId = recipient.Id; // Use the validated recipient ID
        }

        var transfer = new AssetTransfer
        {
            RequestorId = requestorId,
            AssetDescription = transferDto.AssetDescription,
            AssetTagNumber = transferDto.AssetTagNumber,
            CurrentLocation = transferDto.CurrentLocation,
            CurrentCostCenter = transferDto.CurrentCostCenter,
            CurrentOwner = transferDto.CurrentOwner,
            NewLocation = transferDto.NewLocation,
            NewCostCenter = transferDto.NewCostCenter,
            NewOwner = transferDto.NewOwner,
            PurposeOfTransfer = transferDto.PurposeOfTransfer,
            Justification = transferDto.Justification,
            RecipientId = validatedRecipientId, // Use the validated ID or null
            Status = TransferStatus.Draft
        };

        _context.AssetTransfers.Add(transfer);
        
        try {
            await _context.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            // Log the exception
            System.Console.WriteLine($"Database error when creating transfer: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }

        return await GetTransferById(transfer.Id);
    }

    public async Task<AssetTransferDto?> SubmitTransfer(int id, string requestorId)
    {
        var transfer = await _context.AssetTransfers.FindAsync(id);
        if (transfer == null || transfer.RequestorId != requestorId)
        {
            return null;
        }

        transfer.Status = TransferStatus.PendingManagerApproval;
        await _context.SaveChangesAsync();

        return await GetTransferById(transfer.Id);
    }

    public async Task<AssetTransferDto?> UpdateFinancialDetails(int id, UpdateFinancialDetailsDto detailsDto, string accountantId)
    {
        var transfer = await _context.AssetTransfers.FindAsync(id);
        if (transfer == null)
        {
            return null;
        }

        transfer.Cost = detailsDto.Cost;
        transfer.NetBookValue = detailsDto.NetBookValue;
        transfer.AccountantId = accountantId;
        transfer.AccountantSignOffDate = DateTime.UtcNow;

        // Determine next status based on purpose
        if (transfer.PurposeOfTransfer == "Transfer to other intercompany/Cost Center/Owner")
        {
            transfer.Status = TransferStatus.PendingRecipientApproval;
        }
        else
        {
            transfer.Status = TransferStatus.PendingFinanceControllerApproval;
        }

        await _context.SaveChangesAsync();

        return await GetTransferById(transfer.Id);
    }

    public async Task<AssetTransferDto?> ManagerApprove(int id, string managerId)
    {
        var transfer = await _context.AssetTransfers.FindAsync(id);
        if (transfer == null)
        {
            return null;
        }

        transfer.Status = TransferStatus.PendingAccountantUpdate;
        transfer.ManagerApprovalDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetTransferById(transfer.Id);
    }

    public async Task<AssetTransferDto?> RecipientApprove(int id, string recipientId)
    {
        var transfer = await _context.AssetTransfers.FindAsync(id);
        if (transfer == null || transfer.RecipientId != recipientId)
        {
            return null;
        }

        transfer.Status = TransferStatus.PendingRecipientManagerApproval;
        transfer.RecipientApprovalDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetTransferById(transfer.Id);
    }

    public async Task<AssetTransferDto?> RecipientManagerApprove(int id, string managerId)
    {
        var transfer = await _context.AssetTransfers.FindAsync(id);
        if (transfer == null)
        {
            return null;
        }

        transfer.Status = TransferStatus.PendingFinanceControllerApproval;
        transfer.RecipientManagerApprovalDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetTransferById(transfer.Id);
    }

    public async Task<AssetTransferDto?> FinanceControllerApprove(int id, string financeControllerId)
    {
        var transfer = await _context.AssetTransfers.FindAsync(id);
        if (transfer == null)
        {
            return null;
        }

        transfer.Status = TransferStatus.Approved;
        transfer.FinanceControllerApprovalDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetTransferById(transfer.Id);
    }

    public async Task<AssetTransferDto?> RejectTransfer(int id, ApproveRejectDto rejectDto, string rejectedById)
    {
        var transfer = await _context.AssetTransfers.FindAsync(id);
        if (transfer == null)
        {
            return null;
        }

        transfer.Status = TransferStatus.Rejected;
        transfer.RejectionReason = rejectDto.Reason;
        transfer.RejectedById = rejectedById;
        await _context.SaveChangesAsync();

        return await GetTransferById(transfer.Id);
    }

    public async Task<bool> CanUserSubmitRequest(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        return userRoles.Contains("Requestor") || userRoles.Contains("Admin");
    }

    private static AssetTransferDto MapToDto(AssetTransfer transfer)
    {
        return new AssetTransferDto
        {
            Id = transfer.Id,
            RequestorId = transfer.RequestorId,
            RequestorName = transfer.Requestor?.FullName ?? string.Empty,
            RequestDate = transfer.RequestDate,
            AssetDescription = transfer.AssetDescription,
            AssetTagNumber = transfer.AssetTagNumber,
            CurrentLocation = transfer.CurrentLocation,
            CurrentCostCenter = transfer.CurrentCostCenter,
            CurrentOwner = transfer.CurrentOwner,
            NewLocation = transfer.NewLocation,
            NewCostCenter = transfer.NewCostCenter,
            NewOwner = transfer.NewOwner,
            PurposeOfTransfer = transfer.PurposeOfTransfer,
            Justification = transfer.Justification,
            Cost = transfer.Cost,
            NetBookValue = transfer.NetBookValue,
            AccountantSignOffDate = transfer.AccountantSignOffDate,
            AccountantId = transfer.AccountantId ?? string.Empty,
            AccountantName = transfer.Accountant?.FullName ?? string.Empty,
            RecipientId = transfer.RecipientId ?? string.Empty,
            RecipientName = transfer.Recipient?.FullName ?? string.Empty,
            Status = transfer.Status,
            ManagerApprovalDate = transfer.ManagerApprovalDate,
            RecipientApprovalDate = transfer.RecipientApprovalDate,
            RecipientManagerApprovalDate = transfer.RecipientManagerApprovalDate,
            FinanceControllerApprovalDate = transfer.FinanceControllerApprovalDate,
            RejectionReason = transfer.RejectionReason ?? string.Empty,
            RejectedById = transfer.RejectedById ?? string.Empty,
            RejectedByName = transfer.RejectedBy?.FullName ?? string.Empty
        };
    }
}