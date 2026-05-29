using System;
using System.IO;

namespace BankCore.Infrastructure
{
    public class BankLogger
    {
        private static readonly object LockObj = new object();
        private static BankLogger _instance;
        private readonly string _logFilePath;

        private BankLogger()
        {
            // Default to running directory
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            _logFilePath = Path.Combine(directory, "bank_log.txt");
        }

        public static BankLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new BankLogger();
                        }
                    }
                }
                return _instance;
            }
        }

        public string LogFilePath => _logFilePath;

        public void Log(string level, string message, string component = "General")
        {
            try
            {
                lock (LockObj)
                {
                    string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.FFF}] [{level.ToUpper()}] [{component}] {message}";
                    File.AppendAllText(_logFilePath, formattedMessage + Environment.NewLine);
                }
            }
            catch (Exception)
            {
                // Fallback to Console to prevent application crash on log failures
                Console.WriteLine($"[LOG FAILURE] [{level}] [{component}] {message}");
            }
        }

        public void LogInfo(string message, string component = "General")
        {
            Log("INFO", message, component);
        }

        public void LogWarning(string message, string component = "General")
        {
            Log("WARN", message, component);
        }

        public void LogError(string message, Exception ex = null, string component = "General")
        {
            string fullMessage = message;
            if (ex != null)
            {
                fullMessage += $" | Exception: {ex.Message} | StackTrace: {ex.StackTrace}";
            }
            Log("ERROR", fullMessage, component);
        }
    }
}
