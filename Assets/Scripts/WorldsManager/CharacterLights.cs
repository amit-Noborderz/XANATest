using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
public class CharacterLights : MonoBehaviour
{
    //public GameObject characterLights;
    //public GameObject vtuberLight;

    // Start is called before the first frame update
    void Start()
    {
        //if (!GetComponent<PhotonAnimatorView>().photonView.IsMine)
        //{
        //    characterLights.SetActive(false);
        //}
        //VtuberLightForOtherCharacters();
    }

    public void VtuberLightForOtherCharacters()
    {
        //int activeVtuberLightCount = 0;
        //foreach (GameObject player in MutiplayerController.instance.playerobjects)
        //{
        //    if (player == null) continue;

        //    var cbp = player.GetComponent<CharacterBodyParts>();
        //    var characterLights = player.GetComponent<CharacterLights>();

        //    if (cbp != null && (cbp.AvatarGender == AvatarGender.VTuber_Male || cbp.AvatarGender == AvatarGender.VTuber_Female))
        //    {
        //        if (characterLights != null && characterLights.vtuberLight != null && characterLights.vtuberLight.activeInHierarchy)
        //        {
        //            activeVtuberLightCount++;
        //            if (activeVtuberLightCount > 1)
        //            {
        //                characterLights.vtuberLight.SetActive(false);
        //            }
        //        }
        //    }
        //}

        // Disable your light if more than one active VTuber light is found
       // EnableLights(activeVtuberLightCount <= 1);
    }
    void EnableLights(bool status)
    {
        //if (vtuberLight)
        //    vtuberLight.SetActive(status);
    }
}