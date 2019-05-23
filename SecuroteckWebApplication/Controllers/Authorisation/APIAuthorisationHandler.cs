using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SecuroteckWebApplication.Models;

namespace SecuroteckWebApplication.Controllers
{
    public class APIAuthorisationHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
        {

            IEnumerable<string> headerValues;
            var apiKey = string.Empty;
            if(request.Headers.TryGetValues("ApiKey", out headerValues))
            {
                apiKey = headerValues.FirstOrDefault();
                Guid gApiKey = new Guid(apiKey);
                User user = UserDatabaseAccess.CheckUser(gApiKey);

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                var id = new ClaimsIdentity(claims, apiKey);
                var principal = new ClaimsPrincipal(id);
                Thread.CurrentPrincipal = principal;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}