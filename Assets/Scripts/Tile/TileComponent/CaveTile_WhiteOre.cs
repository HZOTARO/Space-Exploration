using UnityEngine;

public class CaveTile_WhiteOre : BaseTile, IMeasureable
{
    [HideInInspector]
    public bool isMined = false;
    [HideInInspector]
    public bool isCollected = false;
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

    void Start()
    {
        value = Random.Range(5, 11);
        startPos = collectableOre.transform.position;
    }
    int IMeasureable.Measured()
    {
        return value;
    }
    public void Mine()
    {
        if (!isMined)
        {
            isMined = true;
            if (normalOre) normalOre.SetActive(false);
            if (destroyedOre) destroyedOre.SetActive(true);
            if (collectableOre) collectableOre.SetActive(true);
            Debug.Log("You mined a White Ore!");
        }
    }
    public void Collect()
    {
        if (isMined && !isCollected)
        {
            isCollected = true;
            Debug.Log("You collected a White Ore!");
            if (collectableOre) collectableOre.SetActive(false);
            if (pointLight) pointLight.SetActive(false);
        }
    }
    void Update()
    {
        if (!isCollected && isMined)
        {
            float shiftedSin = (Mathf.Sin(Time.time * floatSpeed) + 1f) / 2f;
            float newY = startPos.y + (shiftedSin * floatHeight);

            collectableOre.transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }
}
