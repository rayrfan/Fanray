using Fan.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Fan.Plugins
{
    /// <summary>
    /// Plugin base class.
    /// </summary>
    public class Plugin : Extension
    {
        /// <summary>
        /// Plugin meta id.
        /// </summary>
        [JsonIgnore]
        public int Id { get; set; }

        /// <summary>
        /// Returns true if plugin is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Return plugin's foot content view name, default is null.
        /// </summary>
        /// <returns></returns>
        public virtual string GetFootContentViewName() => null;

        /// <summary>
        /// Returns plugin's foot script view name, default is null.
        /// </summary>
        /// <returns></returns>
        public virtual string GetFootScriptsViewName() => null;

        /// <summary>
        /// Returns plugin's styles view name, default is null.
        /// </summary>
        /// <returns></returns>
        public virtual string GetStylesViewName() => null;

        /// <summary>
        /// Plugin's Configure startup method.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        /// <summary>
        /// Plugin's ConfigureService startup method.
        /// </summary>
        /// <param name="services"></param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
