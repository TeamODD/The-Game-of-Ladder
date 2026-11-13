using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cameraTransform;

    [Range(0f, 1f)]
    public float parallaxSpeed = 0.5f;

    private Vector3 previousCameraPosition;

    private void Start()
    {
        previousCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 cameraMovement =
            cameraTransform.position - previousCameraPosition;

        transform.position += new Vector3(
            cameraMovement.x * parallaxSpeed,
            cameraMovement.y * parallaxSpeed,
            0f
        );

        previousCameraPosition = cameraTransform.position;
    }
}