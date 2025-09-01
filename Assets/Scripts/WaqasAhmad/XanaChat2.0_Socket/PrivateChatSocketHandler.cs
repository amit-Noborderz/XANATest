using Newtonsoft.Json;
using Photon.Pun;
using Photon.Voice.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PrivateChatSocketHandler : MonoBehaviour
{
    private string PrivateGroupCreationAPI
    {
        get
        {
            if (APIBasepointManager.instance.IsXanaLive)
            {
                return "https://chat-prod.xana.net/api/v1/initiate-group-chat";
            }
            else
            {
                return "https://chat-testing.xana.net/api/v1/initiate-group-chat";
            }
        }
    }

    private ChatSocketManager chatSocketManager;

    public GroupChatMsgReceivedData receivedMsgForTesting;

    private int myUserId;
    private int chatRequesterId;
    private string receiverId = "";
    private string invitationReceivedForGroupId = "";
    private string myGroupId = ""; //we be assign on acceppting the group or creating the group

    //Setting bools to handle same event multiple times
    //Sometimes same response received multiple times
    private bool receivechatMsgOnce = false;
    private bool receiveMemberListOnce = false;

    private bool isAlreadyInGroup = false;
    private bool isChatInitialized = false;
    private bool hasPendingRequest = false;

    private void Start()
    {
        chatSocketManager = ChatSocketManager.instance;
        myUserId = ConstantsHolder.userId.ParseToInt();
    }
   
    private void OnEnable()
    {
        //ChatSocketManager.OnSocketConnected += OnConnectedToPrivateChatSocket;
        XanaPrivateChat.OnInvitePrivateChatBtnClicked += InitializePrivateChat;
        XanaPrivateChat.OnChatInvitaionAccepted += AcceptedChatInvitation;
        XanaPrivateChat.OnChatInvitaionRejected += RejectChatRequest;
        XanaPrivateChat.OnSendPrivateMsg += SendPrivateChatMessage;
        XanaPrivateChat.OnLeavePrivateChat += LeavePrivateChat;
    }
    private void OnDisable()
    {
        //ChatSocketManager.OnSocketConnected -= OnConnectedToPrivateChatSocket;
        XanaPrivateChat.OnInvitePrivateChatBtnClicked -= InitializePrivateChat;
        XanaPrivateChat.OnChatInvitaionAccepted -= AcceptedChatInvitation;
        XanaPrivateChat.OnChatInvitaionRejected -= RejectChatRequest;
        XanaPrivateChat.OnSendPrivateMsg -= SendPrivateChatMessage;
        XanaPrivateChat.OnLeavePrivateChat -= LeavePrivateChat;
        LeavePrivateChat();
    }
    public void OnConnectedToPrivateChatSocket()
    {
        Debug.Log("Connected to Private Chat Socket");
        var _data = new
        {
            userId = myUserId
        };
        Debug.Log("Emitting chatGrpMemberJoined _data: " + _data);
        chatSocketManager.Manager.Socket.Emit("chatGrpMemberJoined", _data);
        chatSocketManager.Manager.Socket.On<string>("latestGrpMemberList", OnGroupMemberListUpdated);
        chatSocketManager.Manager.Socket.On<string>("userMsgFromFriend", OnGroupChatMsgReceived);
        chatSocketManager.Manager.Socket.On<string>("getChatInviteReq", OnChatInvitaionReceived);
        chatSocketManager.Manager.Socket.On<string>("notifyChatRequestSentSuccess", OnChatInvitaionSentSuccessfully);
        chatSocketManager.Manager.Socket.On<string>("notifyChatRequestRejection", OnChatInvitaionRejection);
        //load old msgs
    }
    private void OnChatInvitaionSentSuccessfully(string response)
    {
        Debug.Log("Chat Request Sent Successfully " + response);
        PrivateChatRequestDetails _chatRequestReceiverDetails = JsonUtility.FromJson<PrivateChatRequestDetails>(response);
        string toastMessage = TextLocalization.GetLocaliseTextByKey("Invited to a private chat. Waiting for a response.");
        XanaChatSystem.instance.xanaPrivateChat.ShowToast(_chatRequestReceiverDetails.userId, toastMessage);
    }
    private void OnChatInvitaionRejection(string response)
    {
        Debug.Log("Chat Request Rejected " + response);
        PrivateChatRequestDetails _chatRequestRejecterDetails = JsonUtility.FromJson<PrivateChatRequestDetails>(response);
        string toastMessage = TextLocalization.GetLocaliseTextByKey("Private chat invitation declined.");
        XanaChatSystem.instance.xanaPrivateChat.ShowToast(_chatRequestRejecterDetails.userId,toastMessage);
    }
    private void InitializePrivateChat(string _receiverId)
    {
        Debug.Log("Initialize Private Chat with: " + _receiverId);
        receiverId = _receiverId;
        if (string.IsNullOrEmpty(myGroupId))
        {
            //if group is not created yet
            StartCoroutine(RequestCreatePrivateChatGroup(_receiverId));
        }
        else
        {
            SendInvitationToJoin(_receiverId);
        }
    }

    private IEnumerator RequestCreatePrivateChatGroup(string _receiverId)
    {
        using (UnityWebRequest _request = UnityWebRequest.Post(PrivateGroupCreationAPI,""))
        {
            var requestData = new { userId = myUserId }; 
            string jsonData = JsonConvert.SerializeObject(requestData); 
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData); 
            _request.uploadHandler = new UploadHandlerRaw(bodyRaw); 
            _request.SetRequestHeader("Content-Type", "application/json");
            _request.SendWebRequest();
            while (!_request.isDone)
                yield return null;
            if (_request.result == UnityWebRequest.Result.ConnectionError || _request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error in Posting Private Chat API: " + _request.error);
            }
            else
            {
                string _jsonResponse = _request.downloadHandler.text;
                Debug.Log("Private Chat Group Creation Response: " + _jsonResponse);
                ApiResponse _apiResponse = JsonConvert.DeserializeObject<ApiResponse>(_jsonResponse);
                if (_apiResponse.success)
                {
                    if (_apiResponse.data == null)
                        yield return null;

                    if (!_apiResponse.data.isExists)
                    {
                        if (_apiResponse.data.data.id != null)
                        {
                            myGroupId = _apiResponse.data.data.id;
                            Debug.Log("Private Chat Group Created using API: " + myGroupId);
                            SendInvitationToJoin(_receiverId);
                        }
                        else
                        {
                            Debug.LogError("Private Chat Group ID is null");
                        }

                    }
                }
                else
                {
                    if (_apiResponse.data.isExists)
                    {
                        myGroupId = _apiResponse.data.data.id;
                        SendInvitationToJoin(_receiverId);
                    }
                }
            }
        }
    }
    private void SendInvitationToJoin(string _receiverId)
    {
        if (myGroupId.IsNullOrEmpty())
        {
            //Debug.LogError("SendInvitationToJoin Failed: Group ID is null");
            return;
        }
        var _data = new
        {
            groupId = myGroupId,
            requesterId = myUserId,
            receiverIds = new[] { _receiverId }
        };
        chatSocketManager.Manager.Socket.Emit("inviteMemberInChatGrp", _data);
        //XanaPrivateChat.OnPrivateChatInitialized?.Invoke();

    }
    private void OnChatInvitaionReceived(string _resp)
    {
        if (hasPendingRequest) 
            return;
        //Debug.Log("Private Chat Invitation received Response: " + _resp);
        ChatInvitationReceivedData _data = JsonUtility.FromJson<ChatInvitationReceivedData>(_resp);

        if (_data.groupId == invitationReceivedForGroupId || _data.groupId == myGroupId)
        {
            return;
        }

        foreach (string _receiverid in _data.receiverIds)
        {
            if (_receiverid == myUserId.ToString())
            {
                //Debug.Log("Got Private Chat Invitation for Group: " + _data.groupId);
                invitationReceivedForGroupId = _data.groupId;
                hasPendingRequest = true;
                if (myGroupId.IsNullOrEmpty())
                {
                    isAlreadyInGroup = false;
                    XanaPrivateChat.OnChatInvitaionReceived?.Invoke(_data, false);
                }
                else
                {
                    isAlreadyInGroup = true;
                    XanaPrivateChat.OnChatInvitaionReceived?.Invoke(_data, true);
                }
                chatRequesterId = _data.requesterId.ParseToInt();
                var requestSuccessData = new
                {
                    requesterId = chatRequesterId,
                    userId = _receiverid.ParseToInt()
                };
                chatSocketManager.Manager.Socket.Emit("chatRequestSentSuccess", requestSuccessData);
                Debug.Log("Chat Request Received Success " + requestSuccessData);
            }
        }
    }

    private void AcceptedChatInvitation()
    {
        //Debug.Log("Accepted Chat Invitation");
        //Debug.Log("Accepting Chat Invitation for Group: " + invitationReceivedForGroupId);
        //Debug.Log("accepterId: " + myUserId);

        if (isAlreadyInGroup)
        {
            LeavePrivateChat();
        }

        var _data = new
        {
            groupId = invitationReceivedForGroupId.ParseToInt(),
            accepterId = myUserId //check
        };
     
        chatSocketManager.Manager.Socket.Emit("acceptGrpChatInvite", _data);
        myGroupId = invitationReceivedForGroupId;
        invitationReceivedForGroupId = "";
        hasPendingRequest = false;
        //get old msgs if needed
    }

    private void RejectChatRequest()
    {
        var requestRejectData = new
        {
            requesterId = chatRequesterId,
            userId = myUserId
        };
        chatSocketManager.Manager.Socket.Emit("rejectChatRequest", requestRejectData);
        Debug.Log("Chat Request Rejected " + requestRejectData);
        invitationReceivedForGroupId = "";
        hasPendingRequest = false;
    }
    private void OnGroupMemberListUpdated(string _resp)
    {
        string keyToLocalize;
        //Debug.Log("Private Chat Group Member List Updated: " + _resp);
        if (receiveMemberListOnce)
            return;
        receiveMemberListOnce = true;
        PrivateGroupData _data = JsonUtility.FromJson<PrivateGroupData>(_resp);

        if (_data.memberIds.Contains(myUserId))
            myGroupId = _data.groupId.ToString();
        else
            return;

        //Debug.Log("Its my Private Chat group: " + myGroupId);
        if (_data.isSomeoneJoined)
        {
            //For Starting a chat                                                                                             //For Joining a chat
            if ((_data.memberIds.Length == 2 && _data.updateUser.id.ToString() == receiverId && !isChatInitialized) || _data.updateUser.id == myUserId)
            {
                //showchat
                XanaPrivateChat.OnPrivateChatInitialized?.Invoke();
                isChatInitialized = true;
            }
            keyToLocalize =TextLocalization.GetLocaliseTextByKey("has joined the chat.");
            chatSocketManager.AddNewMsg(_data.updateUser.name, keyToLocalize, "", _data.updateUser.id.ToString(), 0);
            XanaVoiceChat.instance.SetVoiceGroup((byte)int.Parse(GetValidGroupId(myGroupId)));
            XanaChatSystem.instance.xanaPrivateChat.CloseToastMessage();
            GamePlayUIHandler.inst.tapInfoPanel.SetActive(false);
        }
        else if (!_data.isSomeoneJoined)
        {
            keyToLocalize = TextLocalization.GetLocaliseTextByKey("has left the chat.");
            chatSocketManager.AddNewMsg(_data.updateUser.name, keyToLocalize, "", _data.updateUser.id.ToString(), 0);
           
        }

        new Delayed.Action(() => { receiveMemberListOnce = false; }, 0.2f);

    }

    public void LeavePrivateChat()
    {
        //Debug.Log("Leaving Private Chat");
        //Debug.Log("Leaving Private Chat for Group: " + myGroupId);
        //Debug.Log("Leaving Private Chat for User: " + myUserId);
        isChatInitialized = false;
        if (myGroupId.IsNullOrEmpty())
        {
            //Debug.LogError("Private Chat Group ID is null");
            return;
        }

        var _data = new
        {
            groupId = myGroupId.ParseToInt(),
            leavingUserId = myUserId
        };
        XanaVoiceChat.instance.SetVoiceGroup(1);
        chatSocketManager.Manager.Socket.Emit("memberLeaveChatGrp", _data);
        myGroupId = "";
        //Commented invitationReceivedForGroupId due to get null groupid from 3rd user
        //invitationReceivedForGroupId = "";
        hasPendingRequest = false;
       
    }

    private string GetValidGroupId(string groupId) 
    { 
        int groupIdInt = int.Parse(groupId); 
        if (groupIdInt > 255) 
        { 
            return groupId.Substring(0, 2); 
        } 
        return groupId; 
    }

    #region Chat Messages
    public void SendPrivateChatMessage(string _msgText)
    {
       if (_msgText.All(c => char.IsWhiteSpace(c)) || string.IsNullOrEmpty(_msgText))
        {
            return;
        }

        var _data = new
        {
            msg = _msgText,
            groupId = myGroupId.ParseToInt(),
            msgType = "text",
            senderId = new int[] { myUserId }
        };
        chatSocketManager.Manager.Socket.Emit("grpChatMsgIn", _data);
    }

    private void OnGroupChatMsgReceived(string resp)
    {
        //Debug.Log("Private Chat Message Received: " + resp);
        if (receivechatMsgOnce)
            return;

        receivechatMsgOnce = true;
        GroupChatMsgReceivedData msg = JsonUtility.FromJson<GroupChatMsgReceivedData>(resp);

        if (msg.groupId == myGroupId.ParseToInt())
        {
            //Debug.Log("Private Chat Message Received For Me: " + msg.msg);
            //Debug.Log("Private Chat by : " + msg.senderData.name + " : " + msg.msg);

            string tempUser = msg.senderData.name;

            receivedMsgForTesting = msg;

            if (CheckUserNameIsValid(tempUser))
            {
                tempUser = "XanaUser-(" + msg.senderData.id + ")";//XanaUser-(userId)
            }
            chatSocketManager.AddNewMsg(tempUser, msg.msg, msg.id.ToString(), msg.senderData.id.ToString(), 0);
        }
        new Delayed.Action(() => { receivechatMsgOnce = false; }, 0.2f);
    }

    bool CheckUserNameIsValid(string _UserName)
    {
        if (string.IsNullOrEmpty(_UserName) ||
            _UserName.All(c => char.IsWhiteSpace(c)) ||
            _UserName.Contains("null") ||
            _UserName.Contains("Null"))
            return true;
        else
            return false;
    }
    private void OnApplicationQuit()
    {
        LeavePrivateChat();
    }
    #endregion

    #region Serialize Classes
    [Serializable]
    private class PrivateGroupData
    {
        public int groupId;
        public int[] memberIds;
        public bool isSomeoneJoined;
        public MemberData updateUser;
    }

    [Serializable]
    public class GroupChatMsgReceivedData
    {
        public int id;
        public int groupId;
        public string msg;
        public string msgType;
        public int senderId;
        public string attachmentUrl;
        public MemberData senderData;
    }
    [Serializable] public class UserProfile 
    { 
        public string username;
        public string bio; 
    }
    [Serializable] public class MemberData 
    { 
        public int id; 
        public string name; 
        public string avatar; 
        public string profileIconColor; 
        public UserProfile userProfile; 
    }
    [Serializable] public class ChatInvitationReceivedData 
    {
        public string groupId; 
        public List<string> receiverIds;
        public string requesterId; 
        public List<MemberData> membersData;
    }

    //{"groupId":"6","receiverIds":["17060"],"requesterId":"17198","membersData":[{"id":17198,"name":"Annye","avatar":"https://cdn.xana.net/apitestxana/Defaults/1732883542671--A-DefaultUserProfil.jpg","profileIconColor":"hsl(200, 40.24868482404733%, 67.38623158080053%)","userProfile":{"username":null,"bio":null}}]}
    [Serializable]
    public class ApiResponse
    {
        public bool success;
        public GroupStatus data;
        public string msg;
    }

    [Serializable]
    public class GroupStatus
    {
        public bool isExists;
        public GroupData data;
    }

    [Serializable]
    public class GroupData
    {
        public string id;
        public string[] memberIds;
        public bool isActive;
        public DateTime createdAt;
        public DateTime updatedAt;
    }
    public class PrivateChatRequestDetails
    {
        public int requesterId;
        public int userId;
    }
    #endregion
    //{"groupId":"6","receiverIds":["17060"],"requesterId":"17198","membersData":[{"id":17198,"name":"Annye","avatar":"https://cdn.xana.net/apitestxana/Defaults/1732883542671--A-DefaultUserProfil.jpg","profileIconColor":"hsl(200, 40.24868482404733%, 67.38623158080053%)","userProfile":{"username":null,"bio":null}}]}
    //Give a serializeable class for Unity c# according to this json

}
