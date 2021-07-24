using System;
using System.Collections.Generic;
using TenmoClient.Models;
using TenmoServer.Models;
using ConsoleTables;

namespace TenmoClient
{
    public class ConsoleService
    {
        /// <summary>
        /// Prompts for transfer ID to view, approve, or reject
        /// </summary>
        /// <param name="action">String to print in prompt. Expected values are "Approve" or "Reject" or "View"</param>
        /// <returns>ID of transfers to view, approve, or reject</returns>
        public int PromptForTransferID(string action)
        {
            Console.WriteLine("");
            Console.Write("Please enter transfer ID to " + action + " (0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int auctionId))
            {
                Console.WriteLine("Invalid input. Only input a number.");
                return 0;
            }
            else
            {
                return auctionId;
            }
        }

        public LoginUser PromptForLogin()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            string password = GetPasswordFromConsole("Password: ");

            LoginUser loginUser = new LoginUser
            {
                Username = username,
                Password = password
            };
            return loginUser;
        }

        private string GetPasswordFromConsole(string displayMessage)
        {
            string pass = "";
            Console.Write(displayMessage);
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (!char.IsControl(key.KeyChar))
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Remove(pass.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine("");
            return pass;
        }

        public static string TransferTo()
        {
            string userChoice = "";

            Console.WriteLine("Please select a user to send money to.");
            List<string> allUsers = AccountService.GetUserList();
            Dictionary<int, string> displayUsers = new Dictionary<int, string>();
            for (int i = 1; i <= allUsers.Count; i++)
            {
                displayUsers.Add(i, allUsers[i - 1]);
            }
            foreach (KeyValuePair<int, string> user in displayUsers)
            {
                Console.WriteLine($"{user.Key}: {user.Value}");
            }
            try
            {
                int userKey = Convert.ToInt32(Console.ReadLine());
                if (!displayUsers.ContainsKey(userKey))
                {
                    Console.WriteLine("User does not exsist.");
                }
                else
                {
                    userChoice = displayUsers[userKey];
                }
            }
            catch
            {
                Console.WriteLine("Please typer a number");
            }
            return userChoice;
        }

        public static Dictionary<int, string> DisplayUserListAsDict()
        {
            // Console.Clear()
            // Console.WriteLine() prompt must come before method

            List<string> allUsers = AccountService.GetUserList();
            Dictionary<int, string> displayUsers = new Dictionary<int, string>();
            for (int i = 1; i <= allUsers.Count; i++)
            {
                displayUsers.Add(i, allUsers[i - 1]);
            }
            foreach (KeyValuePair<int, string> user in displayUsers)
            {
                Console.WriteLine($"{user.Key}: {user.Value}");
            }

            return displayUsers;
        }

        public static string ReturnSelectedUserFromList(Dictionary<int, string> users)
        {

            while (true)
            {
                int userKey = 0;
                string userResponse = Console.ReadLine();
                bool notInt = !int.TryParse(userResponse, out userKey);
                bool inputError = notInt && userResponse.ToLower() != "x";

                if (userResponse.ToLower() == "x")
                {
                    return "x";
                }

                else if (inputError)
                {
                    Console.Clear();
                    Console.WriteLine($"You entered {userResponse}");
                    Console.Write("Please select a user by their numerical value (x to cancel): ");
                    DisplayUserListAsDict();
                }
                else if (!users.ContainsKey(userKey))
                {
                    Console.Clear();
                    Console.Write($"{userKey} does not correspond with a listed user (x to cancel): ");
                    DisplayUserListAsDict();
                }
                else
                {
                    return users[userKey];
                }
            }
        }


        public static decimal AmmountToTransfer(string user)
        {
            decimal amountToTransfer = 0.00M;
            try
            {
                Console.WriteLine($"How much would you like to transfer to {user}? ");
                amountToTransfer = Convert.ToDecimal(Console.ReadLine());
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid ammount!");
            }
            return amountToTransfer;
        }

        public static decimal GetAmount()
        {
            while (true)
            {
                decimal amt = 0;
                string userInput = Console.ReadLine();
                bool numEntered = decimal.TryParse(userInput, out amt);

                if (userInput.ToLower() == "x")
                {
                    return 0;
                }
                else if (!numEntered)
                {
                    Console.WriteLine($"{userInput} was not recognized as a valid amount");
                    Console.Write("Please re-enter or type x to cancel: ");
                }
                else
                {
                    return amt;
                }
            }
        }

        public static void DispayTransferHistory()
        {
            var table = new ConsoleTable("ID", "From", "To", "Amount");
            foreach (var xfer in AccountService.GetTransferHistory())
            {
                table.AddRow(xfer.Id, xfer.From, xfer.To, xfer.Amount);
            }
            table.Write();
        }

        public static int GetTransferIdSelectionFromUser()
        {
            int userTransferId = 0;
            bool userError = true;
            Console.Write("Please enter transfer ID to view details (x to cancel): ");
            while (userError)
            {
                string userResponse = Console.ReadLine().Trim();
                bool intEntered = int.TryParse(userResponse, out userTransferId);
                userError = userResponse != "x" && !intEntered;
                if (userError)
                {
                    Console.Write("Transfer Id must be given as digits (x to cancel): ");
                    continue;
                }
            }
            return userTransferId;


        }

        public static Transfer DispaySelectedTransferDetails(int xferId)
        {
            var table = new ConsoleTable("ID", "Type", "Status", "From", "To", "Amount");
            while (xferId != 0)
            {
                List<Transfer> transferDetails = AccountService.GetTransferHistory();
                foreach (Transfer xfer in transferDetails)
                {
                    if (xferId == xfer.Id)
                    {
                        table.AddRow(xfer.Id, xfer.Type, xfer.Status, xfer.From, xfer.To, xfer.Amount);
                        table.Write(Format.Minimal);
                        return xfer;
                    }
                }
                Console.WriteLine($"No transaction by id {xferId} found");
                xferId = GetTransferIdSelectionFromUser();
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
            }
            return null;

        }

        public static Transfer DisplayPendingTransferDetails(int xferId)
        {

            var table = new ConsoleTable("ID", "Type", "Status", "From", "To", "Amount");
            while (xferId != 0)
            {
                List<Transfer> transferDetails = GetPendingTransfers();
                foreach (Transfer xfer in transferDetails)
                {
                    if (xferId == xfer.Id)
                    {
                        table.AddRow(xfer.Id, xfer.Type, xfer.Status, xfer.From, xfer.To, xfer.Amount);
                        table.Write(Format.Minimal);
                        return xfer;
                    }
                }
                Console.WriteLine($"No transaction by id {xferId} found");
                xferId = GetTransferIdSelectionFromUser();
                
            }
            return null;
        }

        public static List<Transfer> GetPendingTransfers()
        {
            List<Transfer> filterToPendings = AccountService.GetTransferHistory();
            filterToPendings.RemoveAll(xfer => xfer.Status.ToLower() != "pending");
            return filterToPendings;
        }

        public static void DisplayPendingTransfers()
        {
            var table = new ConsoleTable("ID", "From", "To", "Amount");

            List<Transfer> pendingTransfers = GetPendingTransfers();

            foreach (var transfer in pendingTransfers)
            {
                table.AddRow(transfer.Id, transfer.From, transfer.To, transfer.Amount);
            }
            table.Write();

        }

        public static Transfer MakeARequestTransferObject(string userToRequestFrom, decimal amount)
        {
            Transfer newTransfer = new Transfer();
            newTransfer.Amount = amount;
            newTransfer.To = UserService.GetUsername();
            newTransfer.From = userToRequestFrom;
            newTransfer.Type = "Request";
            newTransfer.Status = "Pending";

            return newTransfer;
        }

        public static Transfer MakeASendTransferObject(string userToSendTo, decimal ammount)
        {
            Transfer newTransfer = new Transfer();
            newTransfer.Amount = ammount;
            newTransfer.To = userToSendTo;
            newTransfer.From = UserService.GetUsername();
            newTransfer.Type = "Sent";
            newTransfer.Status = "Approved";

            return newTransfer;
        }

        public static void ViewPendingXfers()
        {
            ConsoleService.DisplayPendingTransfers();
            int TransferId = ConsoleService.GetTransferIdSelectionFromUser();
            if (TransferId == 0)
            {
                return;
            }
            Transfer chosenXfer = ConsoleService.DisplayPendingTransferDetails(TransferId); //22222222222222222222222222222222222
            Console.WriteLine("To approve this request type APPROVE");
            Console.WriteLine("To reject this request type REJECT");
            Console.Write("To return to the main menu type X: ");
            while (true)
            {
                string userDecision = Console.ReadLine().ToLower();
                switch (userDecision)
                {
                    case "x":
                        return;
                    case "reject":
                        chosenXfer.Status = "rejected";
                        AccountService.UpdateTransfer(chosenXfer);
                        Console.WriteLine($"Transaction {chosenXfer.Id} cancelled.");
                        break;
                    case "approve":
                        if (chosenXfer.From != UserService.GetUsername())
                        {
                            Console.WriteLine("You may only authorize transactions for which you are the sender");
                            break;
                        }
                        else if (chosenXfer.Amount > AccountService.GetBalance())
                        {
                            Console.WriteLine("You may only authorize transactions for amounts less than your balance");
                            break;
                        }
                        else
                        {
                            chosenXfer.Status = "approved";
                            bool updateXferSuccess = AccountService.UpdateTransfer(chosenXfer);
                            if (!updateXferSuccess)
                            {
                                break;
                            }
                            bool balanceSuccess = AccountService.ApproveTransfer(chosenXfer);
                            if (!balanceSuccess)
                            {
                                break;
                            }

                        }
                        break;
                    default:
                        Console.Write("Acceptable commands are 'approve', 'reject', or 'x': ");
                        continue;
                }
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
                break;
            }
        }

        public static void ViewPastXfers()
        {
            ConsoleService.DispayTransferHistory();
            int TransferId = ConsoleService.GetTransferIdSelectionFromUser();
            if (TransferId == 0)
            {
                return;
            }
            ConsoleService.DispaySelectedTransferDetails(TransferId);
            Console.WriteLine("Press any key to return to main menu.");
            Console.ReadKey();
        }

        public static void SendTeBucks()
        {
            Console.WriteLine("Please choose a user to send TEbucks to (x to cancel):");
            var userDict = ConsoleService.DisplayUserListAsDict();
            string userToSendTo = ConsoleService.ReturnSelectedUserFromList(userDict);
            if (userToSendTo == "x")
            {
                return;
            }
            Console.Write($"How much would you like to send to {userToSendTo} (x to cancel): ");
            decimal amtToSend = ConsoleService.GetAmount();
            if (amtToSend == 0)
            {
                return;
            }
            while (amtToSend > AccountService.GetBalance())
            {
                Console.WriteLine("Cannot complete transaction - insufficient funds.");
                Console.Write($"How much would you like to send to {userToSendTo} (x to cancel): ");
                amtToSend = ConsoleService.GetAmount();
                if (amtToSend == 0m)
                {
                    return;
                }
            }
            Transfer transferObj = ConsoleService.MakeASendTransferObject(userToSendTo, amtToSend);
            Transfer returnedTransfer = AccountService.CreateTransfer(transferObj);
            if (returnedTransfer == null)
            {
                Console.WriteLine("Error creating transfer, press any key to return to Main Menu");
                Console.ReadKey();
                return;
            }
            bool approved = AccountService.ApproveTransfer(returnedTransfer);
            if (!approved)
            {
                Console.WriteLine($"Error transferring funds, current balance remains {AccountService.GetBalance()} ...press any key to return to Main Menu");
                Console.ReadKey();
                return;
            }
            Console.WriteLine($"Your transfer has been processed and your transfer ID is {returnedTransfer.Id}");
            Console.WriteLine("Press any key to return to main menu.");
            Console.ReadKey();
        }

        public static void ReqTeBucks()
        {
            Console.WriteLine("Please choose a user to request TEbucks");
            var userDict = ConsoleService.DisplayUserListAsDict();
            string userToRequestFrom = ConsoleService.ReturnSelectedUserFromList(userDict);
            if (userToRequestFrom == "")
            {
                return;
            }
            Console.Write($"How much would you like to request from {userToRequestFrom}: ");
            decimal amtToRequest = ConsoleService.GetAmount();
            if (amtToRequest == 0)
            {
                return;
            }
            Transfer transferObj = ConsoleService.MakeARequestTransferObject(userToRequestFrom, amtToRequest);
            Transfer returnedTransfer = AccountService.CreateTransfer(transferObj);
            if (returnedTransfer != null)
            {
                Console.WriteLine($"Your request has been received and your transfer ID is {returnedTransfer.Id}");
                Console.WriteLine("Press any key to return to main menu.");
                Console.ReadKey();
            }
        }
    }
}
