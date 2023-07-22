using Config;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPortService
{
    Task<List<Port>> GetAllPorts();
    Task AddPort(Port port);
    Task UpdatePort(Port port);
    Task DeletePort(int id);
}