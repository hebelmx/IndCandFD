using System.Data.SQLite;
using Config;
using System.Text.RegularExpressions;
using Dapper;

class OldProgram
{
    static async Task OldMain(string[] args)
    {
        const string connectionString = "Data Source=config.db";
        var databaseInitializer = new DatabaseInitializer(connectionString);
        databaseInitializer.InitializeDatabase();

        var configService = new ConfigService(connectionString);

        if (args.Length < 1)
        {
            Console.WriteLine(configService.GetHelpText());
            
            return;
        }

       
        var command = args[0].ToLower();

        var id = 0;
        if (args.Length > 1)
        {
             id = int.Parse(args[1]);
        }
         
      

        switch (command)
        {
            case "help":
                Console.WriteLine(configService.GetHelpText());
                break;

            case "read":
                var configData = await configService.Read(id);
                if (configData != null)
                {
                    Console.WriteLine($"Data: {configData.Data}");
                    Console.WriteLine($"DateTime: {configData.DateTime}");
                }
                else
                {
                    Console.WriteLine($"No data found for ID {id}.");
                }
                break;

            case "write":
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: Config write <id> <data>");
                    return;
                }
                var data = args[2];

                // Check if data is in valid format
                if (!Regex.IsMatch(data, @"^[0-9A-Fa-f]{2}( [0-9A-Fa-f]{2})*$"))
                {
                    Console.WriteLine("Data is not in valid format. It should be hex numbers separated by spaces.");
                    return;
                }


                // Read existing data
                var existingData = await configService.Read(id);
                if (existingData != null)
                {
                    Console.WriteLine($"Existing data for ID {id}: {existingData.Data}");
                    Console.WriteLine("Do you want to overwrite this data? (yes/no)");

                    var answer = Console.ReadLine();
                    if (answer?.ToLower() != "yes")
                    {
                        Console.WriteLine("Data not overwritten.");
                        return;
                    }
                }

                await configService.Write(id, data);
                Console.WriteLine("Data written successfully.");
                break;

            case "reset":
                // Read existing data
                var existingDataReset = await configService.Read(id);
                if (existingDataReset != null)
                {
                    Console.WriteLine($"Existing data for ID {id}: {existingDataReset.Data}");
                    Console.WriteLine("Do you want to reset this data? (yes/no)");

                    var answerReset = Console.ReadLine();
                    if (answerReset?.ToLower() != "yes")
                    {
                        Console.WriteLine("Data not reset.");
                        return;
                    }
                }

                await configService.Reset(id);
                Console.WriteLine("Data reset successfully.");
                break;

            case "dump":
                var allData = await configService.GetAll();
                if (allData == null || !allData.Any())
                {
                    Console.WriteLine("No data found.");
                }
                else
                {
                    foreach (var data1 in allData)
                    {
                        Console.WriteLine($"ID: {data1.ID}, Data: {data1.Data}, DateTime: {data1.DateTime}");
                    }
                }
                break;

            case "len":
                if (args.Length < 3)
                {
                    Console.WriteLine(@"""Usage: Config ""updateLength"" ""id"" ""length""""");
                    return;
                }

                if (!int.TryParse(args[1], out id))
                {
                    Console.WriteLine("Please provide a valid ID.");
                    return;
                }

                if (!int.TryParse(args[2], out var length))
                {
                    Console.WriteLine("Please provide a valid length.");
                    return;
                }

                await configService.UpdateCommandLength(id, length);
                Console.WriteLine($"Updated command length for ID {id} to {length}");
                break;

            default:
                Console.WriteLine("Invalid command.");
                break;
        }
    }
}