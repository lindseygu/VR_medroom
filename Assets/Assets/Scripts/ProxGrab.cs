using UnityEngine;
using UnityEngine.XR;

public class ProximityGrab : MonoBehaviour
{
    public Transform leftHand;
    public float grabRadius = 0.5f;
    public Color highlightColor = Color.cyan;
    public float scaleSpeed = 0.5f;
    public float rotateSpeed = 90f;

    private GameObject nearObject;
    private GameObject grabbedObject;
    private Rigidbody grabbedRb;
    private Renderer[] highlightedRenderers;
    private Color[] originalColors;

    private Rigidbody[] allRigidbodies;
    private float refreshTimer = 0f;
    private GameObject indicator;

    void Start()
    {
        allRigidbodies = FindObjectsOfType<Rigidbody>();

        indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.transform.localScale = Vector3.one * 0.05f;
        Destroy(indicator.GetComponent<Collider>());
        indicator.transform.SetParent(leftHand);
        indicator.transform.localPosition = Vector3.zero;

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.cyan;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.cyan * 0.5f);
        indicator.GetComponent<Renderer>().material = mat;
    }

    void Update()
    {
        refreshTimer += Time.deltaTime;
        if (refreshTimer > 3f)
        {
            allRigidbodies = FindObjectsOfType<Rigidbody>();
            refreshTimer = 0f;
        }

        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        bool leftGrip = false;
        bool rightGrip = false;
        float rightTrigger = 0f;
        float leftTrigger = 0f;

        leftDevice.TryGetFeatureValue(CommonUsages.gripButton, out leftGrip);
        rightDevice.TryGetFeatureValue(CommonUsages.gripButton, out rightGrip);
        rightDevice.TryGetFeatureValue(CommonUsages.trigger, out rightTrigger);
        leftDevice.TryGetFeatureValue(CommonUsages.trigger, out leftTrigger);

        if (grabbedObject == null)
        {
            GameObject closest = null;
            float closestDist = grabRadius;

            foreach (Rigidbody rb in allRigidbodies)
            {
                if (rb == null) continue;
                float dist = Vector3.Distance(leftHand.position, rb.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = rb.gameObject;
                }
            }

            if (closest != nearObject)
            {
                RemoveHighlight();
                if (closest != null)
                    ApplyHighlight(closest);
                nearObject = closest;
            }

            if (leftGrip && nearObject != null)
            {
                grabbedObject = nearObject;
                grabbedRb = grabbedObject.GetComponent<Rigidbody>();
                grabbedRb.isKinematic = true;
                grabbedObject.transform.SetParent(leftHand);
            }
        }
        else
        {
            if (rightTrigger > 0.1f)
                grabbedObject.transform.localScale += Vector3.one * scaleSpeed * rightTrigger * Time.deltaTime;
            if (leftTrigger > 0.1f)
                grabbedObject.transform.localScale -= Vector3.one * scaleSpeed * leftTrigger * Time.deltaTime;

            grabbedObject.transform.localScale = Vector3.Max(grabbedObject.transform.localScale, Vector3.one * 0.1f);
            grabbedObject.transform.localScale = Vector3.Min(grabbedObject.transform.localScale, Vector3.one * 5f);

            if (rightGrip)
                grabbedObject.transform.Rotate(0, rotateSpeed * Time.deltaTime, 0, Space.World);

            if (!leftGrip)
            {
                grabbedObject.transform.SetParent(null);
                grabbedRb.isKinematic = false;
                RemoveHighlight();
                grabbedObject = null;
                grabbedRb = null;
                nearObject = null;
            }
        }
    }

    void ApplyHighlight(GameObject obj)
    {
        highlightedRenderers = obj.GetComponentsInChildren<Renderer>();
        originalColors = new Color[highlightedRenderers.Length];
        for (int i = 0; i < highlightedRenderers.Length; i++)
        {
            originalColors[i] = highlightedRenderers[i].material.color;
            highlightedRenderers[i].material.color = highlightColor;
        }
    }

    void RemoveHighlight()
    {
        if (highlightedRenderers == null) return;
        for (int i = 0; i < highlightedRenderers.Length; i++)
        {
            if (highlightedRenderers[i] != null)
                highlightedRenderers[i].material.color = originalColors[i];
        }
        highlightedRenderers = null;
        originalColors = null;
    }
}


