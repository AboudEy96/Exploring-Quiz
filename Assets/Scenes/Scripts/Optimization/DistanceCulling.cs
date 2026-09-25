using UnityEngine;

public class DistanceCulling : MonoBehaviour
{
    public Camera targetCamera;
    public float maxDistance = 50f;

    private Renderer[] renderers;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        Vector3 cameraPosition = targetCamera.transform.position;

        foreach (Renderer rend in renderers)
        {
            if (rend == null)
                continue;

            Vector3 closestPoint = rend.bounds.ClosestPoint(cameraPosition);

            float distance = Vector3.Distance(cameraPosition, closestPoint);

            bool inDistance = distance <= maxDistance;
            bool inView = GeometryUtility.TestPlanesAABB(
                GeometryUtility.CalculateFrustumPlanes(targetCamera),
                rend.bounds
            );

            rend.enabled = inDistance && inView;
        }
    }
}