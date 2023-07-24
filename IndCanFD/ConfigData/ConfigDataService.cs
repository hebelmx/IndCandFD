using System.Collections.ObjectModel;
using System.Data;
using Dapper;
using FluentValidation;
using Newtonsoft.Json;

namespace ConfigDataService;

/// <summary>
/// This class is responsible for interacting with the Config and History tables in the database.
/// It handles CRUD operations on Config table and logs changes into the History table.
/// </summary>
public class ConfigDataService : IConfigDataService
{
    // IDbConnection object for interacting with the database.
    private IDbConnection db;

    // ConfigDataValidator object for validating FramesData before database operations.
    private ConfigDataValidator validator;


    /// <summary>
    /// The dictionary storing the lengths for each command.
    /// Key is the command ID, value is the command length.
    /// </summary>
    public ReadOnlyDictionary<int, int> CommandLengths { get; private set; }

    /// <summary>
    /// Initialize the ConfigDataService class.
    /// This method queries the database for all command lengths
    /// and stores them in the CommandLengths dictionary.
    /// </summary>
    public async Task InitClassAsync()
    {
        // Query the database for all command lengths
        // Convert the result to a dictionary and store it in the CommandLengths property
        var commandLengths = (await db.QueryAsync<CommandLength>("SELECT * FROM CommandLength")).ToDictionary(cl => cl.CommandId, cl => cl.Length);

        // Convert the mutable dictionary to an immutable one
        CommandLengths = new ReadOnlyDictionary<int, int>(commandLengths);
    }

    /// <summary>
    /// Constructor that accepts an IDbConnection for database interactions.
    /// </summary>
    /// <param name="db">IDbConnection object</param>
    public ConfigDataService(IDbConnection db, ConfigDataValidator configDataValidator)
    {
        this.db = db;
        this.validator = configDataValidator;
    }

    /// <inheritdoc/>
    public async Task<FramesData> ReadIdDataAsync(int id)
    {
        // Return the data for the given ID
        return await db.QuerySingleOrDefaultAsync<FramesData>("SELECT * FROM Config WHERE ID = @id", new { id = id });
    }

    /// <inheritdoc/>
    public async Task CreateIdDataAsync(FramesData data)
    {
        // Validate the provided data
        var result = validator.Validate(data);
        if (!result.IsValid) throw new ValidationException(result.Errors);

        // If validation passes, insert the data into the Config table
        await db.ExecuteAsync("INSERT INTO Config(ID, Data, DateTime, UserName) VALUES(@ID, @Data, @DateTime, @UserName)", data);
    }

    /// <inheritdoc/>
    public async Task UpdateIdDataAsync(FramesData data)
    {
        // Validate the provided data
        var result = validator.Validate(data);
        if (!result.IsValid) throw new ValidationException(result.Errors);

        // If validation passes, store the current data in the History table before updating
        var oldData = await ReadIdDataAsync(data.ID);
        await db.ExecuteAsync("INSERT INTO History(ConfigId, Data, DateTime, UserName) VALUES(@ID, @Data, @DateTime, @UserName)", oldData);

        // Update the data in the Config table
        await db.ExecuteAsync("UPDATE Config SET Data = @Data, DateTime = @DateTime, UserName = @UserName WHERE ID = @ID", data);
    }

    /// <inheritdoc/>
    public async Task<List<FramesData>> ReadAllIdDataAsync()
    {
        // Retrieve all data from the Config table and serialize it to a JSON string
        var allData = (await db.QueryAsync<FramesData>("SELECT * FROM Config")).ToList();
        return allData;
    }

    /// <inheritdoc/>
    public async Task DeleteIdDataAsync(int id)
    {
        // Store the current data in the History table before deletion
        var oldData = await ReadIdDataAsync(id);
        await db.ExecuteAsync("INSERT INTO History(ConfigId, Data, DateTime, UserName) VALUES(@ID, @Data, @DateTime, @UserName)", oldData);

        // Delete the data from the Config table
        await db.ExecuteAsync("DELETE FROM Config WHERE ID = @id", new { id = id });
    }

    /// <inheritdoc />

    public async Task<int> GetCommandLengthAsync(int commandId)
    {
        // Try to get the command length from the CommandLengths dictionary
        if (CommandLengths.TryGetValue(commandId, out int length))
        {
            // If the length is in the dictionary, return it
            return length;
        }

        // If the length is not in the dictionary, that means the commandId doesn't exist or hasn't been initialized properly
        // You could throw an exception here or handle it as you see fit.
        throw new KeyNotFoundException($"The Command Id {commandId} does not exist in CommandLengths dictionary. Ensure that the command length dictionary has been correctly initialized.");
    }
}
