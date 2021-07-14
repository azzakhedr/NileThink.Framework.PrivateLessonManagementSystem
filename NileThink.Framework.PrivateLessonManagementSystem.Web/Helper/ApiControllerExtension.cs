using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Helper
{
    public static class ApiControllerExtension
    {
       

        public static ResponseResults ResponseNotFound(this ApiController controller, ResponseViewModel result)
        {
            return new ResponseResults(HttpStatusCode.NotFound, result);
        }
        public static ResponseResults ResponseOK(this ApiController controller, ResponseViewModel result)
        {
            return new ResponseResults(HttpStatusCode.OK, result);
        }
        public static ResponseResults ResponseBadRequest(this ApiController controller, ResponseViewModel result)
        {
            return new ResponseResults(HttpStatusCode.BadRequest, result);
        }
        public static ResponseResults ResponseError(this ApiController controller, ResponseViewModel result)
        {
            return new ResponseResults(HttpStatusCode.InternalServerError, result);
        }
        public static ResponseResults ResponseUnauthorized(this ApiController controller, ResponseViewModel result)
        {
            return new ResponseResults(HttpStatusCode.Unauthorized, result);
        }

    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ResponseCodesAttribute : Attribute
    {
        public ResponseCodesAttribute(params HttpStatusCode[] statusCodes)
        {
            ResponseCodes = statusCodes;
        }

        public IEnumerable<HttpStatusCode> ResponseCodes { get; private set; }
    }
}