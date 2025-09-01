using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/AdvertisementData", fileName = "AdvertisementData")]
public class AdvertisementImageHolder : ScriptableObject
{
    public Dictionary<int, AdBoardImageHolder> AdvertisementData = new Dictionary<int, AdBoardImageHolder>();
    //public Dictionary<int, int> PlaceHolderIdGroupIdMapping = new Dictionary<int,int>();
    public class AdBoardImageHolder
    {
        public int GroupId;
        public HashSet<int> ImagePlaceHolderIds=new HashSet<int>();
        public List<Texture2D> AdBoardTextures = new List<Texture2D>();
        public List<Texture2D> AdBoardTexturesPotrait = new List<Texture2D>();
    }

    public void AddDataInDictionary(int GroupId,bool IsPotrait,Texture2D texture2D)
    {
        AddImageToGroup(GroupId,IsPotrait,texture2D);
    }

    void UpdatePlacehodlderIdInGroup(int groupId,int PlaceHolderId)
    {
        if (!AdvertisementData.ContainsKey(groupId))
        {
            AdvertisementData.Add(groupId, new AdBoardImageHolder());
        }
    }

    void AddImageToGroup(int groupId, bool IsPotrait,Texture2D texture2D)
    {
        if(!AdvertisementData.ContainsKey(groupId))
        {
            AdvertisementData.Add(groupId,new AdBoardImageHolder());
        }
        if (IsPotrait)
            AdvertisementData[groupId].AdBoardTexturesPotrait.Add(texture2D);
        else
            AdvertisementData[groupId].AdBoardTextures.Add(texture2D);
    }


    //void MappingPlaceHolderWithGroupId(int groupId, int PlaceHolderId)
    //{
    //    if (PlaceHolderIdGroupIdMapping.ContainsKey(groupId))
    //        PlaceHolderIdGroupIdMapping[PlaceHolderId] = (groupId);
    //    else
    //        PlaceHolderIdGroupIdMapping.Add(PlaceHolderId, groupId);
    //}

    public Texture2D GetImageTexture(int GroupId,bool IsPotrait)
    {
        if(AdvertisementData.ContainsKey(GroupId))
        {
            if(IsPotrait)
            {
                if(AdvertisementData[GroupId].AdBoardTexturesPotrait.Count>0)
                {
                    int RandomAd = Random.Range(0, AdvertisementData[GroupId].AdBoardTexturesPotrait.Count);
                    return AdvertisementData[GroupId].AdBoardTexturesPotrait[RandomAd];
                }
            }
            else
            {
                if (AdvertisementData[GroupId].AdBoardTextures.Count > 0)
                {
                    int RandomAd = Random.Range(0, AdvertisementData[GroupId].AdBoardTextures.Count);
                    return AdvertisementData[GroupId].AdBoardTextures[RandomAd];
                }
            }
        }
        return null;
    }
}
