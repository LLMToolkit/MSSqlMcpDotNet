using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSqlMcpDotNet;

internal class SetupHelper
{


    /// <summary>
    /// Configures the SQL Server database connection by prompting the user for connection details.
    /// </summary>
    /// <remarks>This method interacts with the user to gather necessary information for setting up a database
    /// connection. It supports both integrated security and username/password authentication. The method saves the
    /// connection string based on the provided details.</remarks>
    public static void SetupDatabaseConnection()
    {
        Console.WriteLine("SQL Server connection configuration not found.");
        Console.WriteLine("Please enter the details:");

        string instancia = PromptWithRetry("Server instance: ");
        if (instancia == null)
            return;

        string integratedSecurity = PromptIntegratedSecurity();
        if (integratedSecurity == null) return;
        if (integratedSecurity.Equals("Y", StringComparison.OrdinalIgnoreCase))
        {
            
            Console.WriteLine("Saving configuration...");
            ConfigHelper.SaveConnectionStringTrusted(instancia);
            Console.WriteLine("Configuration saved. Exiting.");
            return;
        }

        string usuario = PromptWithRetry("User: ");
        if (usuario == null)
            return;

        string password = PromptPasswordWithRetry("Password: ");
        if (password == null)
            return;

        Console.WriteLine("Saving configuration...");
        ConfigHelper.SaveConnectionString(instancia, usuario, password);
        Console.WriteLine("Configuration saved. Exiting.");
    }





    private static string PromptIntegratedSecurity()
    {
        string integratedSecurity = null;
        for (int i = 0; i < 3; i++)
        {
            Console.Write("Integrated Security? (Y/N): ");
            integratedSecurity = Console.ReadLine();
            if (integratedSecurity.Equals("Y", StringComparison.OrdinalIgnoreCase) ||
                integratedSecurity.Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                return integratedSecurity;
            }
            Console.WriteLine("Invalid response.");
            integratedSecurity = null;
        }
        if (integratedSecurity == null)
        {
            Console.WriteLine("Too many failed attempts. Exiting.");
        }
        return null;
    }


    // Requests data from console, retrying up to 3 times if empty or null.
    private static string PromptWithRetry(string prompt)
    {
        for (int i = 0; i < 3; i++)
        {
            Console.Write(prompt);
            string value = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
            Console.WriteLine("Value cannot be empty. Please try again.");
        }
        Console.WriteLine("Too many failed attempts. Exiting.");
        return null;
    }



    // Requests password with masking, retrying up to 3 times if empty or null.
    private static string PromptPasswordWithRetry(string prompt)
    {
        for (int i = 0; i < 3; i++)
        {
            Console.Write(prompt);
            string password = ReadPassword();
            if (!string.IsNullOrWhiteSpace(password))
                return password;
            Console.WriteLine("Value cannot be empty. Please try again.");
        }
        Console.WriteLine("Too many failed attempts. Exiting.");
        return null;
    }

    // Method to read password without displaying it in the console.
    private static string ReadPassword()
    {
        string password = string.Empty;
        ConsoleKeyInfo info;
        do
        {
            info = Console.ReadKey(intercept: true);
            if (info.Key != ConsoleKey.Backspace && info.Key != ConsoleKey.Enter)
            {
                password += info.KeyChar;
                Console.Write("*");
            }
            else if (info.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                int pos = Console.CursorLeft;
                Console.SetCursorPosition(pos - 1, Console.CursorTop);
                Console.Write(" ");
                Console.SetCursorPosition(pos - 1, Console.CursorTop);
            }
        } while (info.Key != ConsoleKey.Enter);
        Console.WriteLine();
        return password;
    }


}