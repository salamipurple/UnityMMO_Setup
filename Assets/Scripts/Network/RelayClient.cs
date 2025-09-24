using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;

public class RelayClient : MonoBehaviour
{
    public string joinCode = "C86DGQ"; // Replace or assign dynamically
    [SerializeField] TextMeshProUGUI joinCodeInput;
    [SerializeField] private string userName = "Client";

    void Update()
    {
        //joinCode = System.Text.RegularExpressions.Regex.Replace(joinCodeInput.text.Trim().ToUpper(), "[^A-Z0-9]", "");

    }
    [ContextMenu("Join Relay Game")]
    public async void JoinRelay()
    {
        Debug.Log($"Client: Attempting to join Relay with code: {joinCode}...");
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogError("Client: Join code is empty. Please enter a valid join code.");
            return;
        }

        try
        {
            Debug.Log("Client: Requesting to join allocation from Unity Relay Service...");
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log($"Client: Successfully joined allocation. Allocation ID: {joinAlloc.AllocationId}");

            var serverData = new RelayServerData(
                joinAlloc.RelayServer.IpV4,
                (ushort)joinAlloc.RelayServer.Port,
                joinAlloc.AllocationIdBytes,
                joinAlloc.ConnectionData,
                joinAlloc.HostConnectionData,
                joinAlloc.Key,
                false // Using UDP instead of WebSockets
            );

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(serverData);

            NetworkManager.Singleton.StartClient();

            // add feedback to the user if the connection is successful
            Debug.Log("Client: Relay client started and attempting to connect to host.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Client: Failed to join Relay or start client: {e.Message}\n{e.StackTrace}");
        }
    }
}