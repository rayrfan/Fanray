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
        /// Return plugin's footer view name. Default is null.
        /// </summary>
        /// <returns></returns>
        public virtual string GetFooterViewName() => null;

        /// <summary>
        /// Plugin's Configure startup method.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
