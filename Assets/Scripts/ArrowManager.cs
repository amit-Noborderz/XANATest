using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Voice.PUN;
using UnityEngine.Animations.Rigging;
using Photon.Pun.Demo.PunBasics;

public class ArrowManager : MonoBehaviourPunCallbacks
{
    GameObject arrow;
    Material clientMat;
    Material playerMat;
    Transform mainPlayerParent;
    private bool iscashed = false;
    Dictionary<object, object> cashed_data = new Dictionary<object, object>();

    public bool isBear;
    public TMPro.TextMeshProUGUI PhotonUserName;
    public Image VoiceImage;
    public bool IsSpeak;
    public GameObject ChatShow;
    public GameObject reactionUi;
    private List<EventData> data = new List<EventData>();
    private List<EventData> chatData = new List<EventData>();
    private PhotonView[] photonplayerObjects;

    public delegate void ReactionDelegate(string iconUrl);
    public delegate void CommentDelegate(string iconUrl);
    public static event ReactionDelegate ReactionDelegateButtonClickEvent;
    public static event CommentDelegate CommentDelegateButtonClickEvent;

    public static int viewID;
    public static string parentTransform;

    public static ArrowManager Instance;
    Coroutine temp = null;
    Coroutine chatco = null;

    public PhotonVoiceView VoiceView;

    internal Canvas nameCanvas;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        nameCanvas = PhotonUserName?.GetComponentInParent<Canvas>();
        try
        {
            if (ConstantsHolder.userName.Length > 12)
            {
                PhotonNetwork.NickName = ConstantsHolder.userName.Substring(0, 12) + "...";
            }
            else
            {
                PhotonNetwork.NickName = ConstantsHolder.userName;
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error setting PhotonNetwork.NickName: {e.Message}");
            PhotonNetwork.NickName = ConstantsHolder.userName;
        }

        arrow = Resources.Load<GameObject>("Arrow");
        Material _mat = Resources.Load<Material>("Material #25");
        clientMat = playerMat = _mat;

        if (this.GetComponent<PhotonView>().IsMine)
        {
            if (ConstantsHolder.xanaConstants.isBuilderScene)
                GamificationComponentData.instance.nameCanvas = PhotonUserName?.GetComponentInParent<Canvas>();
            if (SMBCManager.Instance)
                SMBCManager.Instance.NameCanvas = PhotonUserName?.GetComponentInParent<Canvas>();
            if (AvatarSpawnerOnDisconnect.Instance.currentDummyPlayer == null)
            {
                if (!ConstantsHolder.xanaConstants.isXanaPartyWorld)
                {
                    AvatarSpawnerOnDisconnect.Instance.currentDummyPlayer = this.gameObject;
                }
                //PhotonUserName.text = PhotonNetwork.NickName;

                if (!ConstantsHolder.isPenguin || !ConstantsHolder.xanaConstants.isXanaPartyWorld || !ConstantsHolder.xanaConstants.isBuilderGame)
                {
                    var playerController = AvatarSpawnerOnDisconnect.Instance.spawnPoint.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        playerController.animator = this.GetComponent<Animator>();
                        if (this.GetComponent<Animator>())
                            ActionAnimationApplyToPlayer.PlayerAnimatorInitializer?.Invoke(this.GetComponent<Animator>().runtimeAnimatorController);
                        if (playerController && GetComponent<FirstPersonJump>())
                            playerController.playerRig = GetComponent<FirstPersonJump>().jumpRig;
                    }
                }
            }
            if(PhotonNetwork.NickName != null)
                PhotonUserName.text = PhotonNetwork.NickName;
        }
        StartCoroutine(WaitForArrowIntanstiate(this.transform, !this.GetComponent<PhotonView>().IsMine));
        if (!ConstantsHolder.xanaConstants.isXanaPartyWorld || !ConstantsHolder.xanaConstants.isBuilderGame)
        {
            if (AvatarSpawnerOnDisconnect.Instance != null)
            {
                try
                {
                    if (AvatarSpawnerOnDisconnect.Instance.currentDummyPlayer)
                        AvatarSpawnerOnDisconnect.Instance.currentDummyPlayer.GetComponent<IKMuseum>().Initialize();
                }
                catch (Exception e)
                {
                    Debug.Log($"Error initializing IKMuseum: {e.Message}");
                }
            }
        }
        VoiceView = GetComponent<PhotonVoiceView>();
    }

    public void Update()
    {
        if (VoiceView != null)
        {
            if (VoiceView.IsSpeaking)
            {
                VoiceImage.gameObject.SetActive(true);
                IsSpeak = true;
            }
            else
            {
                VoiceImage.gameObject.SetActive(false);
                IsSpeak = false;
            }
        }
    }

    public static void OnInvokeReactionButtonClickEvent(string url)
    {
        ReactionDelegateButtonClickEvent?.Invoke(url);
    }

    public static void OnInvokeCommentButtonClickEvent(string text)
    {
        CommentDelegateButtonClickEvent?.Invoke(text);
    }

    public override void OnEnable()
    {
        if (!ConstantsHolder.xanaConstants.isXanaPartyWorld)
        {
            ReactionDelegateButtonClickEvent += OnChangeReactionIcon;
        }
        CommentDelegateButtonClickEvent += OnChangeText;
        ConstantsHolder.userNameToggleDelegate += OnChangeUsernameToggle;
        OnChangeUsernameToggle(ConstantsHolder.xanaConstants.userNameVisibilty);
        base.OnEnable();
    }

    public override void OnDisable()
    {
        if (!ConstantsHolder.xanaConstants.isXanaPartyWorld || !ConstantsHolder.xanaConstants.isBuilderGame)
        {
            ReactionDelegateButtonClickEvent -= OnChangeReactionIcon;
        }
        CommentDelegateButtonClickEvent -= OnChangeText;
        ConstantsHolder.userNameToggleDelegate -= OnChangeUsernameToggle;
        base.OnDisable();
    }

    private void OnChangeReactionIcon(string url)
    {
        if (!string.IsNullOrEmpty(url) && !MutiplayerController.instance.isShifting)
        {
            var photonView = gameObject.GetComponent<PhotonView>();
            if (photonView != null)
            {
                photonView.RPC("sendDataReactionUrl", RpcTarget.All, url, ReferencesForGamePlay.instance.m_34player.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    private void OnChangeUsernameToggle(int userNameToggleConstant)
    {
        if (!GetComponent<PhotonView>().IsMine) // calling for mine only
            return;
        if (userNameToggleConstant == 1)
        {
            PhotonUserName.enabled = true;
        }
        else
        {
            PhotonUserName.enabled = false;
        }
    }

    private void OnChangeText(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            var photonView = gameObject.GetComponent<PhotonView>();
            if (photonView != null)
            {
                photonView.RPC("sendDataChatMsg", RpcTarget.All, text, ReferencesForGamePlay.instance.m_34player.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    IEnumerator LoadSpriteEnv(string ImageUrl, int id)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            yield break;
        }

        if (gameObject.GetComponent<PhotonView>().ViewID == id)
        {
            using (WWW www = new WWW(ImageUrl))
            {
                while (!www.isDone)
                {
                    yield return null;
                }

                if (www.error != null)
                {
                    Debug.Log($"WWW download had an error: {www.error}");
                    yield break;
                }

                if (!string.IsNullOrEmpty(ImageUrl))
                {
                    UnityWebRequest www1 = UnityWebRequestTexture.GetTexture(ImageUrl);
                    yield return www1.SendWebRequest();

                    if (www1.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"UnityWebRequest error: {www1.error}");
                        yield break;
                    }

                    Texture2D thumbnailTexture = DownloadHandlerTexture.GetContent(www1);
                    Sprite sprite = Sprite.Create(thumbnailTexture, new Rect(0, 0, thumbnailTexture.width, thumbnailTexture.height), new Vector2(0, 0));
                    if (reactionUi != null)
                    {
                        reactionUi.SetActive(true);
                        reactionUi.GetComponent<Image>().sprite = sprite;

                        yield return new WaitForSeconds(5f);
                        PlayerPrefs.SetString(ConstantsGod.ReactionThumb, "");
                        reactionUi.SetActive(false);
                    }
                }
            }
        }
    }

    IEnumerator ChatShowData(string chatData, int id)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            yield break;
        }

        if (gameObject.GetComponent<PhotonView>().ViewID == id)
        {
            if (chatData.Length <= 20)
            {
                if (ChatShow != null)
                {
                    ChatShow.SetActive(true);
                    ChatShow.transform.GetChild(0).GetComponent<LayoutElement>().enabled = false;
                    ChatShow.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = chatData;

                    yield return new WaitForSeconds(5f);
                    PlayerPrefs.SetString(ConstantsGod.SENDMESSAGETEXT, "");
                    ChatShow.SetActive(false);
                }
            }
            else
            {
                if (ChatShow != null)
                {
                    ChatShow.SetActive(true);
                    ChatShow.transform.GetChild(0).GetComponent<LayoutElement>().enabled = true;
                    ChatShow.transform.GetChild(0).GetComponent<LayoutElement>().preferredWidth = 18;
                    ChatShow.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = chatData;

                    yield return new WaitForSeconds(5f);
                    PlayerPrefs.SetString(ConstantsGod.SENDMESSAGETEXT, "");
                    ChatShow.SetActive(false);
                }
            }
        }
    }

    [PunRPC]
    public void sendDataReactionUrl(string url, int viewId)
    {
        PlayerPrefs.SetString(ConstantsGod.SENDMESSAGETEXT, "");
        ChatShow.SetActive(false);
        if (temp != null)
        {
            StopCoroutine(temp);
        }
        temp = StartCoroutine(LoadSpriteEnv(url, viewId));
    }

    [PunRPC]
    public void sendDataUserNAmeToggle(int UserNameContantToggle, int viewId)
    {
        NameToggle(UserNameContantToggle, viewId);
    }

    public void NameToggle(int ToggleConstant, int id)
    {
        if (gameObject.GetComponent<PhotonView>().ViewID == id)
        {
            if (ToggleConstant == 1)
            {
                PhotonUserName.enabled = true;
            }
            else
            {
                PhotonUserName.enabled = false;
            }
        }
    }

    [PunRPC]
    public void sendDataChatMsg(string chat, int viewId)
    {
        PlayerPrefs.SetString(ConstantsGod.ReactionThumb, "");
        reactionUi.SetActive(false);
        if (chatco != null)
        {
            StopCoroutine(chatco);
        }
        chatco = StartCoroutine(ChatShowData(chat, viewId));
    }

    IEnumerator WaitForArrowIntanstiate(Transform parent, bool isOtherPlayer)
    {
        if (ConstantsHolder.xanaConstants.isXanaPartyWorld && isOtherPlayer)
            PhotonUserName.gameObject.SetActive(false);
        yield return new WaitForSeconds(1.0f);
        if (this.GetComponent<PhotonView>())
            InstantiateArrow(this.transform, !this.GetComponent<PhotonView>().IsMine);
        if (ConstantsHolder.xanaConstants.EnviornmentName.Contains("SaudiExpo"))
        {
            if (this.GetComponent<PhotonView>())
                InstantiateArrow(this.transform, !this.GetComponent<PhotonView>().IsMine);

        }
    }

    public void CallFirstPersonRPC(bool isFirstPerson)
    {
        this.GetComponent<PhotonView>().RPC("SendDataIfPlayerSetFirstPersonView", RpcTarget.Others, isFirstPerson, this.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    public void SendDataIfPlayerSetFirstPersonView(bool isFirstPerson, int viewId)
    {
    }

    public void InstantiateArrow(Transform parent, bool isOtherPlayer)
    {
        GameObject go = Instantiate(arrow, parent);
        go.layer = 17;
        if (isOtherPlayer)
        {
            // Set the player's name
            PhotonUserName.text = gameObject.GetComponent<PhotonView>().Owner.NickName;

            // Get the other player's position
            Transform otherPlayerTransform = gameObject.GetComponent<PhotonView>().transform;

            go.transform.localPosition = new Vector3(-1.6f, -1.46f, -50f);
            go.transform.localEulerAngles = new Vector3(-85, -113.1f, -65);
            go.transform.localScale = new Vector3(10.0f, 10f, 1);

            // Show the player's name if in Xana Party World
            if (ConstantsHolder.xanaConstants.isXanaPartyWorld)
            {
                PhotonUserName.gameObject.SetActive(true);
            }
        }
        else
        {
            if (ConstantsHolder.xanaConstants.EnviornmentName.Contains("SaudiExpo"))
            {
                go.transform.localPosition = new Vector3(-1.6f, -1.46f, -50f);
                go.transform.localEulerAngles = new Vector3(-85, -113.1f, -65);
                go.transform.localScale = new Vector3(10.0f, 10f, 1);
            }
            else
            {
                go.transform.localPosition = new Vector3(-1.6f, -1.46f, -50f);
                go.transform.localEulerAngles = new Vector3(-85, -113.1f, -65);
                go.transform.localScale = new Vector3(10.0f, 10f, 1);
            }
        }
        go.GetComponent<MeshRenderer>().material = playerMat;

        if (isBear)
        {
            go.SetActive(false);
        }

        if (ConstantsHolder.xanaConstants.IsMuseum && WorldItemView.m_EnvName.Contains("J & J WORLD_5"))
            go.SetActive(false);
        if (SoundController.Instance)
        {
            SoundController.Instance.PlayBGM();
        }
    }

    #region ToyotaMeetingArea
    public void UpdateMeetingTxt(string message)
    {
        this.GetComponent<PhotonView>().RPC("RemoteUpdateTxt", RpcTarget.AllBuffered, message);
    }

    [PunRPC]
    public void RemoteUpdateTxt(string message)
    {
        if (NFT_Holder_Manager.instance && NFT_Holder_Manager.instance.meetingTxtUpdate != null)
            NFT_Holder_Manager.instance.meetingTxtUpdate.UpdateMeetingTxt(message);
    }

    public void ChangeVoiceGroup(int ViewID, byte newGroup)
    {
        this.GetComponent<PhotonView>().RPC("RemoteChangeVoice", RpcTarget.All, ViewID, newGroup);
    }

    [PunRPC]
    public void RemoteChangeVoice(int ViewID, byte newGroup)
    {
        if (gameObject.GetComponent<PhotonView>().ViewID == ViewID)
            FindObjectOfType<VoiceManager>().SetVoiceGroup(newGroup);
    }
    #endregion
}
