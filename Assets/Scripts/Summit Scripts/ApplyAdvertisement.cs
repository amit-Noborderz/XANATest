using System.Threading.Tasks;
using UnityEngine;

public class ApplyAdvertisement : MonoBehaviour
{
    public bool IsPotrait;
    public int GroupId;

    private void OnEnable()
    {
        BuilderEventManager.UpdateAdvertise += UpdateAdvertisement;
    }

    private void OnDisable()
    {
        BuilderEventManager.UpdateAdvertise += UpdateAdvertisement;
    }

    async void UpdateAdvertisement()
    {
        await Task.Delay(Random.Range(0, 5000));
        if (this == null || !this.gameObject)
            return;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            return;

        try
        {
            meshRenderer.material.mainTexture=AdBoardController.AdBoardControllerInstance.AdvertisementImageHolder.GetImageTexture(GroupId,IsPotrait);
        }
        catch(System.Exception e)
        {
            Debug.LogError("AdBoard Controller Instance ");
        }

    }
}
