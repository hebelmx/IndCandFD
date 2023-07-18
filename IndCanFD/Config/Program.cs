namespace Config;

/// <summary>
/// Program class to run the application.
/// </summary>
internal class Program
{
    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    private static async Task Main(string[] args)
    {
        //"Data Source=test.db";
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "PcCan.db");
        var connectionString = $"Data Source={dbPath}";
        
        // Create the application
        var app = new ConfigApplication(connectionString);

        switch (args.Length)
        {
            // Command for initializing the database
            case 1 when args[0].ToLower() == "initdb":
                app.InitializeDatabase();
                Console.WriteLine("Database initialized.");
                return;
            // If no command is provided, display the help text
            case < 1:
                Console.WriteLine(app.GetHelpText());
                return;
        }

        // Parse command and ID from input arguments
        var command = args[0].ToLower();
        var id = args.Length > 1 ? int.Parse(args[1]) : 0;  // Default ID to 0 if not provided

        // Handle each command case
        switch (command)
        {
            case "help":
                Console.WriteLine(app.GetHelpText());
                break;

            case "read":
                Console.WriteLine(await app.ReadConfigData(id));
                break;

            case "write":
                if (args.Length >= 3)
                {
                    Console.WriteLine(await app.WriteConfigData(id, args[2]));
                }
                else
                {
                    Console.WriteLine(app.GetHelpText());
                }
                break;

            case "reset":
                Console.WriteLine(await app.ResetConfigData(id));
                break;

            case "dump":
                Console.WriteLine(await app.DumpAllData());
                break;

            case "dumphistory":
                Console.WriteLine(await app.HandleDumpHistoryCommand(args));
                break;

            case "len":
                Console.WriteLine(await app.UpdateCommandLength(args));
                break;

            default:
                Console.WriteLine(app.GetHelpText());
                break;
        }
    }
}