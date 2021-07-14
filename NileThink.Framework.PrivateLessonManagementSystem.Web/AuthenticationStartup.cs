using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.OAuth;
using System.Web.Http;
using NileThink.Framework.PrivateLessonManagementSystem.Web;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;

[assembly: OwinStartup(typeof(NileThink.Framework.PrivateLessonManagementSystem.Web.AuthenticationStartup))]

namespace NileThink.Framework.PrivateLessonManagementSystem.Web
{
    public class AuthenticationStartup
    {
         public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);


            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            var myProvider = new ApiAuthorizationServerProvider();
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                Provider = myProvider
            };

            app.UseOAuthAuthorizationServer(OAuthOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());


            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
        }
    }
}
