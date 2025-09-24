using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class UserName : NetworkBehaviour
{
    [SerializeField] TextMeshPro userNameDisplay;
    [SerializeField] TextMeshProUGUI userNameInput;
    public static string userName;
    [SerializeField] private NetworkVariable<FixedString64Bytes> networkUserName = new NetworkVariable<FixedString64Bytes>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void Awake()
    {
        if (GameObject.Find("NameInputText") != null && userNameInput == null && IsOwner)
        {
            userNameInput = GameObject.Find("NameInputText").GetComponent<TextMeshProUGUI>();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        // Run existing logic in base method
        base.OnNetworkSpawn();

        if (userNameInput != null)
        {
            // Initialize the Network Variable after creation
            networkUserName = new NetworkVariable<FixedString64Bytes>(userNameInput.text);
            // Ensure the user name display is up-to-date when a client joins
            userNameDisplay.text = networkUserName.Value.ToString();
            ChangeUserNameServerRpc(userNameInput.text);
        }
        else
        {
            networkUserName = new NetworkVariable<FixedString64Bytes>("NO NAME");
            // Ensure the user name display is up-to-date when a client joins
            userNameDisplay.text = networkUserName.Value.ToString();
            ChangeUserNameServerRpc(networkUserName.Value.ToString());
        }
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
    public void ChangeUserNameServerRpc(string newName)
    {
        networkUserName.Value = new FixedString64Bytes(newName);
    }
}