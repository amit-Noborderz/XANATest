using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Advanced_InputNavigator : MonoBehaviour
{
    private int current_index = -1;

    EventSystem system;
    // Start is called before the first frame update
    public List<Navigatable> NavigatableObjects = new List<Navigatable>();

    public TMP_FontAsset glowFontTmp;
    public TMP_FontAsset NormalTmp;
    public Font glowFont;
    public Font Normal;
  //  public HomeScene HomeScene;
    private void Start()
    {
        system = EventSystem.current;
       // if (SceneManager.GetActiveScene().name == "_Home")
          //  HomeScene = FindObjectOfType<HomeScene>();
    }

    public void NextIndex(int i)
    {
        resetbasedonIndex(current_index + 1);
        if (i > 2)
        {
            current_index = i - 2;
            ApplybasedonIndex(current_index);
        }
        //if (current_index>=i)
        //current_index  ++;
    }
    public void PrevIndex(int i)
    {
        if (current_index >= i)
            current_index--;
    }
    public void reset()
    {
        Debug.LogError("Reset called");
        if (current_index >= 0)
            resetbasedonIndex(current_index);
        current_index = -1;
    }
    bool allow;
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            allow = false;
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).gameObject.activeInHierarchy)
                    {
                        allow = true;
                    }

                }
            }
            else
            {
                allow = true;
            }
            if (allow)
                if (NavigatableObjects.Count > 0)
                {

                    ApplybasedonIndex(current_index);


                }
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            if (allow)
                if (NavigatableObjects.Count > 0)
                {

                    EnterbasedonIndex(current_index);


                }
        }
    }


    private void resetbasedonIndex(int current_index)
    {
        system.SetSelectedGameObject(null, new BaseEventData(system));
        Debug.LogError(current_index + " Index type  " + NavigatableObjects[current_index].Type);

        switch (NavigatableObjects[current_index].Type)
        {
            case Navigatable.objecttype.Button:

                NavigatableObjects[current_index].buttonObject.apply_normal();
                break;
            case Navigatable.objecttype.MenuButton:

                NavigatableObjects[current_index].menuButtonObject.apply_normal();
                break;
            case Navigatable.objecttype.Selecteable:

                NavigatableObjects[current_index].selectableObject.apply_normal();
                system.SetSelectedGameObject(null, new BaseEventData(system));
                break;
            case Navigatable.objecttype.Inputfield:

                NavigatableObjects[current_index].selectableObject.apply_normal();
                system.SetSelectedGameObject(null, new BaseEventData(system));
                break;
            case Navigatable.objecttype.Text:
                if (NavigatableObjects[current_index].Textobject.oveerideFont)
                {
                    NavigatableObjects[current_index].Textobject.textObject.font = NavigatableObjects[current_index].Textobject.Normal;
                }
                else
                {
                    NavigatableObjects[current_index].Textobject.textObject.font = Normal;
                }
                Debug.LogError("Apply Normal");
                break;
            case Navigatable.objecttype.TextMeshPro:
                if (NavigatableObjects[current_index].TmpproObject.oveerideFont)
                {
                    NavigatableObjects[current_index].TmpproObject.textObject.font = NavigatableObjects[current_index].TmpproObject.NormalTmp;
                }
                else
                {
                    NavigatableObjects[current_index].TmpproObject.textObject.font = NormalTmp;
                }
                break;
            case Navigatable.objecttype.toggle:
                if (NavigatableObjects[current_index].toggleObject.oveerideFont)
                {
                    NavigatableObjects[current_index].toggleObject.textObject.font = NavigatableObjects[current_index].toggleObject.NormalTmp;
                }
                else
                {
                    NavigatableObjects[current_index].toggleObject.textObject.font = NormalTmp;
                }
                system.SetSelectedGameObject(null, new BaseEventData(system));
                break;

            case Navigatable.objecttype.EditInputField:

                if (NavigatableObjects[current_index].EditInputfieldobject.oveerideFont)
                {
                    NavigatableObjects[current_index].EditInputfieldobject.textObject.font = NavigatableObjects[current_index].EditInputfieldobject.NormalTmp;
                }
                else
                {
                    NavigatableObjects[current_index].EditInputfieldobject.textObject.font = NormalTmp;
                }
                if (NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.transform.parent.transform.childCount > 1)
                {
                    GameObject obj = NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.transform.parent.GetChild(1).gameObject;
                    if (obj)
                        if (obj.activeInHierarchy && obj.GetComponent<UnityEngine.UI.Button>())
                        {
                            obj.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                        }
                }

                break;
            case Navigatable.objecttype.Dropdown:

                NavigatableObjects[current_index].Dropdownobject.apply_normal();

                NavigatableObjects[current_index].Dropdownobject.SelectableObject.Hide();
                //  system.SetSelectedGameObject(null, new BaseEventData(system));
                break;
            case Navigatable.objecttype.togglebtn:

                if (NavigatableObjects[current_index].EditInputfieldobject.oveerideFont)
                {
                    NavigatableObjects[current_index].EditInputfieldobject.textObject.font = NavigatableObjects[current_index].EditInputfieldobject.NormalTmp;
                }
                else
                {
                    NavigatableObjects[current_index].EditInputfieldobject.textObject.font = NormalTmp;
                }
                break;
        }
    }

    private void EnterbasedonIndex(int current_index)
    {
        Debug.LogError("EnterbasedonIndex clicked   " + current_index);
        if (current_index == -1)
        {
            ApplybasedonIndex(current_index);
            current_index = 0;

        }
        switch (NavigatableObjects[current_index].Type)
        {
            case Navigatable.objecttype.Button:
                if (NavigatableObjects[current_index].buttonObject.ButtonObject.isActiveAndEnabled && NavigatableObjects[current_index].buttonObject.ButtonObject.interactable && NavigatableObjects[current_index].buttonObject.ButtonObject.gameObject.activeInHierarchy)
                    NavigatableObjects[current_index].buttonObject.ButtonObject.onClick.Invoke();
                break;
            case Navigatable.objecttype.MenuButton:
                //if (NavigatableObjects[current_index].menuButtonObject.MenuButtonObject.isActiveAndEnabled && NavigatableObjects[current_index].menuButtonObject.MenuButtonObject.Interactable && NavigatableObjects[current_index].menuButtonObject.MenuButtonObject.gameObject.activeInHierarchy)
                //    NavigatableObjects[current_index].menuButtonObject.MenuButtonObject.onClickEvent.Invoke();
                break;
            case Navigatable.objecttype.Selecteable:
                if (NavigatableObjects[current_index].selectableObject.SelectableObject.isActiveAndEnabled && NavigatableObjects[current_index].selectableObject.SelectableObject.interactable && NavigatableObjects[current_index].selectableObject.SelectableObject.gameObject.activeInHierarchy)
                    system.SetSelectedGameObject(NavigatableObjects[current_index].selectableObject.SelectableObject.gameObject, new BaseEventData(system));
                ApplybasedonIndex(current_index);
                break;

            case Navigatable.objecttype.toggle:
                if (NavigatableObjects[current_index].toggleObject.togleObject.isActiveAndEnabled && NavigatableObjects[current_index].toggleObject.togleObject.interactable && NavigatableObjects[current_index].toggleObject.togleObject.gameObject.activeInHierarchy)
                    if (NavigatableObjects[current_index].toggleObject.togleObject.isOn)
                    {
                        NavigatableObjects[current_index].toggleObject.togleObject.onValueChanged.Invoke(true);
                    }
                    else
                        NavigatableObjects[current_index].toggleObject.togleObject.isOn = true;

                break;
            case Navigatable.objecttype.EditInputField:
                if (NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.isActiveAndEnabled && NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.gameObject.activeInHierarchy)
                    NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.onClick.Invoke();
                //  system.SetSelectedGameObject(NavigatableObjects[current_index].selectableObject.SelectableObject.gameObject, new BaseEventData(system));

                break;
            case Navigatable.objecttype.Dropdown:
                if (NavigatableObjects[current_index].Dropdownobject.SelectableObject.isActiveAndEnabled && NavigatableObjects[current_index].Dropdownobject.SelectableObject.gameObject.activeInHierarchy && NavigatableObjects[current_index].Dropdownobject.SelectableObject.interactable)
                {
                    system.SetSelectedGameObject(NavigatableObjects[current_index].Dropdownobject.SelectableObject.gameObject, new BaseEventData(system));
                    NavigatableObjects[current_index].Dropdownobject.SelectableObject.Show();
                    //if(HomeScene != null)
                    //HomeScene.OnDropdownClick();
                }
                break;
            case Navigatable.objecttype.Text:
                ApplybasedonIndex(current_index);
                break;
            case Navigatable.objecttype.TextMeshPro:
                ApplybasedonIndex(current_index);

                break;
            case Navigatable.objecttype.togglebtn:
                if (NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.isActiveAndEnabled && NavigatableObjects[current_index].buttonObject.ButtonObject.interactable && NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.gameObject.activeInHierarchy)
                    NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.onClick.Invoke();
                break;
        }
    }
    private void ApplybasedonIndex(int index)
    {
        if (index >= 0)
            resetbasedonIndex(index);

        current_index++;
        if (current_index >= NavigatableObjects.Count)
        {
            current_index = 0;
        }
        int loop = 0;
        bool navigated = false;
        while (navigated == false)
        {

            switch (NavigatableObjects[current_index].Type)
            {
                case Navigatable.objecttype.Button:
                    if (NavigatableObjects[current_index].buttonObject.ButtonObject.isActiveAndEnabled && NavigatableObjects[current_index].buttonObject.ButtonObject.interactable && NavigatableObjects[current_index].buttonObject.ButtonObject.gameObject.activeInHierarchy)
                    {
                        NavigatableObjects[current_index].buttonObject.apply_glow();
                        navigated = true;
                        return;
                    }
                    break;
                case Navigatable.objecttype.MenuButton:
                    //if (NavigatableObjects[current_index].menuButtonObject.MenuButtonObject.isActiveAndEnabled && NavigatableObjects[current_index].menuButtonObject.MenuButtonObject.Interactable && NavigatableObjects[current_index].menuButtonObject.MenuButtonObject.gameObject.activeInHierarchy)
                    //{
                    //    NavigatableObjects[current_index].menuButtonObject.apply_glow();
                    //    navigated = true;
                    //    return;
                    //}
                    break;
                case Navigatable.objecttype.Selecteable:
                    if (NavigatableObjects[current_index].selectableObject.SelectableObject.isActiveAndEnabled && NavigatableObjects[current_index].selectableObject.SelectableObject.interactable && NavigatableObjects[current_index].selectableObject.SelectableObject.gameObject.activeInHierarchy)
                    {
                        NavigatableObjects[current_index].selectableObject.apply_glow();
                        system.SetSelectedGameObject(NavigatableObjects[current_index].selectableObject.SelectableObject.gameObject, new BaseEventData(system));
                        navigated = true;
                        return;
                    }
                    break;
                case Navigatable.objecttype.Inputfield:
                    if (NavigatableObjects[current_index].selectableObject.SelectableObject.isActiveAndEnabled && NavigatableObjects[current_index].selectableObject.SelectableObject.interactable && NavigatableObjects[current_index].selectableObject.SelectableObject.gameObject.activeInHierarchy)
                    {
                        NavigatableObjects[current_index].selectableObject.apply_glow();
                        system.SetSelectedGameObject(NavigatableObjects[current_index].selectableObject.SelectableObject.gameObject, new BaseEventData(system));
                        navigated = true;
                        return;
                    }
                    break;
                case Navigatable.objecttype.Text:
                    if (NavigatableObjects[current_index].Textobject.textObject.isActiveAndEnabled && NavigatableObjects[current_index].Textobject.textObject.gameObject.activeInHierarchy)
                    {
                        if (NavigatableObjects[current_index].Textobject.oveerideFont)
                        {
                            NavigatableObjects[current_index].Textobject.textObject.font = NavigatableObjects[current_index].Textobject.glowFont;
                        }
                        else
                        {
                            NavigatableObjects[current_index].Textobject.textObject.font = glowFont;
                        }
                        navigated = true;
                        return;
                    }
                    break;
                case Navigatable.objecttype.TextMeshPro:
                    if (NavigatableObjects[current_index].TmpproObject.textObject.isActiveAndEnabled && NavigatableObjects[current_index].TmpproObject.textObject.gameObject.activeInHierarchy)
                    {
                        if (NavigatableObjects[current_index].TmpproObject.oveerideFont)
                        {
                            NavigatableObjects[current_index].TmpproObject.textObject.font = NavigatableObjects[current_index].TmpproObject.glowFontTmp;
                        }
                        else
                        {
                            NavigatableObjects[current_index].TmpproObject.textObject.font = glowFontTmp;
                        }
                        navigated = true;
                        return;
                    }
                    break;
                case Navigatable.objecttype.toggle:
                    if (NavigatableObjects[current_index].toggleObject.togleObject.isActiveAndEnabled && NavigatableObjects[current_index].toggleObject.togleObject.interactable && NavigatableObjects[current_index].toggleObject.togleObject.gameObject.activeInHierarchy)
                    {
                        if (NavigatableObjects[current_index].toggleObject.oveerideFont)
                        {
                            NavigatableObjects[current_index].toggleObject.textObject.font = NavigatableObjects[current_index].toggleObject.glowFontTmp;
                        }
                        else
                        {
                            NavigatableObjects[current_index].toggleObject.textObject.font = glowFontTmp;
                        }
                        //  system.SetSelectedGameObject(NavigatableObjects[current_index].toggleObject.togleObject.gameObject, new BaseEventData(system));
                        navigated = true;
                        return;
                    }
                    break;
                case Navigatable.objecttype.EditInputField:
                    if (NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.isActiveAndEnabled && NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.gameObject.activeInHierarchy)
                    {
                        if (NavigatableObjects[current_index].EditInputfieldobject.oveerideFont)
                        {
                            NavigatableObjects[current_index].EditInputfieldobject.textObject.font = NavigatableObjects[current_index].EditInputfieldobject.glowFontTmp;
                        }
                        else
                        {
                            NavigatableObjects[current_index].EditInputfieldobject.textObject.font = glowFontTmp;
                        }

                        navigated = true;
                        return;
                    }
                    break;
                case Navigatable.objecttype.Dropdown:
                    if (NavigatableObjects[current_index].Dropdownobject.SelectableObject.isActiveAndEnabled && NavigatableObjects[current_index].Dropdownobject.SelectableObject.gameObject.activeInHierarchy && NavigatableObjects[current_index].Dropdownobject.SelectableObject.interactable)
                    {
                        NavigatableObjects[current_index].Dropdownobject.apply_glow();
                        navigated = true;
                        return;
                    }
                    break;
                case Navigatable.objecttype.togglebtn:

                    if (NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.isActiveAndEnabled && NavigatableObjects[current_index].buttonObject.ButtonObject.interactable && NavigatableObjects[current_index].EditInputfieldobject.ButtonObject.gameObject.activeInHierarchy)
                    {
                        if (NavigatableObjects[current_index].EditInputfieldobject.oveerideFont)
                        {
                            NavigatableObjects[current_index].EditInputfieldobject.textObject.font = NavigatableObjects[current_index].EditInputfieldobject.glowFontTmp;
                        }
                        else
                        {
                            NavigatableObjects[current_index].EditInputfieldobject.textObject.font = glowFontTmp;
                        }

                        navigated = true;
                        return;
                    }
                    break;
            }
            if (navigated) { return; }
            if (current_index >= NavigatableObjects.Count - 1)
            {
                current_index = 0;
                loop++;
                if (loop > 2) { break; }
            }
            else
                current_index++;
        }
    }

}
[Serializable]
public class Navigatable
{
    public enum objecttype { Text, Button, Selecteable, MenuButton, Slider, TextMeshPro, EditInputField, toggle, togglebtn, Dropdown, Inputfield }
    public objecttype Type;

    public Tmpproperties TmpproObject;
    public textproperties Textobject;
    public Buttonproperties buttonObject;
    public SelectableButtonproperties selectableObject;
    public MenuButtonproperties menuButtonObject;
    public Toggleproperties toggleObject;
    public EditInputfieldproperties EditInputfieldobject;
    public DropdownButtonproperties Dropdownobject;
}



[Serializable]
public class textproperties : Txtproperties
{
    public Text textObject;

}
[Serializable]
public class Tmpproperties : Txtproperties
{
    public TextMeshProUGUI textObject;

}
[Serializable]
public class Toggleproperties : Txtproperties
{
    public Toggle togleObject;
    public TextMeshProUGUI textObject;

}
[Serializable]
public class EditInputfieldproperties : Txtproperties
{
    public UnityEngine.UI.Button ButtonObject;
    public TextMeshProUGUI textObject;

}

[Serializable]
public class MenuButtonproperties : Btnproperties
{
   // public MenuButton MenuButtonObject;

}
[Serializable]
public class SelectableButtonproperties : Btnproperties
{
    public Selectable SelectableObject;

}
[Serializable]
public class DropdownButtonproperties : Btnproperties
{

    public TMP_Dropdown SelectableObject;

}

[Serializable]
public class Buttonproperties : Btnproperties
{
    public UnityEngine.UI.Button ButtonObject;

}


[Serializable]
public class Btnproperties
{
    public Image image;
    public Sprite glowSprite;
    public Sprite NormalSprite;

    public void apply_glow() { image.sprite = glowSprite; }
    public void apply_normal() { image.sprite = NormalSprite; }
}

[Serializable]
public class Txtproperties
{
    public bool oveerideFont;
    public TMP_FontAsset glowFontTmp;
    public TMP_FontAsset NormalTmp;
    public Font glowFont;
    public Font Normal;

}
public class ConditionalPropertyAttribute : PropertyAttribute
{

    public string condition;

    public ConditionalPropertyAttribute(string condition)
    {
        this.condition = condition;
    }
}
