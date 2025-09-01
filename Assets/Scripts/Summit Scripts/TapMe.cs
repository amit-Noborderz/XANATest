using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class TapMe : MonoBehaviour
{
    public string UserServerId { get { return myServerId; } }

    private string myServerId;
    private PhotonView photonView;
    private GameObject infoPanel;

    private float lastTapTime = 0f;
    private const float doubleTapThreshold = 0.3f; // Time in seconds
    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        SetUserID();
        StartCoroutine(FetchFeatures());

        if (GamePlayUIHandler.inst)
            infoPanel = GamePlayUIHandler.inst.tapInfoPanel;
    }
    public void SetUserID()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "UserID", ConstantsHolder.userId } });
            myServerId = ConstantsHolder.userId;
        }
        else
        {
            photonView.Owner.CustomProperties.TryGetValue("UserID", out object userId);
            myServerId = (string)userId;
        }
    }

    void OnMouseDown()
    {
        float currentTime = Time.time;
        if (currentTime - lastTapTime < doubleTapThreshold)
        {
            // Double-tap detected
            Debug.Log("Double-tapped on: " + gameObject.name + " " + myServerId + "  " + gameObject.GetComponent<ArrowManager>().PhotonUserName.text);

            if (infoPanel == null)
                infoPanel = GamePlayUIHandler.inst.tapInfoPanel;
            if (PlayerCameraController.IsPointerOverUIObject()) return;
            foreach (var item in ReferencesForGamePlay.instance.OverlayPanelsList)
            {
                if (item.activeInHierarchy)
                    return;
            }
            GamePlayUIHandler.inst.ClickedPlayerActrNumber = gameObject.GetComponent<PhotonView>().OwnerActorNr;
            if (GamePlayUIHandler.inst && !GamePlayUIHandler.inst.tapInfoPanel.activeInHierarchy)
                GamePlayUIHandler.inst.ShowInfoPanel(myServerId, gameObject.GetComponent<ArrowManager>().PhotonUserName.text);
        }
        else
        {
            // Single tap detected, update the last tap time
            lastTapTime = currentTime;
        }
    }
    IEnumerator FetchFeatures()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(ConstantsGod.API_BASEURL + ConstantsGod.FeaturesListApi + "?app_name=" + Application.productName + "&version=" + Application.version))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", ConstantsGod.AUTH_TOKEN);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error fetching features: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

                ConstantsHolder variables = ConstantsHolder.xanaConstants;
                Dictionary<string, Action<bool>> featureMapping = new Dictionary<string, Action<bool>>()
                {
                    { "tapToKnow_organization", value => variables.tapToKnow_organization = value },
                    { "tapToKnow_profession", value => variables.tapToKnow_profession = value },
                    { "tapToKnow_industry", value => variables.tapToKnow_industry = value },
                    { "tapToKnow_email", value => variables.tapToKnow_email = value },
                    { "tapToKnow_contact", value => variables.tapToKnow_contact = value },
                    { "tapToKnow_join", value => variables.tapToKnow_join = value },
                    { "tapToKnow_hobbies", value => variables.tapToKnow_hobbies = value },
                    { "tapToKnow_status", value => variables.tapToKnow_status = value }
                };

                if (apiResponse.success && apiResponse.data != null)
                {
                    foreach (var data in apiResponse.data)
                    {
                        foreach (var key in featureMapping.Keys)
                        {
                            if (data.feature_list.TryGetValue(key, out bool status))
                            {
                                featureMapping[key](status);
                            }
                            else
                            {
                                featureMapping[key](true); // Default to true if key is not found
                            }
                        }
                    }
                }
                else
                {
                    foreach (var key in featureMapping.Keys)
                    {
                        featureMapping[key](true); // Default to true if key is not found
                    }
                }
            }
        }
    }

}
