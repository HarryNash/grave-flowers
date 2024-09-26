using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickPlacer : MonoBehaviour
{
    public float BlockSize = .2f;
    public float CircleRadius = 6.0f;
    public float minSpeed = 2f;
    public float maxSpeed = 7f;
    public float minDuration = 0.5f;
    public float maxDuration = 2f;
    public float minYStart = -20f;
    public float maxYStart = -10f;

    private Vector3 BlockPosition = new Vector3(-100, -100, -100);
    private Dictionary<Vector3, GameObject> activeBlocks = new Dictionary<Vector3, GameObject>();

    public void SpawnCube(Vector3 position)
    {
        if (activeBlocks.ContainsKey(position))
        {
            return;
        }

        float yStart = Random.Range(minYStart, maxYStart);
        Vector3 loweredPosition = new Vector3(position.x, yStart, position.z);
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = loweredPosition;

        cube.transform.localScale = new Vector3(
            BlockSize - 0.01f,
            BlockSize - 0.01f,
            BlockSize - 0.01f
        );

        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material.color = Color.red;

        activeBlocks[position] = cube;

        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        float randomDuration = Random.Range(minDuration, maxDuration);
        StartCoroutine(RiseUp(cube, position, randomSpeed, randomDuration));
    }

    private IEnumerator RiseUp(GameObject cube, Vector3 targetPosition, float speed, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = cube.transform.position;

        while (elapsedTime < duration)
        {
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

        if (cube != null)
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
            startingPosition.y - BlockSize * 5,
            startingPosition.z
        );

        while (elapsedTime < duration)
        {
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

    void Update()
    {
        Vector3 position = transform.position;
        Vector3 nearestGriddedPosition = new Vector3(
            RoundToNearestMultipleOfX(position.x, BlockSize),
            RoundToNearestMultipleOfX(position.y, BlockSize),
            RoundToNearestMultipleOfX(position.z, BlockSize)
        );

        if (nearestGriddedPosition != BlockPosition)
        {
            List<Vector3> positionsToRemove = new List<Vector3>();

            foreach (var blockPosition in new List<Vector3>(activeBlocks.Keys))
            {
                float distance = Vector3.Distance(
                    new Vector3(nearestGriddedPosition.x, 0, nearestGriddedPosition.z),
                    new Vector3(blockPosition.x, 0, blockPosition.z)
                );

                if (distance > CircleRadius * BlockSize)
                {
                    float randomSpeed = Random.Range(minSpeed, maxSpeed);
                    float randomDuration = Random.Range(minDuration, maxDuration);
                    StartCoroutine(
                        DropDownAndDestroy(
                            activeBlocks[blockPosition],
                            blockPosition,
                            randomSpeed,
                            randomDuration
                        )
                    );
                    positionsToRemove.Add(blockPosition);
                }
            }

            foreach (var pos in positionsToRemove)
            {
                activeBlocks.Remove(pos);
            }

            for (int i = Mathf.FloorToInt(-CircleRadius); i <= Mathf.CeilToInt(CircleRadius); i++)
            {
                for (
                    int j = Mathf.FloorToInt(-CircleRadius);
                    j <= Mathf.CeilToInt(CircleRadius);
                    j++
                )
                {
                    BlockPosition = nearestGriddedPosition;
                    Vector3 eachBrickPosition = new Vector3(
                        BlockPosition.x + i * BlockSize,
                        BlockPosition.y,
                        BlockPosition.z + j * BlockSize
                    );

                    float distanceFromCenter = Vector3.Distance(
                        new Vector3(BlockPosition.x, 0, BlockPosition.z),
                        new Vector3(eachBrickPosition.x, 0, eachBrickPosition.z)
                    );

                    if (distanceFromCenter <= CircleRadius * BlockSize)
                    {
                        SpawnCube(eachBrickPosition);
                    }
                }
            }
        }
    }
}
