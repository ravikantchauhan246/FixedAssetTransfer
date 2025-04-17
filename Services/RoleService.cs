// Services/RoleService.cs
using FixedAssetTransfer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class RoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public RoleService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<List<RoleDto>> GetAllRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return roles.Select(role => new RoleDto 
        { 
            Id = role.Id, 
            Name = role.Name ?? string.Empty,
            Description = role.Description 
        }).ToList();
    }

    public async Task<RoleDto?> CreateRole(RoleDto roleDto)
    {
        var role = new ApplicationRole
        {
            Name = roleDto.Name,
            Description = roleDto.Description
        };

        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            throw new ApplicationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (roleDto.Permissions != null && roleDto.Permissions.Any())
        {
            await AddPermissionsToRole(role.Id, roleDto.Permissions);
        }

        return await GetRoleById(role.Id);
    }

    public async Task AddPermissionsToRole(string roleId, List<string> permissionNames)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            throw new ApplicationException("Role not found");
        }

        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();

        var permissionsToAdd = permissionNames.Except(existingPermissions);

        foreach (var permissionName in permissionsToAdd)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
            if (permission == null)
            {
                permission = new Permission { Name = permissionName };
                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();
            }

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permission.Id
            };

            _context.RolePermissions.Add(rolePermission);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<RoleDto?> GetRoleById(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return null;
        }

        return new RoleDto 
        { 
            Id = role.Id, 
            Name = role.Name ?? string.Empty, 
            Description = role.Description 
        };
    }
}