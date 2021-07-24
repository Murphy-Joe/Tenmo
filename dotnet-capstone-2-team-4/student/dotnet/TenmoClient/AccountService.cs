using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Text;
using TenmoClient.Models;
using TenmoServer.Models;

namespace TenmoClient
{
    // called from client Program.cs
    // sends request to AccountController when requesting a response
    // gets back response and returns it to the calling program, Program.cs

    class AccountService
    {
        private readonly static string API_BASE_URL = "https://localhost:44315/account";
        private readonly static IRestClient client = new RestClient();



        public static decimal GetBalance()
        {
            // resp<> = client get<> request
            RestRequest req = new RestRequest($"{API_BASE_URL}/balance");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<decimal> resp = client.Get<decimal>(req);

            decimal respData = resp.Data;

            return respData;
        }

        public static List<Transfer> GetTransferHistory()
        {
            // resp<> = client get<> request
            RestRequest req = new RestRequest($"{API_BASE_URL}/transfer/history");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<List<Transfer>> resp = client.Get<List<Transfer>>(req);

            List<Transfer> respData = resp.Data;

            return respData;
        }

        public static List<string> GetUserList()
        {
            // resp<> = client get<> request
            RestRequest req = new RestRequest($"{API_BASE_URL}/transfer/users");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<List<string>> resp = client.Get<List<string>>(req);

            List<string> respData = resp.Data;

            return respData;
        }

        public static Transfer CreateTransfer(Transfer transferObj)
        {

            RestRequest req = new RestRequest($"{API_BASE_URL}/transfer");
            req.AddJsonBody(transferObj);
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<Transfer> resp = client.Post<Transfer>(req);

            if (resp.ResponseStatus != ResponseStatus.Completed)
            {
                Console.WriteLine("Error occurred - unable to reach server");
                Console.WriteLine("No transfer has been recorded, press any key to return to the main menu: ");
                Console.ReadKey();
                return null;
            }
            if (!resp.IsSuccessful)
            {
                Console.WriteLine("Error occurred - received a non - success response");
                Console.WriteLine("No transfer has been recorded, press any key to return to the main menu: ");
                Console.ReadKey();
                return null;
            }

            return resp.Data;
        }

        public static bool UpdateTransfer(Transfer transferObj)
        {

            RestRequest req = new RestRequest($"{API_BASE_URL}/transfer/update");
            req.AddJsonBody(transferObj);
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse resp = client.Post<Transfer>(req);

            if (resp.Content == "true")
            {
                return true;
            }
            else if (resp.ResponseStatus != ResponseStatus.Completed)
            {
                Console.WriteLine("Error occurred on transfer update - unable to reach server");
                Console.WriteLine(resp.ErrorMessage);
                return false;
            }
            else if (!resp.IsSuccessful)
            {
                Console.WriteLine("Error occurred on transfer update - received a non - success response");
                Console.WriteLine(resp.ErrorMessage);
                return false;
            }
            else
            {
                Console.WriteLine("No server or repsonse errors, but update was not successful");
                return false;
            }

        }

        public static bool ApproveTransfer(Transfer transferObj)
        {
            {

                RestRequest req = new RestRequest($"{API_BASE_URL}/transfer/approve");
                req.AddJsonBody(transferObj);
                client.Authenticator = new JwtAuthenticator(UserService.GetToken());
                IRestResponse resp = client.Post<Transfer>(req);

                if (resp.Content == "true")
                {
                    Console.WriteLine("Transfer complete");
                    Console.WriteLine($"Your new balance is: ${GetBalance()}");
                    return true;
                }

                else if (resp.ResponseStatus != ResponseStatus.Completed)
                {
                    Console.WriteLine("Error occurred on approving transfer - unable to reach server");
                    Console.WriteLine("No balances have changed");
                    Console.WriteLine(resp.ErrorMessage);
                    return false;
                }
                else if (!resp.IsSuccessful)
                {
                    Console.WriteLine("Error occurred on approving transfer - received a non - success response");
                    Console.WriteLine("No balances have changed");
                    Console.WriteLine(resp.ErrorMessage);
                    return false;
                }

                else
                {
                    Console.WriteLine("No server or repsonse errors, but update was not successful");
                    return false;
                }

                
            }
        }
    }
}