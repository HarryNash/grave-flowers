using System.Collections;
using TMPro; // For TextMeshProUGUI, if you are using it
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundFadeTransition : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource
    public Image fadeImage; // Reference to the FadeImage
    public TextMeshProUGUI fadeText; // Reference to the Text (use TextMeshProUGUI for TMPro, or Text for UI Text)
    public float fadeDuration = 2.0f; // Duration of the fade
    public string nextScene; // Name of the next scene to load
    public float rotationSpeed = 10f; // Speed of rotation

    void Update()
    {
        // Rotate the object around its Y-axis at the specified speed
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void Start()
    {
        // Start the coroutine to play the sound and handle the transition
        StartCoroutine(PlaySoundAndTransition());
    }

    private IEnumerator PlaySoundAndTransition()
    {
        // Play the sound
        audioSource.Play();

        // Wait for the audio to finish
        yield return new WaitForSeconds(audioSource.clip.length);

        // Start the fade to black
        yield return FadeToBlack();

        // Load the next scene
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
        Color textColor = fadeText.color;
        textColor.a = 1; // Assuming text starts visible (full alpha)
        fadeText.color = textColor;

        // Fade both the image and the text to black
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            // Update image and text alpha values
            imageColor.a = alpha;
            textColor.a = 1 - alpha; // Text fades out as the image fades in
            fadeImage.color = imageColor;
            fadeText.color = textColor;

            yield return null;
        }

        // Ensure the image is fully opaque and the text is fully transparent
        imageColor.a = 1;
        fadeImage.color = imageColor;

        textColor.a = 0;
        fadeText.color = textColor;
    }
}
