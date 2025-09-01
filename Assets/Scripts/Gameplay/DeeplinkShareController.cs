using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices; // Add this line for DllImportAttribute

public class DeeplinkShareController : MonoBehaviour
{
    [SerializeField] GameObject copyTextPanel;
    string joinLink = "https://xanapord.page.link/?link=https://www.xana.net/Join%3D";
    string summitLink = "https://xanapord.page.link/?link=https://www.xana.net/data%3DENV";
    string deepLink = "";
    string msgToShow = "";
    private bool isShareSheetOpen = false; // Flag to track if the share sheet is open
    void Start()
    {
        //copyTextPanel.SetActive(false);
    }

    void SetData()
    {
        print("! SET DATA CALL");
        if (ConstantsHolder.isFromXANASummit)
        {
            deepLink = summitLink + ConstantsHolder.domeId.ToString() + "&apn=com.nbi.xana&isi=6642649722&ibi=com.wujie.xsummit&efr=1";
        }
        else
        {
            deepLink = joinLink + ConstantsHolder.xanaConstants.MuseumID + "&apn=com.nbi.xana&isi=6642649722&ibi=com.wujie.xsummit";
        }

        if (LocalizationManager.forceJapanese || GameManager.currentLanguage == "ja")
        {
            msgToShow = "こんにちは、XANAのこの素晴らしいスペースをご覧ください。下のリンクをクリックして、没入感あふれる体験をお楽しみください。";
        }
        else
        {
            msgToShow = "Hi, check out this amazing space in XANA! Dive into an immersive experience by clicking the link below.";
        }
        print("!END SET DATA CALL");
    }

    public void ShareDeepLink()
    {
        if (isShareSheetOpen)
        {
            Debug.Log("Share sheet is already open.");
            return;
        }
        SetData();
        CopyDeepLink();
        print("Deeplink : " + deepLink);

        ShareSheet shareSheet = ShareSheet.CreateInstance();
        if (!string.IsNullOrEmpty(msgToShow))
        {
            shareSheet.AddText(msgToShow);
        }

        // Add screenshot if available
        try
        {
            shareSheet.AddScreenshot();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to add screenshot: " + ex.Message);
        }

        if (!string.IsNullOrEmpty(deepLink))
        {
            //shareSheet.AddURL(new URLString(deepLink));

            shareSheet.AddURL(URLString.URLWithPath(deepLink));
        }

        shareSheet.SetCompletionCallback((result, error) =>
        {
            Debug.Log("Share Sheet was closed. Result code: " + result.ResultCode);
            isShareSheetOpen = false; // Reset the flag when the share sheet is closed
            if (error != null)
            {
                Debug.LogError("Error: " + error);
            }
        });
        isShareSheetOpen = true; // Set the flag when the share sheet is opened
        StartCoroutine(ShowShareSheetAfterDelay(shareSheet));
    }

    private IEnumerator ShowShareSheetAfterDelay(ShareSheet shareSheet)
    {
        //copyTextPanel.SetActive(true);
        //yield return new WaitForSeconds(2);
        //copyTextPanel.SetActive(false);
        shareSheet.Show();
        yield return null;
    }

    void CopyDeepLink()
    {
        string textToCopy = msgToShow + "\n" + deepLink;
        // SetData();
#if UNITY_EDITOR
        GUIUtility.systemCopyBuffer = textToCopy;
        Debug.Log("Deep link copied to clipboard on Editor: " + textToCopy);
#elif UNITY_ANDROID
    AndroidJavaClass clipboardManager = new AndroidJavaClass("android.content.ClipboardManager");
    AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
    AndroidJavaObject clipboard = context.Call<AndroidJavaObject>("getSystemService", "clipboard");
    AndroidJavaObject clip = new AndroidJavaClass("android.content.ClipData").CallStatic<AndroidJavaObject>("newPlainText", "Copied Text", textToCopy);
    clipboard.Call("setPrimaryClip", clip);
    Debug.Log("Deep link copied to clipboard on Android: " + textToCopy);
#elif UNITY_IOS
    CopyToClipboardiOS(textToCopy);
    Debug.Log("Deep link copied to clipboard on iOS: " + textToCopy);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void CopyToClipboardiOS(string text);
#endif

}
