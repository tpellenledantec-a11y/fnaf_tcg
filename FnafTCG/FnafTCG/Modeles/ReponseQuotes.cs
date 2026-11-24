using Newtonsoft.Json;

namespace FnafTCG.Modeles;

public class ReponseQuotes
{
    [JsonProperty("items")]
    public List<Quote> Items { get; set; } = new();
}
