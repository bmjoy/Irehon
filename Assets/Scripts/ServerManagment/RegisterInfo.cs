using SimpleJSON;
using System;

public struct RegisterInfo
{
    public Fraction fraction;
    public RegisterInfo(JSONNode node)
    {
        fraction = (Fraction)Enum.Parse(typeof(Fraction), node["fraction"]);
    }

    public RegisterInfo(Fraction fraction)
    {
        this.fraction = fraction;
    }

    public string ToJsonString()
    {
        JSONObject json = new JSONObject();
        json["fraction"] = fraction.ToString();
        return json.ToString();
    }
}
