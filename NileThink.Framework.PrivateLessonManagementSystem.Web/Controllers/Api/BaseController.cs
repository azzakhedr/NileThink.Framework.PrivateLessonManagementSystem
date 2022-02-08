using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Providers;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Results;
using System.IO;
using System.Web.Hosting;
using System.Web.Http.Description;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Helper;
using System.Net;
using System.Linq;
using System.Data.Entity;
using System.Configuration;
using NileThink.Framework.PrivateLessonManagementSystem.Web;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using System.Globalization;



using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using System.Drawing;
using System.Drawing.Imaging;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{


    public class BaseController : ApiController
    {
        protected string lang
        {
            get
            {

                var re = Request;
                var headers = re.Headers;
                string lang = "ar";
                if (headers.Contains("lang"))
                {
                    lang = headers.GetValues("lang").First();
                }
                else if (headers.Contains("Lang"))
                {
                    lang = headers.GetValues("Lang").First();
                }




                if (lang.ToLower() == "ar")
                {
                    var cultureInfo = new CultureInfo("ar-EG");
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

                }
                else
                {
                    var cultureInfo = new CultureInfo("en-US");
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

                }
                return lang;
            }
        }
    }
}