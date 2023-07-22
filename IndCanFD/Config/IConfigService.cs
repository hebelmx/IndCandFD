using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Config;
public interface IConfigService
{
    Task<ConfigData> Read(int id);
    Task<bool> Write(int id, string data);
    Task<bool> Reset(int id);
    Task<int?> GetCommandLength(int id);
    Task UpdateCommandLength(int id, int length);

    Task<List<ConfigData>> GetAll();
    Task<List<ConfigDataHistory>> DumpHistory(int id);

    Task<List<Port>> GetAllPorts();
    Task AddPort(Port port);
    Task UpdatePort(Port port);
    Task DeletePort(int id);

    /// <summary>
    /// This method will initialize the SQLite database.
    /// </summary>
    public void InitializeDatabase();
    

    string GetHelpText();


}

