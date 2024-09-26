using UnityEngine;

public class Grass : MonoBehaviour
{
    public float detectionRange = 5f; // The distance at which the object starts to lean away from the player
    public float maxLeanAngle = 30f; // Maximum angle the object can lean
    public float leanSpeed = 5f; // Speed at which the object leans away
    public float reverseLeanSpeed = 2f; // Speed at which the object leans away

    private Transform player; // Reference to the player's transform

    private Quaternion initialRotation; // Store the initial rotation of the object

    void Start()
    {
        // Find the player object by tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Grasser");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // Save the initial rotation of the object
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (player == null)
            return;

        // Calculate the distance between the player and this object
        float distance = Vector3.Distance(player.position, transform.position);

        // If the player is within detection range
        if (distance < detectionRange)
        {
            // Calculate the amount of leaning based on how close the player is
            float leanFactor = Mathf.Clamp01(1 - (distance / detectionRange));
            float targetAngle = maxLeanAngle * leanFactor;

            // Determine the direction to lean away from the player
            Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
            Quaternion targetRotation =
                Quaternion.LookRotation(directionAwayFromPlayer)
                * Quaternion.Euler(0, 0, targetAngle);

            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * leanSpeed
            );
        }
        else
        {
            // If the player is outside the detection range, return to the initial rotation
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                initialRotation,
                Time.deltaTime * reverseLeanSpeed
            );
        }
    }
}
