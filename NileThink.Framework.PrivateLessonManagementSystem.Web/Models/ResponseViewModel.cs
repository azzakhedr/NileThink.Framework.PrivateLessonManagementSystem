using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Models
{
    public class ResponseViewModel
    {
        public ResponseViewModel(HttpStatusCode _responseCode, string _message, bool? _status, object _results)
        {
            this.responseCode = _responseCode;
            this.message = _message;
            this.status = _status;
            this.results = _results;
        }
        public HttpStatusCode responseCode { get; set; }
        public string message { get; set; }
        public bool? status { get; set; }
        public object results { get; set; }
    }
}