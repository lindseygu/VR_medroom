using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class VRMovement : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float turnSpeed = 60.0f;
    public Transform cameraTransform;

    private CharacterController cc;
    private float currentTurnVelocity = 0f;

    void Start()
    {
        cc = gameObject.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.9f, 0);
    }

    void Update()
    {
        Vector2 moveInput = Vector2.zero;
        Vector2 turnInput = Vector2.zero;

        // LEFT controller - movement
        InputDevice left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (left.isValid)
            left.TryGetFeatureValue(CommonUsages.primary2DAxis, out moveInput);

        // RIGHT controller - turning
        InputDevice right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (right.isValid)
            right.TryGetFeatureValue(CommonUsages.primary2DAxis, out turnInput);

        // ===== MOVEMENT =====
        Vector3 fwd = cameraTransform.forward;
        Vector3 rgt = cameraTransform.right;
        fwd.y = 0; rgt.y = 0;
        fwd.Normalize(); rgt.Normalize();

        Vector3 dir = fwd * moveInput.y + rgt * moveInput.x;
        dir *= moveSpeed;

        if (!cc.isGrounded)
            dir.y = -9.81f;

        cc.Move(dir * Time.deltaTime);

        // ===== TURNING =====
        float stickX = turnInput.x;

        // Smooth deadzone
        if (Mathf.Abs(stickX) < 0.15f)
            stickX = 0f;
        else
            stickX = (stickX - Mathf.Sign(stickX) * 0.15f) / (1f - 0.15f);

        float targetTurn = stickX * turnSpeed;

        // Smooth interpolation
        currentTurnVelocity = Mathf.Lerp(currentTurnVelocity, targetTurn, Time.deltaTime * 8f);

        // Rotate around camera position (not rig center)
        if (Mathf.Abs(currentTurnVelocity) > 0.01f)
        {
            float turnAmount = currentTurnVelocity * Time.deltaTime;
            transform.RotateAround(cameraTransform.position, Vector3.up, turnAmount);
        }
    }
}


