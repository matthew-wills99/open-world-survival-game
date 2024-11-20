using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    public Image darkOverlay; // Reference to the dark overlay image
    public float dayDuration = 20f; // Duration of full daylight in seconds
    public float nightDuration = 20f; // Duration of full nighttime in seconds
    public float transitionDuration = 5f; // Duration of transitions (day-to-night and night-to-day) in seconds
    public float maxNightAlpha = 0.5f; // Maximum darkness at night (0 to 1)

    public enum CycleState { Day, TransitionToNight, Night, TransitionToDay }
    private CycleState currentState;
    private float stateTimer;
    private float overlayAlpha;

    public void Setup()
    {
        currentState = CycleState.Day; // Start with day
        stateTimer = dayDuration;
        SetOverlayAlpha(0f); // Fully transparent at start
    }

    public void Load(CycleState cs, float timer, float alpha)
    {
        currentState = cs;
        stateTimer = timer;
        overlayAlpha = alpha;
        SetOverlayAlpha(alpha);
    }

    public CycleState GetCurrentState()
    {
        return currentState;
    }

    public float GetTimer()
    {
        return stateTimer;
    }

    public float GetAlpha()
    {
        return overlayAlpha;
    }

    private void Update()
    {
        // Count down the timer for the current state
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            // Transition to the next state
            switch (currentState)
            {
                case CycleState.Day:
                    currentState = CycleState.TransitionToNight;
                    stateTimer = transitionDuration;
                    break;

                case CycleState.TransitionToNight:
                    currentState = CycleState.Night;
                    stateTimer = nightDuration;
                    SetOverlayAlpha(maxNightAlpha); // Set to full darkness for night
                    break;

                case CycleState.Night:
                    currentState = CycleState.TransitionToDay;
                    stateTimer = transitionDuration;
                    break;

                case CycleState.TransitionToDay:
                    currentState = CycleState.Day;
                    stateTimer = dayDuration;
                    SetOverlayAlpha(0f); // Fully transparent for day
                    break;
            }
        }

        // Handle the transition phases
        if (currentState == CycleState.TransitionToNight)
        {
            // Gradually increase overlay alpha from 0 to maxNightAlpha
            float progress = 1 - (stateTimer / transitionDuration);
            SetOverlayAlpha(Mathf.Lerp(0f, maxNightAlpha, progress));
        }
        else if (currentState == CycleState.TransitionToDay)
        {
            // Gradually decrease overlay alpha from maxNightAlpha to 0
            float progress = 1 - (stateTimer / transitionDuration);
            SetOverlayAlpha(Mathf.Lerp(maxNightAlpha, 0f, progress));
        }
    }

    private void SetOverlayAlpha(float alpha)
    {
        darkOverlay.color = new Color(0f, 0f, 0f, alpha);
        overlayAlpha = alpha;
    }
}