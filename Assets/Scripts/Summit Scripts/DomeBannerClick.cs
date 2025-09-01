using Photon.Pun.Demo.PunBasics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class DomeBannerClick : MonoBehaviour, IPointerDownHandler
{
    public int DomeId;

    private bool IsClicked;
    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.LogError(eventData.clickCount+"---"+eventData.lastPress+"---"+eventData);
        if (PlayerController.isJoystickDragging || IsClicked || MutiplayerController.instance.connectionState != ServerConnectionStates.ConnectedToServer)
        {
            return; // Do nothing if the joystick is dragging
        }

        SendVideoEvent();
    }


    async void SendVideoEvent()
    {
        await Task.Delay(200);
        if(!IsClicked)
        {
            SummitDomeImageHandler.ShowNftData?.Invoke(DomeId);
        }
        IsClicked = true;
        await Task.Delay(2000);
        IsClicked = false;
    }
}

