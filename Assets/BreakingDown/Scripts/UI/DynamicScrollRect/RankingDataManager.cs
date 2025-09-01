using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BD
{
    public class RankingDataManager : MonoBehaviour
    {
        public int pageSize = 50;
        private int currentPage = 0;

        internal IEnumerator LoadNextPage(Action<List<DataEntry>> onComplete)
        {
            if (string.IsNullOrEmpty(APIManager.AUTH_TOKEN))
            {
                Debug.LogError("<color=cyan>Ranking:</color> Auth Token is not available. Please login first.");
                yield break;
            }

            currentPage++;
            yield return StartCoroutine(GetPageFromAPI(currentPage, pageSize, onComplete));
        }

        private IEnumerator GetPageFromAPI(int pageNumber, int pageSize, Action<List<DataEntry>> onComplete)
        {
            // Implement your API call here
            List<DataEntry> newEntries = new();

            string url = $"{BD.APIManager.BD_GetAllUserGlobalRanking_API}/{pageNumber}/{pageSize}/";
            //Debug.Log($"<color=cyan>Ranking:</color> GetUsersGlobalRank() URL: {url}");

            using UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.SetRequestHeader("Authorization", $"Bearer {BD.APIManager.AUTH_TOKEN}");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"<color=green>Sucess:</color>Response: {webRequest.downloadHandler.text}");
                Response response = JsonUtility.FromJson<Response>(webRequest.downloadHandler.text);

                foreach (var entry in response.data.rows)
                {
                    newEntries.Add(entry);
                }
            }
            else
            {
                Debug.LogError($"<color=cyan>Ranking:</color> GetUsersGlobalRank() Error: {webRequest.error}");
            }

            onComplete?.Invoke(newEntries);
        }
    }

    [Serializable]
    class Response
    {
        public bool success;
        public Data data;
    }

    [Serializable]
    class Data
    {
        public int count;
        public DataEntry[] rows;
    }

    [Serializable]
    public class DataEntry
    {
        public int rating;
        public int rank;
        public int leagueRank;
        public int leagueId;
        public string league;
        public int battles;
        public int wins;
        public int loose;
        public int draw;
        public object seasonId;
        public int userId;
        public int winningStreak;
        public string name;
        public object tcgAvatar;
        public object avatar;
    }
}