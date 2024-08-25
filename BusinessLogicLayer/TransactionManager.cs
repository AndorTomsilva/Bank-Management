using BankingApplication.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApplication.BusinessLogicLayer
{
    public class TransactionManager
    {
        private readonly SQLiteDatabaseHandler _dbHandler;

        public TransactionManager()
        {
            _dbHandler = new SQLiteDatabaseHandler();
        }

        // Method to get phone number by accountId
        private string GetPhoneNumber(int accountId)
        {
            using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
            {
                connection.Open();

                // Query to get the user's phone number based on accountId
                string query = @"
                SELECT u.PhoneNumber 
                FROM Users u 
                INNER JOIN Accounts a ON u.UserId = a.UserId 
                WHERE a.AccountId = @AccountId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountId", accountId);
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return result.ToString();
                    }
                    else
                    {
                        throw new Exception("Phone number not found for this account.");
                    }
                }
            }
        }

        public void Deposit(int accountId, decimal amount)
        {
            // Deposit logic...

            string phoneNumber = GetPhoneNumber(accountId);
            string message = $"A deposit of {amount:C} has been made to your account.";
            SendNotification(phoneNumber, message);
        }

        public void Withdraw(int accountId, decimal amount)
        {
            // Withdrawal logic...

            string phoneNumber = GetPhoneNumber(accountId);
            string message = $"A withdrawal of {amount:C} has been made from your account.";
            SendNotification(phoneNumber, message);
        }

        public void SendNotification(string phoneNumber, string message)
        {
            // Simulate sending a notification (e.g., SMS)
            Console.WriteLine($"Notification sent to {phoneNumber}: {message}");
        }
    }

}
