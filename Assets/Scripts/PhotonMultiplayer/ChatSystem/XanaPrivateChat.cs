using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using TMPro;
using UnityEngine.Networking;
using DG.Tweening;
using System.Linq;
using EnhancedScrollerDemos.Chat;

public class XanaPrivateChat : MonoBehaviour
{
    public static UnityAction OnPrivateChatInitialized;
    public static UnityAction<string> OnInvitePrivateChatBtnClicked;
    public static UnityAction OnLeavePrivateChat;
    public static UnityAction<PrivateChatSocketHandler.ChatInvitationReceivedData, bool> OnChatInvitaionReceived;
    public static UnityAction OnChatInvitaionAccepted;
    public static UnityAction OnChatInvitaionRejected;
    public static UnityAction<string> OnSendPrivateMsg;
    public static bool IsPrivateChatActive { get; set; }

    [SerializeField] private GameObject PrivateChatHeader;
    [SerializeField] private Button SendInviteButton;

    [Space(20)]
    [Header("Invitation Received Panel Elements")]
    [SerializeField] private RectTransform ChatInvitationPanel;
    [SerializeField] private RectTransform timerImageRect;
    [SerializeField] private Image[] groupMemberIcons;
    [SerializeField] private Sprite defaultIconSprite;
    [SerializeField] private TextMeshProUGUI invitaionFromText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private int requestTimeDuration = 60;
    private string privateChatRequesterUserName;
    public string PrivateChatRequesterId;

    [Space(20)]
    [Header("Private Chat Response Elements")]
    private int _maxTriesToFetchUserDetails = 3;
    [SerializeField] private RectTransform ChatResponsePanel;
    [SerializeField] private Image userProfileIcon;
    [SerializeField] private TextMeshProUGUI DescriptionText;
    [SerializeField] private Button CrossButton;
    private int timer = 0;
    private string receiverId;
    public bool isConditionInviteBtn;  //for disable invite btn of reciever if invite panel activate

    private void Start()
    {
        SendInviteButton.onClick.AddListener(SendInviteBtn);
        StartCoroutine(CheckConditionCoroutine());
    }

    private void OnEnable()
    {
        SendInviteButton.interactable = true;
        OnPrivateChatInitialized += EnablePrivateChatUI;
        OnLeavePrivateChat += DisablePrivateChatUI;
        OnChatInvitaionReceived += ChatInvitaionReceived;
    }
    private void OnDisable()
    {
        OnPrivateChatInitialized -= EnablePrivateChatUI;
        OnLeavePrivateChat -= DisablePrivateChatUI;
        OnChatInvitaionReceived -= ChatInvitaionReceived;
        DisableInvitationPanel();
        //OnChatInvitaionRejected?.Invoke();
    }

    public void SetReceiverId(string _receiverId)
    {
        receiverId = _receiverId;
        CheckToEnableInviteButton();
    }

    private void CheckToEnableInviteButton()
    {
        if (receiverId.IsNullOrEmpty())
            return;

        //if its me or guest disable invite button
        if (receiverId == ConstantsHolder.userId)
        {
            SendInviteButton.gameObject.SetActive(false);
        }
        else
        {
            SendInviteButton.gameObject.SetActive(true);
        }
    }

    public void SendInviteBtn()
    {
        SendInviteButton.interactable = false;

        if (OnInvitePrivateChatBtnClicked is null)
        {
            Debug.LogError("Private Chat OnInvitePrivateChatBtnClicked is empty");
        }
        OnInvitePrivateChatBtnClicked?.Invoke(receiverId);
        StartCoroutine(ResetInvitaionBtnRoutine());
    }
    private IEnumerator ResetInvitaionBtnRoutine()
    {
        yield return new WaitForSeconds(2f); //requestTimeDuration
        SendInviteButton.interactable = true;
    }
    public void ShowToast(int _otherUserId,string _toastMessage)
    {
        if (receiverId != null && receiverId.ParseToInt() == _otherUserId)
        {
            SetUpToastMeaasage(GamePlayUIHandler.inst.tapInfo_UserAvatar, GamePlayUIHandler.inst.tapInfo_Name.text, _toastMessage);
        }
        else
        {
            StartCoroutine(GetUserDetails(_otherUserId.ToString(), _toastMessage));
        }
    }
    IEnumerator GetUserDetails(string UserId,string _toastMessage)
    {
        Debug.Log("Here At User Details");
        string url = ConstantsGod.API_BASEURL + ConstantsGod.GetUserDetailsAPI + "?userId=" + UserId;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            UserData requireData = new UserData();
            if (request.result != UnityWebRequest.Result.ConnectionError && request.result == UnityWebRequest.Result.Success)
            {
                UserDetailsRoot root = JsonUtility.FromJson<UserDetailsRoot>(request.downloadHandler.text);
                requireData = root.data;

                if (request.error == null)
                {
                    if (root.success)
                    {
                        Debug.Log("API Success to Load ");
                        SetUpToastMeaasage(requireData.avatar, requireData.name,_toastMessage);
                    }
                }
                _maxTriesToFetchUserDetails = 3;
            }
            else
            {
                Debug.Log("API Fail to Load " + request.downloadHandler.text);
                if (_maxTriesToFetchUserDetails > 0)
                {
                    _maxTriesToFetchUserDetails--;
                    StartCoroutine(GetUserDetails(UserId,_toastMessage));
                }
            }
        }
    }
    private void SetUpToastMeaasage(string _profileIconUrl, string _userName, string _toastMessaage)
    {
        SetMemberProfileSprite(userProfileIcon, _profileIconUrl);
        DescriptionText.text = _userName + ": " + _toastMessaage;
        OpenToastMessage();
    }
    private void OpenToastMessage()
    {
        if (ChatResponsePanel.gameObject.activeInHierarchy)
        {
            ChatResponsePanel.DOAnchorPos(new Vector2(ChatResponsePanel.anchoredPosition.x, 45f), 0.5f).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    ChatResponsePanel.DOAnchorPos(new Vector2(ChatResponsePanel.anchoredPosition.x, -113f), 0.5f).SetEase(Ease.InOutSine);
                });
        }
        else
        {
            ChatResponsePanel.gameObject.SetActive(true);
            ChatResponsePanel.DOAnchorPos(new Vector2(ChatResponsePanel.anchoredPosition.x, -113f), 0.5f).SetEase(Ease.InOutSine);
        }
        CrossButton.onClick.AddListener(() =>
        {
            CloseToastMessage();
        });
    }
    public void CloseToastMessage()
    {
        if (ChatResponsePanel.gameObject.activeInHierarchy)
        {
            ChatResponsePanel.DOAnchorPos(new Vector2(ChatResponsePanel.anchoredPosition.x, 45f), 0.5f).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    ChatResponsePanel.gameObject.SetActive(false);
                    DescriptionText.text = "";
                });
        }
    }
    public void LeavePrivateChatBtn()
    {
        IsPrivateChatActive = false;
        OnLeavePrivateChat?.Invoke();
        //DisablePrivateChatUI();
    }
    private void EnablePrivateChatUI()
    {
        IsPrivateChatActive = true;
        GamePlayUIHandler.inst.tapInfoPanel.SetActive(false);
        PlayerCameraController.instance.isReturn = false;
        PrivateChatHeader.SetActive(true);
        XanaChatSystem.instance.OpenCloseChatDialog(true);
    }

    private void DisablePrivateChatUI()
    {
        PrivateChatHeader.SetActive(false);
        XanaChatSystem.instance.OpenCloseChatDialog(false);
    }

    private Coroutine invitationPanelDisableRoutine;
    private void ChatInvitaionReceived(PrivateChatSocketHandler.ChatInvitationReceivedData _data, bool _isAlreadyInGroup)
    {
        timer = requestTimeDuration;
        SetupinvitaionPanel(_data, _isAlreadyInGroup);
        invitationPanelDisableRoutine = StartCoroutine(InvitaionPanelDisableRoutine());
        isConditionInviteBtn = true;
        PrivateChatRequesterId = _data.requesterId;

    }

    Tweener tweener;
    private void SetupinvitaionPanel(PrivateChatSocketHandler.ChatInvitationReceivedData _data, bool _isAlreadyInGroup)
    {
        ChatInvitationPanel.gameObject.SetActive(true);
        ChatInvitationPanel.DOAnchorPos(new Vector2(ChatInvitationPanel.anchoredPosition.x, -113f), 0.5f).SetEase(Ease.InOutSine);
        tweener = timerImageRect.DORotate(new Vector3(0, 0, 360f), 2, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        string keyToLocalize;
        int totalMembers = Mathf.Min(_data.membersData.Count, 5);

        for (int i = 0; i < totalMembers; i++)
        {
            groupMemberIcons[i].transform.parent.gameObject.SetActive(true);
            var member = _data.membersData[i];
            SetMemberProfileSprite(groupMemberIcons[i], member.avatar);
        }
        //Setting data to the panel
        if (totalMembers > 1)
        {
            int i = 0;
            foreach (var _member in _data.membersData)
            {
                if (_member.id.ToString() == _data.requesterId)
                {
                    keyToLocalize = TextLocalization.GetLocaliseTextByKey("and others are inviting you to a Private Group Chat");
                    privateChatRequesterUserName = _member.name;
                    invitaionFromText.text = privateChatRequesterUserName + " " + keyToLocalize;
                    // SetMemberProfileSprite(groupMemberIcons[^1].gameObject.GetComponent<Image>(), _member.avatar);
                }
            }
        }
        else
        {
            keyToLocalize = TextLocalization.GetLocaliseTextByKey("is inviting you to a Private Group Chat");
            privateChatRequesterUserName = _data.membersData[0].name;
            invitaionFromText.text = privateChatRequesterUserName + " " + keyToLocalize;

            // SetMemberProfileSprite(groupMemberIcons[^1].gameObject.GetComponent<Image>(), _data.membersData[0].avatar);
        }
    }
    public void RequesterLeftRoom()
    {
        if (ChatInvitationPanel.gameObject.activeInHierarchy)
        {
            InvitaionRejectBtn();
            ChatInvitationPanel.DOAnchorPos(new Vector2(ChatInvitationPanel.anchoredPosition.x, 45f), 0.5f).SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                ChatInvitationPanel.gameObject.SetActive(false);
                ResetInvitaionPanel();
                userProfileIcon.sprite = groupMemberIcons[0].sprite;
                string s1 = TextLocalization.GetLocaliseTextByKey("Invitation Cancled");
                string s2 = TextLocalization.GetLocaliseTextByKey("has left the world");
                DescriptionText.text = s1 + ": " + privateChatRequesterUserName +" "+ s2;
                OpenToastMessage();
            });
        }
    }
    private IEnumerator LoadUserProfileImage(Image profileImage, string url)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                profileImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
        }
        yield return null;
    }

    private void SetMemberProfileSprite(Image profileImage, string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            StartCoroutine(LoadUserProfileImage(profileImage, url));
        }
        else
        {
            profileImage.sprite = defaultIconSprite;
        }
    }


    private IEnumerator InvitaionPanelDisableRoutine()
    {
        while (timer > 0)
        {
            timerText.text = timer.ToString();
            timer--;
            yield return new WaitForSeconds(1f);
        }

        timerText.text = "0";
        DisableInvitationPanel();
        OnChatInvitaionRejected?.Invoke();
    }

    public void InvitaionAcceptBtn()
    {
        DisableInvitationPanel();
        OnChatInvitaionAccepted?.Invoke();
        if (!SendInviteButton.gameObject.activeInHierarchy)
        {
            SendInviteButton.gameObject.SetActive(true);
        }
        isConditionInviteBtn = false;
    }

    public void InvitaionRejectBtn()
    {
        DisableInvitationPanel();
        OnChatInvitaionRejected?.Invoke();
        if (!SendInviteButton.gameObject.activeInHierarchy)
        {
            SendInviteButton.gameObject.SetActive(true);
        }
        isConditionInviteBtn = false;
    }

    private void DisableInvitationPanel()
    {
        ChatInvitationPanel.DOAnchorPos(new Vector2(ChatInvitationPanel.anchoredPosition.x, 45f), 0.5f).SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                ChatInvitationPanel.gameObject.SetActive(false);
                ResetInvitaionPanel();
            });
    }
    private void ResetInvitaionPanel()
    {
        StopCoroutine(invitationPanelDisableRoutine);
        tweener.Rewind();
        tweener.Kill();
        timer = requestTimeDuration;
        timerText.text = timer.ToString();
        foreach (Image _image in groupMemberIcons)
        {
            _image.transform.parent.gameObject.SetActive(false);
        }
    }
    private IEnumerator CheckConditionCoroutine()
    {
        while (true)
        { // Update the button state based on the boolean value
            UpdateButtonState();
            // Wait for the specified interval before checking again
            yield return new WaitForSeconds(0.5f);
        }
    }
    public void UpdateButtonState()
    {
        if (isConditionInviteBtn)
        {
            SendInviteButton.gameObject.SetActive(false); // Disable the button GameObject
        }
        
    }
}