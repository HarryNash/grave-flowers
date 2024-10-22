using UnityEngine;

public class CuboidGenerator : MonoBehaviour
{
    public Vector3 cuboidSize = new Vector3(5f, 1f, 5f);
    public Vector3 cuboidPosition = new Vector3(0f, -5f, -115f);

    void Start()
    {
        // Create a new GameObject for the cuboid
        GameObject cuboid = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Set the cuboid's position and size
        cuboid.transform.position = cuboidPosition;
        cuboid.transform.localScale = cuboidSize;

        // Ensure the BoxCollider is properly set
        BoxCollider collider = cuboid.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = cuboid.AddComponent<BoxCollider>();
        }

        // Force the collider to match the cuboid size (just in case)
        collider.size = Vector3.one; // Set size to default
        collider.center = Vector3.zero; // Ensure it's centered

        // Ensure collider is enabled
        collider.enabled = true;

        // Add Rigidbody if necessary
        Rigidbody rb = cuboid.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = cuboid.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true; // Prevent it from being affected by gravity or forces
    }
}
