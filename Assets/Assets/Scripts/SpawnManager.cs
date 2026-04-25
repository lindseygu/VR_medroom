using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] prefabs;
    public Transform spawnPoint;
    public float spawnDistance = 2f;
    public GameObject spawnMenu;
    public Transform rayOrigin;
    public Camera eventCamera; // Drag CenterEyeAnchor's camera here

    private bool wasTriggerPressed = false;

    void Start()
    {
        // Set the event camera on the canvas
        Canvas canvas = spawnMenu.GetComponent<Canvas>();
        if (canvas != null && eventCamera != null)
            canvas.worldCamera = eventCamera;
    }

    void Update()
    {
        if (!spawnMenu.activeSelf) return;

        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool triggerPressed = false;
        rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);

        if (triggerPressed && !wasTriggerPressed)
        {
            // Find closest button by distance to where ray intersects
            Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

            Button closestBtn = null;
            float closestDist = float.MaxValue;

            Button[] buttons = spawnMenu.GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                // Find closest point on ray to button center
                Vector3 btnPos = btn.transform.position;
                Vector3 rayToBtn = btnPos - ray.origin;
                float t = Vector3.Dot(rayToBtn, ray.direction);
                Vector3 closestPointOnRay = ray.origin + ray.direction * t;

                float dist = Vector3.Distance(closestPointOnRay, btnPos);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestBtn = btn;
                }
            }

            if (closestBtn != null && closestDist < 0.1f)
            {
                Debug.Log("Clicking: " + closestBtn.gameObject.name);
                closestBtn.onClick.Invoke();
            }
        }
        wasTriggerPressed = triggerPressed;
    }




    public void SpawnItem(int index)
    {
        Debug.Log("SpawnItem called with index: " + index);
        if (index < 0 || index >= prefabs.Length) return;
        if (prefabs[index] == null) return;

        Vector3 pos = spawnPoint.position + spawnPoint.forward * spawnDistance;
        GameObject obj = Instantiate(prefabs[index], pos, Quaternion.identity);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
            rb = obj.AddComponent<Rigidbody>();

        Debug.Log("Spawned: " + prefabs[index].name);
    }
}


