using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using TCGServerClasses;
using TMPro;
using UnityEngine.UI;

public class LeaderboardPlayerDetails : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI leagueName;
    public TextMeshProUGUI battlesText;
    public TextMeshProUGUI winsText;
    public TextMeshProUGUI maxStreakText;
    public TextMeshProUGUI rewardText;
  //  public UserDetailsData userDetailsData;
    public Image playerprofile;

    public void PopulateInfo(int rank)
    {
        //playerName.text = userDetailsData.name;
        //if (!string.IsNullOrEmpty(userDetailsData.tcgAvatar))
        //{
        //    StartCoroutine(setImage(userDetailsData.tcgAvatar));
        //}

        //string tempstr = userDetailsData.userLeagues[0].league.name.Replace(" ", string.Empty);
        //string withoutlastchar = tempstr.Remove(tempstr.Length - 1);
        //var lastchar = tempstr[tempstr.Length - 1];
        //if (char.IsDigit(lastchar))
        //    leagueName.text = TextLocalization.GetLocaliseTextByKey(withoutlastchar.Trim()) + " " + lastchar;
        //else
        //    leagueName.text = TextLocalization.GetLocaliseTextByKey(userDetails.userLeagues[0].league.name);

        rankText.text = rank.ToString();
        //ratingText.text = userDetailsData.rating.ToString();
        //battlesText.text = userDetailsData.battles.ToString();
        //winsText.text = userDetailsData.wins.ToString();
        //maxStreakText.text = userDetailsData.maxStreak.ToString();
        //rewardText.text = userDetailsData.userLeagues[0].rewards.ToString();
      

    }
    public IEnumerator setImage(string url)
    {
        Texture2D texture = new Texture2D(playerprofile.sprite.texture.width, playerprofile.sprite.texture.height);//Player.sprite.texture as Texture2D;

        WWW www = new WWW( url);
        yield return www;

        // calling this function with StartCoroutine solves the problem
        Debug.Log("Why on earh is this never called?");
        if (www.error != null)
        {
            Debug.LogError("null found..........................." + www.error);
            yield break;
        }
        www.LoadImageIntoTexture(texture);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
        playerprofile.sprite = sprite;
        www.Dispose();
        www = null;
    }

}
