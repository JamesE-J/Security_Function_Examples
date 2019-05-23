using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SecuroteckWebApplication.Controllers
{
    public class TalkBackController : ApiController
    {
        [ActionName("Hello")]
        public string Get()
        {
            String response = "Hello World";
            return response;
        }

        [ActionName("Sort")]
        public int[] Get([FromUri]int[] integers)
        {
            int[] response = integers;
            bool isBadRequest = response.Contains(0);
            if (isBadRequest == true)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));
            } 
            Array.Sort(response);
            return response;
        }

    }
}
