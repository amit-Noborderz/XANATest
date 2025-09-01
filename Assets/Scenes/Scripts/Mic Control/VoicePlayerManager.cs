using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using Photon.Pun.Demo.PunBasics;
using System.Threading.Tasks;

public class VoicePlayerManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public GameObject PlayerListParent;
    public Button PlayerListButton;
    public Transform playerListContent;
    public GameObject playerItemPrefab;
    public Button muteAllButton;
    public Button unmuteAllButton;
    public TMPro.TextMeshProUGUI MuteAllText;
    public TMPro.TextMeshProUGUI UnMuteAllText;
    public Sprite MuteAllSp;
    public Sprite MuteAllSpHi;
    public Sprite UmuteAllSp;
    public Sprite UmuteAllSpHi;
    public Sprite playerListSp;
    public Sprite playerListSpHi;
    //public List<GameObject> playerobjects;
    private Dictionary<int, PlayerVoiceItem> playerItems = new Dictionary<int, PlayerVoiceItem>();
    private Dictionary<int, bool> playerMuteStates = new Dictionary<int, bool>();

    public static Action<bool> MuteAllPlayersMic;
    public static Action<int, bool> SetUserMicStatus;

    void Start()
    {
        // Initialize buttons
        if (muteAllButton != null)
            muteAllButton.onClick.AddListener(MuteAllPlayer);

        if (unmuteAllButton != null)
            unmuteAllButton.onClick.AddListener(UnmuteAllPlayers);

        if(PlayerListButton!=null)
        {
            PlayerListButton.onClick.AddListener(OpenPlayerListToggle);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        // Subscribe to Photon events
        PhotonNetwork.AddCallbackTarget(this);

        OnJoinedRoom();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        // Unsubscribe from Photon events
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log($"Player {newPlayer.NickName} joined the room");
        CreatePlayerItem(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Debug.Log($"Player {otherPlayer.NickName} left the room");
        RemovePlayerItem(otherPlayer.ActorNumber);
    }

    public override void OnJoinedRoom()
    {
        //Debug.Log("Joined room, creating player list");
        RefreshPlayerList();
    }


    void CreatePlayerItem(Player player)
    {
        if (playerItems.ContainsKey(player.ActorNumber))
            return;
        GameObject item = Instantiate(playerItemPrefab, playerListContent);
        PlayerVoiceItem voiceItem = item.GetComponent<PlayerVoiceItem>();

        if (voiceItem != null)
        {
            voiceItem.Initialize(player, this);
            playerItems[player.ActorNumber] = voiceItem;
            playerMuteStates[player.ActorNumber] = true;
        }
        SetPlayerMute(player.ActorNumber, true);
    }

    void RemovePlayerItem(int actorNumber)
    {
        if (playerItems.ContainsKey(actorNumber))
        {
            if (playerItems[actorNumber] != null)
                Destroy(playerItems[actorNumber].gameObject);

            playerItems.Remove(actorNumber);
            playerMuteStates.Remove(actorNumber);
        }
    }

    void RefreshPlayerList()
    {
        // Clear existing items
        foreach (var item in playerItems.Values)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        playerItems.Clear();
        playerMuteStates.Clear();

        Debug.LogError(PhotonNetwork.PlayerList.Length);
        // Create items for all current players
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.IsLocal)
                CreatePlayerItem(player);
        }
    }

    public void TogglePlayerMute(int actorNumber)
    {
        if (playerMuteStates.ContainsKey(actorNumber))
        {
            bool newMuteState = !playerMuteStates[actorNumber];
            SetPlayerMute(actorNumber, newMuteState);
        }
    }

    public async void SetPlayerMute(int actorNumber, bool muted)
    {
        playerMuteStates[actorNumber] = muted;

        // Update UI
        if (playerItems.ContainsKey(actorNumber))
        {
            playerItems[actorNumber].UpdateMuteState(muted);
        }

        int x = await GetPhotonViewId(actorNumber);
        SetUserMicStatus?.Invoke(x, muted);

        //Debug.Log($"Player {actorNumber} is now {(muted ? "muted" : "unmuted")}");
    }

    async Task<int> GetPhotonViewId(int ActorNumber)
    {
        await Task.Delay(1000);
        List<GameObject> gameObjects = MutiplayerController.instance.playerobjects;
        for (int i = 0; i < gameObjects.Count; i++)
        {
            if (gameObjects[i] != null)
                if (gameObjects[i].GetComponent<PhotonView>().Owner.ActorNumber == ActorNumber)
                {
                    return gameObjects[i].GetComponent<PhotonView>().ViewID;
                }
        }
        return 0;
    }

    public void MuteAllPlayer()
    {
        MuteAllPlayersMic?.Invoke(true);
        foreach (var actorNumber in playerMuteStates.Keys)
        {
            if (actorNumber != PhotonNetwork.LocalPlayer.ActorNumber) // Don't mute self
            {
                playerItems[actorNumber].UpdateMuteState(true);
            }
        }
        MuteAllUIUpdate(true);
        UnMuteAllUIUpdate(false);
    }

    void MuteAllUIUpdate(bool Mute)
    {
        if (Mute)
        {
            MuteAllText.color = Color.white;
            muteAllButton.GetComponent<Image>().sprite = MuteAllSpHi;
        }
        else
        {
            MuteAllText.color = Color.black;
            muteAllButton.GetComponent<Image>().sprite = MuteAllSp;
        }
    }

    public void UnmuteAllPlayers()
    {
        MuteAllPlayersMic?.Invoke(false);
        foreach (var actorNumber in playerMuteStates.Keys)
        {
            if (actorNumber != PhotonNetwork.LocalPlayer.ActorNumber) // Don't mute self
            {
                playerItems[actorNumber].UpdateMuteState(false);
            }
        }
        UnMuteAllUIUpdate(true);
        MuteAllUIUpdate(false);
    }

    void UnMuteAllUIUpdate(bool Unmute)
    {
        if (Unmute)
        {
            UnMuteAllText.color = Color.white;
            unmuteAllButton.GetComponent<Image>().sprite = UmuteAllSpHi;
        }
        else
        {
            UnMuteAllText.color = Color.black;
            unmuteAllButton.GetComponent<Image>().sprite = UmuteAllSp;
        }
    }

    public bool IsPlayerMuted(int actorNumber)
    {
        return playerMuteStates.ContainsKey(actorNumber) && playerMuteStates[actorNumber];
    }

    void OpenPlayerListToggle()
    {
        if (PlayerListParent.activeInHierarchy)
            PlayerListButton.GetComponent<Image>().sprite = playerListSp;
        else
            PlayerListButton.GetComponent<Image>().sprite = playerListSpHi;
        PlayerListParent.SetActive(!PlayerListParent.activeInHierarchy);
    }


    //public void OnPhotonInstantiate(PhotonMessageInfo info)
    //{
    //    playerobjects.Add(info.photonView.gameObject);
    //}

    // IPunObservable implementation for syncing mute states
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        // Send mute states to other clients
    //        stream.SendNext(playerMuteStates.Count);
    //        foreach (var kvp in playerMuteStates)
    //        {
    //            stream.SendNext(kvp.Key);
    //            stream.SendNext(kvp.Value);
    //        }
    //    }
    //    else
    //    {
    //        // Receive mute states from other clients
    //        int count = (int)stream.ReceiveNext();
    //        for (int i = 0; i < count; i++)
    //        {
    //            int actorNumber = (int)stream.ReceiveNext();
    //            bool muteState = (bool)stream.ReceiveNext();

    //            if (playerMuteStates.ContainsKey(actorNumber))
    //            {
    //                playerMuteStates[actorNumber] = muteState;
    //                if (playerItems.ContainsKey(actorNumber))
    //                {
    //                    playerItems[actorNumber].UpdateMuteState(muteState);
    //                }
    //            }
    //        }
    //    }
    //}
}

