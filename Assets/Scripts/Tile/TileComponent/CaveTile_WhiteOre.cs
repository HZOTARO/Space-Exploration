using UnityEngine;

public class CaveTile_WhiteOre : BaseTile, IMeasureable
{
    bool mined = false;
    int value;

    [Header("References")]
    public GameObject destroyedOre;
    void Start() { value = Random.Range(5, 11); }
    int IMeasureable.Measured()
    {
        return value;
    }
    public void Mine()
    {
        if (!mined)
        {
            mined = true;
            if (destroyedOre)
            {
                Destroy(destroyedOre);
            }
            Debug.Log("You mined a White Ore!");
        }
        else
        {
            Debug.Log("This White Ore has already been mined.");
        }
    }
}
