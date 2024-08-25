// See https://aka.ms/new-console-template for more information
using BankingApplication.BusinessLogicLayer;
using BankingApplication.DataAccessLayer;
using BankingApplication.Models;
using System.Data.SQLite;


AccountManager accountManager = new AccountManager();

Console.WriteLine("Welcome to the Banking Application!");

while (true)
{
    Console.WriteLine("Choose an option:");
    Console.WriteLine("1. Register a New User");
    Console.WriteLine("2. Create a New Account");
    Console.WriteLine("3. Deposit Funds");
    Console.WriteLine("4. Withdraw Funds");
    Console.WriteLine("5. Exit");
    Console.WriteLine("6. Check Balance");
    Console.WriteLine("7. View Transaction History");



    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            RegisterUser(accountManager);
            break;
        case "2":
            CreateAccount(accountManager);
            break;
        case "3":
            DepositFunds(accountManager);
            break;
        case "4":
            WithdrawFunds(accountManager);
            break;
        case "5":
            return;
        case "6":
            CheckBalance(accountManager);
            break;
        case "7":
            ViewTransactionHistory(accountManager);
            break;
        default:
            Console.WriteLine("Invalid option. Please try again.");
            break;

    }
}


    static void RegisterUser(AccountManager accountManager)
    {
        Console.WriteLine("Register User:");
        Console.Write("Enter Name: ");
        string name = Console.ReadLine();
        Console.Write("Enter Address: ");
        string address = Console.ReadLine();
        Console.Write("Enter Password: ");
        string password = Console.ReadLine();
        Console.Write("Enter Initial Balance: ");
        decimal initialBalance = decimal.Parse(Console.ReadLine());

        User newUser = new User
        {
            Name = name,
            Address = address,
            Password = password
        };

        accountManager.RegisterUser(newUser, initialBalance);
    }

static void CreateAccount(AccountManager accountManager)
{
    Console.WriteLine("Create a New Account:");
    Console.Write("Enter User ID: ");
    int userId = int.Parse(Console.ReadLine());

    Console.Write("Enter Account Type (Savings/Current): ");
    string accountType = Console.ReadLine();

    Account newAccount = new Account
    {
        UserId = userId,
        AccountType = accountType,
        Balance = 0
    };

    accountManager.CreateAccount(newAccount);
    Console.WriteLine("Account Created Successfully!");
}

static void DepositFunds(AccountManager accountManager)
{
    Console.WriteLine("Deposit Funds:");
    Console.Write("Enter Account ID: ");
    int accountId = int.Parse(Console.ReadLine());

    Console.Write("Enter Amount: ");
    decimal amount = decimal.Parse(Console.ReadLine());

    accountManager.Deposit(accountId, amount);
    Console.WriteLine("Funds Deposited Successfully!");
}

static void WithdrawFunds(AccountManager accountManager)
{
    Console.WriteLine("Withdraw Funds:");
    Console.Write("Enter Account ID: ");
    int accountId = int.Parse(Console.ReadLine());

    Console.Write("Enter Amount: ");
    decimal amount = decimal.Parse(Console.ReadLine());

    try
    {
        accountManager.Withdraw(accountId, amount);
        Console.WriteLine("Funds Withdrawn Successfully!");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

static void CheckBalance(AccountManager accountManager)
{
    Console.WriteLine("Check Balance:");
    Console.Write("Enter Account ID: ");
    int accountId = int.Parse(Console.ReadLine());

    var balance = accountManager.GetBalance(accountId);
    Console.WriteLine($"The current balance for Account ID {accountId} is: {balance:C}");
}

static void ViewTransactionHistory(AccountManager accountManager)
{
    Console.WriteLine("View Transaction History:");
    Console.Write("Enter Account ID: ");
    int accountId = int.Parse(Console.ReadLine());

    using (var connection = new SQLiteConnection(SQLiteDatabaseHandler.ConnectionString))
    {
        connection.Open();

        string query = @"SELECT * FROM Transactions WHERE AccountId = @AccountId ORDER BY Date DESC;";
        using (var command = new SQLiteCommand(query, connection))
        {
            command.Parameters.AddWithValue("@AccountId", accountId);

            using (var reader = command.ExecuteReader())
            {
                Console.WriteLine("Transaction ID | Amount | Type      | Date");

                while (reader.Read())
                {
                    Console.WriteLine($"{reader["TransactionId"],-15} | {reader["Amount"],-6:C} | {reader["TransactionType"],-10} | {reader["Date"]}");
                }
            }
        }
    }
}

static void Login(AccountManager accountManager)
{
    Console.WriteLine("Login:");
    Console.Write("Enter Account Number: ");
    int accountId = int.Parse(Console.ReadLine());
    Console.Write("Enter Password: ");
    string password = Console.ReadLine();

    bool isAuthenticated = accountManager.AuthenticateUser(accountId, password);

    if (isAuthenticated)
    {
        Console.WriteLine("Login successful!");
        // Proceed with further operations
    }
    else
    {
        Console.WriteLine("Invalid Account Number or Password.");
    }
}

static void FreezeAccount(AccountManager accountManager)
{
    Console.Write("Enter the USSD code to freeze your account: ");
    string ussdCode = Console.ReadLine();

    if (ussdCode == "*391#")  // Example USSD code
    {
        Console.Write("Enter your Account ID: ");
        int accountId = int.Parse(Console.ReadLine());
        accountManager.FreezeAccount(accountId);
    }
    else
    {
        Console.WriteLine("Invalid USSD code.");
    }
}
