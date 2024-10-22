using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEventTrigger : MonoBehaviour
{
    public List<AudioClip> soundClips; // List of sounds to play
    public List<GameObject> objectPrefabs; // List of objects to spawn
    public Vector3 spawnOffset = new Vector3(1, 1, 1); // Offset to spawn objects near the player

    public int numberOfPoints = 10; // Number of random points to generate
    public float radius = 100f; // Radius around the origin to distribute points
    public float triggerDistance = 5f; // Distance from the player to trigger an event
    public Color gizmoColor = Color.red; // Color of the Gizmos in the editor
    public float gizmoSize = 1f; // Size of the gizmo spheres in the editor

    private AudioSource audioSource;
    private List<Vector3> triggerPoints = new List<Vector3>(); // Store trigger points
    private HashSet<Vector3> triggeredPoints = new HashSet<Vector3>(); // Store triggered points

    void Start()
    {
        // Initialize AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();

        // Generate random trigger points within a specified radius
        GenerateTriggerPoints();
    }

    void Update()
    {
        // Check if the player is near any of the trigger points
        CheckForTriggers();
    }

    // Generate random points within the circle around the origin (0, 0, 0)
    private void GenerateTriggerPoints()
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            // Random angle in radians
            float angle = Random.Range(0f, Mathf.PI * 2f);

            // Random distance from the origin within the circle's radius
            float distance = Mathf.Sqrt(Random.Range(0f, 1f)) * radius;

            // Calculate X and Z coordinates based on angle and distance
            float x = Mathf.Cos(angle) * distance;
            float z = Mathf.Sin(angle) * distance;

            Vector3 point = new Vector3(x, 0, z); // Points on the XZ plane
            triggerPoints.Add(point);
        }

        // Debug log the points
        foreach (var point in triggerPoints)
        {
            Debug.Log($"Generated Trigger Point: {point}");
        }
    }

    // Check if the player is within the trigger distance of any trigger point
    private void CheckForTriggers()
    {
        foreach (Vector3 point in triggerPoints)
        {
            if (!triggeredPoints.Contains(point)) // If this point hasn't been triggered yet
            {
                float distance = Vector3.Distance(transform.position, point);

                if (distance <= triggerDistance) // Player is within the trigger distance
                {
                    triggeredPoints.Add(point); // Mark point as triggered

                    PlayRandomSound();
                    DropRandomObject();
                }
            }
        }
    }

    // Play a random sound from the list of sound clips
    private void PlayRandomSound()
    {
        if (soundClips.Count > 0)
        {
            AudioClip randomClip = soundClips[Random.Range(0, soundClips.Count)];
            audioSource.PlayOneShot(randomClip);
        }
    }

    // Spawn a random object near the player
    private void DropRandomObject()
    {
        if (objectPrefabs.Count > 0)
        {
            // Choose a random object prefab
            GameObject randomObject = objectPrefabs[Random.Range(0, objectPrefabs.Count)];

            // Spawn the object near the player with a random rotation
            Vector3 spawnPosition = transform.position + spawnOffset;
            Quaternion randomRotation = Random.rotation;

            Instantiate(randomObject, spawnPosition, randomRotation);
        }
    }

    // Draw gizmos in the editor to visualize the trigger points
    private void OnDrawGizmos()
    {
        // Only draw the Gizmos if we are in play mode and the trigger points have been generated
        if (triggerPoints != null)
        {
            Gizmos.color = gizmoColor;

            // Draw a sphere at each trigger point
            foreach (Vector3 point in triggerPoints)
            {
                Gizmos.DrawSphere(point, gizmoSize); // Draw a sphere at each point
            }

            // Optionally, draw a wire sphere around the player to represent the trigger distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
    }
}
