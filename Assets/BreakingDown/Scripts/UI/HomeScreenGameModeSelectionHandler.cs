using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;


namespace BD
{
    public class HomeScreenGameModeSelectionHandler : MonoBehaviour
    {
        [SerializeField] private MainMenuMessagePopup messagePopup;

        [SerializeField] private Button _leagueButton;
        [SerializeField] private Button _trainingButton;
        [SerializeField] private Button letsDuelButton;
        [SerializeField] private Button homeButton;

        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _selectedSprite;

        [SerializeField] private GameObject purchaseFuelPanel;

        private GameMode _selectedGameMode;

        private void Awake()
        {
            _leagueButton.onClick.AddListener(() => OnGameModeSelected(GameMode.League));
            _trainingButton.onClick.AddListener(() => OnGameModeSelected(GameMode.Training));
            homeButton.onClick.AddListener(() => OnClickBackBTN()); //GamePlayUIHandler.inst.OnExitButtonClick());

            letsDuelButton.onClick.AddListener(() => OnLetsDuelButtonClicked());

            OnGameModeSelected(GameMode.League); // Default

            messagePopup.gameObject.SetActive(false);
        }

        public void OnClickBackBTN()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                homeButton.interactable = false;
                AvatarSpawnerOnDisconnect.Instance.ConnectionErrorToast.gameObject.SetActive(true);
                AvatarSpawnerOnDisconnect.Instance.toastMessage.text = TextLocalization.GetLocaliseTextByKey("Connection Error: waiting for network...");
                AvatarSpawnerOnDisconnect.Instance.ConnectionErrorToast.DOAnchorPos(new Vector2(AvatarSpawnerOnDisconnect.Instance.ConnectionErrorToast.anchoredPosition.x, -245f), 0.5f).SetEase(Ease.InOutSine);
                StartCoroutine(WaitForInternet());
                return;
            }
            LoadingHandler.Instance.WithoutApproval = false; 
            GamePlayButtonEvents.OnExitButtonXANASummit?.Invoke();
        }
        public IEnumerator WaitForInternet()
        {
            while(Application.internetReachability == NetworkReachability.NotReachable) { yield return new WaitForSeconds(1f); }
            AvatarSpawnerOnDisconnect.Instance.ConnectionErrorToast.gameObject.SetActive(false);
            GamePlayButtonEvents.OnExitButtonXANASummit?.Invoke();
        }
        private void OnGameModeSelected(GameMode gameMode)
        {
            _leagueButton.image.sprite = (gameMode == GameMode.League) ? _selectedSprite : _normalSprite;
            _trainingButton.image.sprite = (gameMode == GameMode.Training) ? _selectedSprite : _normalSprite;

            _selectedGameMode = gameMode;
        }


        
        private void OnLetsDuelButtonClicked()
        { var profile = ScrollSnapBase.instance._currentPage;
            var opp = PlayerPrefs.GetInt("OpponentID", -1);
            if (profile >= opp) profile++;
            ProfileSelector.Instance.CurrentProfile = profile;
            letsDuelButton.interactable = false;
            StartCoroutine(OnLetsDuelButton());
        }

        public void AvatarSelaction()
        {
           // ProfileSelector.Instance.CurrentProfile = ScrollSnapBase.instance._currentPage;
        }

        public IEnumerator OnLetsDuelButton()
        {
            yield return new WaitForSeconds(1);
            letsDuelButton.interactable = true ;

            switch (_selectedGameMode)
            {
                case GameMode.League:
                    // Go to league mode
                    {
                        GoToVersusModeScreen();
                        break;
                    }
                case GameMode.Training:
                    // Go to training mode
                    {
                        GoToTrainingModeScreen();
                        break;
                    }
            }
        }
        private readonly float _apiCoolDownTime = 30;
        private bool _requestInProgress = false;

        private void GoToVersusModeScreen()
        {
            //if (!FuelManager.hasMinimumFuel)
            //{
            //    Debug.LogError("Not enough fuel to play");
            //    purchaseFuelPanel.SetActive(true);
            //    return;
            //}

            /*    if (PlayerStats.InBattle == 1)
                {
                    if (_requestInProgress)
                    {
                        Debug.LogError("<color=cyan>BD:</color> Awaiting for-end-battle API cooldown.");
                        messagePopup.Show("Failed to start battle",
                            "Failed to start battle as previous battle did not end successfully. Please try again in a few minutes.");
                        return;
                    }

                    Debug.LogError("<color=cyan>BD:</color> Player is already in battle. Force ending...");

                    APIManager.ForceEndUserBattleAsync(ForceEndBattleResult);
                    _requestInProgress = true;
                    Invoke(nameof(ResetCoolDown), _apiCoolDownTime);
                    return;
                }

                if (PlayerStats.InBattle == -1)
                {
                    Debug.LogError("<color=cyan>BD:</color> Player InBattle status is not set.");
                    messagePopup.Show("Failed to start battle",
                        "Failed to start battle because user details are not set. Please login again or contact support.");
                    return;
                }

                if (PlayerStats.userId == -1)
                {
                    Debug.LogError("<color=cyan>BD:</color> Player userId is not set.");
                    messagePopup.Show("Failed to start battle", 
                        "Failed to start battle because user ID is not set. Please login again or contact support.");
                    return;
                }
    */

            FightingGameManager.instance.HomeScreenPlayerAnim.SetAvatar();
            UFE.StartSearchMatchScreen();
        }

        private void ResetCoolDown()
        {
            _requestInProgress = false;
        }

        private async void ForceEndBattleResult(bool result)
        {
            if (result)
            {
                await APIManager.GetUserDetailsByWallet((result) => { if (result) GoToVersusModeScreen(); });
            }
            else
            {
                await APIManager.GetUserInBattleByWalletAddress();

                if (PlayerStats.InBattle == 0)
                {
                    GoToVersusModeScreen();
                }
                else
                {
                    Debug.LogError("<color=cyan>BD:</color> Failed to force end battle.");
                    messagePopup.Show("Failed to start battle",
                        "Failed to start battle as previous battle did not end successfully. Please try again in a few minutes.");
                }
            }
        }

        private void GoToTrainingModeScreen()
        {
            UFE.StartTrainingMode();
        }

        #region Public Methods

        public void BuyFuel()
        {
            // Purchase fuel
            FuelManager.CurrentFuel += 10;
            purchaseFuelPanel.SetActive(false);
        }

        #endregion

        enum GameMode
        {
            League, // vs AI and Multiplayer
            Training
        }




        #region Debug

        //public void SetRandomLeague()
        //{
        //    PlayerStats.LeagueID = Random.Range(1, 6);
        //    PlayerStats.OnPlayerStatsChanged?.Invoke();
        //}

        #endregion
    }
}