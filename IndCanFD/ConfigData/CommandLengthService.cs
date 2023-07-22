using Dapper;
using System.Data;
using System.Threading.Tasks;


namespace ConfigDataApp;


public class CommandLengthService : ICommandLengthService
{
    private readonly IDbConnection _connection;

    public CommandLengthService(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<int> GetCommandLengthAsync(int id)
    {
        string sql = "SELECT Length FROM CommandLength WHERE CommandId = @Id";

        var commandLength = await _connection.QuerySingleOrDefaultAsync<int>(sql, new { Id = id });

        // If the ID was not found in the database, you could throw an exception or handle it some other way
        if (commandLength == 0)
        {
            throw new KeyNotFoundException($"No command length found for ID: {id}");
        }

        return commandLength;
    }
}
