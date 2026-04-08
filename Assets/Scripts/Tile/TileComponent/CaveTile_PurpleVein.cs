using UnityEngine;

public class CaveTile_PurpleVein : BaseTile, IMeasureable
{
    [HideInInspector]
    public bool isDrilled = false;
    [HideInInspector]
    public bool isPumped = false;
    int value;

    public Renderer matRenderer;
    public Material drilledMat;
    public Material pumpedMat;

    void Awake() { haveTile = true; }
    void Start() { value = Random.Range(5, 11); }
    int IMeasureable.Measured()
    {
        return value;
    }
    public void Drill()
    {
        if (!isDrilled)
        {
            isDrilled = true;
            if (matRenderer && drilledMat)
            {
                matRenderer.material = drilledMat;
            }
            Debug.Log("You drilled a Purple Vein!");
        }
        else
        {
            Debug.Log("This Purple Vein has already been drilled.");
        }
    }
    public void Pump()
    {
        if (!isPumped)
        {
            if (isDrilled)
            {
                isPumped = true;
                Debug.Log("You pumped a Purple Vein!");
                if (matRenderer && pumpedMat)
                {
                    matRenderer.material = pumpedMat;
                }
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
