using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SecuroteckWebApplication.Controllers
{
    public class UserController : ApiController
    {
        [ActionName("New")]
        public HttpResponseMessage GET([FromUri] Models.User user)
        {
            bool userExists = 
                Models.UserDatabaseAccess.CheckUser(user.UserName);
            if (userExists == true)
            {
                string result = 
                    "True - User Does Exist! Did you mean to do a POST to create a new user?";
                HttpResponseMessage response =
                    Request.CreateResponse
                    (HttpStatusCode.BadRequest, result);
                return response;
            }
            if(userExists == false)
            {
                string result =
                    "False - User Does Not Exist! Did you mean to do a POST to create a new user?";
                HttpResponseMessage response =
                    Request.CreateResponse
                    (HttpStatusCode.BadRequest, result);
                return response;
            }
            String noStringResult =
                "False - User Does Not Exist! Did you mean to do a POST to create a new user?";
            HttpResponseMessage noStringResponse =
                Request.CreateResponse
                (HttpStatusCode.BadRequest,
                noStringResult);
            return noStringResponse;
        }

        [ActionName("New")]
        public HttpResponseMessage POST([FromBody]Models.User pUser)
        {
            if(pUser.UserName != null)
            {
                Models.User user = Models.UserDatabaseAccess.New(pUser.UserName);
                string apiString = user.ApiKey.ToString();
                return Request.CreateResponse(HttpStatusCode.OK, apiString);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, "Oops. Make sure your body contains a string with your username and your Content - Type is Content - Type:application / json");
        }

        [CustomAuthorise]
        [ActionName("RemoveUser")]
        public HttpResponseMessage DELETE(HttpRequestMessage request,[FromUri]Models.User user)
        {
            IEnumerable<string> headerValues;
            var apiKey = string.Empty;
            if (request.Headers.TryGetValues("ApiKey", out headerValues))
            {
                apiKey = headerValues.FirstOrDefault();
            }
            Guid gApiKey = new Guid(apiKey);
            user = Models.UserDatabaseAccess.CheckUser(gApiKey);
            Models.UserDatabaseAccess.NewLog("Deleted User: " + user.UserName, user);
            Models.UserDatabaseAccess.DeleteUser(gApiKey);
            if (Models.UserDatabaseAccess.CheckUser(gApiKey, user.UserName) == true)
            {
                return Request.CreateResponse(HttpStatusCode.Accepted, false);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Accepted, true);
            }
        }
    }
}
