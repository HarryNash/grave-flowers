using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickPlacer : MonoBehaviour
{
    public float BlockSize = .2f;
    public float CircleRadius = 6.0f;
    public float MinDuration = 0.5f;
    public float MaxDuration = 2f;
    public float MinYStart = -20f;
    public float MaxYStart = -10f;
    public float groutingWidth = 0.01f;

    private Vector3 KeystonePosition = new Vector3(-100, -100, -100);

    private Dictionary<string, GameObject> ActiveBlocks = new Dictionary<string, GameObject>();

    private string Vector3ToString(Vector3 vector)
    {
        return $"{vector.x},{vector.y},{vector.z}";
    }

    private Vector3 StringToVector3(string str)
    {
        string[] values = str.Split(',');
        return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
    }

    public void SpawnCube(Vector3 position)
    {
        string posKey = Vector3ToString(position);
        if (ActiveBlocks.ContainsKey(posKey))
        {
            return;
        }

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        float yStart = Random.Range(MinYStart, MaxYStart);
        cube.transform.position = new Vector3(position.x, yStart, position.z);

        cube.transform.localScale = new Vector3(
            BlockSize - groutingWidth,
            BlockSize - groutingWidth,
            BlockSize - groutingWidth
        );

        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material.color = Random.ColorHSV();

        ActiveBlocks[posKey] = cube;

        float randomDuration = Random.Range(MinDuration, MaxDuration);
        StartCoroutine(RiseUp(cube, position, randomDuration));
    }

    private IEnumerator RiseUp(GameObject cube, Vector3 targetPosition, float duration)
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
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (cube != null)
        {
            cube.transform.position = targetPosition;
        }
    }

    private IEnumerator DropDownAndDestroy(GameObject cube, Vector3 position, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = cube.transform.position;
        float yStart = Random.Range(MinYStart, MaxYStart);
        Vector3 dropTarget = new Vector3(startingPosition.x, yStart, startingPosition.z);

        while (elapsedTime < duration)
        {
            if (cube == null)
                yield break;

            cube.transform.position = Vector3.Lerp(
                startingPosition,
                dropTarget,
                elapsedTime / duration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (cube != null)
        {
            Destroy(cube);
            string posKey = Vector3ToString(position);
            ActiveBlocks.Remove(posKey);
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

        if (nearestGriddedPosition != KeystonePosition)
        {
            List<string> positionsToRemove = new List<string>();

            foreach (var blockPositionKey in new List<string>(ActiveBlocks.Keys))
            {
                Vector3 blockPosition = StringToVector3(blockPositionKey);
                float distance = Vector3.Distance(
                    new Vector3(nearestGriddedPosition.x, 0, nearestGriddedPosition.z),
                    new Vector3(blockPosition.x, 0, blockPosition.z)
                );

                if (distance > CircleRadius * BlockSize)
                {
                    float randomDuration = Random.Range(MinDuration, MaxDuration);
                    StartCoroutine(
                        DropDownAndDestroy(
                            ActiveBlocks[blockPositionKey],
                            blockPosition,
                            randomDuration
                        )
                    );
                    positionsToRemove.Add(blockPositionKey);
                }
            }

            foreach (var posKey in positionsToRemove)
            {
                if (ActiveBlocks[posKey] == null)
                {
                    ActiveBlocks.Remove(posKey);
                }
            }

            for (int i = Mathf.FloorToInt(-CircleRadius); i <= Mathf.CeilToInt(CircleRadius); i++)
            {
                for (
                    int j = Mathf.FloorToInt(-CircleRadius);
                    j <= Mathf.CeilToInt(CircleRadius);
                    j++
                )
                {
                    KeystonePosition = nearestGriddedPosition;
                    Vector3 eachBrickPosition = new Vector3(
                        KeystonePosition.x + i * BlockSize,
                        BlockSize * -0.5f,
                        KeystonePosition.z + j * BlockSize
                    );

                    float distanceFromCenter = Vector3.Distance(
                        new Vector3(KeystonePosition.x, 0, KeystonePosition.z),
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
