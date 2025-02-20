using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public GameObject[] SpecialObjects; // Prefabs of objects to spawn
    public float objectRadius = 5f; // Radius to avoid holes around objects
    public int totalObjects = 3; // Number of special objects
    private List<GameObject> spawnedObjects = new List<GameObject>();
    public Light glowLightPrefab; // Light prefab for glowing effect

    [Range(0f, 1f)]
    public float minVolume = 0.1f;

    [Range(0f, 1f)]
    public float maxVolume = 0.3f;

    [Range(0.5f, 2f)]
    public float minPitch = 0.7f;

    [Range(0.5f, 2f)]
    public float maxPitch = 1.3f;

    public Image fadeImage; // Reference to the FadeImage
    public float fadeDuration = 2.0f; // Duration of the fade
    public string nextScene; // Name of the next scene to load

    public Material BaseMaterial;
    public AudioClip BrickSound;
    public List<AudioClip> objectSounds;
    public Color baseColor = Color.red;
    public float maxColorDistance = 0.5f;

    private AudioSource audioSource;
    private Dictionary<string, GameObject> ActiveBlocks = new Dictionary<string, GameObject>();

    public float proximityRadius = 5f; // Distance at which the light dims and sound plays
    private int objectsCollected = 0;

    public float playSoundYThreshold = -10f; // The Y level below which to play the sound
    public float resetSceneYThreshold = -30f; // The Y level below which the scene will reset

    // Variables for audio
    public AudioClip fallSound; // Assign this in the Inspector
    private bool soundPlayed = false; // To avoid playing sound multiple times
    private AudioSource audioSourceFall;

    public float reductionAmount = 0.1f; // The amount by which to reduce the radius each second
    private Coroutine radiusReductionRoutine; // Reference to the running radius reduction coroutine

    // Hole configuration
    public float HoleRadius = 100f;
    public int numberOfHoles = 5; // Number of holes
    public float minHoleRadius = 1f; // Minimum hole radius
    public float maxHoleRadius = 3f; // Maximum hole radius
    public float holeBorderFactor = 4f;

    private List<Hole> holes = new List<Hole>(); // List of holes

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSourceFall = gameObject.AddComponent<AudioSource>();
        // Create holes at random positions within the CircleRadius
        GenerateHoles();
        SpawnObjects();
    }

    // Struct to represent a hole
    private struct Hole
    {
        public Vector3 center;
        public float radius;

        public Hole(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        foreach (Hole hole in holes)
        {
            Gizmos.DrawSphere(hole.center, hole.radius); // Draw a sphere at each point
        }
    }

    private void GenerateHoles()
    {
        for (int i = 0; i < numberOfHoles; i++)
        {
            // Random position within HoleRadius around (0, 0, 0)
            Vector3 randomPosition = new Vector3(
                Random.Range(-HoleRadius, HoleRadius),
                0, // Keep the holes on the XZ plane
                Random.Range(-HoleRadius, HoleRadius)
            );

            float holeRadius = Random.Range(minHoleRadius, maxHoleRadius);
            holes.Add(new Hole(randomPosition, holeRadius));
        }
    }

    private void SpawnObjects()
    {
        for (int i = 0; i < totalObjects; i++)
        {
            Vector3 randomPosition;
            bool validPosition;

            // Repeat until we find a valid position
            do
            {
                randomPosition = new Vector3(
                    Random.Range(-HoleRadius, HoleRadius),
                    0.75f,
                    Random.Range(-HoleRadius, HoleRadius)
                );

                validPosition = true; // Assume the position is valid initially

                // Ensure the position is not inside the hole
                if (IsInsideHole(randomPosition))
                {
                    validPosition = false;
                }
                else
                {
                    // Ensure it's not within 20 units of any previously spawned object
                    foreach (GameObject spawnedObject in spawnedObjects)
                    {
                        if (
                            Vector3.Distance(randomPosition, spawnedObject.transform.position) < 33f
                        )
                        {
                            validPosition = false;
                            break;
                        }
                    }
                }
            } while (!validPosition);

            // Instantiate the special object
            GameObject obj = Instantiate(SpecialObjects[i], randomPosition, Quaternion.identity);
            spawnedObjects.Add(obj);

            // Add glowing light to the object (optional)
            // Light glow = Instantiate(glowLightPrefab, obj.transform);
            // glow.transform.localPosition = new Vector3(0, 5, 0);

            // Add trigger to detect player proximity
            SphereCollider trigger = obj.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = proximityRadius;

            // Add an AudioSource to the object
            AudioSource objAudioSource = obj.AddComponent<AudioSource>();
            objAudioSource.playOnAwake = false; // Prevent the sound from playing immediately
        }
    }

    private bool IsInsideHole(Vector3 position)
    {
        foreach (var hole in holes)
        {
            float distance = Vector3.Distance(new Vector3(position.x, 0, position.z), hole.center);
            if (distance <= hole.radius)
            {
                return true; // Position is inside a hole
            }
        }
        return false;
    }

    private float GetNearestHoleDistance(Vector3 position)
    {
        float minDistance = float.MaxValue;

        foreach (var hole in holes)
        {
            float distanceToCenter = Vector3.Distance(
                new Vector3(position.x, 0, position.z),
                hole.center
            );
            float distanceToEdge = Mathf.Max(0, distanceToCenter - hole.radius); // Distance to hole's edge

            if (distanceToEdge < minDistance)
            {
                minDistance = distanceToEdge;
            }
        }

        return minDistance;
    }

    private Color GetBrickColorBasedOnHoleProximity(Vector3 position)
    {
        float nearestDistance = GetNearestHoleDistance(position);
        float normalizedDistance =
            Mathf.Clamp01(nearestDistance / maxHoleRadius) * holeBorderFactor;

        // Lerp between dark color and base color
        Color darkColor = Color.red; // Very dark color at the edge of the hole
        return Color.Lerp(
            darkColor,
            GetRandomColorNearBase(baseColor, maxColorDistance),
            normalizedDistance
        );
    }

    public void SpawnCube(Vector3 position)
    {
        if (IsInsideHole(position))
            return; // Skip spawning if inside a hole

        string posKey = Vector3ToString(position);
        if (ActiveBlocks.ContainsKey(posKey))
        {
            return;
        }

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        float yStart = Random.Range(MinYStart, MaxYStart);
        cube.transform.position = new Vector3(position.x, 0, position.z);

        cube.transform.localScale = new Vector3(
            BrickWidth - groutingWidth,
            BrickHeight - groutingWidth,
            BrickDepth - groutingWidth
        );

        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material = BaseMaterial;

        // Adjust color based on proximity to nearest hole
        Color brickColor = GetBrickColorBasedOnHoleProximity(position);
        cubeRenderer.material.color = brickColor;

        // Set up transparency material properties
        cubeRenderer.material.SetFloat("_Surface", 1.0f); // Transparent material
        cubeRenderer.material.SetOverrideTag("RenderType", "Transparent");
        cubeRenderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        cubeRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        cubeRenderer.material.SetInt(
            "_DstBlend",
            (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
        );

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        Vector2 randomOffset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        propertyBlock.SetVector("_BaseMap_ST", new Vector4(1, 1, randomOffset.x, randomOffset.y));
        cubeRenderer.SetPropertyBlock(propertyBlock);

        ActiveBlocks[posKey] = cube;

        float randomDuration = Random.Range(MinDuration, MaxDuration);
        //StartCoroutine(RiseUp(cube, position, randomDuration));
    }

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
        audioSource.volume = Random.Range(minVolume, maxVolume);
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(BrickSound);
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

    // Add this method to handle the player entering a proximity trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's a special object
        if (spawnedObjects.Contains(other.gameObject))
        {
            // Get the object's AudioSource and play the sound
            AudioSource objAudioSource = other.gameObject.GetComponent<AudioSource>();
            if (objAudioSource != null)
            {
                objAudioSource.PlayOneShot(objectSounds[objectsCollected]);
            }

            // Destroy the object after a slight delay to allow the sound to finish
            Destroy(other.gameObject, objectSounds[objectsCollected].length);

            // Increment the collected counter
            objectsCollected++;

            // Check if the collected objects exceed half of the totalObjects
            if (objectsCollected > totalObjects / 2)
            {
                // Start reducing the radius if it's not already being reduced
                if (radiusReductionRoutine == null)
                {
                    radiusReductionRoutine = StartCoroutine(ReduceCircleRadius());
                }
            }

            if (objectsCollected >= totalObjects)
            {
                StartCoroutine(PlaySoundAndTransition());
            }
        }
    }

    private IEnumerator PlaySoundAndTransition()
    {
        yield return FadeToBlack();
        if (nextScene != "")
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    private IEnumerator FadeToBlack()
    {
        // Set the initial color of the fade image to fully transparent
        Color imageColor = fadeImage.color;
        imageColor.a = 0;
        fadeImage.color = imageColor;

        // Set the initial color of the fade text to fully transparent

        // Fade both the image and the text to black
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            // Update image and text alpha values
            imageColor.a = alpha;
            fadeImage.color = imageColor;

            yield return null;
        }

        // Ensure the image is fully opaque and the text is fully transparent
        imageColor.a = 1;
        fadeImage.color = imageColor;
    }

    private IEnumerator ReduceCircleRadius()
    {
        while (CircleRadius > 0)
        {
            CircleRadius = Mathf.Max(0, CircleRadius - reductionAmount); // Reduce the radius, but never below 0
            yield return new WaitForSeconds(1f); // Wait for 1 second before the next reduction
        }

        // Once the radius reaches 0, stop the coroutine
        radiusReductionRoutine = null;
    }

    private void Update()
    {
        Vector3 position = transform.position;
        Vector3 nearestGriddedPosition = new Vector3(
            RoundToNearestMultipleOfX(position.x, BrickWidth),
            RoundToNearestMultipleOfX(position.y, BrickHeight),
            RoundToNearestMultipleOfX(position.z, BrickDepth)
        );

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
                float zPosition = nearestGriddedPosition.z + j * BrickDepth;
                float offsetX =
                    (Mathf.RoundToInt(zPosition / BrickDepth) % 2 == 0) ? 0 : BrickWidth / 2;

                Vector3 eachBrickPosition = new Vector3(
                    nearestGriddedPosition.x + i * BrickWidth + offsetX,
                    BrickHeight * -0.5f,
                    zPosition
                );

                float distanceFromCenter = Vector3.Distance(
                    nearestGriddedPosition,
                    eachBrickPosition
                );

                if (distanceFromCenter <= CircleRadius)
                {
                    SpawnCube(eachBrickPosition);
                }
            }
        }

        if (transform.position.y < playSoundYThreshold && !soundPlayed)
        {
            // Play the sound and mark it as played
            if (audioSource != null && fallSound != null)
            {
                audioSourceFall.PlayOneShot(fallSound);
            }
            soundPlayed = true;
        }

        // Check if the player has fallen below the reset threshold
        if (transform.position.y < resetSceneYThreshold)
        {
            // Reload the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
