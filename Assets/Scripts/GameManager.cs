using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;
using SFB;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour
{
    public InputManager inputManager;
    public ColorPickerController colorPickerController;
    public RoadManager roadManager;
    private Vector3 cameraZenitalPosition = new(9.5f, 11, 9.5f),
        lastCameraPosition = new(10, 5, 0), 
        cameraZenitalRotation = new(90, 0, 0),
        lastCameraRotation = new(35, 0, 0);
    
    private bool isZenitalView = true;
    [SerializeField] private GameObject selectedObject;
    [SerializeField] private GameObject popUpCanvas;
    [SerializeField] private GameObject Gizmos;
    [SerializeField] public Dictionary<int, ObjectMetadata> objectMetadata = new();
    [SerializeField] private List<GameObject> uiElements = new();
    [SerializeField] private GameObject zoomSlider;
    [SerializeField] private GameObject view3DButton;
    [SerializeField] private GameObject viewZenitalButton;
    [SerializeField] private GizmosImagesController gizmosImagesController;
    private TransformGizmos.GizmoController gizmoController;
    private float timeSinceInteractionUI = 0;
    private const float SECONDS_TO_ALLOW_PLACEMENT_BEFORE_DELETE = 0.5f;
    private PrefabMap prefabMap;
    private Canvas colorCanvas;
    private string screenshotFolder = "";

    private void Start()
    {
        inputManager = InputManager.Instance;
        prefabMap = FindFirstObjectByType<PrefabMap>();
        colorPickerController = FindFirstObjectByType<ColorPickerController>();
        colorPickerController.OnColorChange += changeColorHandler;
        colorCanvas = colorPickerController.gameObject.GetComponent<Canvas>();

        inputManager.OnMouseDown += OnMouseDownHandler;;
        inputManager.OnMouseHold += OnMouseHoldHandler;
        inputManager.OnMouseUp += roadManager.FinishPlacement;
        inputManager.OnSecondaryMouseHold += SecondaryMouseHoldHandler;
        inputManager.OnToggleView += ToggleViewHandler;
        inputManager.OnSelectObject += SelectObjectHandler;
        inputManager.OnSelectUIObject += SelectUIObjectHandler;
        inputManager.PlaceUIObject += PlaceUIObjectHandler;
        inputManager.Screenshot += Screenshot;
        inputManager.AfterScreenshot += AfterScreenshotHandler;
        popUpCanvas.SetActive(false);
        gizmoController = Gizmos.GetComponent<TransformGizmos.GizmoController>();
        gizmoController.updateMetadata += UpdateObjectMetadata;
        roadManager.CreateMetadata += CreateMetadataHandler;
        roadManager.RemoveMetadata += RemoveMetadataHandler;
        Gizmos.SetActive(false);
    }

    public void SetZoomLevel()
    {
        if (!isZenitalView) {
            return;
        }
        float value = zoomSlider.GetComponent<Slider>().value;
        float y = cameraZenitalPosition.y - value;
        Vector3 cameraPosition = cameraZenitalPosition;
        cameraPosition.y = y;
        Camera.main.gameObject.transform.position = cameraPosition;
    }

    private void RemoveMetadataHandler(GameObject prefab)
    {
        if (objectMetadata.ContainsKey(prefab.GetInstanceID()))
        {
            objectMetadata.Remove(prefab.GetInstanceID());
        }
        else
        {
            int keyToRemove = objectMetadata.FirstOrDefault(x => x.Value.prefabName.Contains(prefab.name.Replace("(Clone)", "")) && x.Value.position == prefab.transform.position).Key;
            if (keyToRemove != 0)
            {
                objectMetadata.Remove(keyToRemove);
            }
            else
            {
                Debug.LogWarning($"No metadata found for prefab: {prefab.name}");
            }
        }
    }

    private void CreateMetadataHandler(GameObject prefab)
    {
        MeshRenderer meshRenderer = prefab.GetComponent<MeshRenderer>();
        ObjectMetadata metadata = new()
        {
            position = prefab.transform.position,
            rotation = prefab.transform.rotation,
            prefabName = prefab.name.Replace("(Clone)", ""),
            color = meshRenderer ? meshRenderer.material.color : Color.white,
            variant = false
        };
        if (objectMetadata.ContainsKey(prefab.GetInstanceID()))
        {
            objectMetadata[prefab.GetInstanceID()] = metadata;
        }
        else
        {
            objectMetadata.Add(prefab.GetInstanceID(), metadata);
        }
    }

    private bool checkIfIsCrossWalk(Vector3Int position)
    {
        return roadManager.IsCrossWalk(position);
    }

    private void UpdateObjectMetadata()
    {
        if (selectedObject == null || !objectMetadata.ContainsKey(selectedObject.GetInstanceID()))
        {
            return;
        }
        MeshRenderer meshRenderer = selectedObject.GetComponent<MeshRenderer>();
        ObjectMetadata metadata = new()
        {
            position = selectedObject.transform.position,
            rotation = selectedObject.transform.rotation,
            prefabName = selectedObject.name.Replace("(Clone)", ""),
            color = meshRenderer ? meshRenderer.material.color : Color.white,
            variant = selectedObject.CompareTag("Road") && checkIfIsCrossWalk(Vector3Int.RoundToInt(selectedObject.transform.position))
        };
        objectMetadata[selectedObject.GetInstanceID()] = metadata;
    }

    private void PlaceUIObjectHandler(GameObject prefab)
    {
        MeshRenderer meshRenderer = prefab.GetComponent<MeshRenderer>();
        ObjectMetadata metadata = new()
        {
            position = prefab.transform.position,
            rotation = prefab.transform.rotation,
            prefabName = prefab.name.Replace("(Clone)", ""),
            color =  meshRenderer ? meshRenderer.material.color : Color.white,
            variant = false
        };
        objectMetadata.Add(prefab.GetInstanceID(), metadata);
    }

    private void OnMouseDownHandler(Vector3Int position)
    {
        float timePassed = Time.time - timeSinceInteractionUI;
        if (timePassed <= SECONDS_TO_ALLOW_PLACEMENT_BEFORE_DELETE)
        {
            return;
        }
        roadManager.PlaceRoad(position);
    }

    private void changeColorHandler(Color color)
    {
        if (selectedObject != null)
        {
            MeshRenderer meshRenderer = selectedObject.GetComponent<MeshRenderer>();
            if (!meshRenderer)
            {
                return;
            }
            meshRenderer.material.color = color;
            UpdateObjectMetadata();
        }
    }


    private void OnMouseHoldHandler(Vector3Int position)
    {
        float timePassed = Time.time - timeSinceInteractionUI;
        if (timePassed <= SECONDS_TO_ALLOW_PLACEMENT_BEFORE_DELETE || selectedObject != null)
        {
            return;
        }
        roadManager.PlaceRoad(position);
    }


    public void DeleteObjectHandler()
    {
        if (selectedObject.CompareTag("Road"))
        {
            roadManager.RemoveRoad(Vector3Int.RoundToInt(selectedObject.transform.position));
        }
        else
        {
            Gizmos.SetActive(false);
            objectMetadata.Remove(selectedObject.GetInstanceID());
            Destroy(selectedObject);
            inputManager.EnableObjectInteractions();
        }
        ClearSelectedObject();
    }

    public void ClearSelectedObject()
    {
        if (Gizmos.activeSelf)
        {
            gizmoController.ChangeTransformationToNone();
            Gizmos.SetActive(false);
        }
        if (colorCanvas.enabled)
        {
            colorCanvas.enabled = false;
        }
        selectedObject = null;
        popUpCanvas.SetActive(false);
        inputManager.AllowPointerMovement();
        inputManager.EnableObjectInteractions();
        gizmosImagesController.ResetIndex();
    }

    private void SelectUIObjectHandler()
    {
        ClearSelectedObject();
    }

    private void SelectObjectHandler(GameObject obj)
    {
        if (selectedObject != null && obj != null && obj.layer == LayerMask.NameToLayer("UI"))
        {
            if (obj.name == "Delete")
            {
                DeleteObjectHandler();
            } 
            else if (obj.name == "Color")
            {
                colorCanvas.enabled = !colorCanvas.enabled;
            } 
            else if (obj.name == "Close")
            {
                if (!gizmoController.CurrentStateIsNone()) {
                    gizmoController.ChangeTransformationToNone();
                    Gizmos.SetActive(false);
                    inputManager.EnableObjectInteractions();
                }
                Canvas colorCanvas = colorPickerController.gameObject.GetComponent<Canvas>();
                if (colorCanvas.enabled) {
                    colorCanvas.enabled = false;
                }
                ClearSelectedObject();
            }
            else if (obj.name == "CrossWalk" && selectedObject.CompareTag("Road"))
            {
                roadManager.ToggleCrossWalk(Vector3Int.RoundToInt(selectedObject.transform.position));
                ClearSelectedObject();
            } 
            else if (obj.name == "Gizmos" && !selectedObject.CompareTag("Road"))
            {
                if (!Gizmos.activeSelf)
                {
                    Gizmos.SetActive(true);
                    inputManager.DisableObjectInteractions();
                }
                gizmoController.ChangeTransformationState();
                if (selectedObject != gizmoController.GetTargetObject())
                {
                    gizmoController.SetTargetObject(selectedObject);
                }
                if (gizmoController.CurrentStateIsNone())
                {
                    Gizmos.SetActive(false);
                    inputManager.EnableObjectInteractions();
                }
                gizmosImagesController.ToggleImage();
            }
            timeSinceInteractionUI = Time.time;
            return;
        }
        selectedObject = obj;
        if (selectedObject != null)
        {
            Vector3 screenPos = selectedObject.transform.position;
            if (isZenitalView)
            {
                screenPos.x += 1;
            }
            screenPos.y += 1;
            popUpCanvas.SetActive(true);
            popUpCanvas.transform.position = screenPos;
        }
        else
        {
            popUpCanvas.SetActive(false);
        }
    }


    public void ToggleViewHandler()
    {
        if (isZenitalView)
        {
            Camera.main.transform.position = lastCameraPosition;
            Camera.main.transform.eulerAngles = lastCameraRotation;
            view3DButton.gameObject.SetActive(false);
            viewZenitalButton.gameObject.SetActive(true);
        }
        else
        {
            lastCameraPosition = Camera.main.transform.position;
            lastCameraRotation = Camera.main.transform.eulerAngles;
            Camera.main.transform.position = cameraZenitalPosition;
            Camera.main.transform.eulerAngles = cameraZenitalRotation;
            view3DButton.gameObject.SetActive(true);
            viewZenitalButton.gameObject.SetActive(false);
        }
        isZenitalView = !isZenitalView;
        gizmoController.ToggleFixedScale();
    }

    private void SecondaryMouseHoldHandler(Vector2 mouseDelta)
    {
        if (isZenitalView)
        {
            return;
        }
        Quaternion cameraRotation = Camera.main.transform.rotation;
		Quaternion horizontalRotation = Quaternion.AngleAxis( mouseDelta.x, Vector3.up );
		Quaternion verticalRotation = Quaternion.AngleAxis( mouseDelta.y, Vector3.right );
		Camera.main.transform.rotation = horizontalRotation * cameraRotation * verticalRotation;
    }

    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

        // Called from browser
        public void OnFileUpload(string url) {
            StartCoroutine(OutputRoutine(url));
        }

        private IEnumerator OutputRoutine(string url) {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                string content = www.downloadHandler.text;
                ImportScene(content);
                Debug.Log("File imported successfully");
            }
        }

        [DllImport("__Internal")]
        private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

        // Called from browser
        public void OnScreenshotDownload() {
            Debug.Log("Screenshot downloaded successfully");
        }
    #endif

    public void ExportScene()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            string fileName = $"atestator_{DateTime.Now:yyyyMMdd_HHmmss}.jsonl";
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(string.Join("\n", objectMetadata.Values.Select(x => x.ToString())));
            DownloadFile(gameObject.name, "OnFileDownload", fileName, jsonBytes, jsonBytes.Length);
        #elif UNITY_EDITOR
            string[] downloadPaths = StandaloneFileBrowser.OpenFolderPanel("Select export folder", "", false);
            if (downloadPaths.Length == 0)
            {
                Debug.LogError("Folder not selected");
                return;
            }
            string downloadPath = downloadPaths[0];
            if (string.IsNullOrEmpty(downloadPath))
            {
                Debug.LogError("Folder not selected");
                return;
            }
            string filePath = Path.Combine(downloadPath, "atestator_game_data.jsonl");
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    Debug.LogError("File path is empty");
                    return;
                }
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Create(filePath).Close();
                using StreamWriter writer = new(filePath, true);
                foreach (var metadata in objectMetadata)
                {
                    string json = metadata.Value.ToString();
                    writer.WriteLine(json);
                }
                writer.Close();
                Debug.Log($"Data saved to: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving data: {e.Message}");
            }
        #endif
    }

    public void ImportSceneAction()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            UploadFile(gameObject.name, "OnFileUpload", "jsonl", false);
        #elif UNITY_EDITOR
            ExtensionFilter[] extensions = { new ExtensionFilter("Atestator data", "jsonl") };
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select file to import", "", extensions, false);
            if (paths.Length == 0)
            {
                Debug.LogError("File not selected");
                return;
            }
            string filePath = paths[0];
            if (!File.Exists(filePath))
            {
                Debug.LogError("File not found");
                return;
            }
            ImportScene(File.ReadAllText(filePath));
        #endif
    }

    public void ImportScene(string content)
    {
        ClearSceneData();
        foreach (var metadataJson in content.Split("\n", StringSplitOptions.RemoveEmptyEntries))
        {
            try {
                ObjectMetadata metadata = new();
                metadata.fromString(metadataJson);
                GameObject prefab = prefabMap.GetPrefabByName(metadata.prefabName);
                if (prefab.CompareTag("Road"))
                {
                    Vector3Int roundedPosition = Vector3Int.RoundToInt(metadata.position);
                    roadManager.PlaceRoad(roundedPosition, true);
                    if (metadata.variant)
                    {
                        roadManager.ToggleCrossWalk(roundedPosition);
                    }
                }
                else
                {
                    GameObject obj = Instantiate(prefab, metadata.position, metadata.rotation);
                    MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
                    if (meshRenderer)
                    {
                        meshRenderer.material.color = metadata.color;
                    }
                    objectMetadata.Add(obj.GetInstanceID(), metadata);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error importing metadata: {e.Message}");
                Debug.LogError("Make sure the file is the correct one.");
                return;
            }
        }
        roadManager.SetInitialState();
    }

    public void ClearSceneData()
    {
        roadManager.ClearScene();
        foreach (var obj in objectMetadata)
        {
            Destroy(FindObjectsOfType<GameObject>().FirstOrDefault(x => x.GetInstanceID() == obj.Key));
        }
        objectMetadata.Clear();
    }

    public void ScreenshotHandler()
    {
        ToggleUI(false);
        inputManager.TakeScreenshot();
    }

    private void Screenshot()
    {
        string fileName = $"atestator_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        #if UNITY_EDITOR
            if (string.IsNullOrEmpty(screenshotFolder))
            {
                string[] folders = StandaloneFileBrowser.OpenFolderPanel("Select screenshot directory", "", false);
                if (folders.Length == 0)
                {
                    Debug.LogError("Screenshot folder not selected.");
                    return;
                }
                screenshotFolder = folders[0];
            }
            string filePath = Path.Combine(screenshotFolder, fileName);
            ScreenCapture.CaptureScreenshot(filePath);
            Debug.Log($"Screenshot saved to: {filePath}");
        #elif UNITY_WEBGL && !UNITY_EDITOR
            byte[] screenshotBytes = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();
            DownloadFile(gameObject.name, "OnScreenshotDownload", fileName, screenshotBytes, screenshotBytes.Length);
        #endif
    }

    private void AfterScreenshotHandler()
    {
        ToggleUI(true);
    }

    private void ToggleUI(bool visible)
    {
        foreach (GameObject uiElement in uiElements)
        {
            uiElement.SetActive(visible);
        }
    }
}
