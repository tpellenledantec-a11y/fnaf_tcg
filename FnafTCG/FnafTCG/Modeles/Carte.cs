using Newtonsoft.Json;

namespace FnafTCG.Modeles;

public class Carte
{
    #region Attributs

    private string identifiant = "";
    private string marque = "";
    private string modele = "";
    private string variante = "";

    #endregion

    #region Constructeurs

    #endregion

    #region Getter/Setter

    [JsonProperty("id")]
    public string Identifiant
    {
        get { return identifiant; }
        set { identifiant = value; }
    }

    [JsonProperty("brand")]
    public string Marque
    {
        get { return marque; }
        set { marque = value; }
    }

    [JsonProperty("model")]
    public string Modele
    {
        get { return modele; }
        set { modele = value; }
    }

    [JsonProperty("variant")]
    public string Variante
    {
        get { return variante; }
        set { variante = value; }
    }

    #endregion

    #region Methodes

    #endregion
}
