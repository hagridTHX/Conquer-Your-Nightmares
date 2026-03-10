using UnityEngine;

public class Floor : MonoBehaviour
{
    [Tooltip("Co ile metrów podłoga ma 'przeskoczyć' (np. 50). Musi pasować do wielkości tekstury.")]
    public float snapSize = 50f;
    
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 cameraCenterOnGround = ray.GetPoint(distance);

            float snapX = Mathf.Round(cameraCenterOnGround.x / snapSize) * snapSize;
            float snapZ = Mathf.Round(cameraCenterOnGround.z / snapSize) * snapSize;

            transform.position = new Vector3(snapX, 0, snapZ);
        }
    }
}