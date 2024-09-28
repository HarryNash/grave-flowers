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

    public Material BaseMaterial;

    public Color baseColor = Color.red;
    public float maxColorDistance = 0.5f;

    private Vector3 KeystonePosition = new Vector3(-100, -100, -100);

    private Dictionary<string, GameObject> ActiveBlocks = new Dictionary<string, GameObject>();

    private string Vector3ToString(Vector3 vector)
    {
        int x = Mathf.RoundToInt(vector.x * 1000);
        int y = Mathf.RoundToInt(vector.y * 1000);
        int z = Mathf.RoundToInt(vector.z * 1000);
        return $"{x},{y},{z}";
    }

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
        cubeRenderer.material = BaseMaterial;
        cubeRenderer.material.color = GetRandomColorNearBase(baseColor, maxColorDistance);

        // Set the material to Transparent surface type (URP specific keyword)
        cubeRenderer.material.SetFloat("_Surface", 1.0f); // 0 = Opaque, 1 = Transparent
        cubeRenderer.material.SetOverrideTag("RenderType", "Transparent");
        cubeRenderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Enable blending (source alpha and inverse destination alpha for transparency)
        cubeRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        cubeRenderer.material.SetInt(
            "_DstBlend",
            (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
        );

        Vector2 randomOffset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));

        // Initialize the MaterialPropertyBlock
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        // Set the random offset for the _BaseMap_ST property, specific to URP shaders
        propertyBlock.SetVector("_BaseMap_ST", new Vector4(1, 1, randomOffset.x, randomOffset.y));

        // Apply the property block to the renderer
        cubeRenderer.SetPropertyBlock(propertyBlock);

        ActiveBlocks[posKey] = cube;

        float randomDuration = Random.Range(MinDuration, MaxDuration);
        StartCoroutine(RiseUp(cube, position, randomDuration));
    }

    private Color GetRandomColorNearBase(Color baseColor, float maxDistance)
    {
        float r = Mathf.Clamp(baseColor.r + Random.Range(-maxDistance, maxDistance), 0, 1);
        float g = Mathf.Clamp(baseColor.g + Random.Range(-maxDistance, maxDistance), 0, 1);
        float b = Mathf.Clamp(baseColor.b + Random.Range(-maxDistance, maxDistance), 0, 1);

        return new Color(r, g, b);
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
            Renderer cubeRenderer = cube.GetComponent<Renderer>();
            Color color = cubeRenderer.material.color;
            color.a = elapsedTime / duration;
            cubeRenderer.material.color = color;
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
            Renderer cubeRenderer = cube.GetComponent<Renderer>();
            Color color = cubeRenderer.material.color;
            color.a = 1 - (elapsedTime / duration);
            cubeRenderer.material.color = color;
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
                float distance = Vector3.Distance(nearestGriddedPosition, blockPosition);

                if (distance > CircleRadius)
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

            for (int i = -20; i <= 20; i++)
            {
                for (int j = -20; j <= 20; j++)
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
                        KeystonePosition,
                        eachBrickPosition
                    );

                    if (distanceFromCenter <= CircleRadius)
                    {
                        SpawnCube(eachBrickPosition);
                    }
                }
            }
        }
    }
}
