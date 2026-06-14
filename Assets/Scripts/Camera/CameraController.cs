using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Grid Settings (1x1x1)")]
    public int gridWidth = 25;
    public int gridHeight = 25;
    public float tileWidth = 1f;
    public float tileLength = 1f;

    [Header("Camera Boundaries")]
    public float mapPadding = 1.5f;
    [Tooltip("If true, grid locks to center at max zoom. If false, you can drag the grid inside the empty space.")]
    public bool centerWhenZoomedOut = false;

    [Header("UI Padding")]
    public float uiPaddingLeft = 0.05f;
    public float uiPaddingRight = 0.0f;
    public float uiPaddingBottom = 0.0f;
    public float uiPaddingTop = 0.1f;

    [Header("Pan Settings")]
    public float panSpeed = 50f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 75f;
    public float minHeight = 4f;

    [Tooltip("Multiplier for Max Zoom. 1.8 means a 25x25 grid equals exactly 45 Max Height!")]
    public float heightPerTile = 1.6f;

    public float maxHeight;

    [Header("Camera Switching References")]
    public Camera childCamera;
    public Camera thirdPersonCamera;

    private bool isTopDownActive = true;
    private bool isValidPanDrag = false;
    private float mapMinX, mapMaxX, mapMinZ, mapMaxZ;
    private Vector3 dragOrigin;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    public void Initialize()
    {
        if (childCamera == null) childCamera = GetComponentInChildren<Camera>();
        childCamera.orthographic = false;

        isTopDownActive = true;
        childCamera.gameObject.SetActive(true);
        if (thirdPersonCamera != null) thirdPersonCamera.gameObject.SetActive(false);

        // Map Boundaries
        mapMinX = -mapPadding;
        mapMaxX = (gridWidth * tileWidth) + mapPadding;
        mapMinZ = -mapPadding;
        mapMaxZ = (gridHeight * tileLength) + mapPadding;

        // --- DESIGNER CONTROLLED HEIGHT MATH ---
        // Exactly what you asked for: Grid Size * 1.8 = Perfect Height
        float maxDimension = Mathf.Max(gridWidth * tileWidth, gridHeight * tileLength);
        maxHeight = maxDimension * heightPerTile;

        // Reset Position
        Vector3 startPos = transform.position;
        startPos.y = Mathf.Clamp(startPos.y, minHeight, maxHeight);
        transform.position = startPos;

        ClampPosition();
    }

    void LateUpdate()
    {
        if (!isTopDownActive) return;

        HandleZooming();
        HandlePanning();
        ClampPosition();
    }

    public void ToggleCamera()
    {
        isTopDownActive = !isTopDownActive;

        if (childCamera != null) childCamera.gameObject.SetActive(isTopDownActive);
        if (thirdPersonCamera != null) thirdPersonCamera.gameObject.SetActive(!isTopDownActive);
    }

    private void HandleZooming()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        if (scrollData != 0f)
        {
            float activeCenterX = 0.5f + (uiPaddingLeft / 2f) - (uiPaddingRight / 2f);
            float activeCenterY = 0.5f + (uiPaddingBottom / 2f) - (uiPaddingTop / 2f);

            Ray ray;
            Vector3 mouseVP = childCamera.ScreenToViewportPoint(Input.mousePosition);

            if (mouseVP.x >= uiPaddingLeft && mouseVP.x <= (1f - uiPaddingRight) &&
                mouseVP.y >= uiPaddingBottom && mouseVP.y <= (1f - uiPaddingTop))
            {
                ray = childCamera.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                ray = childCamera.ViewportPointToRay(new Vector3(activeCenterX, activeCenterY, 0f));
            }

            Vector3 moveDirection = ray.direction;
            Vector3 proposedPosition = transform.position + (moveDirection * scrollData * zoomSpeed);

            float clampedY = Mathf.Clamp(proposedPosition.y, minHeight, maxHeight);
            float yDifference = clampedY - transform.position.y;

            if (Mathf.Abs(moveDirection.y) > 0.0001f)
            {
                float distanceRatio = yDifference / moveDirection.y;
                transform.position += moveDirection * distanceRatio;
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
            if (groundPlane.Raycast(ray, out float enter)) dragOrigin = ray.GetPoint(enter);
        }

        if (Input.GetMouseButtonUp(0)) isValidPanDrag = false;

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
        float activeCenterX = 0.5f + (uiPaddingLeft / 2f) - (uiPaddingRight / 2f);
        float activeCenterY = 0.5f + (uiPaddingBottom / 2f) - (uiPaddingTop / 2f);

        Ray rCenter = childCamera.ViewportPointToRay(new Vector3(activeCenterX, activeCenterY, 0f));
        if (!groundPlane.Raycast(rCenter, out float dC)) return;
        Vector3 pCenter = rCenter.GetPoint(dC);

        Ray rLeft = childCamera.ViewportPointToRay(new Vector3(uiPaddingLeft, activeCenterY, 0f));
        groundPlane.Raycast(rLeft, out float dL);
        Vector3 pLeft = rLeft.GetPoint(dL);

        Ray rBottom = childCamera.ViewportPointToRay(new Vector3(activeCenterX, uiPaddingBottom, 0f));
        groundPlane.Raycast(rBottom, out float dB);
        Vector3 pBottom = rBottom.GetPoint(dB);

        float halfWidth = Mathf.Abs(pCenter.x - pLeft.x);
        float halfLength = Mathf.Abs(pCenter.z - pBottom.z);

        Vector3 targetCenter = pCenter;

        float minX = mapMinX + halfWidth;
        float maxX = mapMaxX - halfWidth;

        if (minX > maxX)
        {
            if (centerWhenZoomedOut) targetCenter.x = (mapMinX + mapMaxX) / 2f;
            else targetCenter.x = Mathf.Clamp(targetCenter.x, maxX, minX);
        }
        else targetCenter.x = Mathf.Clamp(targetCenter.x, minX, maxX);

        float minZ = mapMinZ + halfLength;
        float maxZ = mapMaxZ - halfLength;

        if (minZ > maxZ)
        {
            if (centerWhenZoomedOut) targetCenter.z = (mapMinZ + mapMaxZ) / 2f;
            else targetCenter.z = Mathf.Clamp(targetCenter.z, maxZ, minZ);
        }
        else targetCenter.z = Mathf.Clamp(targetCenter.z, minZ, maxZ);

        transform.position += (targetCenter - pCenter);
    }
}