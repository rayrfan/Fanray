using Fan.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Fan.Plugins
{
    public class Plugin : Extension
    {
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
