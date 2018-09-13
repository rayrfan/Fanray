using Fan.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fan.Web.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public UsersModel(UserManager<User> userManager,
                          RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public class UserVM
        {
            public string Avatar { get; set; }
            public string UserName { get; set; }
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string JoinedOn { get; set; }
            public bool LockedOut { get; set; }
            public string Password { get; set; }
        }

        public class RoleVM
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        // -------------------------------------------------------------------- properties

        public const string DEFAULT_ROW_PER_PAGE_ITEMS = "[25, 50]";

        public int TotalUsers { get; private set; }
        public string UsersJson { get; private set; } 
        public string RolesJson { get; private set; } 

        // -------------------------------------------------------------------- Public Methods

        public async Task OnGetAsync()
        {
            var users = await GetUsersAsync();
            UsersJson = JsonConvert.SerializeObject(users);
            TotalUsers = users.Count();

            var roles = GetRoles();
            RolesJson = JsonConvert.SerializeObject(roles);
        }

        /// <summary>
        /// POST to create a new user and assigns role to it.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync([FromBody]UserVM model)
        {
            // check dup email
            var foundUser = await _userManager.FindByEmailAsync(model.Email);
            if (foundUser != null)
            {
                return BadRequest("User with same email exists.");
            }

            // check dup username
            foundUser = await _userManager.FindByNameAsync(model.UserName);
            if (foundUser != null)
            {
                return BadRequest("User with same username exists.");
            }

            // prep user
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                DisplayName = model.DisplayName
            };

            // create user
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // re-get the user after create 
                user = await _userManager.FindByNameAsync(model.UserName);

                // assign role to it
                result = await _userManager.AddToRoleAsync(user, model.Role);

                // return new user
                var userVM = MapUser(user, model.Role);
                return new JsonResult(userVM);
            }

            return BadRequest("Create new user failed.");
        }


        // -------------------------------------------------------------------- private methods

        private async Task<List<UserVM>> GetUsersAsync()
        {
            var userVMs = new List<UserVM>();
            var users = _userManager.Users.AsEnumerable(); // TODO better ways to get users

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.ToList()[0]; // there will always be a role for a user

                userVMs.Add(MapUser(user, role));
            }

            return userVMs;
        }

        private UserVM MapUser(User user, string role)
        {
            return new UserVM
            {
                Avatar = GetAvatar(user.Email),
                Email = user.Email,
                Role = role,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                JoinedOn = user.CreatedOn.ToString("yyyy-MM-dd"),
            };
        }

        private List<RoleVM> GetRoles()
        {
            var roleVMs = new List<RoleVM>();
            var roles = _roleManager.Roles.AsEnumerable();
            foreach (var role in roles)
            {
                roleVMs.Add(new RoleVM {
                    Name = role.Name,
                    Description = role.Description,
                });
            }

            return roleVMs;
        }

        private string GetAvatar(string email)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(email));
                var hash = BitConverter.ToString(result).Replace("-", "").ToLower();
                return $"//gravatar.com/avatar/{hash}";
            }
        }
    }
}