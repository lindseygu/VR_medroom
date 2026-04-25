using UnityEngine;
using UnityEngine.XR;

public class MenuToggle : MonoBehaviour
{
    public GameObject spawnMenu;
    public GameObject leftRay; // Drag your left Ray Interactor here
    private bool wasPressed = false;

    void Start()
    {
        spawnMenu.SetActive(false);
    }

    void Update()
    {
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        bool pressed = false;
        leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out pressed);

        if (pressed && !wasPressed)
        {
            bool menuOn = !spawnMenu.activeSelf;
            spawnMenu.SetActive(menuOn);

            // Hide left ray when menu is open, show when closed
            if (leftRay != null)
                leftRay.SetActive(!menuOn);
        }
        wasPressed = pressed;
    }
}


