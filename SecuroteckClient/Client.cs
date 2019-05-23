using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SecuroteckClient
{
    public class User
    {
        public Guid ApiKey { get; set; }
        public string UserName { get; set; }
        public User()
        {
        }
    }
    class Client
    {
        static public string publicKey;
        static bool clientInitialised = false;
        static HttpClient client = new HttpClient();
        static string greetingString = "Hello. What would you like to do?";
        static string waitingString = "…please wait…";
        //To Do: Read user from file
        static User user = new User();

        static async Task GetHello(string type)
        {
            string response = string.Empty;
            HttpResponseMessage responseMessage;
            if (type == "Protected")
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/protected/hello");
                requestMessage.Headers.Add("ApiKey", user.ApiKey.ToString());
                responseMessage = await client.SendAsync(requestMessage);
                response = await responseMessage.Content.ReadAsStringAsync();
                if (response == "{\"Message\":\"Unauthorized. Check ApiKey in Header is correct.\"}")
                {
                    response = "You need to do a User Post or User Set first";
                }
            }
            else
            {
                responseMessage = await client.GetAsync("api/talkback/hello");
                response = await responseMessage.Content.ReadAsStringAsync();
            }
            Console.WriteLine(response);
        }

        static async Task GetSort(string sortString)
        {
            sortString = sortString.Substring(1, sortString.Length - 2);
            string[] sortArrayString = sortString.Split(',');
            string urlString = "api/talkback/sort?";
            bool firstInt = true;
            for (int i = 0; i < sortArrayString.Length; i++)
            {
                if (firstInt == true)
                {
                    urlString = urlString + "integers=" + sortArrayString[i];
                    firstInt = false;
                }
                else
                {
                    urlString = urlString + "&integers=" + sortArrayString[i];
                }
            }
            string response = string.Empty;
            HttpResponseMessage responseMessage = await client.GetAsync(urlString);
            string responseArray = await responseMessage.Content.ReadAsStringAsync();
            Console.WriteLine(responseArray);
        }

        static async Task GetUser(string userName)
        {
            string response = string.Empty;
            HttpResponseMessage responseMessage;
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/user/new?username=" + userName);
            responseMessage = await client.SendAsync(requestMessage);
            response = await responseMessage.Content.ReadAsStringAsync();
            Console.WriteLine(response);
        }

        static async Task PostUser(string userName)
        {
            string response = string.Empty;
            HttpResponseMessage responseMessage;
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/user/new");
            requestMessage.Content = new StringContent("{\"UserName\":\"" + userName + "\"}", Encoding.UTF8, "application/json");
            responseMessage = await client.SendAsync(requestMessage);
            response = await responseMessage.Content.ReadAsStringAsync();
            response = response.Replace("\"", "");
            if ((int)responseMessage.StatusCode == 200)
            {
                user.ApiKey = new Guid(response);
                user.UserName = userName;
                Console.WriteLine("Got API Key");
            }
            else
            {
                Console.WriteLine(response);
            }
        }

        static async Task SetUser(string pUserName, string pApiKey)
        {
            user.UserName = pUserName;
            Guid apiKey = new Guid(pApiKey);
            user.ApiKey = apiKey;
            Console.WriteLine("Stored");
        }

        static async Task DeleteUser()
        {
            if(user.UserName != null && user.ApiKey != null)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "api/user/removeuser?username=" + user.UserName);
                requestMessage.Headers.Add("ApiKey", user.ApiKey.ToString());
                HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                string result = await responseMessage.Content.ReadAsStringAsync();            
                if (result == "true")
                {
                    Console.WriteLine("True");
                }
                else
                {
                    Console.WriteLine("False");
                }
            }
            else
            {
                Console.WriteLine("You need to do a User Post or User Set first");
            }
        }

        static async Task ProtectedSHA1(string pMessage)
        {
            if (user.UserName != null && user.ApiKey != null)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/protected/sha1?message=" + pMessage);
                requestMessage.Headers.Add("ApiKey", user.ApiKey.ToString());
                HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                string response = await responseMessage.Content.ReadAsStringAsync();
                Console.WriteLine(response);
            }
            else
            {
                Console.WriteLine("You need to do a User Post or User Set first");
            }
        }

        static async Task ProtectedSHA256(string pMessage)
        {
            if (user.UserName != null && user.ApiKey != null)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/protected/sha256?message=" + pMessage);
                requestMessage.Headers.Add("ApiKey", user.ApiKey.ToString());
                HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                string response = await responseMessage.Content.ReadAsStringAsync();
                Console.WriteLine(response);
            }
            else
            {
                Console.WriteLine("You need to do a User Post or User Set first");
            }
        }
        static async Task GetPublicKey()
        {
            if (user.UserName != null && user.ApiKey != null)
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/protected/getpublickey");
                requestMessage.Headers.Add("ApiKey", user.ApiKey.ToString());
                HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                string response = await responseMessage.Content.ReadAsStringAsync();
                if (responseMessage.StatusCode == HttpStatusCode.Accepted)
                {
                    response = response.Replace("\"", "");
                    publicKey = response;
                    Console.WriteLine("Got Public Key");
                }
                else
                {
                    Console.WriteLine("Couldn’t Get the Public Key");
                }
            }
            else
            {
                Console.WriteLine("You need to do a User Post or User Set first");
            }
        }

        static async Task getSign(string pMessage)
        {
            if (user.UserName != null && user.ApiKey != null)
            {
                if (publicKey != null)
                {
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/protected/sign?message=" + pMessage);
                    requestMessage.Headers.Add("ApiKey", user.ApiKey.ToString());
                    HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                    string response = await responseMessage.Content.ReadAsStringAsync();
                    response = response.Replace("\"", "");
                    byte[] byteResponse = response.Split('-').Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber))
                        .ToArray();
                    using(RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                    {
                        try
                        {
                            RSA.FromXmlString(publicKey);
                            if (RSA.VerifyData(new UnicodeEncoding().GetBytes(pMessage), new SHA1CryptoServiceProvider(), byteResponse))
                            {
                                Console.WriteLine("Message was successfully signed");
                            }
                            else
                            {
                                Console.WriteLine("Message was not successfully signed");
                            }
                        }
                        catch(CryptographicException e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Client doesn’t have the public key");
                }
            }
            else
            {
                Console.WriteLine("You need to do a User Post or User Set first");
            }
        }

        static async Task GetAddFifty(string pIntString)
        {
            if (user.UserName != null && user.ApiKey != null)
            {
                try
                {
                    int integer = int.Parse(pIntString);
                    if (publicKey != null)
                    {
                        using (AesManaged AES = new AesManaged())
                        {
                            AES.GenerateKey();
                            AES.GenerateIV();
                            byte[] aesKey = AES.Key;
                            byte[] aesIV = AES.IV;
                            using(RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                            {
                                RSA.FromXmlString(publicKey);
                                byte[] encryptedInteger = RSA.Encrypt(BitConverter.GetBytes(integer), false);
                                byte[] encryptedKey = RSA.Encrypt(aesKey, false);
                                byte[] encryptedIV = RSA.Encrypt(aesIV, false);
                                string hexEncryptedInteger = BitConverter.ToString(encryptedInteger);
                                string hexEncryptedKey = BitConverter.ToString(encryptedKey);
                                string hexEncryptedIV = BitConverter.ToString(encryptedIV);
                                var requestMessage = new HttpRequestMessage(HttpMethod.Get, 
                                    "api/protected/addfifty?encryptedInteger=" + hexEncryptedInteger 
                                    + "&encryptedsymkey=" + hexEncryptedKey 
                                    + "&encryptedIV=" + hexEncryptedIV);
                                requestMessage.Headers.Add("ApiKey", user.ApiKey.ToString());
                                HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                                string response = await responseMessage.Content.ReadAsStringAsync();
                                response = response.Replace("\"", "");
                                try
                                {
                                    byte[] encryptedByteResponse = response.Split('-').Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber))
                                        .ToArray();
                                    byte[] byteResponse = AES.CreateDecryptor(aesKey, aesIV).TransformFinalBlock(encryptedByteResponse, 0, encryptedByteResponse.Length);
                                    int integerOutput = BitConverter.ToInt32(byteResponse, 0);
                                    Console.WriteLine(integerOutput.ToString());
                                }catch(Exception e)
                                {
                                    Console.WriteLine("An error occurred!");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Client doesn’t have the public key");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("A valid integer must be given!");
                }
            }
            else
            {
                Console.WriteLine("You need to do a User Post or User Set first");
            }
        }

            static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine(greetingString);
                string inputString = Console.ReadLine();
                string[] inputArgs = inputString.Split(null);
                RunAsync(inputArgs).GetAwaiter().GetResult();
                greetingString = "What would you like to do next?";
            }
        }

        static async Task RunAsync(string[] args)
        {
            if (clientInitialised == false)
            {
                client.BaseAddress = new Uri("http://localhost:24702/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                clientInitialised = true;
            }
            Console.WriteLine(waitingString);
            try
            {
                switch (args[0])
                {
                    case "Exit":
                        Environment.Exit(0);
                        break;
                    case "TalkBack":
                        switch (args[1])
                        {
                            case "Hello":
                                await GetHello("unprotected");
                                break;
                            case "Sort":
                                await GetSort(args[2]);
                                break;
                            default:
                                Console.WriteLine("Error: Unrecognised Command");
                                break;
                        }
                        break;
                    case "User":
                        switch (args[1])
                        {
                            case "Get":
                                await GetUser(args[2]);
                                break;
                            case "Post":
                                await PostUser(args[2]);
                                break;
                            case "Set":
                                await SetUser(args[2], args[3]);
                                break;
                            case "Delete":
                                await DeleteUser();
                                break;
                            default:
                                Console.WriteLine("Error: Unrecognised Command");
                                break;
                        }
                        break;
                    case "Protected":
                        switch (args[1])
                        {
                            case "Hello":
                                await GetHello("Protected");
                                break;
                            case "SHA1":
                                await ProtectedSHA1(args[2]);
                                break;
                            case "SHA256":
                                await ProtectedSHA256(args[2]);
                                break;
                            case "Get":
                                switch (args[2])
                                {
                                    case "PublicKey":
                                        await GetPublicKey();
                                        break;
                                    default:
                                        Console.WriteLine("Error: Unrecognised Command");
                                        break;
                                }
                                break;
                            case "Sign":
                                await getSign(args[2]);
                                break;
                            case "AddFifty":
                                await GetAddFifty(args[2]);
                                break;
                            default:
                                Console.WriteLine("Error: Unrecognised Command");
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine("Error: Unrecognised Command");
                        break;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
