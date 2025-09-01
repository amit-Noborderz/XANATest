using System.Threading.Tasks;
using UnityEngine;

public class PresentationAdminAccessCheck : MonoBehaviour
{
    public int AreaId;
    public GameObject UploadButton;
    public GameObject PlayerListButton;
    public bool ShowUploadButton;
    public bool ShowPlayerListButton;
    public XANASummitDataContainer XANASummitDataContainer;

    private async void OnEnable()
    {
        if(await CheckAdminAccess())
        {
            if (ShowUploadButton && UploadButton)
                UploadButton.SetActive(true);
            else if(UploadButton)
                UploadButton.SetActive(false);

            if (ShowPlayerListButton && PlayerListButton)
                PlayerListButton.SetActive(true);
            else if(PlayerListButton)
                PlayerListButton.SetActive(false);
        }
    }

    private void OnDisable()
    {
        
    }

    async Task<bool> CheckAdminAccess()
    {
        XANASummitDataContainer.PresentationTab presentationTab =await XANASummitDataContainer.GetPresentationTabData(ConstantsHolder.domeId.ToString());
        if (presentationTab!=null )
        {
            if (presentationTab.data.Length>0 && presentationTab.data[0].isAccessGiven)
            {
                for (int i=0;i< presentationTab.data.Length;i++)
                {
                    if (presentationTab.data[i].area_id==AreaId)
                    {
                        if (!string.IsNullOrEmpty(presentationTab.data[i].email) && presentationTab.data[i].email == PlayerPrefs.GetString("LoggedInMail"))
                            return true;
                        else if (!string.IsNullOrEmpty(presentationTab.data[i].wallet_Address) && presentationTab.data[i].wallet_Address == PlayerPrefs.GetString("publicID"))
                            return true;
                        else
                            return false;
                    }

                }
            }
        }
        return true;
    }

}
