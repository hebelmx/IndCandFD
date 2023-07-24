using Dapper;
using Microsoft.Data.Sqlite;

namespace FramesDataService;

public interface IDatabaseInitializer
{
	Task<bool> InitializeDatabaseAsync();
}
public class DatabaseInitializer : IDatabaseInitializer
{
	private readonly string _connectionString;

	public DatabaseInitializer(string connectionString)
	{
		_connectionString = connectionString;
	}

	public async Task<bool> InitializeDatabaseAsync()
	{
		await using var connection = new SqliteConnection(_connectionString);
		await connection.OpenAsync();

		await using var transaction = connection.BeginTransaction();

		// Create Frames table
		await connection.ExecuteAsync(
			@"CREATE TABLE IF NOT EXISTS Frames (
			ID INTEGER PRIMARY KEY,
			Data TEXT NOT NULL,
			DateTime DATETIME NOT NULL
		);",
			transaction: transaction);

		// Create History table
		await connection.ExecuteAsync(
			@"CREATE TABLE IF NOT EXISTS History (
			ID INTEGER PRIMARY KEY AUTOINCREMENT,
			FramesId INTEGER NOT NULL,
			Data TEXT NOT NULL,
			DateTime DATETIME NOT NULL
		);",
			transaction: transaction);


		// Populate Frames table with default values
		for (var commandId = 1; commandId <= 7; commandId++)
		{
			await connection.ExecuteAsync(
				@"INSERT INTO Frames (ID, Data, DateTime, UserName, MaxLen) VALUES (@ID, @Data, @DateTime, @UserName, @MaxLen)
					ON CONFLICT(ID) DO UPDATE SET Data = excluded.Data, DateTime = excluded.DateTime, UserName = excluded.UserName, 
					MaxLen = excluded.MaxLen;",
				new { ID = commandId, Data = "", DateTime = DateTime.Now, UserName = "", MaxLen = 255 },
				transaction: transaction);
		}

		// Create Ports table
		await connection.ExecuteAsync(
			@"CREATE TABLE IF NOT EXISTS Ports (
				ID INTEGER PRIMARY KEY,
				Data TEXT NOT NULL
			);",
			transaction: transaction);

		// Populate Ports table with default values for IDs 1 to 7
		for (var id = 1; id <= 2; id++)
		{
			await connection.ExecuteAsync(
				@"INSERT INTO Ports (ID, Data) VALUES (@ID, @Data)
					ON CONFLICT(ID) DO UPDATE SET Data = excluded.Data;",
				new { ID = id, Data = $"vcan{id}" },
				transaction: transaction);
		}

		// Commit the transaction to save all changes to the database
		transaction.Commit();

		// Check database have value value
		var portsCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM Ports");
		var framesCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM Frames");



		return portsCount==2 && framesCount==7;
	}
}
