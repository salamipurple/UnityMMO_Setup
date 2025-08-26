using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class RelayInitializer : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("Initializing Unity Services...");
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Signing in anonymously...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
        else
        {
            Debug.Log("Already signed in");
        }
    }
}