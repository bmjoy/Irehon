using Irehon.Entitys;
using SimpleJSON;
using System;

public struct RegisterInfo
{
    public Fraction fraction;
    public RegisterInfo(JSONNode node)
    {
        this.fraction = (Fraction)Enum.Parse(typeof(Fraction), node["fraction"]);
    }

    public RegisterInfo(Fraction fraction)
    {
        this.fraction = fraction;
    }

    public string ToJsonString()
    {
        JSONObject json = new JSONObject();
        json["fraction"] = this.fraction.ToString();
        return json.ToString();
    }
}
