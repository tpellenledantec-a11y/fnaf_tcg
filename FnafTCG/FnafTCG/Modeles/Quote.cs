using Newtonsoft.Json;
using System.Globalization;

namespace FnafTCG.Modeles;

public class Quote
{
    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("change")]
    public decimal Change { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; } = "CRD";

    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonProperty("version")]
    public long Version { get; set; }

    public string PriceText => $"{Price:N2} {Currency}";
    public string TimestampText => Timestamp.ToLocalTime().ToString(
        "dd/MM HH:mm:ss", CultureInfo.CurrentCulture);
}
