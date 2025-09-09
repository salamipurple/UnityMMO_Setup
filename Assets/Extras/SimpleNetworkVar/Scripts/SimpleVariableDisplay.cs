using UnityEngine;
using TMPro;

public class SimpleVariableDisplay : MonoBehaviour
{
    [SerializeField] TextMeshPro textDisplay;

    public void UpdateDisplay(int newValue)
    {
        textDisplay.text = newValue.ToString();
    }
}