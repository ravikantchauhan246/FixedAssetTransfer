using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace FixedAssetTransfer.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedRolesAndPermissions(RoleManager<ApplicationRole> roleManager)
        {
            // Seed roles
            var roles = new[]
            {
                new ApplicationRole { Name = "Admin", Description = "Administrator with full access" },
                new ApplicationRole { Name = "Requestor", Description = "Employee who can request asset transfers" },
                new ApplicationRole { Name = "Manager", Description = "Department manager who approves requests" },
                new ApplicationRole { Name = "Accountant", Description = "Accountant who updates financial details" },
                new ApplicationRole { Name = "Recipient", Description = "Recipient of transferred asset" },
                new ApplicationRole { Name = "RecipientManager", Description = "Recipient's manager" },
                new ApplicationRole { Name = "FinanceController", Description = "Finance controller/manager" }
            };

            foreach (var role in roles)
            {
                if (role.Name != null && !await roleManager.RoleExistsAsync(role.Name))
                {
                    await roleManager.CreateAsync(role);
                }
                else if (role.Name != null)
                {
                    var existingRole = await roleManager.FindByNameAsync(role.Name);
                    if (existingRole != null && role.Description != null)
                    {
                        existingRole.Description = role.Description;
                        await roleManager.UpdateAsync(existingRole);
                    }
                }
            }

            // Seed permissions
            var permissions = new[]
            {
                new { Name = "ViewAssets", Description = "View asset transfers" },
                new { Name = "CreateAsset", Description = "Create asset transfer requests" },
                new { Name = "EditAsset", Description = "Edit asset transfer requests" },
                new { Name = "DeleteAsset", Description = "Delete asset transfer requests" },
                new { Name = "SubmitAsset", Description = "Submit asset transfer requests" },
                new { Name = "ApproveManager", Description = "Approve as manager" },
                new { Name = "UpdateFinancials", Description = "Update financial details" },
                new { Name = "ApproveRecipient", Description = "Approve as recipient" },
                new { Name = "ApproveRecipientManager", Description = "Approve as recipient manager" },
                new { Name = "ApproveFinance", Description = "Approve as finance controller" },
                new { Name = "ManageUsers", Description = "Manage users" },
                new { Name = "ManageRoles", Description = "Manage roles and permissions" }
            };

            // Assign permissions to roles
            await AssignPermissionsToRole(roleManager, "Admin", permissions.Select(p => p.Name).ToArray());
            await AssignPermissionsToRole(roleManager, "Requestor", new[] { "ViewAssets", "CreateAsset", "EditAsset", "DeleteAsset", "SubmitAsset" });
            await AssignPermissionsToRole(roleManager, "Manager", new[] { "ViewAssets", "ApproveManager" });
            await AssignPermissionsToRole(roleManager, "Accountant", new[] { "ViewAssets", "UpdateFinancials" });
            await AssignPermissionsToRole(roleManager, "Recipient", new[] { "ViewAssets", "ApproveRecipient" });
            await AssignPermissionsToRole(roleManager, "RecipientManager", new[] { "ViewAssets", "ApproveRecipientManager" });
            await AssignPermissionsToRole(roleManager, "FinanceController", new[] { "ViewAssets", "ApproveFinance" });
        }

        private static async Task AssignPermissionsToRole(RoleManager<ApplicationRole> roleManager, string roleName, string[] permissionNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var existingClaims = await roleManager.GetClaimsAsync(role);
                var existingPermissionNames = existingClaims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

                // Add new permissions
                foreach (var permissionName in permissionNames.Except(existingPermissionNames))
                {
                    await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", permissionName));
                }

                // Remove old permissions
                foreach (var existingPermission in existingPermissionNames.Except(permissionNames))
                {
                    var claim = existingClaims.First(c => c.Type == "Permission" && c.Value == existingPermission);
                    await roleManager.RemoveClaimAsync(role, claim);
                }
            }
        }

        public static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@fixedasset.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FullName = "Administrator",
                    Department = "IT",
                    Position = "System Administrator",
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        public static async Task SeedTestData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            // Seed test users for each role
            var roles = await roleManager.Roles.ToListAsync();
            
            foreach (var role in roles)
            {
                if (role.Name == null) continue;
                
                var testEmail = $"{role.Name.ToLower()}@fixedasset.com";
                var testUser = await userManager.FindByEmailAsync(testEmail);
                
                if (testUser == null)
                {
                    testUser = new ApplicationUser
                    {
                        UserName = role.Name.ToLower(),
                        Email = testEmail,
                        FullName = $"{role.Name} User",
                        Department = "Test Department",
                        Position = $"{role.Name} Position",
                        EmailConfirmed = true
                    };
                    
                    var result = await userManager.CreateAsync(testUser, $"{role.Name}@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(testUser, role.Name);
                    }
                }
            }

            // Seed test asset transfers if none exist
            if (!await context.AssetTransfers.AnyAsync())
            {
                var requestor = await userManager.FindByNameAsync("requestor");
                var recipient = await userManager.FindByNameAsync("recipient");

                if (requestor != null && recipient != null)
                {
                    var transfers = new[]
                    {
                        new AssetTransfer
                        {
                            RequestorId = requestor.Id,
                            AssetDescription = "Laptop Dell XPS 15",
                            AssetTagNumber = "FA-1001",
                            CurrentLocation = "Head Office",
                            CurrentCostCenter = "CC-100",
                            CurrentOwner = "IT Department",
                            NewLocation = "Branch Office",
                            NewCostCenter = "CC-200",
                            NewOwner = "Marketing Department",
                            PurposeOfTransfer = "Employee transfer",
                            Justification = "Employee moving to marketing department",
                            RecipientId = recipient.Id,
                            Status = TransferStatus.PendingManagerApproval,
                            RequestDate = DateTime.UtcNow.AddDays(-2)
                        },
                        new AssetTransfer
                        {
                            RequestorId = requestor.Id,
                            AssetDescription = "Office Desk",
                            AssetTagNumber = "FA-1002",
                            CurrentLocation = "Warehouse",
                            CurrentCostCenter = "CC-300",
                            CurrentOwner = "Facilities",
                            NewLocation = "Head Office",
                            NewCostCenter = "CC-100",
                            NewOwner = "IT Department",
                            PurposeOfTransfer = "New employee setup",
                            Justification = "New hire in IT department needs workstation",
                            Status = TransferStatus.PendingAccountantUpdate,
                            RequestDate = DateTime.UtcNow.AddDays(-1)
                        }
                    };

                    await context.AssetTransfers.AddRangeAsync(transfers);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}