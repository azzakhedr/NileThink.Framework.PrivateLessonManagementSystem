using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Helper
{
    public class ResponseResults : IHttpActionResult
    {
        private readonly ResponseViewModel _message;

        private readonly HttpStatusCode _statusCode;

        public ResponseResults(HttpStatusCode statusCode, ResponseViewModel response)
        {
            _statusCode = statusCode;
            _message = response;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new HttpResponseMessage(_statusCode)
            {
                Content = new ObjectContent(_message.GetType(), _message, new JsonMediaTypeFormatter())
            };
            return Task.FromResult(response);
        }
    }
}