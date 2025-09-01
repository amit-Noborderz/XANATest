using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BD;


namespace BD
{
    public class ProfilesManager : MonoBehaviour
    {
        public Image TargetSprite;
        private List<Button> ProfileButtons = new List<Button>();

        [SerializeField] private Button buttonPrefab;
        [SerializeField] private Transform buttonsParent;

        private void Start()
        {
            TargetSprite.gameObject.SetActive(false);
            //ProfileButtons = GetComponentsInChildren<Button>();


            //foreach (var button in ProfileButtons)

            //button.GetComponentInChildren<TextMeshProUGUI>().text = GetProfileNameByIndex(button.transform.GetSiblingIndex());


        }
        private void OnEnable()
        {
            TargetSprite.gameObject.transform.SetParent(buttonsParent.transform);
            foreach (var button in ProfileButtons) { 
                    DestroyImmediate(button.gameObject);
            
            }
            ProfileButtons.Clear();

           // for (var i = 0; i < PlayerStats.PlayerCharacters.Count; i++)
            for (var i = 0; i < PlayerStats.PlayerCharacters.Count; i++)
            {
                var t = PlayerStats.PlayerCharacters[i];
                var btn = Instantiate(buttonPrefab, buttonsParent, false);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = $"{t.Profile} {t.TokenID}";
                btn.onClick.AddListener(() => { ProfileSelected(btn.transform.GetSiblingIndex()); });

                ProfileButtons.Add(btn);
            }

            TargetSprite.transform.parent = ProfileButtons[0].transform;

            // button.GetComponentInChildren<TextMeshProUGUI>().text = $"{PlayerStats.PlayerCharacters[ProfileSelector.Instance.CurrentProfile].Profile} {PlayerStats.PlayerCharacters[ProfileSelector.Instance.CurrentProfile].TokenID}";
            // button.onClick.AddListener(() =>
            // {
            //     ProfileSelected(button.transform.GetSiblingIndex());
            // });


            //EnableAvailableCharacters();

            ProfileSelected(ProfileSelector.Instance.CurrentProfile);
        }
        //private void EnableAvailableCharacters()
        //{
        //    foreach (var button in ProfileButtons)
        //    {
        //        //Debug.Log("Profile Name: " + GetProfileNameByIndex(button.transform.GetSiblingIndex()));
        //        button.interactable = NFTManager.HasCharacter(PlayerStats.PlayerCharacters[button.transform.GetSiblingIndex()].Profile);
        //    }
        //}

        // private string GetProfileNameByIndex(int index)
        // {
        //     return ProfileSelector.Instance.profiles[index];
        // }

        private void ProfileSelected(int i)
        {
            ProfileSelector.Instance.CurrentProfile = i;
            CharacterClothes.clothChange?.Invoke();
            TargetSprite.rectTransform.position = ProfileButtons[i].transform.position;
            TargetSprite.transform.parent = ProfileButtons[i].transform;
            TargetSprite.gameObject.SetActive(true);

            for (int j = 0; j < ProfileButtons.Count; j++)
            {
                if (j == i)
                {
                    ProfileButtons[j].GetComponentInChildren<TextMeshProUGUI>().color = new Color(1, 0, 0.8980392f);
                }
                else
                {
                    ProfileButtons[j].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
                }
            }
        }

    }
}