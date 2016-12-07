using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ConstraintProgramming.FrontEnd.Startup))]
namespace ConstraintProgramming.FrontEnd
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
