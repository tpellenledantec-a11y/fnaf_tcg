using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FnafTCG.Modeles
{
    public class Quote
    {
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("change")]
        public decimal Change { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }
    }
}
