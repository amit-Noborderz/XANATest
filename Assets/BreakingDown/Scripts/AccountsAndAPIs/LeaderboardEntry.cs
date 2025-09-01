using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BD
{
    public class LeaderboardEntry : MonoBehaviour
    {
        public int Index { get; set; }

        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI lpText;
        [SerializeField] private Image leagueImage;
        [SerializeField] private Image leagueSubTierImage;
        [SerializeField] private TextMeshProUGUI leagueSubTierText;
        [SerializeField] private TextMeshProUGUI duelText;
        [SerializeField] private TextMeshProUGUI winText;
        [SerializeField] private TextMeshProUGUI loseText;
        [SerializeField] private TextMeshProUGUI drawText;
        [SerializeField] private TextMeshProUGUI streakText;

        [SerializeField] private Sprite[] leagueSprites;

        public void SetData(DataEntry data)
        {
            rankText.text = data.rank > 0 ? data.rank.ToString() : "-";
            nameText.text = data.name;
            lpText.text = data.rating.ToString();

            var (leagueName, LeagueSubTier) = BD.APIManager.GetLeagueNameAndSubTier(data.league);
            SetLeagueImage(leagueName);
            if (string.IsNullOrEmpty(LeagueSubTier))
            {
                leagueSubTierImage.gameObject.SetActive(false);
            }
            else
            {
                leagueSubTierImage.gameObject.SetActive(true);
                leagueSubTierText.text = LeagueSubTier;
            }
            
            duelText.text = data.battles.ToString();
            winText.text = data.wins.ToString();
            loseText.text = data.loose.ToString();
            drawText.text = data.draw.ToString();
            streakText.text = data.winningStreak.ToString();
        }

        private void SetLeagueImage(string league)
        {
            if (Enum.TryParse(league, out League lg))
            {
                leagueImage.sprite = leagueSprites[(int)lg]; // TODO: Need to handle possible IndexOutOfRangeException
            }
            else
            {
                Debug.LogError($"<color=cyan>LeaderboardEntry:</color> SetLeagueImage() Error: Could not parse league: {league}");
            }
        }

        private enum League
        {
            Trial,
            Bronze,
            Silver,
            Gold,
            Platinum,
            Diamond
        }
    }
}