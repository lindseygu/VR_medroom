using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Prefabs to Spawn")]
    public GameObject[] spawnablePrefabs;

    [Header("Controller References")]
    public Transform controllerTransform;
    public InputActionProperty triggerAction;
    public InputActionProperty gripAction;

    [Header("Spawn Settings")]
    public float maxRayDistance = 10f;
    public LayerMask groundLayer; 
    public Color previewColor = new Color(0f, 1f, 0f, 0.4f);

    [Header("Ray Visual")]
    public LineRenderer lineRenderer;

    private int currentPrefabIndex = 0;
    private GameObject previewObject;
    private bool gripWasPressed = false;

    void Start()
    {
        // Enable input actions
        triggerAction.action.Enable();
        gripAction.action.Enable();

        // Create initial preview
        CreatePreview();
    }

    void Update()
    {
        // --- Grip: Cycle through items ---
        bool gripPressed = gripAction.action.ReadValue<float>() > 0.5f;
        if (gripPressed && !gripWasPressed)
        {
            CycleItem();
        }
        gripWasPressed = gripPressed;

        // --- Raycast from controller ---
        Ray ray = new Ray(controllerTransform.position, controllerTransform.forward);
        RaycastHit hit;
        bool didHit = Physics.Raycast(ray, out hit, maxRayDistance, groundLayer);

        // Update line renderer (the visual ray)
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, controllerTransform.position);
            if (didHit)
                lineRenderer.SetPosition(1, hit.point);
            else
                lineRenderer.SetPosition(1, controllerTransform.position + controllerTransform.forward * maxRayDistance);
        }

        // Update preview position
        if (didHit && previewObject != null)
        {
            previewObject.SetActive(true);
            previewObject.transform.position = hit.point;
            // Match controller's Y rotation so user can control orientation
            float yRotation = controllerTransform.eulerAngles.y;
            previewObject.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
        else if (previewObject != null)
        {
            previewObject.SetActive(false);
        }

        // --- Trigger: Spawn the item ---
        if (triggerAction.action.WasPressedThisFrame() && didHit)
        {
            SpawnObject(hit.point);
        }
    }

    void CycleItem()
    {
        currentPrefabIndex = (currentPrefabIndex + 1) % spawnablePrefabs.Length;

        // Recreate preview with new prefab
        if (previewObject != null)
            Destroy(previewObject);

        CreatePreview();
    }

    void CreatePreview()
    {
        if (spawnablePrefabs.Length == 0) return;

        previewObject = Instantiate(spawnablePrefabs[currentPrefabIndex]);
        previewObject.name = "SpawnPreview";

        // Disable physics on preview
        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Disable colliders on preview
        foreach (Collider col in previewObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Make it transparent
        SetPreviewMaterial(previewObject);

        previewObject.SetActive(false);
    }

    void SetPreviewMaterial(GameObject obj)
    {
        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in renderer.materials)
            {
                // Set to transparent
                mat.color = previewColor;
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }
    }

    void SpawnObject(Vector3 position)
    {
        // Spawn the actual object
        GameObject spawned = Instantiate(spawnablePrefabs[currentPrefabIndex], position, previewObject.transform.rotation);
        spawned.name = spawnablePrefabs[currentPrefabIndex].name + "_Spawned";

        // Make sure it has physics
        Rigidbody rb = spawned.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = spawned.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.useGravity = true;

        // Make sure it has a collider
        if (spawned.GetComponent<Collider>() == null && spawned.GetComponentInChildren<Collider>() == null)
        {
            spawned.AddComponent<BoxCollider>();
        }
    }
}
