using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

/// <summary>
/// This class manages the movement, camera control, and name synchronization for a player character.
/// It must be attached to a GameObject that has a NetworkObject component.
/// This script only allows movement for the object that the player owns.
/// </summary>
public class PlayerMove : NetworkBehaviour
{

    // A reference to a UI TextMeshPro Input Field, likely used for the player to enter their name.
    private TextMeshProUGUI playerNameInput;

    // A reference to a UI panel, probably related to server controls or information.
    private GameObject serverPanel;

    // This is a NetworkVariable, which synchronizes its value from the server to all clients.
    // It's used here to store the player's name.
    // NetworkVariableWritePermission.Server means only the server can change this value, which is a secure practice.
    // NetworkVariableReadPermission.Everyone means all clients can read the value.
    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // A reference to the TextMeshPro component that displays the player's name in the game world (e.g., above their head).
    public TextMeshPro myName;

    void Awake()
    {

        // Subscribe our custom method 'OnPlayerNameChanged' to the OnValueChanged event of the playerName NetworkVariable.
        // This means whenever the playerName value changes on the server, our method will be called on all clients.
        playerName.OnValueChanged += OnPlayerNameChanged;
    }

    /// <summary>
    /// OnNetworkSpawn is a special Netcode method called when the object is spawned on the network.
    /// This happens for all players when they join the game.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // It's important to call the base method to ensure Netcode's internal setup is done.
        base.OnNetworkSpawn();

        // When a new client joins, the playerName NetworkVariable will already have a value for existing players.
        // This code ensures the TextMeshPro display is updated with that value as soon as this player object is spawned for the new client.
        if (!string.IsNullOrEmpty(playerName.Value.ToString()))
        {
            myName.text = playerName.Value.ToString();
            Debug.Log($"Network spawned with existing name: {playerName.Value}");
        }
    }

    /// <summary>
    /// This method is called on all clients when the 'playerName' NetworkVariable changes.
    /// </summary>
    /// <param name="previousValue">The old name.</param>
    /// <param name="newValue">The new name.</param>
    private void OnPlayerNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        // Update the in-game name display with the new value.
        myName.text = newValue.ToString();
    }
}