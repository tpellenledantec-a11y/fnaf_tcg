using FnafTCG.Modeles;
using Newtonsoft.Json;

namespace FnafTCG.Models;

public class ReponseCatalogue
{
    [JsonProperty("cards")]
    public List<Carte> Cartes { get; set; } = new();
}