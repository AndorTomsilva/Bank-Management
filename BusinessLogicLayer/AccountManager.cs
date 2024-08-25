using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using BankingApplication.Models;
using BankingApplication.DataAccessLayer;

namespace BankingApplication.BusinessLogicLayer
{
    public class AccountManager
    {
        private readonly SQLiteDatabaseHandler _dbHandler;

        public AccountManager()
        {
            _dbHandler = new SQLiteDatabaseHandler();
        }

        // Existing methods...

        private static int GenerateRandomNumber(int digits)
        {
            Random random = new Random();
            return random.Next((int)Math.Pow(10, digits - 1), (int)Math.Pow(10, digits) - 1);
        }

        public void RegisterUser(User user, decimal initialBalance)
        {
            using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    // Generate 11-digit UserId
                    user.UserId = GenerateRandomNumber(11);

                    // Insert the user into the Users table
                    string userQuery = @"INSERT INTO Users (UserId, Name, Address, Password, PhoneNumber, Email) 
                                 VALUES (@UserId, @Name, @Address, @Password, @PhoneNumber, @Email);";

                    using (var userCommand = new SQLiteCommand(userQuery, connection))
                    {
                        userCommand.Parameters.AddWithValue("@UserId", user.UserId);
                        userCommand.Parameters.AddWithValue("@Name", user.Name);
                        userCommand.Parameters.AddWithValue("@Address", user.Address);
                        userCommand.Parameters.AddWithValue("@Password", user.Password);
                        userCommand.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                        userCommand.Parameters.AddWithValue("@Email", user.Email);

                        userCommand.ExecuteNonQuery();
                    }

                    // Generate 10-digit AccountId
                    int accountId = GenerateRandomNumber(10);

                    // Create an account for the new user
                    string accountQuery = @"INSERT INTO Accounts (AccountId, UserId, AccountType, Balance) 
                                    VALUES (@AccountId, @UserId, 'Savings', @Balance);";

                    using (var accountCommand = new SQLiteCommand(accountQuery, connection))
                    {
                        accountCommand.Parameters.AddWithValue("@AccountId", accountId);
                        accountCommand.Parameters.AddWithValue("@UserId", user.UserId);
                        accountCommand.Parameters.AddWithValue("@Balance", initialBalance);

                        accountCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    // Display the UserId and AccountId to the user
                    Console.WriteLine($"Registration successful! Your User ID is: {user.UserId}");
                    Console.WriteLine($"Your Account Number is: {accountId}");
                }
            }
        }


        public void CreateAccount(Account account)
            {
                using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
                {
                    connection.Open();

                    string query = @"INSERT INTO Accounts (UserId, AccountType, Balance) 
                                 VALUES (@UserId, @AccountType, @Balance);";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", account.UserId);
                        command.Parameters.AddWithValue("@AccountType", account.AccountType);
                        command.Parameters.AddWithValue("@Balance", account.Balance);

                        command.ExecuteNonQuery();
                    }
                }
            }
        public decimal GetBalance(int accountId)
        {
            using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
            {
                connection.Open();

                string query = "SELECT Balance FROM Accounts WHERE AccountId = @AccountId;";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountId", accountId);

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDecimal(result);
                    }
                    else
                    {
                        throw new Exception("Account not found.");
                    }
                }
            }
        }




        public void Deposit(int accountId, decimal amount)
            {
                using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
                {
                    connection.Open();

                    string query = @"UPDATE Accounts SET Balance = Balance + @Amount WHERE AccountId = @AccountId;";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Amount", amount);
                        command.Parameters.AddWithValue("@AccountId", accountId);
                        command.ExecuteNonQuery();
                    }

                    LogTransaction(accountId, amount, "Deposit");

                }
          
        }

       



        public void Withdraw(int accountId, decimal amount)
        {
            const int maxRetries = 3;
            int attempts = 0;
            bool success = false;

            while (attempts < maxRetries && !success)
            {
                try
                {
                    attempts++;
                    // Withdraw logic here...

                    using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                decimal currentBalance = GetBalance(accountId);

                                if (currentBalance >= amount)
                                {
                                    decimal newBalance = currentBalance - amount;

                                    string query = "UPDATE Accounts SET Balance = @Balance WHERE AccountId = @AccountId;";
                                    using (var command = new SQLiteCommand(query, connection))
                                    {
                                        command.Parameters.AddWithValue("@Balance", newBalance);
                                        command.Parameters.AddWithValue("@AccountId", accountId);

                                        command.ExecuteNonQuery();
                                    }

                                    transaction.Commit();
                                    Console.WriteLine($"Withdrawal successful. New balance: {newBalance:C}");
                                    // Send notification, etc.
                                }
                                else
                                {
                                    Console.WriteLine("Insufficient funds.");
                                }
                            }
                            catch (Exception)
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }

                    success = true; // Set this if the operation succeeds
                }
                catch (SQLiteException ex) when (ex.Message.Contains("database is locked"))
                {
                    if (attempts == maxRetries)
                    {
                        throw new Exception("Failed to complete the operation after multiple attempts.", ex);
                    }

                    // Optionally, add a delay before retrying
                    System.Threading.Thread.Sleep(100);
                }
            }
        }




        private void LogTransaction(int accountId, decimal amount, string transactionType)
            {
                using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
                {
                    connection.Open();

                    string query = @"INSERT INTO Transactions (AccountId, Amount, TransactionType) 
                         VALUES (@AccountId, @Amount, @TransactionType);";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AccountId", accountId);
                        command.Parameters.AddWithValue("@Amount", amount);
                        command.Parameters.AddWithValue("@TransactionType", transactionType);
                        command.ExecuteNonQuery();
                    }
                }
            }

        public bool AuthenticateUser(int accountId, string password)
        {
            using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
            {
                connection.Open();

                string query = @"SELECT COUNT(*) FROM Users
                         JOIN Accounts ON Users.UserId = Accounts.UserId
                         WHERE Accounts.AccountId = @AccountId AND Users.Password = @Password;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountId", accountId);
                    command.Parameters.AddWithValue("@Password", password);

                    long count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }


        public void FreezeAccount(int accountId)
{
    using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
    {
        connection.Open();

        string query = "UPDATE Accounts SET IsFrozen = 1 WHERE AccountId = @AccountId;";
        using (var command = new SQLiteCommand(query, connection))
        {
            command.Parameters.AddWithValue("@AccountId", accountId);
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                Console.WriteLine("Your account has been successfully frozen due to suspected fraudulent activity.");
            }
            else
            {
                Console.WriteLine("Account not found or already frozen.");
            }
        }
    }
}

    }
}


