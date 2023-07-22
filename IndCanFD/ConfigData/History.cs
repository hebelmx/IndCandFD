namespace ConfigDataApp;

public class History
{
    public int ID { get; set; }
    public int ConfigId { get; set; }
    public string Data { get; set; }
    public DateTime DateTime { get; set; }
    public string UserName { get; set; }
}