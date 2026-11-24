using Newtonsoft.Json;

namespace FnafTCG.Modeles;

public class Cotation
{
    #region Attributs

    private decimal prix;
    private decimal variation;
    private string devise = "";

    #endregion

    #region Constructeurs

    #endregion

    #region Getter/Setter

    [JsonProperty("price")]
    public decimal Prix
    {
        get { return prix; }
        set { prix = value; }
    }

    [JsonProperty("change")]
    public decimal Variation
    {
        get { return variation; }
        set { variation = value; }
    }

    [JsonProperty("currency")]
    public string Devise
    {
        get { return devise; }
        set { devise = value; }
    }

    #endregion

    #region Methodes

    #endregion
}
