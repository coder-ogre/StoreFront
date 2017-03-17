using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Data.Startup))]
namespace Data
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
