using System.Data.Common;
using System.Data.SQLite;
using System.Text;
using Dapper;
using static System.Text.RegularExpressions.Regex;

namespace Config;

/// <summary>
/// Represents a service for handling configuration data.
/// </summary>
public class ConfigService : IConfigService
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string to the SQLite database.</param>
    public ConfigService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Fetches all the configuration data.
    /// </summary>
    public async Task<List<ConfigData>> GetAll()
    {
        await using var connection = new SQLiteConnection(_connectionString);
        connection.OpenAsync();
        var result = await connection.QueryAsync<ConfigData>("SELECT * FROM Config;");
        return result.ToList();
    }

    /// <summary>
    /// Dumps the history of the configuration data for a specific ID.
    /// </summary>
    public async Task<List<ConfigDataHistory>> DumpHistory(int id)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        var configData = await connection.QueryAsync<ConfigDataHistory>("SELECT * FROM history WHERE ID = @ID", new { ID = id });
        return configData.ToList();
    }


    /// <summary>
    /// Updates the length of a specific command.
    /// </summary>
    public async Task UpdateCommandLength(int id, int length)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        connection.OpenAsync();
        await connection.ExecuteAsync("INSERT OR REPLACE INTO CommandLength (CommandId, Length) VALUES (@id, @length);", new { id, length });
    }

    /// <summary>
    /// Reads the configuration data for a specific ID.
    /// </summary>
    public async Task<ConfigData> Read(int id)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        connection.OpenAsync();
        var configData = await connection.QueryFirstOrDefaultAsync<ConfigData>("SELECT * FROM Config WHERE ID = @Id;", new { Id = id });
        return configData;
    }

    /// <summary>
    /// Retrieves the length of a specific command.
    /// </summary>
    public async Task<int?> GetCommandLength(int id)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        connection.OpenAsync();
        var length = await connection.QuerySingleOrDefaultAsync<int?>("SELECT Length FROM CommandLength WHERE CommandId = @id;", new { id });
        return length;
    }

    /// <summary>
    /// Writes data for a specific ID. Returns false if the data is not in a valid format or the data length does not match the expected length for this command.
    /// </summary>
    public async Task<bool> Write(int id, string data)
    {
        // Check if data is in valid format
        if (!IsMatch(data, @"^[0-9A-Fa-f]{2}( [0-9A-Fa-f]{2})*$"))
        {
            Console.WriteLine("Data is not in valid format. It should be hex numbers separated by spaces.");
            return false;
        }

        // Fetch length from the database and compare with input data length
        var lengthInDatabase = await GetCommandLength(id);
        var lengthOfData = data.Split(' ').Length;
        if (lengthInDatabase != null && lengthInDatabase < lengthOfData)
        {
            Console.WriteLine("Data length does not match the expected length for this command.");
            return false;
        }

        await using var connection = new SQLiteConnection(_connectionString);
        connection.OpenAsync();
        var configData = await connection.QueryFirstOrDefaultAsync<ConfigData>("SELECT * FROM Config WHERE ID = @Id;", new { Id = id });

        if (configData != null)
        {
            // Write old data to history before updating the current record
            var historyData = new HistoryData
            {
                ID = configData.ID,
                Data = configData.Data,
                DateTime = configData.DateTime,
                ChangeDateTime = DateTime.UtcNow.ToString("o")
            };
            await connection.ExecuteAsync("INSERT INTO History (ID, Data, DateTime, ChangeDateTime) VALUES (@ID, @Data, @DateTime, @ChangeDateTime);", historyData);
            // Update existing record
            await connection.ExecuteAsync("UPDATE Config SET Data = @Data, DateTime = @DateTime WHERE ID = @ID;", new { ID = id, Data = data, DateTime = DateTime.UtcNow.ToString("o") });
        }
        else
        {
            // Insert new record
            await connection.ExecuteAsync("INSERT INTO Config (ID, Data, DateTime) VALUES (@ID, @Data, @DateTime);", new { ID = id, Data = data, DateTime = DateTime.UtcNow.ToString("o") });
        }
        return true;
    }

    /// <summary>
    /// Resets the configuration data for a specific ID. Returns false if no data exists for the ID.
    /// </summary>
    public async Task<bool> Reset(int id)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        connection.OpenAsync();

        var configData = await connection.QueryFirstOrDefaultAsync<ConfigData>("SELECT * FROM Config WHERE ID = @Id;", new { Id = id });

        if (configData == null) return false;

        // Write old data to history before resetting the current record
        var historyData = new HistoryData
        {
            ID = configData.ID,
            Data = configData.Data,
            DateTime = configData.DateTime,
            ChangeDateTime = DateTime.UtcNow.ToString("o")
        };
        await connection.ExecuteAsync("INSERT INTO History (ID, Data, DateTime, ChangeDateTime) VALUES (@ID, @Data, @DateTime, @ChangeDateTime);", historyData);

        // Remove existing record
        await connection.ExecuteAsync("DELETE FROM Config WHERE ID = @ID;", new { ID = id });
        return true;
    }


    public async Task<List<Port>> GetAllPorts()
    {
        await using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT * FROM Ports";
        var result = await connection.QueryAsync<Port>(sql);
        return result.ToList();
    }

    public async Task AddPort(Port editingPort)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "INSERT INTO Ports (ID, Data) VALUES (@ID, @Data)";
        await connection.ExecuteAsync(sql, new { ID = editingPort.ID, Data = editingPort.Data });
    }

    public async Task UpdatePort(Port editingPort)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        connection.OpenAsync();

        var sql = "UPDATE Ports SET Data = @Data WHERE ID = @ID";
        await connection.ExecuteAsync(sql, new { ID = editingPort.ID, Data = editingPort.Data });
    }

    public async Task DeletePort(int id)
    {
        await using var connection = new SQLiteConnection(_connectionString);
        connection.OpenAsync();

        var sql = "DELETE FROM Ports WHERE ID = @ID";
        await connection.ExecuteAsync(sql, new { ID = id });
    }



    /// <inheritdoc/>
    public void InitializeDatabase()
    {
        var databaseInitializer = new DatabaseInitializer(_connectionString);
        databaseInitializer.InitializeDatabase();
    }

    /// <summary>
    /// Returns a string containing the help text.
    /// </summary>
    public string GetHelpText()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(@"""Usage: Config ""command"" ""id"" [""data""/""length""]""");
        stringBuilder.AppendLine(@"""Available commands: ""read"", ""write"", ""reset"", ""dump"", ""updateLength"", ""dumpHistory""""");
        stringBuilder.AppendLine(@"""Available ID: 1 through 7""");
        stringBuilder.AppendLine(@"""Example: Config read 1""");
        stringBuilder.AppendLine(@"""Example: Config write 1 ""34 45 34 32""""");
        stringBuilder.AppendLine(@"""Example: Config reset 1""");
        stringBuilder.AppendLine(@"""Example: Config dump""");
        stringBuilder.AppendLine(@"""Example: Config updateLength 1 16""");
        stringBuilder.AppendLine(@"""Example: Config dumpHistory 1""");
        return stringBuilder.ToString();
    }
}