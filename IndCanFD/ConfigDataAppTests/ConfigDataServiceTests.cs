using System.Data;
using Dapper;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Xunit;
using System.Threading.Tasks;
using ConfigDataService;

public class ConfigDataServiceTests : IAsyncLifetime
{
    private ConfigDataService.ConfigDataService service;
    private IDbConnection db;

    public async Task InitializeAsync()
    {

        // Assuming ConfigDataValidator doesn't have dependencies for simplicity

        

        var connection = new SqliteConnection("DataSource=D:\\Logs\\Valeo\\IndCanProgram\\IndCandFD\\IndCanFD\\BlazorApp1\\PcCan.db");
        await connection.OpenAsync();
        db = connection;

        var cp = new CommandLengthService(db);
        var validator = new ConfigDataValidator(cp);

        //var createConfigTableQuery = "CREATE TABLE Config (ID INTEGER, Data TEXT NOT NULL, DateTime DATETIME NOT NULL, UserName TEXT, PRIMARY KEY(ID))";
        //var createHistoryTableQuery = "CREATE TABLE History (ID INTEGER, ConfigId INTEGER NOT NULL, Data TEXT NOT NULL, DateTime DATETIME NOT NULL, UserName TEXT, PRIMARY KEY(ID AUTOINCREMENT))";

        //await db.ExecuteAsync(createConfigTableQuery);
        //await db.ExecuteAsync(createHistoryTableQuery);

        // Delete all data from Config table
        await db.ExecuteAsync("DELETE FROM Config");

        service = new ConfigDataService.ConfigDataService(db, validator);
    }

    public Task DisposeAsync()
    {
        db.Close();
        db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ReadIdData_ShouldReturnCorrectData()
    {
        // Arrange
        var expectedData = new FramesData { ID = 1, Data = "AF 91 40 05", DateTime = DateTime.Now, UserName = "user1" };
        await db.ExecuteAsync("INSERT INTO Config(ID, Data, DateTime, UserName) VALUES(@ID, @Data, @DateTime, @UserName)", expectedData);

        // Act
        var actualData = await service.ReadIdDataAsync(1);

        // Assert
        actualData.Should().BeEquivalentTo(expectedData);
    }


    [Fact]
    public async Task CreateIdData_ShouldInsertDataCorrectly()
    {
        // Arrange
        var expectedData = new FramesData { ID = 2, Data = "AF 91 40 06", DateTime = DateTime.Now, UserName = "user2" };

        // Act
        await service.CreateIdDataAsync(expectedData);
        var actualData = await service.ReadIdDataAsync(2);

        // Assert
        actualData.Should().BeEquivalentTo(expectedData);
    }

    [Fact]
    public async Task UpdateIdData_ShouldUpdateDataCorrectly()
    {
        // Arrange
        var initialData = new FramesData { ID = 3, Data = "AF 91 40 07", DateTime = DateTime.Now, UserName = "user3" };
        await service.CreateIdDataAsync(initialData);
        var updatedData = new FramesData { ID = 3, Data = "AF 91 40 08", DateTime = DateTime.Now, UserName = "user3" };

        // Act
        await service.UpdateIdDataAsync(updatedData);
        var actualData = await service.ReadIdDataAsync(3);

        // Assert
        actualData.Should().BeEquivalentTo(updatedData);
    }

    [Fact]
    public async Task ReadAllIdData_ShouldReturnAllDataCorrectly()
    {
        // Arrange
        var data1 = new FramesData { ID = 4, Data = "AF 91 40 09", DateTime = DateTime.Now, UserName = "user4" };
        var data2 = new FramesData { ID = 5, Data = "AF 91 40 10", DateTime = DateTime.Now, UserName = "user5" };
        await service.CreateIdDataAsync(data1);
        await service.CreateIdDataAsync(data2);
        var expectedDataList = new List<FramesData> { data1, data2 };

        // Act
        var actualDataList = await service.ReadAllIdDataAsync();
     

        // Assert
        actualDataList.Should().BeEquivalentTo(expectedDataList);
    }

    [Fact]
    public async Task DeleteIdData_ShouldDeleteDataCorrectly()
    {
        // Arrange
        var data = new FramesData { ID = 6, Data = "AF 91 40 11", DateTime = DateTime.Now, UserName = "user6" };
        await service.CreateIdDataAsync(data);

        // Act
        await service.DeleteIdDataAsync(6);
        var deletedData = await service.ReadIdDataAsync(6);

        // Assert
        deletedData.Should().BeNull();
    }
}
