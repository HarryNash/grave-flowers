using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickPlacer : MonoBehaviour
{
    private Vector3 BlockPosition = new Vector3(-100, -100, -100);

    // Start is called before the first frame update
    void Start() { }

    public void SpawnCube(Vector3 position)
    {
        Vector3 loweredPosition = new Vector3(position.x, -0.51f, position.z);
        // Create a cube at the given position with default rotation
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = loweredPosition;

        Destroy(cube.GetComponent<Collider>());

        // Optional: Set the size of the cube (scaling)
        cube.transform.localScale = new Vector3(1, 1, 1);

        // Optional: Set the color of the cube's material (requires a Renderer component)
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material.color = Color.red; // Change color to red (or any other)
    }

    public float RoundToNearestMultipleOfX(float number, float x)
    {
        return Mathf.RoundToInt(number / x) * x;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        Vector3 nearestGriddedPosition = new Vector3(
            RoundToNearestMultipleOfX(position.x, 1f),
            RoundToNearestMultipleOfX(position.y, 1f),
            RoundToNearestMultipleOfX(position.z, 1f)
        );
        if (nearestGriddedPosition != BlockPosition)
        {
            BlockPosition = nearestGriddedPosition;
            SpawnCube(BlockPosition);
        }
    }
}
