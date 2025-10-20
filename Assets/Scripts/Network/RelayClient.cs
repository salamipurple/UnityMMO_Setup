// ============================================================================
// RELAY CLIENT - Unity Relay Networking Client Implementation
// ============================================================================
// This script handles the client-side connection to a Unity Relay server.
// Unity Relay is a service that helps players connect to multiplayer games
// without requiring port forwarding or complex NAT traversal.
//
// HOW UNITY RELAY WORKS:
// 1. Host creates a "relay allocation" (reserved server space) and gets a join code
// 2. Clients use the join code to connect to the same relay allocation
// 3. All network traffic goes through Unity's relay servers instead of direct P2P
// 4. This solves NAT/firewall issues that prevent direct connections
//
// NETWORKING FLOW:
// Client -> Unity Relay Server -> Host
// All messages are routed through the relay, making connections reliable
// ============================================================================

using UnityEngine;
using Unity.Services.Relay;           // Core Unity Relay service API
using Unity.Services.Relay.Models;    // Data models for relay operations (Allocation, JoinAllocation, etc.)
using Unity.Netcode;                  // Unity's networking framework
using Unity.Netcode.Transports.UTP;   // Unity Transport Protocol - handles low-level networking
using Unity.Networking.Transport.Relay; // Relay-specific transport data structures
using TMPro;                         // TextMeshPro for UI elements

// RelayClient handles connecting to an existing Unity Relay session using a join code.
// This is the client-side component that allows players to join multiplayer games
// hosted by someone using RelayHost.
public class RelayClient : MonoBehaviour
{
    // ============================================================================
    // JOIN CODE MANAGEMENT
    // ============================================================================

    // The join code that clients use to connect to a relay session.
    // Join codes are 6-character alphanumeric strings (e.g., "C86DGQ").
    // This should be provided by the host player who created the relay session.
    public string joinCode = "C86DGQ"; // Replace or assign dynamically


    // Sanitizes and sets the join code for connecting to a relay session.
    // This method ensures the join code is in the correct format by:
    // 1. Trimming whitespace
    // 2. Converting to uppercase
    // 3. Removing any invalid characters (keeping only A-Z and 0-9)
    // Parameter: code - The raw join code input from user
    public void SetJoinCode(string code)
    {
        // Regex removes any characters that aren't letters or numbers
        // This prevents invalid characters from breaking the relay connection
        joinCode = System.Text.RegularExpressions.Regex.Replace(code.Trim().ToUpper(), "[^A-Z0-9]", "");
    }

    // ============================================================================
    // RELAY CONNECTION LOGIC
    // ============================================================================

    // Main method to join an existing Unity Relay session using the join code.
    // This method handles the complete client connection process:
    // 1. Validates the join code
    // 2. Requests connection data from Unity Relay service
    // 3. Configures the network transport with relay server information
    // 4. Starts the client connection
    [ContextMenu("Join Relay Game")]
    public async void JoinRelay()
    {
        Debug.Log($"Client: Attempting to join Relay with code: {joinCode}...");
        
        // Validate join code before attempting connection
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogError("Client: Join code is empty. Please enter a valid join code.");
            return;
        }

        try
        {
            // ============================================================================
            // STEP 1: REQUEST RELAY ALLOCATION DATA
            // ============================================================================
            // The JoinAllocationAsync call contacts Unity's relay servers to get
            // connection information for the specific relay session identified by joinCode.
            // This returns a JoinAllocation object containing all the data needed
            // to establish a connection through the relay.
            Debug.Log("Client: Requesting to join allocation from Unity Relay Service...");
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log($"Client: Successfully joined allocation. Allocation ID: {joinAlloc.AllocationId}");

            // ============================================================================
            // STEP 2: CREATE RELAY SERVER DATA
            // ============================================================================
            // RelayServerData contains all the information needed to connect through
            // the relay server. This includes:
            // - Server IP and port (where the relay server is located)
            // - Allocation ID bytes (identifies this specific relay session)
            // - Connection data (authentication/encryption data for this client)
            // - Host connection data (data needed to communicate with the host)
            // - Key (encryption key for secure communication)
            // - WebSocket flag (false = use UDP, true = use WebSockets)
            var serverData = new RelayServerData(
                joinAlloc.RelayServer.IpV4,        // IP address of the relay server
                (ushort)joinAlloc.RelayServer.Port, // Port number of the relay server
                joinAlloc.AllocationIdBytes,       // Unique identifier for this relay session
                joinAlloc.ConnectionData,          // Client-specific connection data
                joinAlloc.HostConnectionData,      // Data needed to communicate with host
                joinAlloc.Key,                     // Encryption key for secure communication
                false // Using UDP instead of WebSockets for better performance
            );

            // ============================================================================
            // STEP 3: CONFIGURE NETWORK TRANSPORT
            // ============================================================================
            // Unity Transport Protocol (UTP) is the low-level networking system.
            // We configure it to use the relay server data instead of direct connections.
            // This tells the transport layer to route all network traffic through
            // the Unity Relay servers instead of trying to connect directly to the host.
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(serverData);

            // ============================================================================
            // STEP 4: START CLIENT CONNECTION
            // ============================================================================
            // This initiates the actual network connection. The NetworkManager will:
            // 1. Use the configured transport to connect to the relay server
            // 2. Establish a connection through the relay to the host
            // 3. Begin the client-side networking session
            NetworkManager.Singleton.StartClient();

            // Provide user feedback that connection attempt has started
            Debug.Log("Client: Relay client started and attempting to connect to host.");
        }
        catch (System.Exception e)
        {
            // Handle any errors that occur during the relay connection process
            // Common errors include:
            // - Invalid join code
            // - Network connectivity issues
            // - Relay service unavailable
            // - Authentication problems
            Debug.LogError($"Client: Failed to join Relay or start client: {e.Message}\n{e.StackTrace}");
        }
    }
}