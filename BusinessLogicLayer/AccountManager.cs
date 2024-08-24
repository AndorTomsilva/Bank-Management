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
                    string userQuery = @"INSERT INTO Users (UserId, Name, Address, Password) 
                                         VALUES (@UserId, @Name, @Address, @Password);";

                    using (var userCommand = new SQLiteCommand(userQuery, connection))
                    {
                        userCommand.Parameters.AddWithValue("@UserId", user.UserId);
                        userCommand.Parameters.AddWithValue("@Name", user.Name);
                        userCommand.Parameters.AddWithValue("@Address", user.Address);
                        userCommand.Parameters.AddWithValue("@Password", user.Password);

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
                        // Cast to double and then convert to decimal
                        double balanceAsDouble = (double)result;
                        return Convert.ToDecimal(balanceAsDouble);
                    }
                    else
                    {
                        Console.WriteLine("Account not found. ");
                       // throw new Exception("Account not found.");
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
                using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
                {
                    connection.Open();

                    string balanceQuery = @"SELECT Balance FROM Accounts WHERE AccountId = @AccountId;";
                    using (var balanceCommand = new SQLiteCommand(balanceQuery, connection))
                    {
                        balanceCommand.Parameters.AddWithValue("@AccountId", accountId);
                        var balance = (decimal)balanceCommand.ExecuteScalar();

                        if (balance < amount)
                        {
                            throw new InvalidOperationException("Insufficient funds.");
                        }
                    }

                    string query = @"UPDATE Accounts SET Balance = Balance - @Amount WHERE AccountId = @AccountId;";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Amount", amount);
                        command.Parameters.AddWithValue("@AccountId", accountId);
                        command.ExecuteNonQuery();
                    }

                    LogTransaction(accountId, amount, "Withdrawal");
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

    }
}


