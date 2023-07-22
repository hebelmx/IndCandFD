using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Config;
using Dapper;

public class PortService : IPortService
{
    private readonly IDbConnection _connection;

    public PortService(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<Port>> GetAllPorts()
    {
        string sql = "SELECT * FROM Ports";
        var ports = (await _connection.QueryAsync<Port>(sql)).ToList();
        return ports;
    }

    public async Task AddPort(Port port)
    {
        string sql = "INSERT INTO Ports (ID, Data) VALUES (@ID, @Data)";
        await _connection.ExecuteAsync(sql, new { port.ID, port.Data });
    }

    public async Task UpdatePort(Port port)
    {
        string sql = "UPDATE Ports SET Data = @Data WHERE ID = @ID";
        await _connection.ExecuteAsync(sql, new { port.ID, port.Data });
    }

    public async Task DeletePort(int id)
    {
        string sql = "DELETE FROM Ports WHERE ID = @ID";
        await _connection.ExecuteAsync(sql, new { ID = id });
    }
}