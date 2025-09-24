using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class UserName : NetworkBehaviour
{
    [SerializeField] TextMeshPro userNameDisplay;

    public static string userName;
    [SerializeField] private NetworkVariable<FixedString64Bytes> networkUserName = new NetworkVariable<FixedString64Bytes>("USERNAME");

    public override void OnNetworkSpawn()
    {
        // Run existing logic in base method
        base.OnNetworkSpawn();
        // Ensure the user name display is up-to-date when a client joins
        userNameDisplay.text = networkUserName.Value.ToString();
    }

    // Subscribe to score changes when this object is enabled
    void OnEnable()
    {
        networkUserName.OnValueChanged += OnNameChanged;
    }

    // Unsubscribe when disabled to avoid memory leaks
    void OnDisable()
    {
        networkUserName.OnValueChanged -= OnNameChanged;
    }

    // Called whenever a user name changes, on all clients
    private void OnNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        // Update the user name display for everyone
        userNameDisplay.text = newValue.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeUserName(string newName)
    {
        networkUserName.Value = FixedString64Bytes.FromString(newName);
    }
}