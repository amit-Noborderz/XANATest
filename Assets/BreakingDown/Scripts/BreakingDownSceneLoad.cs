using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BD
{

    public class BreakingDownSceneLoad : MonoBehaviour
    {
        public async void OnClickBreakinDownSceneLoad()
        {
          // await BD.APIManager.GetUserDetailsByWallet();
         //   await BD.NFTManager.GetNFTs(BD.APIManager.WalletAddress);
            SceneManager.LoadScene("Demo_Fighter3D - Type 2");
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }

    }
}
