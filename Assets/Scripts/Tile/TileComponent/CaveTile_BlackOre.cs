using CodiceApp;
using UnityEngine;

public class CaveTile_BlackOre : BaseTile, IMeasureable
{
    [HideInInspector]
    public bool isMined = false;
    [HideInInspector]
    public bool isCollected = false;
    [HideInInspector]
    public bool isPurified = false;
    int value;

    [Header("References")]
    public GameObject normalOre;
    public GameObject destroyedOre;
    public GameObject collectableOre;
    public GameObject pointLight;

    [Header("Floating Settings")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.5f;
    private Vector3 startPos;

    void Start() { 
        value = Random.Range(5, 11); 
        startPos = collectableOre.transform.position;
    }
    int IMeasureable.Measured()
    {
        return value;
    }
    public void Purify()
    {
        if (!isMined && !isPurified)
        {
            isPurified = true;
            if (pointLight.TryGetComponent<Light>(out Light myLight))
            {
                myLight.color = new Color(0.5073529f, 0.7361954f, 1f);
            }
            Debug.Log("You purified a Black Ore into a Purple Vein!");
        }
        else
        {
            Debug.Log("Cannot purify.");
        }
    }

    public void Mine()
    {
        if (!isMined)
        {
            isMined = true;

            if (isPurified)
            {
                Debug.Log("You mined a Black Ore!");
            }
            else
            {
                value /= 2;
                Debug.Log("Ore exploded");
            }

            if (normalOre) normalOre.SetActive(false);
            if (destroyedOre) destroyedOre.SetActive(true);
            if (collectableOre) collectableOre.SetActive(true);
        }
        else
        {
            Debug.Log("This Black Ore has already been mined.");
        }
    }
    public int Collect()
    {
        if (isMined)
        {
            if (!isCollected)
            {
                isCollected = true;
                Debug.Log("You collected a Black Ore!");
                if (collectableOre) collectableOre.SetActive(false);
                if (pointLight) pointLight.SetActive(false);

                return value;
            }
            else
            {
                Debug.Log("This White Ore has already been collected.");
            }
        }
        else
        {
            Debug.Log("You need to mine the Black Ore before collecting.");
        }
        return -1;
    }
    public void Update()
    {
        if (!isCollected && isMined)
        {
            float shiftedSin = (Mathf.Sin(Time.time * floatSpeed) + 1f) / 2f;
            float newY = startPos.y + (shiftedSin * floatHeight);

            collectableOre.transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }
}
