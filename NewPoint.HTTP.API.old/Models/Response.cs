using Newtonsoft.Json;

namespace NewPoint.Models;

public class Response
{
    private IDataEntry[]? _data;

    [JsonProperty("error")]
    public string Error { get; set; } = string.Empty;

    [JsonProperty("data")]
    public IDataEntry[] Data
    {
        get => _data;
        set
        {
            _data = value;

            var currentId = 1ul;
            foreach (var dataEntry in _data)
            {
                dataEntry.Id = currentId;
                currentId++;
            }
        }
    }

    public Response()
    {
    }

    public Response(IDataEntry dataEntry)
    {
        _data = new[] { dataEntry };
    }

    public Response(IDataEntry[] dataEntries)
    {
        _data = dataEntries;
    }
}
