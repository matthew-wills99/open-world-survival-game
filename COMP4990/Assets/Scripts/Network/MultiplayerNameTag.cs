using Unity.Netcode;
using TMPro; // Only if using TextMeshPro
using UnityEngine;

public class NetworkplayerNameTag : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTag; // Or use Text if not using TMP_Text

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (IsHost)
            {
                SetName("Player 1");
            }
            else
            {
                SetName("Player 2");
            }
        }
    }

    private void SetName(string playerName)
    {
        if (nameTag != null)
        {
            nameTag.text = playerName;
        }
        else
        {
            Debug.LogError("Name tag Text is not assigned!");
        }
    }
}