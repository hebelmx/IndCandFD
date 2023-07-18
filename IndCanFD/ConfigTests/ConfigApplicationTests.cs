using Config;
using Xunit;

namespace ConfigTests;

public class ConfigApplicationTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly string _connectionString;
    private readonly DatabaseInitializer _databaseInitializer;

    public ConfigApplicationTests()
    {
        //"Data Source=test.db";
        _testDbPath = Path.Combine(Directory.GetCurrentDirectory(), "PCCan.db");
        _connectionString = $"Data Source={_testDbPath}";
        _databaseInitializer = new DatabaseInitializer(_connectionString);
        _databaseInitializer.InitializeDatabase();
    }

    [Fact]
    public async Task ReadConfigData_ValidId_ReturnsExpectedResult()
    {
        // Arrange
        var app = new ConfigApplication(_connectionString);
        await app.WriteConfigData(1, "34 45 34 32");

        // Act
        var result = await app.ReadConfigData(1);

        // Assert
        Assert.Contains("Data: 34 45 34 32", result);
        Assert.Contains("DateTime:", result);
    }

    [Fact]
    public async Task WriteConfigData_ValidData_ReturnsExpectedResult()
    {
        // Arrange
        var app = new ConfigApplication(_connectionString);

        // Act
        var result = await app.WriteConfigData(1, "34 45 34 32");

        // Assert
        Assert.Contains("Data written successfully.", result);
    }

    [Fact]
    public async Task ResetConfigData_ValidId_ReturnsExpectedResult()
    {
        // Arrange
        var app = new ConfigApplication(_connectionString);
        await app.WriteConfigData(1, "34 45 34 32");

        // Act
        var result = await app.ResetConfigData(1);

        // Assert
        Assert.Contains("Data reset successfully.", result);
    }

    [Fact]
    public async Task DumpAllData_NoData_ReturnsExpectedResult()
    {
        // Arrange
        var app = new ConfigApplication(_connectionString);

        // Act
        var result = await app.DumpAllData();

        // Assert
        Assert.Contains("No data found.", result);
    }

    [Fact]
    public async Task DumpAllData_WithData_ReturnsExpectedResult()
    {
        // Arrange
        var app = new ConfigApplication(_connectionString);
        await app.WriteConfigData(1, "34 45 34 32");
        await app.WriteConfigData(2, "12 34 56 78");

        // Act
        var result = await app.DumpAllData();

        // Assert
        Assert.Contains("ID: 1, Data: 34 45 34 32, DateTime:", result);
        Assert.Contains("ID: 2, Data: 12 34 56 78, DateTime:", result);
    }

    [Fact]
    public async Task HandleDumpHistoryCommand_ValidId_ReturnsExpectedResult()
    {
        // Arrange
        var app = new ConfigApplication(_connectionString);
        await app.WriteConfigData(1, "34 45 34 32");

        var args = new string[] { "dumphistory", "1" };

        // Act
        var result = await app.HandleDumpHistoryCommand(args);

        // Assert
        Assert.Contains("ID: 1, Data: 34 45 34 32, DateTime:", result);
    }

    [Fact]
    public async Task UpdateCommandLength_ValidArguments_ReturnsExpectedResult()
    {
        // Arrange
        var app = new ConfigApplication(_connectionString);

        var args = new string[] { "updateLength", "1", "16" };

        // Act
        var result = await app.UpdateCommandLength(args);

        // Assert
        Assert.Contains("Updated command length for ID 1 to 16", result);
    }

    [Fact]
    public void GetHelpText_ReturnsExpectedResult()
    {
        // Arrange
        var app = new ConfigApplication(_connectionString);

        // Act
        var result = app.GetHelpText();

        // Assert
        Assert.Contains("Usage: Config \"command\" \"id\" [\"data\"/\"length\"]", result);
        Assert.Contains("Available commands: \"read\", \"write\", \"reset\", \"dump\", \"updateLength\", \"dumpHistory\"", result);
        Assert.Contains("Available ID: 1 through 7", result);
    }

    public void Dispose()
    {
        // Clean up the test database file
        if (File.Exists(_testDbPath))
        {
  //          File.Delete(_testDbPath);
        }
    }
}