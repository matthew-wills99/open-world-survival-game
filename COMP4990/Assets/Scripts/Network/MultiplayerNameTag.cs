using Unity.Netcode;
using TMPro;
using UnityEngine;

public class MultiplayerNameTag : NetworkBehaviour
{
    public GameObject nameTagPrefab;
    public Vector3 nameTagOffset = new Vector3(0, 1.5f, 0);    
    private GameObject nameTagInstance;
    private TMP_Text nameText;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        nameTagInstance = Instantiate(nameTagPrefab);
        nameTagInstance.transform.SetParent(transform);
        nameTagInstance.transform.localPosition = nameTagOffset;
        nameText = nameTagInstance.GetComponentInChildren<TMP_Text>();
        nameText.text = $"Player {OwnerClientId + 1}";
    }

    void LateUpdate()
    {
        if (nameTagInstance != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                nameTagInstance.transform.LookAt(mainCamera.transform);
                nameTagInstance.transform.Rotate(0, 180, 0);
            }
        }
    }

    private void OnDestroy()
    {
        if (nameTagInstance != null)
        {
            Destroy(nameTagInstance);
        }
    }
}