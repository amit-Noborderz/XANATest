// Additional helper script for voice connection management
using UnityEngine;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
using UnityEngine.UI;
using Photon.Realtime;

public class VoiceConnectionHelper : MonoBehaviour
{
    public static VoiceConnectionHelper Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Method to get speaker for a specific player
    public Speaker GetSpeakerForPlayer(int actorNumber)
    {
        PhotonVoiceView[] voiceViews = FindObjectsOfType<PhotonVoiceView>();
        foreach (var voiceView in voiceViews)
        {
            Photon.Pun.PhotonView photonView = voiceView.GetComponent<Photon.Pun.PhotonView>();
            if (photonView != null && photonView.Owner != null && photonView.Owner.ActorNumber == actorNumber)
            {
                return voiceView.SpeakerInUse;
            }
        }
        return null;
    }
    
    // Method to check if player has voice transmission
    public bool IsPlayerTransmitting(int actorNumber)
    {
        PhotonVoiceView[] voiceViews = FindObjectsOfType<PhotonVoiceView>();
        foreach (var voiceView in voiceViews)
        {
            Photon.Pun.PhotonView photonView = voiceView.GetComponent<Photon.Pun.PhotonView>();
            if (photonView != null && photonView.Owner != null && photonView.Owner.ActorNumber == actorNumber)
            {
                Recorder recorder = voiceView.RecorderInUse;
                return recorder != null && recorder.IsCurrentlyTransmitting;
            }
        }
        return false;
    }
}


// Enhanced player item with voice level indicator


/*
UI HIERARCHY SETUP GUIDE:
==========================

1. Create a Canvas with the following hierarchy:

Canvas
├── PlayerListPanel
│   ├── Header
│   │   ├── Title (Text: "Voice Chat Players")
│   │   ├── MuteAllButton (Button: "Mute All")
│   │   └── UnmuteAllButton (Button: "Unmute All")
│   ├── ScrollView
│   │   └── Viewport
│   │       └── Content (This is your playerListParent)
│   └── PlayerItemPrefab (Create as prefab)
│       ├── Background (Image)
│       ├── PlayerName (TextMeshPro)
│       ├── ConnectionIndicator (Image - small circle)
│       ├── VoiceLevelSlider (Slider)
│       └── MuteButton (Button with Image)

2. PREFAB SETUP for PlayerItemPrefab:
   - Add PlayerVoiceItem component
   - Assign all UI references in the inspector
   - Set mute/unmute sprites
   - Configure colors

3. MANAGER SETUP:
   - Add VoicePlayerManager to a GameObject in scene
   - Assign playerListParent (Content from ScrollView)
   - Assign playerItemPrefab
   - Assign mute/unmute buttons

4. PHOTON VOICE SETUP:
   - Ensure PhotonVoiceNetwork is in scene
   - Add PhotonVoiceView to player prefab
   - Configure Recorder and Speaker components
   - Set up PUN PhotonView with VoicePlayerManager script

REQUIRED COMPONENTS ON PLAYER PREFAB:
====================================
- PhotonView (with VoicePlayerManager observed)
- PhotonVoiceView
- Recorder (for sending voice)
- Speaker (for receiving voice)
- AudioSource (attached to Speaker)
*/