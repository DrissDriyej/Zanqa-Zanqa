using UnityEngine;
using Unity.XR.CoreUtils;

public class VRCollisionController : MonoBehaviour
{
    private CharacterController characterController;
    private XROrigin xrOrigin;
    private float gravity = -9.81f;
    private float verticalVelocity = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        xrOrigin = GetComponent<XROrigin>();
    }

    void Update()
    {
        // Appliquer la gravité
        if (characterController.isGrounded)
            verticalVelocity = -1f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        // Synchroniser le Character Controller avec la position de la caméra
        Vector3 cameraPosition = xrOrigin.Camera.transform.position;
        Vector3 capsuleCenter = transform.position;
        capsuleCenter.x = cameraPosition.x;
        capsuleCenter.z = cameraPosition.z;
        transform.position = capsuleCenter;

        // Appliquer la gravité
        characterController.Move(new Vector3(0, verticalVelocity * Time.deltaTime, 0));
    }
}