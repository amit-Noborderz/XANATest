using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetImageInfo : MonoBehaviour
{
    public RawImage Thumbnail;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;

    public void Init(Texture2D _Thumbnail,string _Name, string _Description)
    {
        Thumbnail.texture = _Thumbnail;
        Name.text = _Name;
        Description.text = _Description;
    }
}
