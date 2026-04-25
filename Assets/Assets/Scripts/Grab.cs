using UnityEngine;
using UnityEngine.XR;

public class SimpleGrab : MonoBehaviour
{
    public Transform rayOrigin;
    public float rayDistance = 10f;
    public Color highlightColor = Color.blue;
    public float scaleSpeed = 0.5f;
    public float rotateSpeed = 90f; // degrees per second

    private GameObject grabbedObject;
    private Rigidbody grabbedRb;

    private GameObject currentlyHighlighted;
    private Color[] originalColors;
    private Renderer[] highlightedRenderers;

    void Update()
    {
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        bool rightGrip = false;
        bool leftGrip = false;
        rightDevice.TryGetFeatureValue(CommonUsages.gripButton, out rightGrip);
        leftDevice.TryGetFeatureValue(CommonUsages.gripButton, out leftGrip);

        // ===== HIGHLIGHTING =====
        if (grabbedObject == null)
        {
            if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, rayDistance))
            {
                GameObject hitObj = hit.collider.gameObject;
                if (hitObj.GetComponent<Rigidbody>() != null)
                {
                    if (currentlyHighlighted != hitObj)
                    {
                        RemoveHighlight();
                        ApplyHighlight(hitObj);
                    }
                }
                else RemoveHighlight();
            }
            else RemoveHighlight();
        }

        // ===== GRAB (right grip) =====
        if (rightGrip && grabbedObject == null)
        {
            if (currentlyHighlighted != null)
            {
                grabbedObject = currentlyHighlighted;
                grabbedRb = grabbedObject.GetComponent<Rigidbody>();
                grabbedRb.isKinematic = true;
                grabbedObject.transform.SetParent(rayOrigin);
            }
        }
        else if (!rightGrip && grabbedObject != null)
        {
            grabbedObject.transform.SetParent(null);
            grabbedRb.isKinematic = false;
            grabbedObject = null;
            grabbedRb = null;
        }

        // ===== SCALE + ROTATE (while grabbing) =====
        if (grabbedObject != null)
        {
            float rightTrigger = 0f;
            float leftTrigger = 0f;
            rightDevice.TryGetFeatureValue(CommonUsages.trigger, out rightTrigger);
            leftDevice.TryGetFeatureValue(CommonUsages.trigger, out leftTrigger);

            // Scale
            Vector3 currentScale = grabbedObject.transform.localScale;
            if (rightTrigger > 0.1f)
                currentScale += Vector3.one * scaleSpeed * rightTrigger * Time.deltaTime;
            if (leftTrigger > 0.1f)
                currentScale -= Vector3.one * scaleSpeed * leftTrigger * Time.deltaTime;

            currentScale = Vector3.Max(currentScale, Vector3.one * 0.1f);
            currentScale = Vector3.Min(currentScale, Vector3.one * 5f);
            grabbedObject.transform.localScale = currentScale;

            // Rotate around horizontal (Y) axis using LEFT GRIP
            if (leftGrip)
            {
                grabbedObject.transform.Rotate(0, rotateSpeed * Time.deltaTime, 0, Space.World);
            }
        }
    }

    void ApplyHighlight(GameObject obj)
    {
        currentlyHighlighted = obj;
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
        if (currentlyHighlighted == null) return;

        for (int i = 0; i < highlightedRenderers.Length; i++)
        {
            if (highlightedRenderers[i] != null)
                highlightedRenderers[i].material.color = originalColors[i];
        }

        currentlyHighlighted = null;
        highlightedRenderers = null;
        originalColors = null;
    }
}


