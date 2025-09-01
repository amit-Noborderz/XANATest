using SuperStar.Helpers;
using System.Threading.Tasks;
using UnityEngine;

public class AdBoardController : MonoBehaviour
{
    public NFTContentHolder NFTDataHolder;
    public AdvertisementImageHolder AdvertisementImageHolder;

    public static AdBoardController AdBoardControllerInstance;


    // Start is called before the first frame update
    void OnEnable()
    {
        AdBoardControllerInstance = this;
        BuilderEventManager.AfterWorldOffcialWorldsInatantiated += GetAdBoardContent;
    }

    private void OnDisable()
    {
        BuilderEventManager.AfterWorldOffcialWorldsInatantiated -= GetAdBoardContent;
        //AdBoardTextures.Clear();
        //AdBoardTexturesPotrait.Clear();
    }

    async void GetAdBoardContent()
    {
        await NFTDataHolder.GetNFTDATA(ConstantsGod.GETDOMENFT, XANASummitDataContainer.DomeIdForSummit.ToString());
        for (int i = 0; i < NFTDataHolder.DomeNFTDataHolder.getcontentbyDomeId.Count; i++)
        {
            if (NFTDataHolder.DomeNFTDataHolder.getcontentbyDomeId[i].isAccessGiven == 0)
                return;

            if (NFTDataHolder.DomeNFTDataHolder.getcontentbyDomeId[i].type==2)
            {
                bool isPotrait = NFTDataHolder.DomeNFTDataHolder.getcontentbyDomeId[i].proportionType == "9:16";
                string thumbnailUrl = NFTDataHolder.DomeNFTDataHolder.getcontentbyDomeId[i].thumbnail + "?width=" + NFTDataHolder.DomeNFTDataHolder.width;
                int groupId = NFTDataHolder.DomeNFTDataHolder.getcontentbyDomeId[i].groupId;
                await DownloadThumbnail(groupId, thumbnailUrl, isPotrait);
            }
        }

        StartAdvertisement();
    }

    async Task DownloadThumbnail(int GroupId, string url, bool isPotrait)
    {
        if (!string.IsNullOrEmpty(url))
        {
            if (AssetCache.Instance.HasFile(url))
            {
                Texture2D TempImgae = AssetCache.Instance.LoadImage(url);
                AdvertisementImageHolder.AddDataInDictionary(GroupId, isPotrait, TempImgae);
            }
            else
            {
                AssetCache.Instance.EnqueueOneResAndWait(url, url, (success) =>
                {
                    if (success)
                    {
                        Texture2D TempImgae = AssetCache.Instance.LoadImage(url);
                        AdvertisementImageHolder.AddDataInDictionary(GroupId, isPotrait, TempImgae);
                    }
                });
            }
        }

        return;
    }

    async void StartAdvertisement()
    {
    InvokeAgain:
        BuilderEventManager.UpdateAdvertise?.Invoke();
        await Task.Delay(15000);
        goto InvokeAgain;
    }
}
