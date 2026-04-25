using UnityEngine;
using UnityEngine.XR;

public class TravelIndicator : MonoBehaviour
{
    public Transform playerTransform; // OVRPlayerController
    public float distanceAhead = 1.5f;
    private GameObject indicator;

    void Start()
    {
        // Create arrow from a cube (body) + cube (head)
        indicator = new GameObject("TravelArrow");

        // Arrow body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(indicator.transform);
        body.transform.localScale = new Vector3(0.05f, 0.01f, 0.3f);
        body.transform.localPosition = new Vector3(0, 0, -0.05f);
        Destroy(body.GetComponent<Collider>());

        // Arrow head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.transform.SetParent(indicator.transform);
        head.transform.localScale = new Vector3(0.15f, 0.01f, 0.15f);
        head.transform.localPosition = new Vector3(0, 0, 0.15f);
        head.transform.localRotation = Quaternion.Euler(0, 45, 0); // Diamond shape
        Destroy(head.GetComponent<Collider>());

        // Make both green and glowing
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.green;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.green * 0.5f);
        body.GetComponent<Renderer>().material = mat;
        head.GetComponent<Renderer>().material = mat;

        indicator.SetActive(false);
    }


    void Update()
    {
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        Vector2 moveInput = Vector2.zero;
        leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out moveInput);
        Debug.Log("Move input: " + moveInput);


        if (moveInput.magnitude > 0.1f)
        {
            indicator.SetActive(true);

            // Calculate move direction
            Vector3 forward = playerTransform.forward;
            Vector3 right = playerTransform.right;
            forward.y = 0; right.y = 0;
            forward.Normalize(); right.Normalize();

            Vector3 moveDir = forward * moveInput.y + right * moveInput.x;

            // Place indicator on the ground ahead
            Vector3 pos = playerTransform.position + moveDir.normalized * distanceAhead;
            pos.y = playerTransform.position.y + 0.01f; // Just above floor
            indicator.transform.position = pos;
        }
        else
        {
            indicator.SetActive(false);
        }
    }
}


