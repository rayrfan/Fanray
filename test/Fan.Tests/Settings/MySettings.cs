using Fan.Models;
using Fan.Settings;
using System.Collections.Generic;

namespace Fan.Tests.Settings
{
    /// <summary>
    /// A settings class help test <see cref="SettingService"/>.
    /// </summary>
    /// <remarks>
    /// This is a complex class with simple property and reference type and array.
    /// </remarks>
    public class MySettings : ISettings
    {
        public int Age { get; set; } = 13;
        public User User { get; set; } = new User { DisplayName = "John Smith" };
        public List<Role> Roles { get; set; } = new List<Role> {
            new Role { Name = "Admin" },
            new Role { Name = "Editor" }
        };
    }
}
