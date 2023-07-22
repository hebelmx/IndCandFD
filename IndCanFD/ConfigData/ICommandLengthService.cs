namespace ConfigDataApp;

public interface ICommandLengthService
{
    Task<int> GetCommandLengthAsync(int id);
}