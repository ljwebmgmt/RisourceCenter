using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(newrisourcecenter.Startup))]
namespace newrisourcecenter
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
