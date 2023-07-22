namespace Config;

public interface IConfigApplication
{
    /// <summary>
    /// Reads and displays config data for the specified ID.
    /// </summary>
    Task<string> ReadConfigData(int id);

    /// <summary>
    /// Writes new data for the specified ID, after checking its validity and confirming with the user.
    /// </summary>
    Task<string> WriteConfigData(int id, string data);

    /// <summary>
    /// Resets data for the specified ID, after confirming with the user.
    /// </summary>
    Task<string> ResetConfigData(int id);

    /// <summary>
    /// Initializes the database.
    /// </summary>
    void InitializeDatabase();

    /// <summary>
    /// Displays all config data.
    /// </summary>
    Task<string> DumpAllData();

    /// <summary>
    /// Displays all config data.
    /// </summary>
    Task<List<ConfigData>> DumpListAllData();

    /// <summary>
    /// Handles the "dumpHistory" command to fetch and print the history of a given ID if it exists.
    /// </summary>
    Task<string> HandleDumpHistoryCommand(string[] args);

    /// <summary>
    /// Updates command length for the specified ID, after checking its validity.
    /// </summary>
    Task<string> UpdateCommandLength(string[] args);

    /// <summary>
    /// Returns the help text for the application.
    /// </summary>
    string GetHelpText();

    Task<List<Port>> GetAllPorts();
    Task AddPort(Port editingPort);
    Task UpdatePort(Port editingPort);
    Task DeletePort(int id);
}