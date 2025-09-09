using UnityEngine;
using Unity.Netcode;

public class StepCounter : NetworkBehaviour
{
    [SerializeField] private SimpleVariableDisplay simpleVariableDisplay;

    // NetworkVariables automatically synchronize their values across all clients
    [SerializeField] private NetworkVariable<int> simpleVariable = new NetworkVariable<int>(0);

    // Public properties to access scores
    public int SimpleVariable => simpleVariable.Value;

    public override void OnNetworkSpawn()
    {
        // Run existing logic in base method
        base.OnNetworkSpawn();
        // Ensure the score display is up-to-date when a client joins
        simpleVariableDisplay.UpdateDisplay(simpleVariable.Value);
    }

    // Subscribe to score changes when this object is enabled
    void OnEnable()
    {
        simpleVariable.OnValueChanged += OnScoreChanged;
    }

    // Unsubscribe when disabled to avoid memory leaks
    void OnDisable()
    {
        simpleVariable.OnValueChanged -= OnScoreChanged;
    }

    // Called whenever either score changess, on all clients
    private void OnScoreChanged(int previousValue, int newValue)
    {
        // Update the score display for everyone
        simpleVariableDisplay.UpdateDisplay(simpleVariable.Value);
    }

    void OnMouseDown()
    {
        simpleVariable.Value += 1;

        // Debug.Log("Step detected");
    }
}
