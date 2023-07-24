namespace ConfigDataService;

public interface ICommandLengthService
{
    Task<int> GetCommandLengthAsync(int id);
}