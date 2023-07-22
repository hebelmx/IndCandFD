using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentValidation;

namespace ConfigDataApp;

/// <summary>
/// Interface for Config Data Service
/// </summary>
public interface IConfigDataService
{
    /// <summary>
    /// Retrieves config data for a specific ID
    /// </summary>
    /// <param name="id">ID of the config data to retrieve</param>
    /// <returns>ConfigData object with specified ID</returns>
    Task<ConfigData> ReadIdDataAsync(int id);



    /// <summary>
    /// The dictionary storing the lengths for each command.
    /// Key is the command ID, value is the command length.
    /// </summary>
    public ReadOnlyDictionary<int, int> CommandLengths { get; }

    /// <summary>
    /// Initialize the ConfigDataService class.
    /// This method queries the database for all command lengths
    /// and stores them in the CommandLengths dictionary.
    /// </summary>
    Task InitClassAsync();

    /// <summary>
    /// Creates new config data
    /// </summary>
    /// <param name="data">ConfigData object to add</param>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    Task CreateIdDataAsync(ConfigData data);

    /// <summary>
    /// Updates existing config data
    /// </summary>
    /// <param name="data">Updated ConfigData object</param>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    Task UpdateIdDataAsync(ConfigData data);

    /// <summary>
    /// Retrieves all config data as a JSON string
    /// </summary>
    /// <returns>A JSON string of all ConfigData</returns>
    Task<string> ReadAllIdDataAsync();

    /// <summary>
    /// Deletes config data with a specific ID
    /// </summary>
    /// <param name="id">ID of the ConfigData to delete</param>
    Task DeleteIdDataAsync(int id);

    /// <summary>
    /// Retrieves the command length for a given command ID.
    /// It first tries to retrieve the length from the CommandLengths dictionary.
    /// If it's not found there, it queries the database for the length,
    /// stores it in the dictionary for future use, and then returns it.
    /// </summary>
    /// <param name="commandId">The command ID to get the length for.</param>
    /// <returns>The length of the command associated with the given ID.</returns>
    Task<int> GetCommandLengthAsync(int commandId);
}