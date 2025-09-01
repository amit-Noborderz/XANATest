using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
// Player item component for the UI list
public class PlayerVoiceItem : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI playerNameText;
    public Button muteButton;
    public Image muteButtonImage;
    public Sprite muteSprite;
    public Sprite unmuteSprite;
    public Image connectionIndicator;
    public Color connectedColor = Color.green;
    public Color disconnectedColor = Color.red;

    public Player player;
    private VoicePlayerManager manager;
    private bool isMuted = false;

    public virtual void Initialize(Player playerData, VoicePlayerManager voiceManager)
    {
        player = playerData;
        manager = voiceManager;

        // Set player name
        if (playerNameText != null)
            playerNameText.text = player.NickName;

        // Setup mute button
        if (muteButton != null)
            muteButton.onClick.AddListener(OnMuteButtonClicked);

        // Update connection status
        UpdateConnectionStatus(true);

        // Initialize mute state
        UpdateMuteState(true);
    }

    void OnMuteButtonClicked()
    {
        if (manager != null && player != null)
        {
            manager.TogglePlayerMute(player.ActorNumber);
        }
    }

    public void UpdateMuteState(bool muted)
    {
        isMuted = muted;

        if (muteButtonImage != null)
        {
            muteButtonImage.sprite = muted ? muteSprite : unmuteSprite;
        }

        // Update button color or other visual feedback
        if (muteButton != null)
        {
            ColorBlock colors = muteButton.colors;
            colors.normalColor = muted ? Color.red : Color.white;
            muteButton.colors = colors;
        }
    }

    public void UpdateConnectionStatus(bool connected)
    {
        if (connectionIndicator != null)
        {
            connectionIndicator.color = connected ? connectedColor : disconnectedColor;
        }
    }

    public void Update()
    {
        // Update connection status based on player's connection
        if (player != null)
        {
            bool isConnected = PhotonNetwork.PlayerList.Contains(player);
            UpdateConnectionStatus(isConnected);
        }
    }
}