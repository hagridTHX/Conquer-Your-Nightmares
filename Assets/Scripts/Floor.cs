using UnityEngine;

public class Floor : MonoBehaviour
{
    [Tooltip("Grid snap size in meters; keep it aligned with the floor texture scale.")]
    [SerializeField] private float snapSize = 50f;
    
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