using UnityEngine;

public class RandomPrefabSpawner : MonoBehaviour
{
    // Reference to the prefab you want to instantiate
    public GameObject prefabToSpawn;

    // Reference to the plane
    public GameObject plane;

    // Number of prefabs to spawn
    public int numberOfPrefabs = 100;

    void Start()
    {
        // Get the dimensions of the plane
        MeshRenderer planeMesh = plane.GetComponent<MeshRenderer>();
        Vector3 planeSize = planeMesh.bounds.size;

        Vector3 planePosition = plane.transform.position;

        // Get half of the plane's size to calculate random positions
        float halfWidth = planeSize.x / 2;
        float halfLength = planeSize.z / 2;

        // Loop to instantiate the prefabs
        for (int i = 0; i < numberOfPrefabs; i++)
        {
            // Random x and z positions within the bounds of the plane
            float randomX = Random.Range(planePosition.x - halfWidth, planePosition.x + halfWidth);
            float randomZ = Random.Range(
                planePosition.z - halfLength,
                planePosition.z + halfLength
            );

            // Spawn position on the plane, keeping y at the plane's y position
            Vector3 spawnPosition = new Vector3(randomX, planePosition.y, randomZ);

            // Instantiate the prefab at the random position
            GameObject spawnedPrefab = Instantiate(
                prefabToSpawn,
                spawnPosition,
                Quaternion.identity
            );

            // Apply random scaling (between 90% and 110%) to the spawned prefab
            float randomScaleX = Random.Range(0.04f, 0.06f);
            float randomScaleY = Random.Range(1.4f, 1.6f);
            float randomScaleZ = Random.Range(0.04f, 0.06f);

            // Set the scale of the spawned prefab
            spawnedPrefab.transform.localScale = new Vector3(
                randomScaleX,
                randomScaleY,
                randomScaleZ
            );
        }
    }
}
