using System;
using System.Data.SQLite;
using System.IO;

namespace BankCore.Infrastructure
{
    public class DatabaseManager
    {
        private static readonly object LockObj = new object();
        private static DatabaseManager _instance;
        private readonly string _dbFilePath;
        private readonly string _connectionString;

        private DatabaseManager()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _dbFilePath = Path.Combine(baseDir, "bank.db");
            _connectionString = $"Data Source={_dbFilePath};Version=3;";
            InitializeDatabase();
        }

        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public string ConnectionString => _connectionString;

        public SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private void InitializeDatabase()
        {
            BankLogger.Instance.LogInfo("Initializing database schema...", "Database");
            try
            {
                using (var conn = GetConnection())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        // Enable Foreign Keys in SQLite
                        cmd.CommandText = "PRAGMA foreign_keys = ON;";
                        cmd.ExecuteNonQuery();

                        // 1. Customers Table
                        cmd.CommandText = @"
                            CREATE TABLE IF NOT EXISTS Customers (
                                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                Name TEXT NOT NULL,
                                Age INTEGER NOT NULL,
                                Gender TEXT NOT NULL,
                                Address TEXT NOT NULL,
                                NationalID TEXT UNIQUE NOT NULL,
                                CreatedAt TEXT NOT NULL
                            );";
                        cmd.ExecuteNonQuery();

                        // 2. Accounts Table
                        cmd.CommandText = @"
                            CREATE TABLE IF NOT EXISTS Accounts (
                                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                CustomerID INTEGER NOT NULL,
                                Balance REAL NOT NULL,
                                Type TEXT NOT NULL,
                                OpenedAt TEXT NOT NULL,
                                IsClosed INTEGER NOT NULL DEFAULT 0,
                                FOREIGN KEY(CustomerID) REFERENCES Customers(ID) ON DELETE CASCADE
                            );";
                        cmd.ExecuteNonQuery();

                        // 3. Transactions Table
                        cmd.CommandText = @"
                            CREATE TABLE IF NOT EXISTS Transactions (
                                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                AccountID INTEGER NOT NULL,
                                Type TEXT NOT NULL,
                                Amount REAL NOT NULL,
                                Description TEXT,
                                Timestamp TEXT NOT NULL,
                                FOREIGN KEY(AccountID) REFERENCES Accounts(ID) ON DELETE CASCADE
                            );";
                        cmd.ExecuteNonQuery();

                        // 4. Certificates Table
                        cmd.CommandText = @"
                            CREATE TABLE IF NOT EXISTS Certificates (
                                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                CustomerID INTEGER NOT NULL,
                                Price REAL NOT NULL,
                                Period INTEGER NOT NULL,
                                InterestRate REAL NOT NULL,
                                PurchaseDate TEXT NOT NULL,
                                FOREIGN KEY(CustomerID) REFERENCES Customers(ID) ON DELETE CASCADE
                            );";
                        cmd.ExecuteNonQuery();

                        // 5. CreditCards Table
                        // CustomerID is UNIQUE to enforce one card per customer constraint at the database layer.
                        cmd.CommandText = @"
                            CREATE TABLE IF NOT EXISTS CreditCards (
                                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                CustomerID INTEGER UNIQUE NOT NULL,
                                CashLimit REAL NOT NULL,
                                CurrentDebt REAL NOT NULL DEFAULT 0,
                                IssuedDate TEXT NOT NULL,
                                Status TEXT NOT NULL,
                                FOREIGN KEY(CustomerID) REFERENCES Customers(ID) ON DELETE CASCADE
                            );";
                        cmd.ExecuteNonQuery();

                        // 6. ServiceActivities Table
                        cmd.CommandText = @"
                            CREATE TABLE IF NOT EXISTS ServiceActivities (
                                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                CustomerID INTEGER NOT NULL,
                                ServiceType TEXT NOT NULL,
                                Description TEXT,
                                Timestamp TEXT NOT NULL,
                                FOREIGN KEY(CustomerID) REFERENCES Customers(ID) ON DELETE CASCADE
                            );";
                        cmd.ExecuteNonQuery();
                    }
                }
                BankLogger.Instance.LogInfo("Database schema initialization completed successfully.", "Database");
            }
            catch (Exception ex)
            {
                BankLogger.Instance.LogError("Error initializing database schema", ex, "Database");
                throw;
            }
        }

        public void ClearDatabase()
        {
            // Helpful for unit testing to reset state
            using (var conn = GetConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA foreign_keys = OFF;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "DELETE FROM ServiceActivities; DELETE FROM CreditCards; DELETE FROM Certificates; DELETE FROM Transactions; DELETE FROM Accounts; DELETE FROM Customers;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "VACUUM;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "PRAGMA foreign_keys = ON;";
                    cmd.ExecuteNonQuery();
                }
            }
            BankLogger.Instance.LogInfo("Database tables cleared successfully.", "Database");
        }
    }
}
