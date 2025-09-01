using UnityEngine;

namespace BD
{
    public class SettingsPanel : MonoBehaviour
    {
        public static SettingsPanel instance;

        [SerializeField] private GameObject _soundSettingsPanel;
        [SerializeField] private Transform _soundSettingsDropdownArrow;

        [SerializeField] private GameObject _profilesPanel;
        [SerializeField] private Transform _profilesDropdownArrow;

        public bool isShow = false;

        private void Awake()
        {
            if (instance == null)
                instance = this;

        }
        private void Start()
        {
            AvatarSelection();
        }

        public void AvatarSelection()
        {

            if (isShow)
            {
                ProfilesButtonClicked();
                _profilesPanel.SetActive(true);
            }
        }

        public void SoundSettingsDropdownButtonClicked()
        {
            if (_soundSettingsPanel) _soundSettingsPanel.SetActive(!_soundSettingsPanel.activeSelf);
            if (_soundSettingsDropdownArrow) _soundSettingsDropdownArrow.localScale = new Vector3(1, _soundSettingsPanel.activeSelf ? -1 : 1, 1);

            if (_profilesPanel) _profilesPanel.SetActive(false);
            if (_profilesDropdownArrow) _profilesDropdownArrow.localScale = new Vector3(1, 1, 1);
        }

        public void ProfilesButtonClicked()
        {
            if (_profilesPanel) _profilesPanel.SetActive(!_profilesPanel.activeSelf);
            if (_profilesDropdownArrow) _profilesDropdownArrow.localScale = new Vector3(1, _profilesPanel.activeSelf ? -1 : 1, 1);

            if (_soundSettingsPanel) _soundSettingsPanel.SetActive(false);
            if (_soundSettingsDropdownArrow) _soundSettingsDropdownArrow.localScale = new Vector3(1, 1, 1);
        }
    }
}