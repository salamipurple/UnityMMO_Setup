using UnityEngine;
using Unity.Netcode;
using TMPro;

public class UserName : NetworkBehaviour
{
    [SerializeField] TextMeshPro userNameDisplay;

    public static string userName;
    [SerializeField] private NetworkVariable<string> networkUserName = new NetworkVariable<string>("userName");

    public override void OnNetworkSpawn()
    {
        // Run existing logic in base method
        base.OnNetworkSpawn();
        // Ensure the user name display is up-to-date when a client joins
        userNameDisplay.text = networkUserName.Value;
    }

    // Subscribe to score changes when this object is enabled
    void OnEnable()
    {
        networkUserName.OnValueChanged += OnScoreChanged;
    }

    // Unsubscribe when disabled to avoid memory leaks
    void OnDisable()
    {
        networkUserName.OnValueChanged -= OnScoreChanged;
    }

    // Called whenever a user name changes, on all clients
    private void OnScoreChanged(string previousValue, string newValue)
    {
        // Update the user name display for everyone
        userNameDisplay.text = networkUserName.Value;
    }
}