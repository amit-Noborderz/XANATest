using SuperStar.Helpers;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SummitDomeShaderApply : MonoBehaviour
{
    [HideInInspector]
    public int DomeId;
    [HideInInspector]
    public string ImageUrl;
    [HideInInspector]
    public string LogoUrl;
    public GameObject DomeBannerParent;
    public GameObject InnerDomeText;
    public GameObject OuterDomeText;
    public GameObject Frame;
    public MeshRenderer DomeMeshRenderer;
    public SpriteRenderer LogoSpriteRenderer;
    public DomeBannerClick clickListener;
    public bool smallDome;
    public float TextValueSmall;
    public float TextValueMid;
    public async void Init()
    {
        DomeBannerParent.SetActive(true);
        clickListener.DomeId = DomeId;
        if (!string.IsNullOrEmpty(ImageUrl))
        {
            if (DomeId != 113 && DomeId != 112)
                ImageUrl = ImageUrl + "?width=" + ConstantsHolder.DomeImageCompression;
            DownloadDomeTexture(ImageUrl);
        }
        else
        {
            float targetY = smallDome ? TextValueSmall : TextValueMid;

            // Update InnerDomeText position safely
            if (InnerDomeText != null)
            {
                Transform innerTransform = InnerDomeText.transform;
                innerTransform.localPosition = new Vector3(0, targetY, innerTransform.localPosition.z);
            }

            // Update OuterDomeText position safely
            if (OuterDomeText != null)
            {
                Transform outerTransform = OuterDomeText.transform;
                outerTransform.localPosition = new Vector3(0, targetY, outerTransform.localPosition.z);
            }
        }
        if (!string.IsNullOrEmpty(LogoUrl))
        {
            if (DomeId != 113 && DomeId != 112)
                ImageUrl = ImageUrl + "?width=" + ConstantsHolder.DomeImageCompression;
            DownloadLogoTexture(LogoUrl);
        }
    }

    private Sprite ConvertToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
    private float targetWidth = 1.8f;
    private void ScaleSpriteToTargetSize()
    {
        if (smallDome)
            targetWidth = 1.5f;

        if (LogoSpriteRenderer == null || LogoSpriteRenderer.sprite == null) return;

        // Get the native size of the sprite
        float spriteWidth = LogoSpriteRenderer.sprite.bounds.size.x;
        float spriteHeight = LogoSpriteRenderer.sprite.bounds.size.y;

        // Calculate the aspect ratio
        float aspectRatio = spriteWidth / spriteHeight;

        // Calculate the target height based on the target width and aspect ratio
        float targetHeight = targetWidth / aspectRatio;

        // Calculate the scale factor needed to achieve the target size
        Vector3 scale = LogoSpriteRenderer.transform.localScale;
        scale.x = targetWidth / spriteWidth;
        scale.y = targetHeight / spriteHeight;

        // Apply the scale to the sprite
        LogoSpriteRenderer.transform.localScale = scale;
    }

    void DownloadDomeTexture(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            if (AssetCache.Instance.HasFile(url))
            {
                SetDomeTexture(url);
            }
            else
            {
                AssetCache.Instance.EnqueueOneResAndWait(url, url, (success) =>
                {
                    if (success)
                    {
                        SetDomeTexture(url);
                    }
                });
            }
        }
    }

    void SetDomeTexture(string url)
    {
        if (DomeMeshRenderer != null)
        {
            Texture2D texture = AssetCache.Instance.LoadImage(url);
            if (DomeMeshRenderer.materials.Count() > 0)
            {
                DomeMeshRenderer.material.mainTexture = texture;
            }
            DomeMeshRenderer.gameObject.SetActive(true);
        }
        Frame.SetActive(true);
        TurnOffFrameEmission();
    }
    void TurnOffFrameEmission()
    {
        if (Frame != null)
        {
            Renderer frameRenderer = Frame.GetComponent<Renderer>();
            if (frameRenderer != null && frameRenderer.material.HasProperty("_EmissionColor"))
            {
                // Set the emission color to black to turn it off
                frameRenderer.material.SetColor("_EmissionColor", Color.black);

                // Disable the emission keyword
                frameRenderer.material.DisableKeyword("_EMISSION");

                frameRenderer.material.color = Color.white;
            }
        }
    }
    void DownloadLogoTexture(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            if (AssetCache.Instance.HasFile(url))
            {
                SetLogoTexture(url);
            }
            else
            {
                AssetCache.Instance.EnqueueOneResAndWait(url, url, (success) =>
                {
                    if (success)
                    {
                        SetLogoTexture(url);
                    }
                });
            }
        }
    }


    void SetLogoTexture(string url)
    {
        Texture2D texture = AssetCache.Instance.LoadImage(url);
        if (texture != null && LogoSpriteRenderer != null)
        {
            LogoSpriteRenderer.sprite = ConvertToSprite(texture);
            LogoSpriteRenderer.material.shader = Shader.Find("Sprites/Default");
            ScaleSpriteToTargetSize();
        }
    }
}
