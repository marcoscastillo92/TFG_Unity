using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public Action<Vector3Int> OnMouseDown, OnMouseHold;
    public Action OnMouseUp, OnSecondaryMouseUp, OnToggleView, OnSelectUIObject, Screenshot, AfterScreenshot;
    public Action<GameObject> OnSelectObject, PlaceUIObject;

    public Action<Vector2> OnArrowInput, OnSecondaryMouseHold;
    public static InputManager Instance { get; private set; }

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask selectableMask;
    [SerializeField] private LayerMask uiMask;
    [SerializeField] private float lookSensitivity = 2.0f;
    [SerializeField] private float cameraSpeed = 500.0f;
    [SerializeField] private float accSprintMultiplier = 4.0f;
    [SerializeField] private float dampingCoefficient = 1.0f;
    [SerializeField] private GameObject cellPointer;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject zoomPanel;
    [SerializeField] private Slider zoomSlider;
    [SerializeField] private Button zoomButton;
    [SerializeField] private GameObject popUpCanvas;
    [SerializeField] private float popupCanvasScaleFactor = 10.0f;
    private bool zenitalViewEnabled = true;
    private bool allowPointerMovement = true;
    private bool placingAttachedPrefab = false;
    private bool draggingAttachedPrefab = false;
    private bool objectInteractions = true;
    private bool screenshot, screenshotTaken = false;
    private GameObject attachedPrefab;
    private float screenshotTime, afterScreenshotTime = 0.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator WaitAndDisablePlacing()
    {
        yield return new WaitForSeconds(0.1f);
        placingAttachedPrefab = false;
        draggingAttachedPrefab = false;
    }

    private void Start()
    {
        foreach (ClickablePrefabUI clickablePrefabUI in Resources.FindObjectsOfTypeAll<ClickablePrefabUI>())
        {
            clickablePrefabUI.OnSelectUIObject += SelectUIObjectHandler;
            clickablePrefabUI.OnDragUIObject += DragUIObjectHandler;
        }
    }

    private void DragUIObjectHandler(GameObject prefab)
    {
        OnSelectUIObject?.Invoke();
        if (attachedPrefab != null)
        {
            Destroy(attachedPrefab);
        }
        attachedPrefab = Instantiate(prefab);
        attachedPrefab.GetComponent<Collider>().enabled = false;
        placingAttachedPrefab = true;
        draggingAttachedPrefab = true;
    }

    private void DropUIObjectHandler()
    {
        if (!attachedPrefab && !draggingAttachedPrefab && !placingAttachedPrefab)
        {
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, selectableMask))
        {
            attachedPrefab.transform.position = hit2.point;
            attachedPrefab.GetComponent<Collider>().enabled = true;
            PlaceUIObject?.Invoke(attachedPrefab);
            attachedPrefab = null;
        }
        else if (Physics.Raycast(ray, out RaycastHit hit3, Mathf.Infinity, groundMask)) 
        {
            attachedPrefab.transform.position = hit3.point;
            attachedPrefab.GetComponent<Collider>().enabled = true;
            PlaceUIObject?.Invoke(attachedPrefab);
            attachedPrefab = null;
        }
        StartCoroutine(WaitAndDisablePlacing());
    }

    private void SelectUIObjectHandler(GameObject prefab)
    {
        OnSelectUIObject?.Invoke();
        if (attachedPrefab != null)
        {
            Destroy(attachedPrefab);
        }
        attachedPrefab = Instantiate(prefab);
        attachedPrefab.GetComponent<Collider>().enabled = false;
        placingAttachedPrefab = true;
    }

    public bool IsPlacingAttachedPrefab()
    {
        return placingAttachedPrefab;
    }


    public void ToggleCameraMovement()
    {
        zenitalViewEnabled = !zenitalViewEnabled;
        if (!zenitalViewEnabled)
        {
            zoomPanel.SetActive(false);
            zoomSlider.value = 0.0f;
        }
        zoomButton.enabled = zenitalViewEnabled;
    }

    private void Update()
    {
        CheckScreenshotEvent();
        CheckClickDownEvent();
        CheckClickUpEvent();
        CheckClickHoldEvent();
        CheckArrowInputEvent();
        CheckSecondaryClickHoldEvent();
        CheckSecondaryClickUpEvent();
        CheckToggleViewEvent();
        CheckMousePosition();
        CheckEscButtonEvent();
        ResizePopUpCanvas();
    }

    private void ResizePopUpCanvas()
    {
        float cameraDistance = Vector3.Distance(Camera.main.transform.position, cellPointer.transform.position);
        float scaleFactor = cameraDistance / popupCanvasScaleFactor;
        scaleFactor = Mathf.Clamp(scaleFactor, 0.01f, 1.0f);
        popUpCanvas.transform.localScale = new Vector3(0.02f * scaleFactor, 0.02f * scaleFactor, 0.02f * scaleFactor);
    }

    private void CheckScreenshotEvent()
    {
        if (screenshot)
        {
            float elapsedScreenshotTime = Time.time - screenshotTime;
            if (elapsedScreenshotTime > 0.5f)
            {
                Screenshot.Invoke();
                screenshot = false;
                screenshotTaken = true;
            }
        }
        else if (screenshotTaken)
        {
            float elapsedAfterScreenshotTime = Time.time - afterScreenshotTime;
            if (elapsedAfterScreenshotTime > 0.5f)
            {
                AfterScreenshot.Invoke();
                screenshotTaken = false;
            }
        }
    }

    public void TakeScreenshot()
    {
        screenshot = true;
        screenshotTime = Time.time;
    }

    public void EndScreenshot()
    {
        afterScreenshotTime = Time.time;
    }


    private void CheckMousePosition()
    {
        if (!allowPointerMovement || !objectInteractions)
        {
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
        {
            Vector3Int? gridPosition = Vector3Int.RoundToInt(hit.point);
            if (gridPosition.HasValue)
            {
                cellPointer.transform.position = gridPosition.Value;
                if (attachedPrefab != null)
                {
                    attachedPrefab.transform.position = hit.point;
                }
            }
        }
    }


    private void CheckToggleViewEvent()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleCameraMovement();
            OnToggleView?.Invoke();
        }
    }

    private void CheckSecondaryClickUpEvent()
    {
        if (Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            OnSecondaryMouseUp?.Invoke();
        }
    }

    private void CheckSecondaryClickHoldEvent()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Vector2 mouseDelta = lookSensitivity * new Vector2( Input.GetAxis( "Mouse X" ), -Input.GetAxis( "Mouse Y" ) );
            OnSecondaryMouseHold?.Invoke(mouseDelta);
        }
    }

    private void CheckArrowInputEvent()
    {
        Vector3 movement = GetAccelerationVector() * Time.deltaTime;
        movement = Vector3.Lerp(movement, Vector3.zero, dampingCoefficient * Time.deltaTime);
        Camera.main.transform.position += movement * Time.deltaTime;
    }

    private void CheckClickHoldEvent()
    {
        if (!objectInteractions)
        {
            return;
        }
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
            {
                if (IsPlacingAttachedPrefab())
                {
                    if (draggingAttachedPrefab)
                    {
                        return;
                    }
                    StartCoroutine(WaitAndDisablePlacing());
                    return;
                }
                Vector3Int? gridPosition = Vector3Int.RoundToInt(hit.point);
                OnMouseHold?.Invoke(gridPosition.Value);
            }
        }
    }

    private void CheckClickUpEvent()
    {
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            OnMouseUp?.Invoke();
            if (attachedPrefab != null && draggingAttachedPrefab)
            {
                DropUIObjectHandler();
            }
        }
    }

    private void CheckClickDownEvent()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(confirmPanel.activeSelf)
            {
                confirmPanel.SetActive(false);
            }
        }
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, uiMask))
            {
                allowPointerMovement = false;
                OnSelectObject?.Invoke(hit.collider.gameObject);
                Debug.Log("UI clicked: " + hit.collider.gameObject.name);
            }
            else if (Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, selectableMask))
            {
                if (!objectInteractions)
                {
                    return;
                }
                GameObject hitGameObject = hit2.collider.gameObject;
                if (IsPlacingAttachedPrefab())
                {
                    if (hitGameObject.tag != "Road")
                    {
                        return;
                    }
                    attachedPrefab.transform.position = hit2.point;
                    attachedPrefab.GetComponent<Collider>().enabled = true;
                    PlaceUIObject?.Invoke(attachedPrefab);
                    attachedPrefab = null;
                }
                else
                {
                    allowPointerMovement = false;
                    cellPointer.transform.position = hitGameObject.transform.position;
                    OnSelectObject?.Invoke(hitGameObject);
                    Debug.Log("Selectable clicked: " + hitGameObject.name);
                }
            }
            else if (Physics.Raycast(ray, out RaycastHit hit3, Mathf.Infinity, groundMask)) 
            {
                if (!objectInteractions)
                {
                    return;
                }
                Vector3Int? gridPosition = Vector3Int.RoundToInt(hit3.point);
                if (IsPlacingAttachedPrefab())
                {
                    attachedPrefab.transform.position = hit3.point;
                    attachedPrefab.GetComponent<Collider>().enabled = true;
                    PlaceUIObject?.Invoke(attachedPrefab);
                    attachedPrefab = null;
                }
                else
                {
                    allowPointerMovement = true;
                    Debug.Log("Ground hit");
                    OnSelectObject?.Invoke(null);
                    OnMouseDown?.Invoke(gridPosition.Value);
                }
            }
        }
    }

    private void CheckEscButtonEvent()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnSelectObject?.Invoke(null);
            allowPointerMovement = true;
        }
    }

    public void AllowPointerMovement()
    {
        allowPointerMovement = true;
    }

    private Vector3 GetAccelerationVector() 
    {
		Vector3 moveInput = default;

		void AddMovement(KeyCode key, Vector3 dir) 
        {
			if (Input.GetKey(key))
            {
                moveInput += dir;
            }
		}

        if (zenitalViewEnabled)
        {
            AddMovement(KeyCode.W, Vector3.up);
            AddMovement(KeyCode.S, Vector3.down);
            AddMovement(KeyCode.D, Vector3.right);
            AddMovement(KeyCode.A, Vector3.left);
        }
        else
        {
            AddMovement(KeyCode.W, Vector3.forward);
            AddMovement(KeyCode.S, Vector3.back);
            AddMovement(KeyCode.D, Vector3.right);
            AddMovement(KeyCode.A, Vector3.left);
            AddMovement(KeyCode.E, Vector3.up);
            AddMovement(KeyCode.Q, Vector3.down);
        }
		
		Vector3 direction = Camera.main.transform.TransformVector(moveInput.normalized);

        if (Input.GetKey(KeyCode.LeftShift))
        {
			return direction * (cameraSpeed * accSprintMultiplier);
        }

		return direction * cameraSpeed;
	}

    public void DisableObjectInteractions()
    {
        objectInteractions = false;
    }

    public void EnableObjectInteractions()
    {
        objectInteractions = true;
    }
}
