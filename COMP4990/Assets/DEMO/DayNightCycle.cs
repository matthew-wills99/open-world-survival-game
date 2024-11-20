using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    public Image darkOverlay; // Reference to the dark overlay image
    public float dayLength = 60f; // Length of a full day in seconds
    public float maxNightAlpha = 0.5f; // Maximum darkness at night (0 to 1)

    private float timeOfDay = 0.25f;

    private void Update()
    {
        // Increment time within the day-night cycle
        timeOfDay += Time.deltaTime / dayLength;
        if (timeOfDay >= 1f) timeOfDay = 0f; // Reset cycle after a full day

        // Calculate opacity based on the time of day
        float alpha = Mathf.Cos(timeOfDay * Mathf.PI * 2f) * 0.5f + 0.5f;
        darkOverlay.color = new Color(0f, 0f, 0f, alpha * maxNightAlpha);
    }
}