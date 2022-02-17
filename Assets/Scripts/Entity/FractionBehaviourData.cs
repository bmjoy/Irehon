using Irehon.Entitys;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fraction behaviour", menuName = "Fraction")]
public class FractionBehaviourData : SerializedScriptableObject
{
    public Dictionary<Fraction, FractionBehaviour> Behaviours;
}
