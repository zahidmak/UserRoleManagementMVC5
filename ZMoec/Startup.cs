using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ZMoec.Startup))]
namespace ZMoec
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
