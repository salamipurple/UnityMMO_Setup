// ============================================================================
// RELAY INITIALIZER - Unity Services Authentication Setup
// ============================================================================
// This script handles the initialization of Unity Services, which is required
// before using Unity Relay. Unity Services provides authentication and other
// cloud-based services that power Unity Relay functionality.
//
// AUTHENTICATION REQUIREMENTS:
// Unity Relay requires users to be authenticated through Unity Services.
// This script handles anonymous authentication, which is the simplest approach
// for getting started with Unity Relay without requiring user accounts.
//
// INITIALIZATION FLOW:
// 1. Initialize Unity Services core systems
// 2. Check if user is already authenticated
// 3. If not authenticated, sign in anonymously
// 4. Ready to use Unity Relay services
// ============================================================================

using UnityEngine;
using Unity.Services.Core;        // Core Unity Services API for initialization
using Unity.Services.Authentication; // Authentication service for user management

// RelayInitializer handles the setup and authentication required for Unity Relay services.
// This component must be present in the scene before attempting to use RelayHost or RelayClient.
// It ensures that Unity Services are properly initialized and the user is authenticated.
public class RelayInitializer : MonoBehaviour
{
    // Unity lifecycle method called when the GameObject starts.
    // This method handles the complete initialization process for Unity Services
    // and ensures the user is properly authenticated for using Unity Relay.
    // 
    // The initialization process is asynchronous to avoid blocking the main thread
    // while communicating with Unity's cloud services.
    async void Start()
    {
        // ============================================================================
        // STEP 1: INITIALIZE UNITY SERVICES CORE
        // ============================================================================
        // UnityServices.InitializeAsync() sets up the core Unity Services infrastructure.
        // This includes:
        // - Establishing connection to Unity's cloud services
        // - Setting up internal service managers
        // - Preparing authentication and other service APIs
        // 
        // This call is required before using any Unity Services, including Relay.
        Debug.Log("Initializing Unity Services...");
        await UnityServices.InitializeAsync();

        // ============================================================================
        // STEP 2: CHECK AUTHENTICATION STATUS
        // ============================================================================
        // Before using Unity Relay, users must be authenticated. We check if the
        // user is already signed in to avoid unnecessary authentication calls.
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            // ============================================================================
            // STEP 3: ANONYMOUS AUTHENTICATION
            // ============================================================================
            // SignInAnonymouslyAsync() creates a temporary anonymous user account.
            // This approach is ideal for:
            // - Quick prototyping and testing
            // - Games that don't require persistent user accounts
            // - Getting started with Unity Relay without complex user management
            //
            // The anonymous account provides:
            // - Unique player identification
            // - Access to Unity Services (including Relay)
            // - Temporary session-based authentication
            Debug.Log("Signing in anonymously...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            // Log the assigned player ID for debugging purposes
            // This ID is unique to this anonymous session
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
        else
        {
            // User is already authenticated, no need to sign in again
            Debug.Log("Already signed in");
        }

        // ============================================================================
        // INITIALIZATION COMPLETE
        // ============================================================================
        // At this point, Unity Services are initialized and the user is authenticated.
        // The application is now ready to use Unity Relay services through
        // RelayHost and RelayClient components.
        Debug.Log("Unity Services initialization complete. Ready to use Unity Relay!");
    }
}