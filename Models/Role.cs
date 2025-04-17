// Models/Role.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

public class ApplicationRole : IdentityRole
{
    public string Description { get; set; } = string.Empty;
    
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}