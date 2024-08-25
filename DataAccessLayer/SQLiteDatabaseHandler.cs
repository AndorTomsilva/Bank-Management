using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace BankingApplication.DataAccessLayer
{
    public class SQLiteDatabaseHandler
    {
        public const string ConnectionString = "Data Source=banking.db;Version=3;";

        public SQLiteDatabaseHandler()
        {
            CreateTables();
        }

        private void CreateTables()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string userTable = @"CREATE TABLE IF NOT EXISTS Users (
                                UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                                Name TEXT NOT NULL,
                                Address TEXT NOT NULL,
                                Password TEXT NOT NULL,
                                PhoneNumber TEXT NOT NULL,
                                Email TEXT NOT NULL
                            );";

                string accountTable = @"CREATE TABLE IF NOT EXISTS Accounts (
                                   AccountId INTEGER PRIMARY KEY AUTOINCREMENT,
                                   UserId INTEGER NOT NULL,
                                   AccountType TEXT NOT NULL,
                                   Balance REAL NOT NULL,
                                   FOREIGN KEY(UserId) REFERENCES Users(UserId)
                               );";

                string transactionTable = @"CREATE TABLE IF NOT EXISTS Transactions (
                                        TransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
                                        AccountId INTEGER NOT NULL,
                                        Amount REAL NOT NULL,
                                        TransactionType TEXT NOT NULL,
                                        Date DATETIME DEFAULT CURRENT_TIMESTAMP,
                                        FOREIGN KEY(AccountId) REFERENCES Accounts(AccountId)
                                    );";

                using (var command = new SQLiteCommand(userTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(accountTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(transactionTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
