using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;


namespace BD
{
    public class PlayerName : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInputField;
        //[SerializeField] private Image _setPlayerNameButton;

        //[SerializeField] private GameObject errorText;

        [SerializeField] private Sprite editSprite;
        [SerializeField] private Sprite saveSprite;

        //[SerializeField] private GameObject fakeTransImage;

        #region Unity Callbacks

        private void Awake()
        {
            if (nameInputField == null)
            {
                Debug.LogError("<color=cyan>PlayerName:</color> Name InputField is not set!");
            }

            nameInputField.onSelect.AddListener(delegate { StartEditing(); });
            nameInputField.onEndEdit.AddListener(delegate { SetPlayerName(); });
        }

        private void Start()
        {
            ////////nameInputField.onSelect.AddListener(delegate { EditButtonClicked(); });
            ////////nameInputField.onDeselect.AddListener(delegate { ResetAll(); });
            /// This schieße misbehaved af. Had to cover the inputField with a trans image.

            ResetAll();
        }


        #endregion

        private void ResetAll()
        {
            //errorText.SetActive(true);
            //fakeTransImage.SetActive(true);
            //_setPlayerNameButton.interactable = true;
            nameInputField.text = PlayerStats.userName;
            //_setPlayerNameButton.sprite = editSprite;
            //_setPlayerNameButton.onClick.RemoveAllListeners();
            //_setPlayerNameButton.onClick.AddListener(StartEditing);
        }

        private async void SetPlayerName()
        {
            //_setPlayerNameButton.interactable = false;

            if (string.IsNullOrEmpty(nameInputField.text))
            {
                Debug.LogError("<color=cyan>PlayerName:</color> Name InputField is empty!");
                ResetAll();
                return;
            }

            if (string.Equals(nameInputField.text, PlayerStats.userName))
            {
                ResetAll();
                return;
            }

            if (nameInputField.text.Length > 20)
            {
                Debug.LogError("<color=cyan>PlayerName:</color> Name is too long!");
                //ResetAll();
                return;
            }

            await APIManager.SetPlayerName(nameInputField.text);
            
            ResetAll();
        }

        private void StartEditing()
        {
            // simulate a click on the input field
            //nameInputField.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));

            //nameInputField.onValueChanged.AddListener(delegate { 
            //    //errorText.SetActive(false);

            //});

            //fakeTransImage.SetActive(false);

            //_setPlayerNameButton.sprite = saveSprite;
            nameInputField.MoveTextEnd(false);

            //_setPlayerNameButton.onClick.RemoveAllListeners();
            //_setPlayerNameButton.onClick.AddListener(SetPlayerName);

            //nameInputField.ActivateInputField();
        }
    }
}