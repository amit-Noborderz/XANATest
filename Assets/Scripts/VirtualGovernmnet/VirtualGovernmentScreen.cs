using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VirtualGovernmentScreen : MonoBehaviour
{
    public List <ScreenOverlay> ScreenOverlayText = new List<ScreenOverlay>(); // List to hold screen overlays
    public static VirtualGovernmentScreen Instance; // Singleton instance
    [Header("Screen Components")]
    public GameObject screenMesh; // Assign the screen mesh in the inspector
    public GameObject overlayObject; // Assign the overlay object in the inspector
    public GameObject offScreen; // Assign the off-screen object in the inspector

    public string TopicName;
    private void Awake()
    {
        // Ensure this is the only instance and destroy on load
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    public void UpdateTextOnOverlay(string name, string descriptionTxt, string topicName) 
    {
        foreach (var overlay in ScreenOverlayText)
        {
            overlay.NameText.text = name;
            overlay.DescriptionText.text = descriptionTxt;
            overlay.TopicNameBelow.text = topicName;
        }
    }

    [System.Serializable]
    public class ScreenOverlay { 
        public TextMeshPro NameText; // Text component for the name
        public TextMeshPro DescriptionText; // Text component for the description
        public TextMeshPro TopicName;
        public TextMeshPro TopicNameBelow;
    }

}
