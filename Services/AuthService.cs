// Services/AuthService.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly TokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    public async Task<UserResponseDto?> Register(UserRegisterDto userDto)
    {
        var user = new ApplicationUser
        {
            FullName = userDto.FullName,
            UserName = userDto.UserName,
            Email = userDto.Email,
            Department = userDto.Department,
            Position = userDto.Position
        };

        var result = await _userManager.CreateAsync(user, userDto.Password);

        if (!result.Succeeded)
        {
            throw new ApplicationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Assign default role if needed
        // await _userManager.AddToRoleAsync(user, "Requestor");

        return await GetUserResponse(user);
    }

    public async Task<string> Login(UserLoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.UserName);

        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded)
        {
            throw new ApplicationException("Invalid password");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return _tokenService.GenerateToken(user, roles);
    }

    public async Task<UserResponseDto?> GetCurrentUser(ClaimsPrincipal userClaims)
    {
        var user = await _userManager.GetUserAsync(userClaims);
        return await GetUserResponse(user);
    }

    private async Task<UserResponseDto?> GetUserResponse(ApplicationUser? user)
    {
        if (user == null) return null;

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
}