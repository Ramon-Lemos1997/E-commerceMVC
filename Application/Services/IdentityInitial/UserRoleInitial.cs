using Domain.Interfaces.Roles;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.IdentityRoles
{
    //criar as roles e users iniciais
    public class UserRoleInitial : IUserRoleInitial
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRoleInitial(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }


        //-------------------------------------------------------------------------
        /// <summary>
        /// Verifica se as funções "Admin" e "User" existem no sistema e, se não existirem, cria essas funções.
        /// </summary>
        public async Task RolesAsync()
        {           
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Admin";
                role.NormalizedName = "ADMIN";
                role.ConcurrencyStamp = Guid.NewGuid().ToString();

                IdentityResult roleResult = await _roleManager.CreateAsync(role);
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                IdentityRole role = new IdentityRole();
                role.Name = "User";
                role.NormalizedName = "USER";
                role.ConcurrencyStamp = Guid.NewGuid().ToString();

                IdentityResult roleResult = await _roleManager.CreateAsync(role);
            }

        }

        /// <summary>
        /// Verifica se o usuário administrador com o e-mail "admin@localhost" existe no sistema e, se não existir, cria o usuário com a função "Admin".
        /// </summary>
        public async Task UsersAsync()
        {         
            if (await _userManager.FindByEmailAsync("admin@localhost") == null)
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = "admin@localhost";
                user.Email = "admin@localhost";
                user.NormalizedUserName = "ADMIN@LOCALHOST";
                user.NormalizedEmail = "ADMIN@LOCALHOST";
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;

                user.SecurityStamp = Guid.NewGuid().ToString();

                IdentityResult result = await _userManager.CreateAsync(user, "@Bolo24071997");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }




        //---------------------------------------------------------------------------------
    }
}