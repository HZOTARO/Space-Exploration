using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public float tileCount = 25f;

    [Header("Map Boundaries (Absolute Edges)")]
    public float mapMinX = 0f;
    public float mapMaxX = 11f;
    public float mapMinZ = -12f;
    public float mapMaxZ = 9f;

    [Header("Pan Settings")]
    public float panSpeed = 20f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 20f;
    public float minZoom = 2f;
    public float maxZoom = 15f;

    private Camera cam;

    private bool isValidPanDrag = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        // 2 per tile
        mapMaxX += 2 * (tileCount - 4);
        // 2.5 per tile
        mapMaxZ = mapMaxZ + 2.5f * (tileCount - 3) + mapMinZ;
        // Zoom is around 1 block per zoom in screen, so 2 = 2x2
        maxZoom = Mathf.Max(6, Mathf.Min(tileCount / 2, 16));
    }

    void LateUpdate()
    {
        HandleZooming();
        HandlePanning();
    }

    private void HandleZooming()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        float scrollData = Input.GetAxis("Mouse ScrollWheel");

        if (scrollData != 0f)
        {
            float oldSize = cam.orthographicSize;
            float targetSize = oldSize - (scrollData * zoomSpeed);

            float maxAllowedZoom = CalculateMaxZoom();
            float actualMaxZoom = Mathf.Min(maxZoom, maxAllowedZoom);

            float newSize = Mathf.Clamp(targetSize, minZoom, actualMaxZoom);

            if (newSize != oldSize)
            {
                cam.orthographicSize = newSize;

                float sizeDifference = newSize - oldSize;

                float moveZ = sizeDifference;
                float moveX = sizeDifference * cam.aspect;

                Vector3 newPos = transform.position;
                newPos.x += moveX;
                newPos.z += moveZ;
                transform.position = newPos;
            }
        }
    }

    private void HandlePanning()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                isValidPanDrag = false;
            }
            else
            {
                isValidPanDrag = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isValidPanDrag = false;
        }

        Vector3 newPosition = transform.position;

        if (isValidPanDrag && Input.GetMouseButton(0))
        {
            float currentZoomMultiplier = cam.orthographicSize / 5f;
            float moveX = -Input.GetAxis("Mouse X") * panSpeed * currentZoomMultiplier * Time.deltaTime;
            float moveZ = -Input.GetAxis("Mouse Y") * panSpeed * currentZoomMultiplier * Time.deltaTime;

            newPosition.x += moveX;
            newPosition.z += moveZ;
        }

        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = cam.orthographicSize * cam.aspect;

        float limitMinX = mapMinX + camHalfWidth;
        float limitMaxX = mapMaxX - camHalfWidth;

        float limitMinZ = mapMinZ + camHalfHeight;
        float limitMaxZ = mapMaxZ - camHalfHeight;

        newPosition.x = Mathf.Clamp(newPosition.x, limitMinX, limitMaxX);
        newPosition.z = Mathf.Clamp(newPosition.z, limitMinZ, limitMaxZ);

        transform.position = newPosition;
    }

    private float CalculateMaxZoom()
    {
        float mapWidth = mapMaxX - mapMinX;
        float mapHeight = mapMaxZ - mapMinZ;

        float maxZoomVertical = mapHeight / 2f;
        float maxZoomHorizontal = (mapWidth / 2f) / cam.aspect;

        return Mathf.Min(maxZoomVertical, maxZoomHorizontal);
    }
}