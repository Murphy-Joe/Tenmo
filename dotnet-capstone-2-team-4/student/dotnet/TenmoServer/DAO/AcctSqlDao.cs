using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class AcctSqlDao : IAcctDao
    {
        private readonly string connectionString;

        public AcctSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public decimal GetBalance(string userName)
        {
            decimal balance = 0m;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"select balance from accounts where user_id = 
                                                            (select user_id from users where username = @userName)", conn);
                    cmd.Parameters.AddWithValue("@userName", userName);
                    balance = Convert.ToDecimal(cmd.ExecuteScalar());

                }
            }
            catch (SqlException)
            {
                throw;
            }
            return balance;
        }
        public Transfer CreateTransfer(Transfer newTransfer)
        {
            int typeId = 0;
            int transferStatusId = 0;
            int fromAccountId = 0;
            int toAccountId = 0;
            int newTransferId = 0;
            if (newTransfer.Type.ToLower() == "sent")
            {
                typeId = 2;
            }
            else
            {
                typeId = 1;
            }
            if (newTransfer.Status.ToLower() == "pending")
            {
                transferStatusId = 1;
            }
            else if (newTransfer.Status.ToLower() == "approved")
            {
                transferStatusId = 2;
            }
            else if (newTransfer.Status.ToLower() == "rejected")
            {
                transferStatusId = 3;
            }



            fromAccountId = GetAcctIdFromUserName(newTransfer.From);
            toAccountId = GetAcctIdFromUserName(newTransfer.To);


            // Updating Database with new transfer
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"INSERT INTO transfers (transfer_type_id, transfer_status_id, account_from, account_to, amount) OUTPUT INSERTED.transfer_id VALUES(@transfer_type_id, @transfer_status_id, @account_from, @account_to, @amount) ", conn);
                    cmd.Parameters.AddWithValue("@transfer_type_id", typeId);
                    cmd.Parameters.AddWithValue("@transfer_status_id", transferStatusId);
                    cmd.Parameters.AddWithValue("@account_from", fromAccountId);
                    cmd.Parameters.AddWithValue("@account_to", toAccountId);
                    cmd.Parameters.AddWithValue("@amount", newTransfer.Amount);

                    newTransferId = Convert.ToInt32(cmd.ExecuteScalar());
                }

            }
            catch (Exception)
            {

                throw;
            }
            newTransfer.Id = newTransferId;

            return newTransfer;



        }

        public List<Transfer> GetTransferHistory(string userName)
        {
            List<Transfer> transfers = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"SELECT transfer_id, transfer_type_desc, transfer_status_desc, 
                    (SELECT username
                    FROM users
                    JOIN accounts ON users.user_id = accounts.user_id
                    WHERE account_id = account_from) as 'From',
                    (SELECT username
                    FROM users
                    JOIN accounts ON users.user_id = accounts.user_id
                    WHERE account_id = account_to) as 'To',
                    amount 
                    FROM users
                    JOIN accounts on users.user_id = accounts.user_id
                    JOIN transfers on account_id = account_from OR account_id = account_to
                    JOIN transfer_types on transfers.transfer_type_id = transfer_types.transfer_type_id
                    JOIN transfer_statuses on transfers.transfer_status_id = transfer_statuses.transfer_status_id
                    WHERE username = @userName", conn);
                    cmd.Parameters.AddWithValue("@userName", userName);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Transfer xfer = GetTransferFromReader(reader);
                        transfers.Add(xfer);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return transfers;
        }

        public Transfer GetTransferFromReader(SqlDataReader reader)
        {
            Transfer xfer = new Transfer()
            {
                Id = Convert.ToInt32(reader["transfer_id"]),
                Type = Convert.ToString(reader["transfer_type_desc"]),
                Status = Convert.ToString(reader["transfer_status_desc"]),
                From = Convert.ToString(reader["From"]),
                To = Convert.ToString(reader["To"]),
                Amount = Convert.ToDecimal(reader["amount"])
            };

            return xfer;
        }


        public string GetUserNameFromAcctId(int acctId)
        {
            string userNameFromAcctId = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"SELECT username
                        FROM users
                        JOIN accounts ON users.user_id = accounts.user_id
                        WHERE account_id = @acctId", conn);
                    cmd.Parameters.AddWithValue("@acctId", acctId);
                    userNameFromAcctId = Convert.ToString(cmd.ExecuteScalar());
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return userNameFromAcctId;
        }

        public int GetAcctIdFromUserName(string userName)
        {
            int toAccountId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"SELECT account_id FROM accounts JOIN users ON accounts.user_id = users.user_id WHERE username = @username ", conn);
                    cmd.Parameters.AddWithValue("@username", userName);
                    toAccountId = Convert.ToInt32(cmd.ExecuteScalar());
                }

            }
            catch (Exception)
            {

                throw;
            }
            return toAccountId;
        }

        public bool UpdateBalance(decimal amount, string username)
        {
            int rowsAffected = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"UPDATE accounts SET balance += @amount WHERE user_id = (SELECT user_id FROM users WHERE username = @username)", conn);
                    cmd.Parameters.AddWithValue(@"username", username);
                    cmd.Parameters.AddWithValue(@"amount", amount);

                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Update balance failed.");
            }
            return rowsAffected == 1;
        }

        public bool UpdateTransfer(Transfer updatedTransfer)
        {
            int transferId = Convert.ToInt32(updatedTransfer.Id);
            string status = updatedTransfer.Status.ToLower();
            int statusId = 0;
            if (status == "pending")
            {
                statusId = 1;
            }
            else if (status == "approved")
            {
                statusId = 2;
            }
            else if (status == "rejected")
            {
                statusId = 3;
            }
            
            
            int rowsAffected = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"UPDATE transfers SET transfer_status_id = @statusId WHERE transfer_id = @transferId", conn);
                    cmd.Parameters.AddWithValue(@"transferId", transferId);
                    cmd.Parameters.AddWithValue(@"statusId", statusId);

                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Update balance failed.");
            }
            return rowsAffected == 1;
        }

        public bool ApproveTransfer(Transfer approvedTransfer)
        {
            try
            {
                UpdateBalance(approvedTransfer.Amount * (-1), approvedTransfer.From);
                UpdateBalance(approvedTransfer.Amount, approvedTransfer.To);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
            
        }
    }
}

