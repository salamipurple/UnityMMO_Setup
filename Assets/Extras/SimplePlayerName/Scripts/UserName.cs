using UnityEngine;
using Unity.Netcode;          // Unity's networking framework for multiplayer
using Unity.Collections;       // For FixedString types used in networking
using TMPro;                   // TextMeshPro for UI text display

// Handles networked player name display and synchronization across all clients.
// This script demonstrates proper NetworkVariable usage and ServerRpc patterns.
public class UserName : NetworkBehaviour
{
    [SerializeField] TextMeshPro userNameDisplay;    // The 3D text that shows above the player (visible to all)
    private TextMeshProUGUI userNameInput;           // Reference to UI input field (found at runtime)
    public static string userName;                   // Static storage for local player name (optional)
    [SerializeField] private GameObject playerBody;
    [SerializeField] private float yOffset = 2.5f;

    // NetworkVariable: A special variable that automatically synchronizes across all clients
    // - FixedString64Bytes: A network-safe string type (max 64 bytes)
    // - default: Initial value is empty/default when object spawns
    // - NetworkVariableReadPermission.Everyone: All clients can READ this value
    // - NetworkVariableWritePermission.Server: Only the SERVER can WRITE/change this value
    // This ensures only the server can modify names, preventing cheating/conflicts
    private NetworkVariable<FixedString64Bytes> networkUserName = new NetworkVariable<FixedString64Bytes>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, playerBody.transform.position.y + yOffset, transform.position.z);    
    }

    // Awake() runs once when the object is created, before Start()
    // This is the BEST place to subscribe to NetworkVariable changes because:
    // - It happens early in the object lifecycle
    // - It ensures we don't miss any value changes
    // - It works for both host and clients
    void Awake()
    {
        // Subscribe to network variable changes (like PlayerMove does)
        // This means whenever networkUserName changes, OnNameChanged() will be called
        // This happens on ALL clients automatically - that's the magic of NetworkVariables!
        networkUserName.OnValueChanged += OnNameChanged;
    }

    // OnNetworkSpawn() is called when this networked object is spawned/activated
    // This runs on ALL clients (host and clients) when the object appears in the game
    // Important: This is NOT where we set initial values - that's done in Start() for the owner
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Update the name display with current value (important for clients joining after host)
        // This handles the case where a client joins AFTER the host has already set their name
        // Without this, late-joining clients wouldn't see existing player names
        if (!string.IsNullOrEmpty(networkUserName.Value.ToString()))
        {
            userNameDisplay.text = networkUserName.Value.ToString();
            Debug.Log($"Network spawned with existing name: {networkUserName.Value}");
        }
    }

    // Start() runs after Awake() and OnNetworkSpawn()
    // This is where we handle OWNER-ONLY initialization logic
    // Only the client who owns this player object should set their own name
    void Start()
    {
        // CRITICAL: Only the owner of this networked object should execute this code
        // This prevents other clients from trying to set this player's name
        if (!IsOwner) return;

        // Find the name input UI element in the scene
        // GameObject.Find() searches the entire scene for an object with this name
        GameObject nameInputObject = GameObject.Find("NameInputText");
        if (nameInputObject != null)
        {
            // Get the TextMeshProUGUI component from the found object
            userNameInput = nameInputObject.GetComponent<TextMeshProUGUI>();

            // Request the server to set our player name (like PlayerMove does)
            // This is the PROPER way to set networked data - always go through the server
            if (!string.IsNullOrEmpty(userNameInput.text))
            {
                // User has entered a name in the UI - use that
                SetUserNameServerRpc(userNameInput.text);
            }
            else
            {
                // No input found or input is empty - use a default name
                // This prevents players from having blank or "NO NAME" names
                SetUserNameServerRpc("Player");
            }

            if (GameObject.Find("Server Info") != null)
            {
                GameObject.Find("Server Info").SetActive(false);
            } else
            {
                Debug.LogWarning("Server panel element not found in scene.");
            }
        }
    }

    // ServerRpc: A special method that can ONLY be executed on the server
    // [ServerRpc] tells Unity Netcode that this method should run on the server, 
    // even if called from a client
    // 
    // How it works:
    // 1. Client calls SetUserNameServerRpc("John")
    // 2. The call gets sent over the network to the server
    // 3. The server executes this method and sets networkUserName.Value = "John"
    // 4. Because networkUserName is a NetworkVariable, the change automatically 
    //    gets sent to ALL clients
    // 5. OnNameChanged() gets called on all clients, updating their displays
    // 
    // This pattern ensures the server is the "source of truth" for all player data
    [ServerRpc]
    void SetUserNameServerRpc(string name)
    {
        // Only the server can modify NetworkVariable values
        // This assignment will automatically sync to all clients
        networkUserName.Value = name;
        Debug.Log($"Server set user name to: {name}");
    }

    // This callback method is automatically called whenever networkUserName changes
    // It runs on ALL clients (host and remote clients) simultaneously
    // This is the "magic" of NetworkVariables - automatic synchronization!
    // 
    // Parameters:
    // - previousValue: What the name was before the change
    // - newValue: What the name is now after the change
    // 
    // This method ensures all players see the same name above each player character
    private void OnNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        // Update the user name display for everyone
        // This visual change happens on ALL clients automatically
        userNameDisplay.text = newValue.ToString();
        Debug.Log($"User name changed from '{previousValue}' to '{newValue}'");
    }

    // OnDestroy() is called when this object is being destroyed
    // CRITICAL: We must unsubscribe from events to prevent memory leaks
    // If we don't do this, the event system will keep references to this object
    // even after it's destroyed, causing memory leaks and potential crashes
    public override void OnDestroy()
    {
        // Unsubscribe from network variable changes to prevent memory leaks
        // This is the cleanup pair to the subscription we did in Awake()
        networkUserName.OnValueChanged -= OnNameChanged;
        base.OnDestroy();
    }

    // Public utility method that can be called from UI buttons or other scripts
    // This provides a clean interface for changing player names at runtime
    // Example usage: GetComponent<UserName>().ChangeUserName("NewName");
    public void ChangeUserName(string newName)
    {
        // Security check: Only the owner of this player can change their own name
        // This prevents other players from changing someone else's name
        if (IsOwner)
        {
            SetUserNameServerRpc(newName);
        }
    }
}