using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickPlacer : MonoBehaviour
{
    private Vector3 BlockPosition = new Vector3(-100, -100, -100);
    private float blockSize = .2f;
    private Dictionary<Vector3, GameObject> activeBlocks = new Dictionary<Vector3, GameObject>(); // Dictionary to track block positions
    private float circleRadius = 6.0f; // Radius of the circle in which blocks are placed

    // Min and max speed and duration for rise/fall animations
    public float minSpeed = 2f;
    public float maxSpeed = 7f;
    public float minDuration = 0.5f;
    public float maxDuration = 2f;

    // Start is called before the first frame update
    void Start() { }

    public void SpawnCube(Vector3 position)
    {
        // Check if there is already a block at this position
        if (activeBlocks.ContainsKey(position))
        {
            return; // If a block is already here, skip spawning
        }

        // Lower the position to below the visible area
        Vector3 loweredPosition = new Vector3(position.x, position.y - blockSize * 5, position.z);
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = loweredPosition;

        // Set the size of the cube (scaling)
        cube.transform.localScale = new Vector3(
            blockSize - 0.01f,
            blockSize - 0.01f,
            blockSize - 0.01f
        );

        // Set the color of the cube's material (optional)
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material.color = Color.red;

        // Add the cube to the dictionary of active blocks
        activeBlocks[position] = cube;

        // Start the coroutine to animate the block rising up to the correct position
        float randomSpeed = Random.Range(minSpeed, maxSpeed); // Random speed
        float randomDuration = Random.Range(minDuration, maxDuration); // Random duration
        StartCoroutine(RiseUp(cube, position, randomSpeed, randomDuration));
    }

    private IEnumerator RiseUp(GameObject cube, Vector3 targetPosition, float speed, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = cube.transform.position;

        while (elapsedTime < duration)
        {
            // Check if the cube has been destroyed
            if (cube == null)
                yield break;

            cube.transform.position = Vector3.Lerp(
                startingPosition,
                targetPosition,
                elapsedTime / duration
            );
            elapsedTime += Time.deltaTime * speed;
            yield return null;
        }

        // Ensure the final position is accurate
        if (cube != null) // Check again before setting position
        {
            cube.transform.position = targetPosition;
        }
    }

    private IEnumerator DropDownAndDestroy(
        GameObject cube,
        Vector3 position,
        float speed,
        float duration
    )
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = cube.transform.position;
        Vector3 dropTarget = new Vector3(
            startingPosition.x,
            startingPosition.y - blockSize * 5,
            startingPosition.z
        );

        while (elapsedTime < duration)
        {
            // Check if the cube has been destroyed
            if (cube == null)
                yield break;

            cube.transform.position = Vector3.Lerp(
                startingPosition,
                dropTarget,
                elapsedTime / duration
            );
            elapsedTime += Time.deltaTime * speed;
            yield return null;
        }

        // Destroy the block and remove it from the dictionary only if it's still valid
        if (cube != null)
        {
            Destroy(cube);
            activeBlocks.Remove(position);
        }
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
            RoundToNearestMultipleOfX(position.x, blockSize),
            RoundToNearestMultipleOfX(position.y, blockSize),
            RoundToNearestMultipleOfX(position.z, blockSize)
        );

        if (nearestGriddedPosition != BlockPosition)
        {
            // Drop blocks that are no longer within the circular range
            List<Vector3> positionsToRemove = new List<Vector3>();

            foreach (var blockPosition in new List<Vector3>(activeBlocks.Keys)) // Use a copy of the keys
            {
                // Calculate distance from the center point (ignoring y-axis)
                float distance = Vector3.Distance(
                    new Vector3(nearestGriddedPosition.x, 0, nearestGriddedPosition.z),
                    new Vector3(blockPosition.x, 0, blockPosition.z)
                );

                if (distance > circleRadius * blockSize)
                {
                    float randomSpeed = Random.Range(minSpeed, maxSpeed); // Random speed for fall
                    float randomDuration = Random.Range(minDuration, maxDuration); // Random duration for fall
                    StartCoroutine(
                        DropDownAndDestroy(
                            activeBlocks[blockPosition],
                            blockPosition,
                            randomSpeed,
                            randomDuration
                        )
                    );
                    positionsToRemove.Add(blockPosition); // Mark for removal after coroutine starts
                }
            }

            // Remove the blocks after starting their drop down animations
            foreach (var pos in positionsToRemove)
            {
                activeBlocks.Remove(pos);
            }

            // Spawn new blocks within the circular grid
            for (int i = Mathf.FloorToInt(-circleRadius); i <= Mathf.CeilToInt(circleRadius); i++)
            {
                for (
                    int j = Mathf.FloorToInt(-circleRadius);
                    j <= Mathf.CeilToInt(circleRadius);
                    j++
                )
                {
                    BlockPosition = nearestGriddedPosition;
                    Vector3 eachBrickPosition = new Vector3(
                        BlockPosition.x + i * blockSize,
                        BlockPosition.y,
                        BlockPosition.z + j * blockSize
                    );

                    // Calculate the distance of this brick from the center
                    float distanceFromCenter = Vector3.Distance(
                        new Vector3(BlockPosition.x, 0, BlockPosition.z),
                        new Vector3(eachBrickPosition.x, 0, eachBrickPosition.z)
                    );

                    // Only spawn blocks within the circular radius
                    if (distanceFromCenter <= circleRadius * blockSize)
                    {
                        SpawnCube(eachBrickPosition);
                    }
                }
            }
        }
    }
}
