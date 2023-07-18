namespace Config;

public class ConfigDataHistory
{
    /// <summary>
    /// Gets or sets the ID of the config data.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Gets or sets the data of the config data.
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of when the data was updated.
    /// </summary>
    public DateTime DateTime { get; set; }
}