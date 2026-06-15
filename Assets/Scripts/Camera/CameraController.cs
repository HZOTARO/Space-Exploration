using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public float uiPaddingBottom = 0.1f;
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
    public Button toggleCameraButton;
    public Camera topDownCamera;       // The only rendering camera
    public Transform thirdPersonCamera; // We just need the Transform of the 3rd person target!

    [Header("Blend Settings")]
    public float blendDuration = 0.8f; // How many seconds the transition takes
    public float thirdPersonFOV = 60f; // Wide FOV for 3rd person

    private bool isTopDownActive = true;
    private bool isBlending = false;
    private Coroutine blendCoroutine;

    // To remember where the top-down camera sits relative to the Rig
    private Vector3 topDownLocalPos;
    private Quaternion topDownLocalRot;
    private float topDownFOV;

    private bool isValidPanDrag = false;
    private float mapMinX, mapMaxX, mapMinZ, mapMaxZ;
    private Vector3 dragOrigin;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    public void Initialize()
    {
        if (toggleCameraButton != null)
        {
            toggleCameraButton.onClick.RemoveAllListeners();
            toggleCameraButton.onClick.AddListener(ToggleCamera);
        }

        if (topDownCamera == null) topDownCamera = GetComponentInChildren<Camera>();
        topDownCamera.orthographic = false;

        // Save the Top-Down defaults so we can blend back to them
        topDownLocalPos = topDownCamera.transform.localPosition;
        topDownLocalRot = topDownCamera.transform.localRotation;
        topDownFOV = topDownCamera.fieldOfView;

        isTopDownActive = true;
        isBlending = false;

        // Turn off the Camera component on the 3rd person target so we don't render the game twice!
        if (thirdPersonCamera != null)
        {
            Camera tpCam = thirdPersonCamera.GetComponent<Camera>();
            if (tpCam != null) tpCam.enabled = false;

            AudioListener tpAudio = thirdPersonCamera.GetComponent<AudioListener>();
            if (tpAudio != null) tpAudio.enabled = false;
        }

        // Map Boundaries
        mapMinX = -mapPadding;
        mapMaxX = (gridWidth * tileWidth) + mapPadding;
        mapMinZ = -mapPadding;
        mapMaxZ = (gridHeight * tileLength) + mapPadding;

        // Height Math
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
        // 1. If we are currently transitioning, halt all input!
        if (isBlending) return;

        // 2. If we are in 3rd Person, lock the camera to the Robot's shoulder and halt map input!
        if (!isTopDownActive && thirdPersonCamera != null)
        {
            topDownCamera.transform.position = thirdPersonCamera.position;
            topDownCamera.transform.rotation = thirdPersonCamera.rotation;
            return;
        }

        // 3. Otherwise, do normal Top-Down panning and zooming
        HandleZooming();
        HandlePanning();
        ClampPosition();
    }

    public void ToggleCamera()
    {
        if (thirdPersonCamera == null) return;

        isTopDownActive = !isTopDownActive;

        // Stop any current transitions and start a new one
        if (blendCoroutine != null) StopCoroutine(blendCoroutine);
        blendCoroutine = StartCoroutine(BlendCameraTransition(isTopDownActive));
    }

    private IEnumerator BlendCameraTransition(bool returningToTopDown)
    {
        isBlending = true;
        float elapsed = 0f;

        // Capture exactly where the camera is right now
        Vector3 startPos = topDownCamera.transform.position;
        Quaternion startRot = topDownCamera.transform.rotation;
        float startFOV = topDownCamera.fieldOfView;

        float targetFOV = returningToTopDown ? topDownFOV : thirdPersonFOV;

        while (elapsed < blendDuration)
        {
            elapsed += Time.deltaTime;
            // SmoothStep makes the sweep start slow, speed up, and slow down at the end
            float t = Mathf.SmoothStep(0f, 1f, elapsed / blendDuration);

            // Dynamically calculate the target position (in case the robot is moving while we blend!)
            Vector3 targetPos = returningToTopDown ? transform.TransformPoint(topDownLocalPos) : thirdPersonCamera.position;
            Quaternion targetRot = returningToTopDown ? transform.rotation * topDownLocalRot : thirdPersonCamera.rotation;

            // Apply the sweep
            topDownCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            topDownCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            topDownCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);

            yield return null;
        }

        // Ensure perfect snap at the end of the transition
        if (returningToTopDown)
        {
            topDownCamera.transform.localPosition = topDownLocalPos;
            topDownCamera.transform.localRotation = topDownLocalRot;
        }
        topDownCamera.fieldOfView = targetFOV;

        isBlending = false;
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
            Vector3 mouseVP = topDownCamera.ScreenToViewportPoint(Input.mousePosition);

            if (mouseVP.x >= uiPaddingLeft && mouseVP.x <= (1f - uiPaddingRight) &&
                mouseVP.y >= uiPaddingBottom && mouseVP.y <= (1f - uiPaddingTop))
            {
                ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                ray = topDownCamera.ViewportPointToRay(new Vector3(activeCenterX, activeCenterY, 0f));
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
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out float enter)) dragOrigin = ray.GetPoint(enter);
        }

        if (Input.GetMouseButtonUp(0)) isValidPanDrag = false;

        if (isValidPanDrag && Input.GetMouseButton(0))
        {
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
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

        Ray rCenter = topDownCamera.ViewportPointToRay(new Vector3(activeCenterX, activeCenterY, 0f));
        if (!groundPlane.Raycast(rCenter, out float dC)) return;
        Vector3 pCenter = rCenter.GetPoint(dC);

        Ray rLeft = topDownCamera.ViewportPointToRay(new Vector3(uiPaddingLeft, activeCenterY, 0f));
        groundPlane.Raycast(rLeft, out float dL);
        Vector3 pLeft = rLeft.GetPoint(dL);

        Ray rBottom = topDownCamera.ViewportPointToRay(new Vector3(activeCenterX, uiPaddingBottom, 0f));
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