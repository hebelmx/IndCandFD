using System.Data.SQLite;
using Dapper;
using Microsoft.Data.Sqlite;
public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void InitializeDatabase()
    {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        // Create Config table
        connection.Execute(
            @"CREATE TABLE IF NOT EXISTS Config (
            ID INTEGER PRIMARY KEY,
            Data TEXT NOT NULL,
            DateTime DATETIME NOT NULL
        );",
            transaction: transaction);

        // Create History table
        connection.Execute(
            @"CREATE TABLE IF NOT EXISTS History (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            ConfigId INTEGER NOT NULL,
            Data TEXT NOT NULL,
            DateTime DATETIME NOT NULL
        );",
            transaction: transaction);

        // Create CommandLength table
        connection.Execute(
            @"CREATE TABLE IF NOT EXISTS CommandLength (
            CommandId INTEGER PRIMARY KEY,
            Length INTEGER NOT NULL
        );",
            transaction: transaction);

        // Populate CommandLength table with default values
        for (var commandId = 1; commandId <= 7; commandId++)
        {
            connection.Execute(
                @"INSERT INTO CommandLength (CommandId, Length) VALUES (@CommandId, @Length)
          ON CONFLICT(CommandId) DO UPDATE SET Length = excluded.Length;",
                new { CommandId = commandId, Length = 255 },
                transaction: transaction);
        }
        
          // Create Ports table
    	connection.Execute(
        	@"CREATE TABLE IF NOT EXISTS Ports (
		ID INTEGER PRIMARY KEY,
		Data TEXT NOT NULL
	    	);",
        transaction: transaction);

    	// Populate Ports table with default value
    	connection.Execute(
		@"INSERT INTO Ports (ID, Data) VALUES (@ID, @Data)
	      	ON CONFLICT(ID) DO UPDATE SET Data = excluded.Data;",
		new { ID = 1, Data = "vcan0" },
        transaction: transaction);
        
	// Commit the transaction to save all changes to the database
        transaction.Commit();
    }
}
