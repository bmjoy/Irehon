using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName ="Fraction behaviour",menuName = "Fraction")]
public class FractionBehaviourData : SerializedScriptableObject
{
    public Dictionary<Fraction, FractionBehaviour> Behaviours;
}
