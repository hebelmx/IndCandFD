using System.Text;
using System.Text.RegularExpressions;

namespace Config;

/// <summary>
/// Application class for managing configuration data.
/// </summary>
public class ConfigApplication : IConfigApplication
{
    private readonly IConfigService _configService;

    public ConfigApplication(string connectionString)
    {
        var databaseInitializer = new DatabaseInitializer(connectionString);
        databaseInitializer.InitializeDatabase();

        _configService = new ConfigService(connectionString);
    }

    /// <summary>
    /// Reads and displays config data for the specified ID.
    /// </summary>
    public async Task<string> ReadConfigData(int id)
    {
        var configData = await _configService.Read(id);

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Data: {configData.Data}");
        stringBuilder.AppendLine($"DateTime: {configData.DateTime}");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Writes new data for the specified ID, after checking its validity and confirming with the user.
    /// </summary>
    public async Task<string> WriteConfigData(int id, string data)
    {
        var stringBuilder = new StringBuilder();

        // Check if data is in valid format
        if (!Regex.IsMatch(data, @"^[0-9A-Fa-f]{2}( [0-9A-Fa-f]{2})*$"))
        {
            stringBuilder.AppendLine("Data is not in valid format. It should be hex numbers separated by spaces.");
            return stringBuilder.ToString();
        }

        // Read existing data
        var existingData = await _configService.Read(id);
        if (existingData != null)
        {
            stringBuilder.AppendLine($"Existing data for ID {id}: {existingData.Data}");
            stringBuilder.AppendLine("Do you want to overwrite this data? (yes/no)");

            var answer = Console.ReadLine();
            if (answer?.ToLower() != "yes")
            {
                stringBuilder.AppendLine("Data not overwritten.");
                return stringBuilder.ToString();
            }
        }

        await _configService.Write(id, data);
        stringBuilder.AppendLine("Data written successfully.");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Resets data for the specified ID, after confirming with the user.
    /// </summary>
    public async Task<string> ResetConfigData(int id)
    {
        var stringBuilder = new StringBuilder();

        // Read existing data
        var existingDataReset = await _configService.Read(id);
        if (existingDataReset != null)
        {
            stringBuilder.AppendLine($"Existing data for ID {id}: {existingDataReset.Data}");
            stringBuilder.AppendLine("Do you want to reset this data? (yes/no)");

            var answerReset = Console.ReadLine();
            if (answerReset?.ToLower() != "yes")
            {
                stringBuilder.AppendLine("Data not reset.");
                return stringBuilder.ToString();
            }
        }

        await _configService.Reset(id);
        stringBuilder.AppendLine("Data reset successfully.");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Initializes the database.
    /// </summary>
    public void InitializeDatabase()
    {
        _configService.InitializeDatabase();
    }

    /// <summary>
    /// Displays all config data.
    /// </summary>
    public async Task<string> DumpAllData()
    {
        var allData = await _configService.GetAll();

        var stringBuilder = new StringBuilder();

        if (allData == null || !allData.Any())
        {
            stringBuilder.AppendLine("No data found.");
        }
        else
        {
            foreach (var data in allData)
            {
                stringBuilder.AppendLine($"ID: {data.ID}, Data: {data.Data}, DateTime: {data.DateTime}");
            }
        }

        return stringBuilder.ToString();
    }
    

    /// <summary>
    /// Displays all config data.
    /// </summary>
    public  Task<List<ConfigData>> DumpListAllData()
    {
        var allData =  _configService.GetAll();
        return allData;
    }


    /// <summary>
    /// Handles the "dumpHistory" command to fetch and print the history of a given ID if it exists.
    /// </summary>
    public async Task<string> HandleDumpHistoryCommand(string[] args)
    {
        var stringBuilder = new StringBuilder();

        // Check if the ID argument is provided
        if (args.Length < 2)
        {
            stringBuilder.AppendLine(@"""Usage: Config ""dumpHistory"" ""id""""");
            return stringBuilder.ToString();
        }

        // Try to parse the ID from the argument
        if (!int.TryParse(args[1], out var id))
        {
            stringBuilder.AppendLine("Please provide a valid ID.");
            return stringBuilder.ToString();
        }

        // Fetch and display the history for the given ID
        var history = await _configService.DumpHistory(id);
        if (history == null || !history.Any())
        {
            stringBuilder.AppendLine($"No history found for ID {id}.");
        }
        else
        {
            foreach (var historyItem in history)
            {
                stringBuilder.AppendLine(
                    $"ID: {historyItem.ID}, Data: {historyItem.Data}, DateTime: {historyItem.DateTime}");
            }
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Updates command length for the specified ID, after checking its validity.
    /// </summary>
    public async Task<string> UpdateCommandLength(string[] args)
    {
        var stringBuilder = new StringBuilder();

        if (args.Length < 3)
        {
            stringBuilder.AppendLine(@"""Usage: Config ""updateLength"" ""id"" ""length""""");
            return stringBuilder.ToString();
        }

        if (!int.TryParse(args[1], out var id))
        {
            stringBuilder.AppendLine("Please provide a valid ID.");
            return stringBuilder.ToString();
        }

        if (!int.TryParse(args[2], out var length))
        {
            stringBuilder.AppendLine("Please provide a valid length.");
            return stringBuilder.ToString();
        }

        await _configService.UpdateCommandLength(id, length);
        stringBuilder.AppendLine($"Updated command length for ID {id} to {length}");

        return stringBuilder.ToString();
    }


    
    /// <summary>
    /// Returns the help text for the application.
    /// </summary>
    public string GetHelpText()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(@"""Usage: Config ""command"" ""id"" [""data""/""length""]""");
        stringBuilder.AppendLine(
            @"""Available commands: ""read"", ""write"", ""reset"", ""dump"", ""updateLength"", ""dumpHistory""""");
        stringBuilder.AppendLine(@"""Available ID: 1 through 7""");
        stringBuilder.AppendLine(@"""Example: Config read 1""");
        stringBuilder.AppendLine(@"""Example: Config write 1 ""34""");

        return stringBuilder.ToString();
    }

    public Task<List<Port>> GetAllPorts()
    {
        return _configService.GetAllPorts();
    }

    public Task AddPort(Port editingPort)
    {
        return _configService.AddPort(editingPort);
    }

    public Task UpdatePort(Port editingPort)
    {
        return _configService.UpdatePort(editingPort);
    }

    public Task DeletePort(int id)
    {
      return  _configService.DeletePort(id);
    }
}