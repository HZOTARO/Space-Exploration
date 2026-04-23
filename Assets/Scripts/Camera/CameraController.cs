using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 25;
    public int gridHeight = 25;
    public float tileWidth = 2f;
    public float tileLength = 2.5f;

    [Header("UI Padding")]
    public float uiPaddingLeft = 0.0f;  
    public float uiPaddingRight = 0.0f; 
    public float uiPaddingBottom = 0.2f;
    public float uiPaddingTop = 0.2f;

    [Header("Pan Settings")]
    public float panSpeed = 50f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;
    public float minZoom = 2f;
    public float maxZoom = 15f;

    [Header("References")]
    public Camera childCamera;

    private float cameraPitchAngle;
    private bool isValidPanDrag = false;

    float mapMinX, mapMaxX, mapMinZ, mapMaxZ;
    private Vector3 dragOrigin;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    public void Initialize()
    {
        if (childCamera == null) childCamera = GetComponentInChildren<Camera>();

        mapMinX = 0;
        mapMaxX = gridWidth * tileWidth + 2;

        mapMinZ = 6;
        mapMaxZ = gridHeight * tileLength + 10;

        int lowerDimension = Mathf.Min(gridWidth, gridHeight);
        maxZoom = lowerDimension + 2 + Mathf.Floor(lowerDimension / 10) * 1;

        cameraPitchAngle = childCamera.transform.eulerAngles.x * Mathf.Deg2Rad;
    }

    void LateUpdate()
    {
        HandleZooming();
        HandlePanning();
        ClampPosition();
    }

    private void HandleZooming()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        if (scrollData != 0f)
        {
            float targetX = childCamera.pixelRect.xMin + (childCamera.pixelWidth * uiPaddingLeft);
            float targetY = childCamera.pixelRect.yMin + (childCamera.pixelHeight * uiPaddingBottom);

            Vector3 zoomScreenTarget = new Vector3(targetX, targetY, 0f);

            Ray rayBefore = childCamera.ScreenPointToRay(zoomScreenTarget);
            Vector3 worldTargetBefore = Vector3.zero;
            if (groundPlane.Raycast(rayBefore, out float enterBefore))
            {
                worldTargetBefore = rayBefore.GetPoint(enterBefore);
            }

            childCamera.orthographicSize -= scrollData * zoomSpeed;
            childCamera.orthographicSize = Mathf.Clamp(childCamera.orthographicSize, minZoom, maxZoom);

            Ray rayAfter = childCamera.ScreenPointToRay(zoomScreenTarget);
            if (groundPlane.Raycast(rayAfter, out float enterAfter))
            {
                Vector3 worldTargetAfter = rayAfter.GetPoint(enterAfter);
                transform.position += (worldTargetBefore - worldTargetAfter);
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
                return;
            }

            isValidPanDrag = true;
            Ray ray = childCamera.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float enter))
            {
                dragOrigin = ray.GetPoint(enter);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isValidPanDrag = false;
        }

        if (isValidPanDrag && Input.GetMouseButton(0))
        {
            Ray ray = childCamera.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 currentPoint = ray.GetPoint(enter);
                Vector3 difference = dragOrigin - currentPoint;
                transform.position += difference;
            }
        }
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;

        float viewHeight = (childCamera.orthographicSize * 2f) / Mathf.Sin(cameraPitchAngle);
        float viewWidth = (childCamera.orthographicSize * 2f) * childCamera.aspect;

        float uiWorldLeft = viewWidth * uiPaddingLeft;
        float uiWorldRight = viewWidth * uiPaddingRight;
        float uiWorldBottom = viewHeight * uiPaddingBottom;
        float uiWorldTop = viewHeight * uiPaddingTop;

        float minAllowedX = mapMinX + (viewWidth / 2f) - uiWorldLeft;
        float maxAllowedX = mapMaxX - (viewWidth / 2f) + uiWorldRight;

        float minAllowedZ = mapMinZ + (viewHeight / 2f) - uiWorldBottom;
        float maxAllowedZ = mapMaxZ - (viewHeight / 2f) + uiWorldTop;

        if (minAllowedX > maxAllowedX)
            pos.x = (minAllowedX + maxAllowedX) / 2f;
        else
            pos.x = Mathf.Clamp(pos.x, minAllowedX, maxAllowedX);

        if (minAllowedZ > maxAllowedZ)
            pos.z = (minAllowedZ + maxAllowedZ) / 2f;
        else
            pos.z = Mathf.Clamp(pos.z, minAllowedZ, maxAllowedZ);

        transform.position = pos;
    }
}