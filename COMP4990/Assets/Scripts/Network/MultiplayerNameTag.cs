using Unity.Netcode;
using TMPro; // Use Text if you're not using TextMeshPro
using UnityEngine;

public class MultiplayerNameTag : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTag; // Reference to the Text component
    private NetworkVariable<string> playerName = new NetworkVariable<string>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (IsHost)
            {
                playerName.Value = "Player 1";
            }
            else
            {
                playerName.Value = "Player 2";
            }
        }

        // Subscribe to changes in the playerName NetworkVariable
        playerName.OnValueChanged += UpdateNameTag;

        // Set initial text (for cases when it has already been set before spawn)
        UpdateNameTag("", playerName.Value);
    }

    private void UpdateNameTag(string oldValue, string newValue)
    {
        if (nameTag != null)
        {
            nameTag.text = newValue;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        playerName.OnValueChanged -= UpdateNameTag;
    }
}