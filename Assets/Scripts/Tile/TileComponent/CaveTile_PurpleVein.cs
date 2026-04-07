using UnityEngine;

public class CaveTile_PurpleVein : BaseTile, IMeasureable
{
    bool drilled = false;
    bool pumped = false;
    int value;



    void Awake() { haveTile = true; }
    void Start() { value = Random.Range(5, 11); }
    int IMeasureable.Measured()
    {
        return value;
    }
    public void Drill()
    {
        if (!drilled)
        {
            drilled = true;
            Debug.Log("You drilled a Purple Vein!");
        }
        else
        {
            Debug.Log("This Purple Vein has already been drilled.");
        }
    }
    public void Pump()
    {
        if (!pumped)
        {
            if (drilled)
            {
                pumped = true;
                Debug.Log("You pumped a Purple Vein!");
            }
            else
            {
                Debug.Log("You need to drill the Purple Vein before pumping.");
            }
        }
        else
        {
            Debug.Log("Cannot pump.");
        }
    }
}
