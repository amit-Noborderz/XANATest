using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace BD
{
    public class RankingScrollController : MonoBehaviour
    {
        public ScrollRect scrollRect;
        public RectTransform content;
        public GameObject entryPrefab;

        private List<DataEntry> dataEntries = new List<DataEntry>();
        private List<LeaderboardEntry> uiEntries = new List<LeaderboardEntry>();
        private int poolSize;
        private int totalCount;
        private bool isLoadingData = false;

        private RankingDataManager dataManager;

        //private int dataBufferSize = 100; // Max number of data entries to keep in memory

        //private void OnGUI()
        //{
        //    // set gui label font size
        //    GUI.skin.label.fontSize = 40;
        //    GUI.color = Color.green;

        //    GUI.Label(new Rect(10, 10, 500, 100), "DataManager[]: " + dataEntries.Count);
        //}
        private void Awake()
        {
            dataManager = GetComponent<RankingDataManager>();
        }
        private void Start()
        {
           
            scrollRect.onValueChanged.AddListener(OnScrollChanged);

            // Calculate pool size based on viewport height and entry height
            float entryHeight = ((RectTransform)entryPrefab.transform).sizeDelta.y;
            float viewportHeight = scrollRect.viewport.rect.height;
            int visibleCount = Mathf.CeilToInt(viewportHeight / entryHeight);
            int extraBuffer = 4; // Extra entries to preload above and below
            poolSize = visibleCount + extraBuffer;

            // Initialize UI element pool
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(entryPrefab, content);
                LeaderboardEntry uiEntry = obj.GetComponent<LeaderboardEntry>();
                uiEntries.Add(uiEntry);
                uiEntry.gameObject.SetActive(false);
            }

            // Load initial data
           
        }
        private void OnEnable()
        {
            StartCoroutine(dataManager.LoadNextPage(OnDataLoaded));
        }
        private void OnDataLoaded(List<DataEntry> newEntries)
        {
            isLoadingData = false;
            dataEntries.AddRange(newEntries);

            //// Remove data entries that are too far above the current view
            //if (dataEntries.Count > dataBufferSize)
            //{
            //    int removeCount = dataEntries.Count - dataBufferSize;
            //    dataEntries.RemoveRange(0, removeCount);
            //    AdjustContentPosition(removeCount);
            //}

            UpdateContentHeight();
            UpdateVisibleItems();
        }

        //private void AdjustContentPosition(int removedEntries)
        //{
        //    float entryHeight = ((RectTransform)entryPrefab.transform).sizeDelta.y;
        //    Vector2 offset = new Vector2(0, removedEntries * entryHeight);
        //    content.anchoredPosition -= offset;
        //}

        private void OnScrollChanged(Vector2 scrollPosition)
        {
            UpdateVisibleItems();
            CheckIfNeedMoreData();
        }

        private void UpdateContentHeight()
        {
            float entryHeight = ((RectTransform)entryPrefab.transform).sizeDelta.y;
            float contentHeight = dataEntries.Count * entryHeight;
            content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
        }

        private void UpdateVisibleItems()
        {
            float entryHeight = ((RectTransform)entryPrefab.transform).sizeDelta.y;
            float viewportHeight = scrollRect.viewport.rect.height;
            float scrollY = content.anchoredPosition.y;

            int firstVisibleIndex = Mathf.FloorToInt(scrollY / entryHeight);
            int lastVisibleIndex = Mathf.CeilToInt((scrollY + viewportHeight) / entryHeight);

            int totalCount = dataEntries.Count;
            int extraBuffer = 2;

            firstVisibleIndex = Mathf.Clamp(firstVisibleIndex - extraBuffer, 0, totalCount - 1);
            lastVisibleIndex = Mathf.Clamp(lastVisibleIndex + extraBuffer, 0, totalCount - 1);

            int index = 0;
            for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
            {
                if (index < uiEntries.Count)
                {
                    LeaderboardEntry uiEntry = uiEntries[index];
                    uiEntry.Index = i;
                    uiEntry.SetData(dataEntries[i]);
                    ((RectTransform)uiEntry.transform).anchoredPosition = new Vector2(0, -i * entryHeight);
                    uiEntry.gameObject.SetActive(true);
                    index++;
                }
            }

            // Deactivate unused UI elements
            for (int i = index; i < uiEntries.Count; i++)
            {
                uiEntries[i].gameObject.SetActive(false);
            }
        }

        private void CheckIfNeedMoreData()
        {
            float scrollY = content.anchoredPosition.y;
            float viewportHeight = ((RectTransform)scrollRect.viewport).rect.height;
            float contentHeight = content.sizeDelta.y;

            if (scrollY + viewportHeight > contentHeight - 200)
            {
                if (!isLoadingData)
                {
                    isLoadingData = true;
                    StartCoroutine(dataManager.LoadNextPage(OnDataLoaded));
                }
            }
        }
    }
}