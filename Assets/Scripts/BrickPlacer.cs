using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickPlacer : MonoBehaviour
{
    public float BrickWidth = 0.2f;
    public float BrickHeight = 0.2f;
    public float BrickDepth = 0.2f;
    public float CircleRadius = 6.0f;
    public float MinDuration = 0.5f;
    public float MaxDuration = 2f;
    public float MinYStart = -20f;
    public float MaxYStart = -10f;
    public float groutingWidth = 0.01f;

    private Vector3 KeystonePosition = new Vector3(-100, -100, -100);

    private Dictionary<string, GameObject> ActiveBlocks = new Dictionary<string, GameObject>();

    // Change this method to use int representation
    private string Vector3ToString(Vector3 vector)
    {
        int x = Mathf.RoundToInt(vector.x * 1000);
        int y = Mathf.RoundToInt(vector.y * 1000);
        int z = Mathf.RoundToInt(vector.z * 1000);
        return $"{x},{y},{z}";
    }

    // Update this method to convert back from int representation
    private Vector3 StringToVector3(string str)
    {
        string[] values = str.Split(',');
        return new Vector3(
            float.Parse(values[0]) / 1000f,
            float.Parse(values[1]) / 1000f,
            float.Parse(values[2]) / 1000f
        );
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
            BrickWidth - groutingWidth,
            BrickHeight - groutingWidth,
            BrickDepth - groutingWidth
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
            RoundToNearestMultipleOfX(position.x, BrickWidth),
            RoundToNearestMultipleOfX(position.y, BrickHeight),
            RoundToNearestMultipleOfX(position.z, BrickDepth)
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

                if (distance > CircleRadius * BrickWidth)
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
                    float zPosition = KeystonePosition.z + j * BrickDepth;
                    float offsetX =
                        (Mathf.RoundToInt(zPosition / BrickDepth) % 2 == 0) ? 0 : BrickWidth / 2;

                    Vector3 eachBrickPosition = new Vector3(
                        KeystonePosition.x + i * BrickWidth + offsetX,
                        BrickHeight * -0.5f,
                        zPosition
                    );
                    float distanceFromCenter = Vector3.Distance(
                        new Vector3(KeystonePosition.x, 0, KeystonePosition.z),
                        new Vector3(eachBrickPosition.x, 0, eachBrickPosition.z)
                    );

                    if (distanceFromCenter <= CircleRadius * BrickWidth)
                    {
                        SpawnCube(eachBrickPosition);
                    }
                }
            }
        }
    }
}
