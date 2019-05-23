using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Cryptography;
using System.Web.Security;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;

namespace SecuroteckWebApplication.Controllers
{
    public class ProtectedController : ApiController
    {
        public class MessageHolder
        {
            public string Message { get; set; }
        }
        public class AddFifty
        {
            public string EncryptedInteger { get; set; }
            public string EncryptedSymKey { get; set; }
            public string EncryptedIV { get; set; }
        }
        [CustomAuthorise]
        [ActionName("Hello")]
        public string Get(HttpRequestMessage request)
        {
            IEnumerable<string> headerValues;
            var apiKey = string.Empty;
            if (request.Headers.TryGetValues("ApiKey", out headerValues))
            {
                apiKey = headerValues.FirstOrDefault();
            }
            Guid gApiKey = new Guid(apiKey);
            Models.User user = Models.UserDatabaseAccess.CheckUser(gApiKey);
            Models.UserDatabaseAccess.NewLog("Protected Hello", user);
            string userName = user.UserName;
            return "Hello " + userName;
        }

        [CustomAuthorise]
        [HttpGet]
        [ActionName("SHA1")]
        public string GetSHA1([FromUri]MessageHolder messageHolder)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(messageHolder.Message));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
            
        }

        [CustomAuthorise]
        [HttpGet]
        [ActionName("SHA256")]
        public string GetSHA256([FromUri]MessageHolder messageHolder)
        {
            using (SHA256Managed sha1 = new SHA256Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(messageHolder.Message));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }

        }

        [CustomAuthorise]
        [HttpGet]
        [ActionName("GetPublicKey")]
        public HttpResponseMessage GetPublicKey(HttpRequestMessage request)
        {
            IEnumerable<string> headerValues;
            var apiKey = string.Empty;
            if (request.Headers.TryGetValues("ApiKey", out headerValues))
            {
                apiKey = headerValues.FirstOrDefault();
            }
            Guid gApiKey = new Guid(apiKey);
            Models.User user = Models.UserDatabaseAccess.CheckUser(gApiKey);
            Models.UserDatabaseAccess.NewLog("Get Public Key", user);
            bool userExists = Models.UserDatabaseAccess.CheckUser(gApiKey, user.UserName);
            if (userExists == true)
            {
                return Request.CreateResponse(HttpStatusCode.Accepted, WebApiConfig.publicKey);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [CustomAuthorise]
        [HttpGet]
        [ActionName("Sign")]
        public HttpResponseMessage GetSign(HttpRequestMessage request, [FromUri]MessageHolder messageHolder)
        {
            IEnumerable<string> headerValues;
            var apiKey = string.Empty;
            if (request.Headers.TryGetValues("ApiKey", out headerValues))
            {
                apiKey = headerValues.FirstOrDefault();
            }
            Guid gApiKey = new Guid(apiKey);
            Models.User user = Models.UserDatabaseAccess.CheckUser(gApiKey);
            Models.UserDatabaseAccess.NewLog("Get Signed Message", user);
            bool userExists = Models.UserDatabaseAccess.CheckUser(gApiKey, user.UserName);
            if (userExists == true)
            {
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.FromXmlString(WebApiConfig.privateKey);
                    RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(RSA);
                    RSAFormatter.SetHashAlgorithm("SHA1");
                    SHA1Managed SHhash = new SHA1Managed();
                    byte[] SignedHashValue = RSAFormatter.CreateSignature(SHhash.ComputeHash(new UnicodeEncoding().GetBytes(messageHolder.Message)));
                    string sign = BitConverter.ToString(SignedHashValue);
                    return Request.CreateResponse(HttpStatusCode.Accepted, sign);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        [CustomAuthorise]
        [HttpGet]
        [ActionName("AddFifty")]
        public HttpResponseMessage GetAddFifty(HttpRequestMessage request, [FromUri]AddFifty addFifty)
        {
            try
            {
                IEnumerable<string> headerValues;
                var apiKey = string.Empty;
                if (request.Headers.TryGetValues("ApiKey", out headerValues))
                {
                    apiKey = headerValues.FirstOrDefault();
                }
                Guid gApiKey = new Guid(apiKey);
                Models.User user = Models.UserDatabaseAccess.CheckUser(gApiKey);
                Models.UserDatabaseAccess.NewLog("Get AddFifty", user);
                byte[] aesKeyEncrypted = addFifty.EncryptedSymKey.Split('-').
                    Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber))
                    .ToArray();
                byte[] aesIVEncrypted = addFifty.EncryptedIV.Split('-').
                    Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber))
                    .ToArray();
                byte[] integerEncrypted = addFifty.EncryptedInteger.Split('-').
                    Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber))
                    .ToArray();
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.FromXmlString(WebApiConfig.privateKey);
                    byte[] aesKey = RSA.Decrypt(aesKeyEncrypted, false);
                    byte[] aesIV = RSA.Decrypt(aesIVEncrypted, false);
                    byte[] integerByteArray = RSA.Decrypt(integerEncrypted, false);
                    int integer = BitConverter.ToInt32(integerByteArray, 0);
                    integer = integer + 50;
                    integerByteArray = BitConverter.GetBytes(integer);
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                    {
                        byte[] aesEncryptedInteger = aes.CreateEncryptor(aesKey, aesIV)
                            .TransformFinalBlock(integerByteArray, 0,
                            integerByteArray.Length);
                        string hexEncryptedInteger = BitConverter.ToString(aesEncryptedInteger);
                        return Request.CreateResponse(HttpStatusCode.Accepted, hexEncryptedInteger);
                    }
                }
            }catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}
