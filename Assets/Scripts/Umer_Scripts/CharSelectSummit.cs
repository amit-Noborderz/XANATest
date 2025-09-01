using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;
using DG.Tweening;
using System;
using BD;


public class CharSelectSummit : MonoBehaviour
{
    public Transform contentParent;
    public GameObject backBtnstore;
    private GameObject SelectedOBJ;
    HorizontalScrollSnap hssRef;
    public List<GameObject> items;
    public static CharSelectSummit instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        var opp = PlayerPrefs.GetInt("OpponentID", -1);
        Debug.Log("ooponent provile Id " + opp);
        hssRef = this.GetComponent<HorizontalScrollSnap>();
        for (int i = 0; i < items.Count; i++)
        {
            if (i == opp) continue;
            GameObject itemGameObject = Instantiate(items[i], contentParent);
        }
        /* (GameObject item in items)
         {

             //itemGameObject.transform.GetChild(0).AddComponent<Canvas>().overrideSorting = false;
         }*/
    }

    bool firstTime = true;
    private void OnEnable()
    {
        if (!hssRef.enabled)
        {
            hssRef.enabled = true;
        }

        if (GameManager.Instance.UiManager.isAvatarSelectionBtnClicked)
        {
            backBtnstore.SetActive(true);
        }
        else
        {
            backBtnstore.SetActive(false);
        }


        if (firstTime)
        {
            firstTime = false;
            Invoke(nameof(SetPoition), 0.1f);
        }
        else
        {
            CharacterSelectionChange();
        }

    }


    void SetPoition()
    {

        hssRef.CurrentPage = hssRef.indexToStart;
        CharacterSelectionChange();
    }

    public Vector3 selectedScale = new Vector3(1.75f, 1.45f, 0.1f);
    public Vector3 unselectedScale = new Vector3(1.2f, 1.2f, 0.1f);
    public Vector3 selectedInnerScale = new Vector3(0.90f, 1.01f, 0.1f);
    public Vector3 unselectedInnerScale = new Vector3(1.01f, 1.01f, 0.1f);
    public Vector3 neighborScale = new Vector3(1.3f, 1.3f, 0.1f);

    public float selectedYPos = -6f;
    public float unselectedYPos = -60f;
    public float tweenDuration = 0.1f;

    public int selectedSortingOrder = 6;


    public void CharacterSelectionChange()
    {
        int childCount = contentParent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = contentParent.GetChild(i).GetChild(0);
            bool isCurrentPage = hssRef.CurrentPage == i;

            child.DOScale(isCurrentPage ? selectedScale : unselectedScale, tweenDuration);
            child.localPosition = new Vector3(child.localPosition.x, isCurrentPage ? selectedYPos : unselectedYPos, child.localPosition.z);
            child.GetChild(0).DOScale(isCurrentPage ? selectedInnerScale : unselectedInnerScale, tweenDuration);
            child.GetComponent<Image>().enabled = isCurrentPage;

            if (isCurrentPage)
            {
                SelectedOBJ = child.gameObject;
                Canvas canvas = SelectedOBJ.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = SelectedOBJ.AddComponent<Canvas>();
                }
                canvas.overrideSorting = true;
                canvas.sortingOrder = selectedSortingOrder;

                if (i == 0)
                {
                    contentParent.GetChild(1).GetChild(0).GetComponent<Image>().enabled = false;
                }
                else if (i == childCount - 1)
                {
                    contentParent.GetChild(i - 1).GetChild(0).DOScale(neighborScale, tweenDuration);
                }
                else
                {
                    contentParent.GetChild(i - 1).GetChild(0).DOScale(neighborScale, tweenDuration);
                    contentParent.GetChild(i + 1).GetChild(0).DOScale(neighborScale, tweenDuration);
                }


            }
            else
            {
                Canvas canvas = child.GetComponent<Canvas>();
                if (canvas != null)
                {
                    Destroy(canvas);
                }
            }
        }

    }

    public void OnClickNext()
    {
        if (SelectedOBJ != null)
        {
            GameManager.Instance.HomeCameraInputHandler(true);
            //if (!ConstantsHolder.xanaConstants.SwitchXanaToXSummit)
            //{
            //    UserLoginSignupManager.instance.SelectedPresetImage.sprite = SelectedOBJ.transform.GetChild(0).GetComponent<Image>().sprite;
            //    UserLoginSignupManager.instance.SelectPresetImageforEditProfil.sprite = SelectedOBJ.transform.GetChild(0).GetComponent<Image>().sprite;

            //}
            //UserRegisterationManager.instance.LogoImage.GetComponent<Image>().sprite = SelectedOBJ.transform.GetChild(0).GetComponent<Image>().sprite;
            //UserRegisterationManager.instance.LogoImage2.GetComponent<Image>().sprite = SelectedOBJ.transform.GetChild(0).GetComponent<Image>().sprite;
            //UserRegisterationManager.instance.LogoImage3.GetComponent<Image>().sprite = SelectedOBJ.transform.GetChild(0).GetComponent<Image>().sprite;
            //Debug.LogError("selected obj name :- "+SelectedOBJ.name);
            SelectedOBJ.GetComponent<PresetData_Jsons>().ChangecharacterFromPresetPanel();
            GameManager.Instance.HomeCamera.GetComponent<HomeCameraController>().CenterAlignCam();
            if (ConstantsHolder.xanaConstants.LoggedInAsGuest || ConstantsHolder.xanaConstants.SwitchXanaToXSummit)
            {
                UserLoginSignupManager.instance.UserNameFieldObj.SetActive(false);
            }
            else
            {
                UserLoginSignupManager.instance.UserNameFieldObj.SetActive(true);
            }
        }
    }

}