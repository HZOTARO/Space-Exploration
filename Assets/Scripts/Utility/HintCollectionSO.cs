using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HintCollection", menuName = "Scriptable Objects/HintSO")]
public class HintCollectionSO : ScriptableObject
{
    [Header("Paginated Hints")]
    public List<HintSO> hints = new List<HintSO>();
}