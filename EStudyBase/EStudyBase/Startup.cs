using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EStudyBase.Startup))]
namespace EStudyBase
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
