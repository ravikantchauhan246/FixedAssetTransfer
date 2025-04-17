// Controllers/AssetTransfersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

[ApiController]
// Add the following using directive at the top of the file to resolve the CS0246 error.

[Route("api/[controller]")]
[Authorize]
public class AssetTransfersController : ControllerBase
{
    private readonly AssetTransferService _assetTransferService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssetTransfersController(
        AssetTransferService assetTransferService,
        UserManager<ApplicationUser> userManager)
    {
        _assetTransferService = assetTransferService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssetTransferDto>>> GetAll()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized("User not authenticated properly");
        }
        var transfers = await _assetTransferService.GetTransfersForUser(userId);
        return Ok(transfers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssetTransferDto>> GetById(int id)
    {
        var transfer = await _assetTransferService.GetTransferById(id);
        if (transfer == null)
        {
            return NotFound();
        }
        return Ok(transfer);
    }

    [HttpPost]
    public async Task<ActionResult<AssetTransferDto>> Create(CreateAssetTransferDto transferDto)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized("User not authenticated properly");
        }
        var transfer = await _assetTransferService.CreateTransfer(transferDto, userId);
        if (transfer == null)
        {
            return BadRequest("Could not create transfer");
        }
        return CreatedAtAction(nameof(GetById), new { id = transfer.Id }, transfer);
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<AssetTransferDto>> Submit(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized("User not authenticated properly");
        }
        var transfer = await _assetTransferService.SubmitTransfer(id, userId);
        if (transfer == null)
        {
            return NotFound();
        }
        return Ok(transfer);
    }

    [HttpPost("{id}/financial-details")]
    [Authorize(Roles = "Accountant")]
    public async Task<ActionResult<AssetTransferDto>> UpdateFinancialDetails(int id, UpdateFinancialDetailsDto detailsDto)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized("User not authenticated properly");
        }
        var transfer = await _assetTransferService.UpdateFinancialDetails(id, detailsDto, userId);
        if (transfer == null)
        {
            return NotFound();
        }
        return Ok(transfer);
    }

    [HttpPost("{id}/manager-approve")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<AssetTransferDto>> ManagerApprove(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized("User not authenticated properly");
        }
        var transfer = await _assetTransferService.ManagerApprove(id, userId);
        if (transfer == null)
        {
            return NotFound();
        }
        return Ok(transfer);
    }

    [HttpPost("{id}/recipient-approve")]
    [Authorize(Roles = "Recipient")]
    public async Task<ActionResult<AssetTransferDto>> RecipientApprove(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized("User not authenticated properly");
        }
        var transfer = await _assetTransferService.RecipientApprove(id, userId);
        if (transfer == null)
        {
            return NotFound();
        }
        return Ok(transfer);
    }

    [HttpPost("{id}/recipient-manager-approve")]
    [Authorize(Roles = "RecipientManager")]
    public async Task<ActionResult<AssetTransferDto>> RecipientManagerApprove(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized("User not authenticated properly");
        }
        var transfer = await _assetTransferService.RecipientManagerApprove(id, userId);
        if (transfer == null)
        {
            return NotFound();
        }
        return Ok(transfer);
    }

    [HttpPost("{id}/finance-controller-approve")]
    [Authorize(Roles = "FinanceController")]
    public async Task<ActionResult<AssetTransferDto>> FinanceControllerApprove(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized("User not authenticated properly");
        }
        var transfer = await _assetTransferService.FinanceControllerApprove(id, userId);
        if (transfer == null)
        {
            return NotFound();
        }
        return Ok(transfer);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<AssetTransferDto>> Reject(int id, ApproveRejectDto rejectDto)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized("User not authenticated properly");
        }
        var transfer = await _assetTransferService.RejectTransfer(id, rejectDto, userId);
        if (transfer == null)
        {
            return NotFound();
        }
        return Ok(transfer);
    }
}