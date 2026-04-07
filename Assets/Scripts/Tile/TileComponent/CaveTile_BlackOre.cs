using UnityEngine;

public class CaveTile_BlackOre : BaseTile, IMeasureable
{
    bool mined = false;
    bool purified = false;
    int value;

    [Header("References")]
    public GameObject destroyedOre;
    void Start() { value = Random.Range(5, 11); }
    int IMeasureable.Measured()
    {
        return value;
    }
    public void Purify()
    {
        if (!mined && !purified)
        {
            purified = true;
            Debug.Log("You purified a Black Ore into a Purple Vein!");
        }
        else
        {
            Debug.Log("Cannot purify.");
        }
    }

    public void Mine()
    {
        if (!mined)
        {
            mined = true;
            if (purified)
            {
                Debug.Log("You mined a Black Ore!");
            }
            else
            {
                Debug.Log("Ore exploded");
            }

            if (destroyedOre)
            {
                Destroy(destroyedOre);
            }
        }
        else
        {
            Debug.Log("This Black Ore has already been mined.");
        }
    }
}
