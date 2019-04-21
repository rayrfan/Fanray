using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Fan.Plugins
{
    public class Plugin
    {
        public virtual string GetEditUrl() => null;
        public virtual string GetInfoUrl() => null;

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
