using Microsoft.AspNetCore.Identity;

namespace Bison.Razor.Data;

public static class IdentitySeeder
{
    public static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        await CreateUserIfMissingAsync(userManager, "edka@itu.dk", "123456");
        await CreateUserIfMissingAsync(userManager, "debloh@itu.dk", "password");
    }

    private static async Task CreateUserIfMissingAsync(UserManager<IdentityUser> userManager, string email, string password)
    {
        IdentityUser? existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return;
        }

        IdentityUser user = new IdentityUser();
        user.UserName = email;
        user.Email = email;
        user.EmailConfirmed = true;

        IdentityResult result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Could not create user " + email);
        }
    }
}
