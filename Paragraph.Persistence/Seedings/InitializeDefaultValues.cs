using Microsoft.AspNetCore.Identity;
using Paragraph.Core.Enum;
using Paragraph.Core.IdentityEntities;

namespace Paragraph.Persistence.Seedings
{
    public static class InitializeDefaultValues
    {
        public static async Task SeedIdentityRoles(RoleManager<CustomRole> roleManager,
            UserManager<CustomUser> userManager)
        {
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new CustomRole() { Name = RoleNames.Admin });
                await roleManager.CreateAsync(new CustomRole() { Name = RoleNames.Teacher });
                await roleManager.CreateAsync(new CustomRole() { Name = RoleNames.Student });
            }

            if (!userManager.Users.Any())
            {
                var user1 = new CustomUser()
                {
                    Email = "admin@paragraph.com.tr",
                    UserName = "admin",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    PhoneNumber = "05005005050"
                };
                await userManager.CreateAsync(user1, "123456aa");
                await userManager.AddToRoleAsync(user1, RoleNames.Admin);


                var user2 = new CustomUser()
                {
                    Email = "ogretmen@paragraph.com.tr",
                    UserName = "ogretmen",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    PhoneNumber = "05005005050"
                };
                await userManager.CreateAsync(user2, "123456aa");
                await userManager.AddToRoleAsync(user2, RoleNames.Teacher);


                var user3 = new CustomUser()
                {
                    Email = "ogrenci@paragraph.com.tr",
                    UserName = "ogrenci",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    PhoneNumber = "05005005050"
                };
                await userManager.CreateAsync(user3, "123456aa");
                await userManager.AddToRoleAsync(user3, RoleNames.Student);
            }
        }
    }
}