using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BD
{
    public class PlayerStatsUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _userNameText;
        [SerializeField] private GameObject _winStreakParentGO;
        [SerializeField] private TextMeshProUGUI _winStreakText;
        [SerializeField] private TextMeshProUGUI _xetaText;
        [SerializeField] private TextMeshProUGUI _fuelText;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private TextMeshProUGUI _leagueNameText;
        [SerializeField] private TextMeshProUGUI _leagueSubTierText;
        [SerializeField] private TextMeshProUGUI _leaguePointsText;
        [SerializeField] private TextMeshProUGUI _pointsToNextLeagueText;
        [SerializeField] private TextMeshProUGUI _battlesText;
        [SerializeField] private TextMeshProUGUI _winsText;
        [SerializeField] private TextMeshProUGUI _lossesText;
        [SerializeField] private TextMeshProUGUI _drawsText;
        [SerializeField] private TextMeshProUGUI _seasonText;
        [SerializeField] private TextMeshProUGUI _nftCountText;

        [SerializeField] private Button _refreshButton;

        private void OnEnable()
        {
            PlayerStats.OnPlayerStatsChanged += UpdatePlayerStatsUI;
        }

        private void OnDisable()
        {
            PlayerStats.OnPlayerStatsChanged -= UpdatePlayerStatsUI;
        }

        private void Start()
        {
            RefreshButtonPressed(); // To update stats on start or coming back to main menu.
            UpdatePlayerStatsUI();
            if (_refreshButton) _refreshButton.interactable = true;
        }

        private void UpdatePlayerStatsUI()
        {
            if (_userNameText) _userNameText.text = PlayerStats.userName;

            if (_winStreakParentGO) _winStreakParentGO.SetActive(PlayerStats.WinStreak > 0);
            if (_winStreakText) _winStreakText.text = PlayerStats.WinStreak > -1 ? PlayerStats.WinStreak.ToString() : "-";

            if (_xetaText) _xetaText.text = PlayerStats.xeta > -1 ? PlayerStats.xeta.ToString() : "-";
            if (_fuelText) _fuelText.text = PlayerStats.fuel > -1 ? PlayerStats.fuel.ToString() : "-";
            if (_rankText) _rankText.text = PlayerStats.Rank > 0 ? PlayerStats.Rank.ToString() : "-";
            if (_leagueNameText) _leagueNameText.text = PlayerStats.LeagueName;
            if (_leagueSubTierText) _leagueSubTierText.text = PlayerStats.LeagueSubTier;
            if (_leaguePointsText) _leaguePointsText.text = PlayerStats.LeaguePoints > -1 ? PlayerStats.LeaguePoints.ToString() : "-";
            if (_pointsToNextLeagueText) _pointsToNextLeagueText.text = PlayerStats.PointsToNextLeague > -1 ? PlayerStats.PointsToNextLeague.ToString() : "-";
            if (_battlesText) _battlesText.text = PlayerStats.Battles > -1 ? PlayerStats.Battles.ToString() : "-";
            if (_winsText) _winsText.text = PlayerStats.Wins > -1 ? PlayerStats.Wins.ToString() : "-";
            if (_lossesText) _lossesText.text = PlayerStats.Losses > -1 ? PlayerStats.Losses.ToString() : "-";
            if (_drawsText) _drawsText.text = PlayerStats.Draws > -1 ? PlayerStats.Draws.ToString() : "-";
            if (_seasonText) _seasonText.text = PlayerStats.Season > -1 ? PlayerStats.Season.ToString() : "-";
            if(PlayerStats.PlayerCharacters.Count > 0)
            if (_nftCountText) _nftCountText.text = PlayerStats.PlayerCharacters[ProfileSelector.Instance.CurrentProfile].Profile;
            SettingsPanel.instance.AvatarSelection();
        }

        private void SetAllStatsToDefault()
        {
            if (_userNameText) _userNameText.text = "-";
            if (_winStreakText) _winStreakText.text = "-";
            if (_xetaText) _xetaText.text = "-";
            if (_fuelText) _fuelText.text = "-";
            if (_rankText) _rankText.text = "-";
            if (_leagueNameText) _leagueNameText.text = "-";
            if (_leagueSubTierText) _leagueSubTierText.text = "-";
            if (_leaguePointsText) _leaguePointsText.text = "-";
            if (_pointsToNextLeagueText) _pointsToNextLeagueText.text = "-";
            if (_battlesText) _battlesText.text = "-";
            if (_winsText) _winsText.text = "-";
            if (_lossesText) _lossesText.text = "-";
            if (_drawsText) _drawsText.text = "-";
            if (_seasonText) _seasonText.text = "-";
            if (_nftCountText) _nftCountText.text = "-";
        }

        public async void RefreshButtonPressed()
        {
            if (_refreshButton) _refreshButton.interactable = false;
            SetAllStatsToDefault();

            await APIManager.GetUserDetailsByWallet(); // no need to await this.
            await NFTManager.GetNFTs(APIManager.WalletAddress);

            if (_refreshButton) _refreshButton.interactable = true;
        }
    }
}
