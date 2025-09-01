using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace BD
{
    public class LoginOptionsController : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private string appName = "BreakingDown";
        private string appVersion => Application.version;


        [SerializeField] private GameObject _walletLoginBtn;
        [SerializeField] private GameObject _emailLoginBtn;
        [SerializeField] private GameObject _googleLoginBtn;
        [SerializeField] private GameObject _appleLoginBtn;
        [SerializeField] private Button _skipLoginBtn;


        #endregion

        #region Get Features List

        private static string GET_FEATURES_LIST_API = "https://api-test.xana.net/admin/v2/get-features-list";

        private async Task GetFeaturesList()
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(GET_FEATURES_LIST_API);
            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error getting login features list: {webRequest.error}. Enabling all options.");
                _walletLoginBtn.SetActive(true);
                _emailLoginBtn.SetActive(true);
                _googleLoginBtn.SetActive(true);
                _appleLoginBtn.SetActive(true);
            }
            else
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                Response response = JsonUtility.FromJson<Response>(webRequest.downloadHandler.text);

                if (response.success)
                {
                    // Check if the app name matches
                    foreach (var data in response.data)
                    {
                        if (data.app_name == appName
                            && data.version == appVersion
                            && data.is_active)
                        {
                            _walletLoginBtn.SetActive(data.feature_list.walletBtn);
                            _emailLoginBtn.SetActive(data.feature_list.emailBtn);
                            _googleLoginBtn.SetActive(data.feature_list.googleBtn);
                            _appleLoginBtn.SetActive(data.feature_list.appleBtn);
                            return;
                        }
                    }

                    Debug.LogError($"App name not found in login features list: {response.msg}. Enabling all options.");
                    _walletLoginBtn.SetActive(true);
                    _emailLoginBtn.SetActive(true);
                    _googleLoginBtn.SetActive(true);
                    _appleLoginBtn.SetActive(true);

                }
                else
                {

                    Debug.LogError($"Error getting login features list: {response.msg}. Enabling all options.");
                    _walletLoginBtn.SetActive(true);
                    _emailLoginBtn.SetActive(true);
                    _googleLoginBtn.SetActive(true);
                    _appleLoginBtn.SetActive(true);
                }
            }
        }

        [System.Serializable]
        class Response
        {
            public bool success;
            public Data[] data;
            public string msg;

            [System.Serializable]
            public class Data
            {
                int id;
                public string app_name;
                public string version;
                public bool is_active;
                public FeatureList feature_list;

                [System.Serializable]
                public class FeatureList
                {
                    public bool walletBtn;
                    public bool emailBtn;
                    public bool googleBtn;
                    public bool appleBtn;
                }
            }
        }

        /* Sample Response

        {
        "success": true,
        "data": [
            {
                "id": 3,
                "app_name": "XANA",
                "version": "24.09.20",
                "is_active": true,
                "feature_list": {
                    "walletbtn": true
                }
            },
            {
                "id": 4,
                "app_name": "XSUMMIT",
                "version": "24.09.20",
                "is_active": true,
                "feature_list": {
                    "btn": false,
                    "wallet": true
                }
            },
            {
                "id": 1,
                "app_name": "XANA",
                "version": "1.1.0",
                "is_active": false,
                "feature_list": {
                    "walletbtn": false
                }
            },
            {
                "id": 2,
                "app_name": "XANA",
                "version": "1.0.0",
                "is_active": true,
                "feature_list": {
                    "googleBtn": false,
                    "walletBtn": true
                }
            }
        ],
        "msg": "Records retrieved successfully"
    }

        */

        #endregion

        #region Unity Callbacks

        private async void Start()
        {
            await GetFeaturesList();

            _skipLoginBtn.onClick.AddListener(() => { UnityEngine.SceneManagement.SceneManager.LoadScene(1); });
        }

        #endregion
    }
}