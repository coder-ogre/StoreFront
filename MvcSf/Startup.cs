using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcSf.Startup))]
namespace MvcSf
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
