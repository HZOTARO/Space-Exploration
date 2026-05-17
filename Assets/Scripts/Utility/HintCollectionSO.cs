using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HintCollection", menuName = "Scriptable Objects/HintCollectionSO")]
public class HintCollectionSO : ScriptableObject
{
    public string collectionName;

    [Header("Paginated Hints")]
    public List<HintSO> hints = new List<HintSO>();
}