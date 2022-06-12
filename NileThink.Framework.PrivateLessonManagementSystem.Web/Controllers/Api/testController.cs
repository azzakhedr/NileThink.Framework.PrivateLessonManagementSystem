using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{
    public class testController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {

            string accountSid = Environment.GetEnvironmentVariable("AC03f1700cf7cb3f244c33c03afb79b44d");
            string authToken = Environment.GetEnvironmentVariable("25aad5bec0e65246295bbb2177899f0e");

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: "Hi there",
                from: new Twilio.Types.PhoneNumber("+15017122661"),
                to: new Twilio.Types.PhoneNumber("+15558675310")
            );
            return new string[] { "value1", "value2" };
        }


     
        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}