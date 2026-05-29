using System;
using System.Reflection;
using System.Collections.Generic;

namespace BankCore.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Banking System - Unit Test Runner";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================");
            Console.WriteLine("          BANKING SYSTEM - UNIT TEST RUNNER         ");
            Console.WriteLine("====================================================");
            Console.ResetColor();
            Console.WriteLine();

            var testTypes = new List<Type>
            {
                typeof(Tests.CustomerServiceTests),
                typeof(Tests.AccountServiceTests),
                typeof(Tests.CertificateServiceTests),
                typeof(Tests.CreditCardServiceTests),
                typeof(Tests.ReportingServiceTests)
            };

            int passed = 0;
            int failed = 0;
            int total = 0;

            foreach (var type in testTypes)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Running suite: {type.Name}");
                Console.WriteLine(new string('-', type.Name.Length + 15));
                Console.ResetColor();

                object instance = Activator.CreateInstance(type);
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var method in methods)
                {
                    if (method.Name.StartsWith("Test"))
                    {
                        total++;
                        Console.Write($"  -> {method.Name,-45} ");

                        try
                        {
                            method.Invoke(instance, null);
                            passed++;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("[PASSED]");
                            Console.ResetColor();
                        }
                        catch (TargetInvocationException tie)
                        {
                            failed++;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[FAILED]");
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine($"     Error: {tie.InnerException?.Message}");
                            Console.ResetColor();
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[ERROR]");
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine($"     Error: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================");
            Console.WriteLine("                    SUMMARY                         ");
            Console.WriteLine("====================================================");
            Console.ResetColor();

            Console.Write("  Total Tests Run: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(total);
            Console.ResetColor();

            Console.Write("  Passed:          ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(passed);
            Console.ResetColor();

            Console.Write("  Failed:          ");
            if (failed > 0) Console.ForegroundColor = ConsoleColor.Red;
            else Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(failed);
            Console.ResetColor();

            Console.WriteLine();
            if (failed == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ✓ ALL TESTS COMPLETED SUCCESSFULLY!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ✗ SOME TESTS FAILED. PLEASE REVIEW RESULTS.");
            }
            Console.ResetColor();
            Console.WriteLine("====================================================");
            Console.WriteLine();

            if (Environment.UserInteractive)
            {
                Console.WriteLine("Press any key to exit...");
                try { Console.ReadKey(); } catch { }
            }

            Environment.Exit(failed == 0 ? 0 : -1);
        }
    }
}
