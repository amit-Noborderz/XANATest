using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TextSetPlayer : MonoBehaviour

{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI leagueText;
    public TextMeshProUGUI rankTextHome;
    public TextMeshProUGUI ratingTextP;
    public TextMeshProUGUI nameTextP;
    public TextMeshProUGUI leagueTextP;
    public Image playerprofile;
    public void SetUI(int i)
    {
        //rankText.text = rankTextP;
        //ratingText.text = ratingTextP.text;
        //nameText.text = nameTextP.text;
        //leagueText.text = leagueTextP.text;

        if (i == 1)
        {
            rankText.text = 1 + "<size=50>st</size>";
        }
        else if (i == 2)
        {
            rankText.text = 2 + "<size=50>nd</size>";
        }
        else if (i == 3)
        {
            rankText.text = 3 + "<size=50>rd</size>";
        }
        else
        {
            rankText.text = i + "<size=50>th</size>";
        }
    }
    private void OnEnable()
    {
        setdefault();
       // ratingText.text = LeagueManager.Instance.getUserDetails.data.battle_data.leaguePoints.ToString("#,##0");
        nameText.text = nameTextP.text;
        //leagueText.text = FindObjectOfType<HomeScene>().league;
    }
    public void setdefault()
    {
      //  rankText.text = LeagueManager.Instance.playerRank;
    }
    public IEnumerator setImage(string url)
    {
        Texture2D texture = new Texture2D(playerprofile.sprite.texture.width, playerprofile.sprite.texture.height);//Player.sprite.texture as Texture2D;

        WWW www = new WWW(url);
        yield return www;

        // calling this function with StartCoroutine solves the problem
        Debug.Log("Why on earh is this never called?");
        if (www.error!=null)
        {
            Debug.LogError("Image Not Found.................." + www.error);
            yield break;
        }
        www.LoadImageIntoTexture(texture);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
        playerprofile.sprite = sprite;
        www.Dispose();
        www = null;
    }
    public void GlobalToLeagueFix()
    {
        //rankText.text = LeagueManager.Instance.playerRank;
    }
}
