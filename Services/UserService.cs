// Services/UserService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class UserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<List<UserResponseDto>> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserResponseDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var rolePermissions = await _roleManager.GetClaimsAsync(role);
                    permissions.AddRange(rolePermissions.Select(p => p.Value));
                }
            }

            userDtos.Add(new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Department = user.Department,
                Position = user.Position,
                Roles = roles,
                Permissions = permissions.Distinct().ToList()
            });
        }

        return userDtos;
    }

    public async Task<UserResponseDto?> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = new List<string>();

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var rolePermissions = await _roleManager.GetClaimsAsync(role);
                permissions.AddRange(rolePermissions.Select(p => p.Value));
            }
        }

        return new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Department = user.Department,
            Position = user.Position,
            Roles = roles,
            Permissions = permissions.Distinct().ToList()
        };
    }

    public async Task AssignRolesToUser(string userId, List<string> roleNames)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var existingRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = roleNames.Except(existingRoles);
        var rolesToRemove = existingRoles.Except(roleNames);

        await _userManager.AddToRolesAsync(user, rolesToAdd);
        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
    }

    public async Task<List<UserResponseDto>> GetUsersByRole(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        var userDtos = new List<UserResponseDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Department = user.Department,
                Position = user.Position,
                Roles = roles
            });
        }

        return userDtos;
    }
}