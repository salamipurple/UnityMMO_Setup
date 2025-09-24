using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using TMPro;

public class RelayHost : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI joinCodeDisplay;
    [SerializeField] public string joinCode;
    [SerializeField] private string userName = "Host";
    void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnTransportFailure += HandleTransportFailure;
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnTransportFailure -= HandleTransportFailure;
        }
    }

    private void HandleTransportFailure()
    {
        Debug.LogError("Host: NetworkManager.OnTransportFailure event triggered! The Relay connection was likely lost or invalidated.");
        // You might want to add logic here to inform the user, attempt to clean up,
        // or even try to re-establish the Relay connection after a delay.
        // For example, you could call NetworkManager.Singleton.Shutdown() if not already shutting down.
    }

    [ContextMenu("Start Host With Relay")]
    public async void StartHostWithRelay()
    {
        try
        {
            Debug.Log("Host: Requesting allocation from Unity Relay Service...");

            var allocationTask = RelayService.Instance.CreateAllocationAsync(5);
            var timeoutTask = Task.Delay(10000);

            var completedTask = await Task.WhenAny(allocationTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Debug.LogError("Host: Relay allocation request timed out.");
                return;
            }

            if (allocationTask.IsFaulted)
            {
                if (allocationTask.Exception.InnerException is RelayServiceException relayEx)
                {
                    Debug.LogError($"Host: RelayServiceException during allocation: {relayEx.Message} (Reason: {relayEx.Reason})");
                }
                else
                {
                    Debug.LogError($"Host: Exception during allocation: {allocationTask.Exception.InnerException?.Message ?? allocationTask.Exception.Message}");
                }
                return;
            }

            Allocation alloc = allocationTask.Result;
            Debug.Log($"Host: Successfully allocated Relay server. Allocation ID: {alloc.AllocationId}");

            Debug.Log("Host: Requesting join code...");
            var joinCodeTask = RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            timeoutTask = Task.Delay(10000);

            completedTask = await Task.WhenAny(joinCodeTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Debug.LogError("Host: Get join code request timed out.");
                return;
            }

            if (joinCodeTask.IsFaulted)
            {
                if (joinCodeTask.Exception.InnerException is RelayServiceException relayEx)
                {
                    Debug.LogError($"Host: RelayServiceException getting join code: {relayEx.Message} (Reason: {relayEx.Reason})");
                }
                else
                {
                    Debug.LogError($"Host: Exception getting join code: {joinCodeTask.Exception.InnerException?.Message ?? joinCodeTask.Exception.Message}");
                }
                return;
            }

            joinCode = joinCodeTask.Result;
            Debug.Log($"Host: Successfully retrieved join code: {joinCode}");

            if (joinCodeDisplay != null)
                joinCodeDisplay.text = joinCode;

            var serverData = new RelayServerData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.ConnectionData,
                alloc.ConnectionData,
                alloc.Key,
                false
            );

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(serverData);

            NetworkManager.Singleton.StartHost();
            Debug.Log($"Host: Relay server started and listening. IP: {alloc.RelayServer.IpV4}, Port: {alloc.RelayServer.Port}");

            
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Host: RelayServiceException: {e.Message}\nReason: {e.Reason}\n{e.StackTrace}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Host: Relay host setup failed: {e.Message}\n{e.StackTrace}");
        }
    }

    public void CopyJoinCode()
    {
        GUIUtility.systemCopyBuffer = joinCode;
    }
}