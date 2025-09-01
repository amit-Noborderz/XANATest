using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BD
{
    public class NFTManager : MonoBehaviour
    {
        private static Dictionary<string, NFTCharacter> nftCharacterMap = new();

        private static readonly string NFT_BASE_URL_MAINNET = "https://prod-backend.xanalia.com/nfts/";
        private static readonly string NFT_BASE_URL_TESTNET = "https://test-backend.xanalia.com/nfts/";
        private static readonly string NFT_COLLECTION_URL = "specific-collection-owned-nfts-by-useraddress?collectionAddress=0x6c019F2Df0b5A7D80f0429324c8c4BCB28eD6170&userAddress={UserWalletAddress}&network=Ethereum";

        public static async Task GetNFTs(string userWalletAddress)
        {
            //string url = APIManager.IsMainNet ? NFT_BASE_URL_MAINNET : NFT_BASE_URL_TESTNET + NFT_COLLECTION_URL.Replace("{UserWalletAddress}", userWalletAddress);
            //Debug.Log("NFT_COLLECTION_URL: " + url);

            // For testing. Testnet using a static URL.
            string url;
            if (APIManager.IsMainNet)
            {
                url = NFT_BASE_URL_MAINNET + NFT_COLLECTION_URL.Replace("{UserWalletAddress}", userWalletAddress);
            }
            else
            {
                url = "https://prod-backend.xanalia.com/nfts/specific-collection-owned-nfts-by-useraddress?collectionAddress=0x6c019F2Df0b5A7D80f0429324c8c4BCB28eD6170&userAddress=0xa779febb5caf8a688ce783caa44ddb4468396904&network=Ethereum";
            }
            //

            using UnityWebRequest webRequest = UnityWebRequest.Get(url);

            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("<color=cyan>NFTManager:</color> GetNFTs() " + webRequest.downloadHandler.text);
                var NFTResponse = JsonUtility.FromJson<NFTResponse>(webRequest.downloadHandler.text);

                //PlayerStats.PlayerCharacters.Clear();

                /*   if (NFTResponse.list == null || NFTResponse.list.Length == 0)
                   {
                       Debug.LogWarning("<color=cyan>NFTManager:</color> GetNFTs(): No NFTs found for this user. Enabling default character.");
                       //UnlockOneCharacter();
                       UnlockAllCharacters();
                       return;
                   }

                   foreach (var nft in NFTResponse.list)
                   {
                       //Debug.Log("NFT: " + nft.name);

                       NFTData data = NFTDataLoader.Instance.GetNFTData(nft.tokenId);
                       if (data != null)
                       {
                           NFTCharacter nftCharacter = CreateNFTCharacter(data);
                           nftCharacterMap[nft.tokenId] = nftCharacter;
                           PlayerStats.PlayerCharacters.Add(nftCharacter);
                       }
                       else
                       {
                           Debug.LogWarning($"Data for TokenID {nft.tokenId} not found.");
                       }
                }
                 */
                UnlockAllCharacters();
                
            

              //  PlayerStats.OnPlayerStatsChanged?.Invoke();

                // Example usage:
                //foreach (KeyValuePair<string, NFTCharacter> kv in nftCharacterMap)
                //{
                //    Debug.Log($"TokenID: {kv.Key}");
                //    Debug.Log($"Character Name: {kv.Value.Character.Name}");
                //    //Debug.Log($"Power: {kv.Value.Properties.Power}");
                //}

                //NFTCharacter nftCharacter = GetNFTCharacter("tokenId");

                //if (nftCharacter != null)
                //{
                //    Debug.Log($"Character Name: {nftCharacter.Character.Name}");
                //    Debug.Log($"Power: {nftCharacter.Properties.Power}");
                //}
            }
        }

        // sample response:
        /*

        {
        "list": [
            {
                "id": 123172,
                "name": "XANA x BreakingDown",
                "description": "Web 3.0 metaverse gaming NFT for the most famous MMA entertainment from Japan.",
                "collectionsId": 344,
                "smallImage": "https://ik.imagekit.io/xanalia/breaking-down/v12/0128.png",
                "largeImage": "https://ik.imagekit.io/xanalia/breaking-down/v12/0128.png",
                "previewImage": "https://ik.imagekit.io/xanalia/breaking-down/v12/0128.png",
                "networkId": 1,
                "tokenId": "4828",
                "voice_character": null,
                "user_voice_id": null,
                "voice_name": null,
                "collection": {
                    "id": 344,
                    "name": "XANA x BreakingDown",
                    "address": "0x6c019F2Df0b5A7D80f0429324c8c4BCB28eD6170",
                    "avatar": "https://ik.imagekit.io/xanalia/CollectionMainData/bdIconImageCollection.png"
                },
                "owner": {
                    "userId": 113522,
                    "nftId": 123172,
                    "name": "shintarou",
                    "avatar": null,
                    "role": 1,
                    "address": "0xa779febb5caf8a688ce783caa44ddb4468396904"
                }
            },...
        ],
        "count": 5
        }
        */

        // Method to create an NFTCharacter from NFTData
        private static NFTCharacter CreateNFTCharacter(NFTData data)
        {
            CharacterBase characterBase = new() { Name = data.Profile };

            CharacterProperties properties = new()
            {
                //SkinColor = data.SkinColor,
                // Set other properties as needed
                LP_Damage = data.LP_Damage,
                LK_Damage = data.LK_Damage,
                HP_Damage = data.HP_Damage,
                HK_Damage = data.HK_Damage,
                SP_Damage = data.SP_Damage
            };

            NFTCharacter nftCharacter = new()
            {
                TokenID = data.TokenID,
                Character = characterBase,
                Properties = properties,
                Profile = data.Profile
            };

            return nftCharacter;
        }

        // Method to get an NFTCharacter by tokenID
        public static NFTCharacter GetNFTCharacter(string tokenID)
        {
            if (nftCharacterMap.TryGetValue(tokenID, out NFTCharacter nftCharacter))
            {
                return nftCharacter;
            }
            else
            {
                Debug.LogWarning("NFTCharacter with this TokenID not found.");
                return null;
            }
        }

        public static bool HasCharacter(string characterName)
        {
            foreach (var character in PlayerStats.PlayerCharacters)
            {
                if (character.Character.Name == characterName)
                {
                    return true;
                }
            }

            return false;
        }

        [System.Serializable]
        class NFTResponse
        {
            public NFT[] list;
            public int count;

            [System.Serializable]
            public class NFT
            {
                public int id;
                public string name;
                public string description;
                public int collectionsId;
                public string smallImage;
                public string largeImage;
                public string previewImage;
                public int networkId;
                public string tokenId;
                public object voice_character;
                public object user_voice_id;
                public object voice_name;
                public Collection collection;
                public Owner owner;

                [System.Serializable]
                public class Collection
                {
                    public int id;
                    public string name;
                    public string address;
                    public string avatar;
                }

                [System.Serializable]
                public class Owner
                {
                    public int userId;
                    public int nftId;
                    public string name;
                    public object avatar;
                    public int role;
                    public string address;
                }
            }
        }

        #region Debug

        private static void UnlockOneCharacter()
        {
            var defaultNFT = new NFTResponse.NFT
            {
                tokenId = "1"
            };

            NFTData data = NFTDataLoader.Instance.GetNFTData(defaultNFT.tokenId);
            if (data != null)
            {
                NFTCharacter nftCharacter = CreateNFTCharacter(data);
                nftCharacterMap[defaultNFT.tokenId] = nftCharacter;
                PlayerStats.PlayerCharacters.Add(nftCharacter);
            }
            else
            {
                Debug.LogError($"Data for TokenID {defaultNFT.tokenId} not found.");
            }
        }

        internal static void UnlockAllCharacters()
        {
            var testNFTs = new NFTResponse.NFT[]
            {
                new() { tokenId = "1" },    // Juggernaut
                new() { tokenId = "3" },    // Phoenix
                new() { tokenId = "4" },    // Brawler
                new() { tokenId = "6" },    // Berserker
                new() { tokenId = "9" },    // Unstoppable
                new() { tokenId = "11" },   // Killer
                new() { tokenId = "13" },   // Rogue
                new() { tokenId = "19" },   // Infiltrator
                new() { tokenId = "30" },   // Commando
                new() { tokenId = "37" },   // Warrior
                new() { tokenId = "5000" }  // Puppeteer
            };
            List<NFTCharacter>NFTCHAR = new List<NFTCharacter>();
            foreach (var nft in testNFTs)
            {
                NFTData data = NFTDataLoader.Instance.GetNFTData(nft.tokenId);
                if (data != null)
                {
                    NFTCharacter nftCharacter = CreateNFTCharacter(data);
                    nftCharacterMap[nft.tokenId] = nftCharacter;
                    NFTCHAR.Add(nftCharacter);
                    //Debug.LogError($"Character {nftCharacter.Character.Name}, {nftCharacter.TokenID} unlocked.");
                }
                else
                {
                    Debug.LogWarning($"Data for TokenID {nft.tokenId} not found.");
                }
            }
            PlayerStats.PlayerCharacters = NFTCHAR;
            PlayerStats.OnPlayerStatsChanged?.Invoke();
        }

        #endregion
    }

    [System.Serializable]
    public class NFTData
    {
        public string TokenID;
        public string Profile;
        //public string SkinColor;
        // Add other properties as needed
        public int LP_Damage;
        public int LK_Damage;
        public int HP_Damage;
        public int HK_Damage;
        public int SP_Damage;
    }

    [System.Serializable]
    public class CharacterBase
    {
        public string Name;
    }

    [System.Serializable]
    public class CharacterProperties
    {
        //public string SkinColor;
        public int LP_Damage;
        public int LK_Damage;
        public int HP_Damage;
        public int HK_Damage;
        public int SP_Damage;
        // Add other properties as needed
    }

    [System.Serializable]
    public class NFTCharacter
    {
        public string TokenID;
        public CharacterBase Character;
        public CharacterProperties Properties;
        public string Profile;
    }
}