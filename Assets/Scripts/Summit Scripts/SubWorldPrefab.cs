using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.UI.ProceduralImage;
using static XANASummitDataContainer;

public class SubWorldPrefab : MonoBehaviour
{
    public Button SubWorldPrefabButton;
    public string WorldId;
    public int SubDomeId;
    public string SubWorldName;
    public string WorldDescription;
    public string CreatorName;
    public bool IsBuilderWorld;
    public string WorldCategory;
    public string subWorldType;
    public string WorldTimeEstimate;
    public string WorldDomeId;
    public string ThumbnailUrl;
    public Sprite WorldImage;
    public TMPro.TextMeshProUGUI WorldName;
    public Vector3 PlayerReturnPosition;
    //public OfficialWorldDetails subworlddata;
    // public bool IsBuilderWorld;
    public void Init()
    {
        StartCoroutine(DownloadTexture());
    }

    public void OnPrefabClicked()
    {
        ConstantsHolder.IsSubWorld = true;
        ConstantsHolder.SubDomeId = SubDomeId;
        SubWorldsHandler.OpenSubWorldDescriptionPanel?.Invoke(WorldImage,WorldId.ToString(),SubWorldName,WorldDescription,CreatorName,IsBuilderWorld,WorldCategory,WorldDomeId,PlayerReturnPosition,IsBuilderWorld,SubDomeId);
    }

    IEnumerator DownloadTexture()
    {
        
        ThumbnailUrl = ThumbnailUrl + "?width="+ConstantsHolder.DomeImageCompression;
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(ThumbnailUrl);
        request.SendWebRequest();
        while(!request.isDone)
        {
            yield return null;
        }
        Texture2D texture2D = DownloadHandlerTexture.GetContent(request);
        WorldImage = SubWorldPrefabButton.GetComponent<ProceduralImage>().sprite = ConvertToSprite(texture2D);
        ConstantsHolder.xanaConstants.haveSubDomeEnabled = true;
        ConstantsHolder.xanaConstants.summitSubDomeCatagory = WorldCategory;
        ConstantsHolder.xanaConstants.summitSubDomeType = subWorldType;
        SubWorldPrefabButton.interactable = true;


    }

    private Sprite ConvertToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

   
}
