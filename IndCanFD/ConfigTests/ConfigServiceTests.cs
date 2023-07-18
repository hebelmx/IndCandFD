using System.Data.SQLite;
using Config;
using Dapper;
using Xunit;

namespace ConfigTests;

public class ConfigServiceTests
{

    private const string TestConnectionString = "Data Source=test.db";
    private readonly DatabaseInitializer _initializer;
    private readonly IConfigService _service;


    [Fact]
    public void InitializeDatabase_NoExceptionThrown()
    {
        // Arrange
        var configService = new ConfigService(TestConnectionString);

        // Act
        var exception = Record.Exception(() => configService.InitializeDatabase());

        // Assert
        Assert.Null(exception);
    }

  
    [Fact]
    public async Task InitializeDatabase_AllTablesExist()
    {
        // Arrange
       
        var databaseInitializer = new DatabaseInitializer(TestConnectionString);
        databaseInitializer.InitializeDatabase();
        var configService = new ConfigService(TestConnectionString);

        // Act
        await using var connection = new SQLiteConnection(TestConnectionString);
        connection.Open();

        // Query the tables to ensure that they were properly initialized
        var configTableExists = await connection.ExecuteScalarAsync<int>("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Config';");
        var historyTableExists = await connection.ExecuteScalarAsync<int>("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='History';");
        var commandLengthTableExists = await connection.ExecuteScalarAsync<int>("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='CommandLength';");

        // Assert
        Assert.Equal(1, configTableExists);
        Assert.Equal(1, historyTableExists);
        Assert.Equal(1, commandLengthTableExists);
    }

    public ConfigServiceTests()
    {
        // Initialize database and service
        _initializer = new DatabaseInitializer(TestConnectionString);
        _initializer.InitializeDatabase();
        _service = new ConfigService(TestConnectionString);
    }

    [Fact]
    public async Task CanReadConfigData()
    {
        // Arrange
        var testId = 1;
        var testData = "34 45 34 32 A3 F1";
        var testDateTime = DateTime.UtcNow.ToString("o");
        await using (var connection = new SQLiteConnection(TestConnectionString))
        {
            connection.Open();
            await connection.ExecuteAsync("INSERT INTO Config (ID, Data, DateTime) VALUES (@ID, @Data, @DateTime);",
                new { ID = testId, Data = testData, DateTime = testDateTime });
        }

        // Act
        var configData = await _service.Read(testId);

        // Assert
        Assert.NotNull(configData);
        Assert.Equal(testId, configData.ID);
        Assert.Equal(testData, configData.Data);
        Assert.Equal(testDateTime, configData.DateTime);

        // Clean up
        await using (var connection = new SQLiteConnection(TestConnectionString))
        {
            connection.Open();
            await connection.ExecuteAsync("DELETE FROM Config WHERE ID = @ID;", new { ID = testId });
        }
    }

    [Fact]
    public async Task CanWriteConfigData()
    {
        // Arrange
        var testId = 2;
        var testData = "34 45 34 32 A3 F1";

        // Act
        await _service.Write(testId, testData);

        // Assert
        await using (var connection = new SQLiteConnection(TestConnectionString))
        {
            connection.Open();
            var configData = await connection.QueryFirstOrDefaultAsync<ConfigData>("SELECT * FROM Config WHERE ID = @Id;", new { Id = testId });

            Assert.NotNull(configData);
            Assert.Equal(testId, configData.ID);
            Assert.Equal(testData, configData.Data);

            // Clean up
            await connection.ExecuteAsync("DELETE FROM Config WHERE ID = @ID;", new { ID = testId });
            await connection.ExecuteAsync("DELETE FROM History WHERE ID = @ID;", new { ID = testId });
        }
    }

    [Fact]
    public async Task CanResetConfigData()
    {
        // Arrange
        var testId = 3;
        var testData = "34 45 34 32 A3 F1";

        await using (var connection = new SQLiteConnection(TestConnectionString))
        {
            connection.Open();
            await connection.ExecuteAsync("INSERT INTO Config (ID, Data, DateTime) VALUES (@ID, @Data, @DateTime);",
                new { ID = testId, Data = testData, DateTime = DateTime.UtcNow.ToString("o") });
        }

        // Act
        await _service.Reset(testId);

        // Assert
        await using (var connection = new SQLiteConnection(TestConnectionString))
        {
            connection.Open();
            var configData = await connection.QueryFirstOrDefaultAsync<ConfigData>("SELECT * FROM Config WHERE ID = @Id;", new { Id = testId });

            Assert.Null(configData);

            var historyData = await connection.QueryFirstOrDefaultAsync<HistoryData>("SELECT * FROM History WHERE ID = @Id;", new { Id = testId });

            Assert.NotNull(historyData);
            Assert.Equal(testId, historyData.ID);
            Assert.Equal(testData, historyData.Data);

            // Clean up
            await connection.ExecuteAsync("DELETE FROM History WHERE ID = @ID;", new { ID = testId });
        }
    }


    [Fact]
    public async Task GetAll_ReturnsAllData()
    {
        // Arrange
        await ClearDatabase();
        const string connectionString = "Data Source=config_test.db";
        var databaseInitializer = new DatabaseInitializer(connectionString);
        databaseInitializer.InitializeDatabase();

        var configService = new ConfigService(connectionString);
        await configService.UpdateCommandLength(1, 10);
        var frame1 = "34 45 34 32";
        await configService.Write(1, frame1);
        var frame2 = "34 45 34 32 A3 F1";
        await configService.UpdateCommandLength(1, 12);
        await configService.Write(2, frame2);

        // Act
        var result = await configService.GetAll();

        // Assert
        Assert.Equal(2, result.Count);

        var data1 = result.Find(x => x.ID == 1);
        Assert.NotNull(data1);
        Assert.Equal(frame1, data1.Data);

        var data2 = result.Find(x => x.ID == 2);
        Assert.NotNull(data2);
        Assert.Equal(frame2, data2.Data);
    }

    [Fact]
    public async Task Write_InvalidData_ReturnsFalse()
    {
        // Arrange
        const string connectionString = "Data Source=config_test.db";
        var databaseInitializer = new DatabaseInitializer(connectionString);
        databaseInitializer.InitializeDatabase();

        var configService = new ConfigService(connectionString);

        // Act
        var result = await configService.Write(1, "Invalid Data");

        // Assert
        Assert.False(result);
    }


    [Fact]
    public async Task Write_InvalidData_DoesNotChangeData()
    {
        // Arrange
        const string connectionString = "Data Source=config_test.db";
        var databaseInitializer = new DatabaseInitializer(connectionString);
        databaseInitializer.InitializeDatabase();

        var configService = new ConfigService(connectionString);
        var id = 1;
        var initialData = "34 45 34 32";  // Valid initial data

        // Write initial valid data
        await configService.Write(id, initialData);

        // Act
        var result = await configService.Write(id, "Invalid Data");

        // Assert
        Assert.False(result);

        // Get the data again and check it
        var dataAfterInvalidWrite = await configService.Read(id);

        Assert.NotNull(dataAfterInvalidWrite);
        Assert.Equal(initialData, dataAfterInvalidWrite.Data);
    }


    [Fact]
    public async Task UpdateAndGetCommandLength_Test()
    {
        // Arrange
        const string connectionString = "Data Source=config_test.db";
        var databaseInitializer = new DatabaseInitializer(connectionString);
        databaseInitializer.InitializeDatabase();
        var configService = new ConfigService(connectionString);
        var id = 1;
        var length = 10;

        // Act
        await configService.UpdateCommandLength(id, length);
        var result = await configService.GetCommandLength(id);

        // Assert
        Assert.Equal(length, result);
    }

    [Fact]
    public async Task GetCommandLength_NoLengthSet_ReturnsNull()
    {
        // Arrange
        const string connectionString = "Data Source=config_test.db";
        var databaseInitializer = new DatabaseInitializer(connectionString);
        databaseInitializer.InitializeDatabase();
        var configService = new ConfigService(connectionString);
        var id = 1;

        // Remove length if it exists
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            await connection.ExecuteAsync("DELETE FROM CommandLength WHERE CommandId = @id", new { id });
        }

        // Act
        var result = await configService.GetCommandLength(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DumpHistory_ValidID_ReturnsHistory()
    {
        // Arrange
        await ClearDatabase();
        const string connectionString = "Data Source=config_test.db";
        var databaseInitializer = new DatabaseInitializer(connectionString);
        databaseInitializer.InitializeDatabase();

        var configService = new ConfigService(connectionString);

        // Assume we have previously written some data for ID 1
        var frame1 = "34 45 34 32";
        await configService.Write(1, frame1);

        await configService.Reset(1);
        var frame2 = "A3 F1";
        await configService.Write(1, frame2);

        // Act
        var history = await configService.DumpHistory(1);

        // Assert
        Assert.NotNull(history);
        Assert.True(history.Count() >= 2);
        Assert.Equal(frame1, history.ElementAt(history.Count() - 1).Data);
       
    }

    private async Task ClearDatabase()
    {
        await using var connection = new SQLiteConnection(TestConnectionString);
        connection.Open();
        await connection.ExecuteAsync("DELETE FROM Config;");
        await connection.ExecuteAsync("DELETE FROM CommandLength;");
        await connection.ExecuteAsync("DELETE FROM History;");
    }

    public void Dispose()
    {
        ClearDatabase().Wait();
    }

}