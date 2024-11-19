using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }
    public GameObject tooltipUI;
    public RectTransform tooltipRectTransform;
    private TextMeshProUGUI tooltipText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional if you want the tooltip to persist across scenes
            tooltipText = tooltipUI.GetComponent<TextMeshProUGUI>();
            tooltipUI.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowTooltip(string text)
    {
        tooltipUI.SetActive(true);
        tooltipText.text = text;

        Vector3 cursorPosition = Input.mousePosition;
        Vector3 tooltipPosition = new Vector3(cursorPosition.x + tooltipRectTransform.rect.width / 2f, cursorPosition.y, cursorPosition.z);

        // Ensure tooltip stays within screen bounds
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        tooltipPosition.x = Mathf.Clamp(tooltipPosition.x, 0, screenWidth - tooltipRectTransform.rect.width);
        tooltipPosition.y = Mathf.Clamp(tooltipPosition.y, 0, screenHeight);

        tooltipUI.transform.position = tooltipPosition;
    }

    public void HideTooltip()
    {
        tooltipUI.SetActive(false);
    }

    public void UpdatePosition()
    {
        Vector3 cursorPosition = Input.mousePosition;
        Vector3 tooltipPosition = new Vector3(cursorPosition.x + tooltipRectTransform.rect.width / 2f, cursorPosition.y, cursorPosition.z);

        // Ensure tooltip stays within screen bounds
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        tooltipPosition.x = Mathf.Clamp(tooltipPosition.x, 0, screenWidth - tooltipRectTransform.rect.width);
        tooltipPosition.y = Mathf.Clamp(tooltipPosition.y, 0, screenHeight);

        tooltipUI.transform.position = tooltipPosition;
    }
}