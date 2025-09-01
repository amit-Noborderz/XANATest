using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UFE3D;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FightAudioManager : MonoBehaviour
{
    public int FightVarient = 1;
    [SerializeField] private AudioClip fightAudio;
    public List<string> audioAddressableKeys = new();
    public Action audioLoaded;
    private int counter = 0;
    private List<AsyncOperationHandle<AudioClip>> handles = new List<AsyncOperationHandle<AudioClip>>();
    /// <summary>
    /// Bucket path:  s3://xana-prod-item/xanaprod/BreakingDown/
    /// </summary>
    private string Base_URL_Testnet = "https://cdn.xana.net/xanaprod/BreakingDown/"; //Currently both are same but can be changed in future
    /// <summary>
    /// Bucket path:  s3://xana-prod-item/xanaprod/BreakingDown/
    /// </summary>
    private string Base_URL_Mainnet = "https://cdn.xana.net/xanaprod/BreakingDown/";
    private string Base_URL;
    public bool CheckForKeyAvailability(string key) => audioAddressableKeys.Contains(key);
    private void Start()
    {
        Base_URL = APIBasepointManager.instance.IsXanaLive ? Base_URL_Mainnet : Base_URL_Testnet;
        Debug.Log("Base URL"+ Base_URL);
    }
    public void LoadAudioplayer( UFE3D.CharacterInfo player)
    {
        var playername = player.characterName;
        var value = UnityEngine.Random.Range(1, FightVarient+1);
        

      StartCoroutine(  PreloadAudio($"{playername.ToUpper()}_ATTACK_{value}.ogg", (clip) =>
        {
            var result = clip;



            foreach (var item in Resources.Load<StanceInfo>(player.stanceResourcePath[0]).ConvertData().attackMoves)
            {
                if (item.isSpecial)
                {
                    Debug.Log("Updated Special Attack");
                    item.soundEffects[0].sounds[0] = result;
                }
            }
        }));
        StartCoroutine( PreloadAudio($"{playername.ToUpper()}_KO_{value}.ogg", (clip) =>
        {
            var result = clip;
            player.koSound = result;
        }));
       StartCoroutine( PreloadAudio($"{playername.ToUpper()}_START_{value}.ogg", (clip) =>
        {
            var result = clip;
            player.openingSound = result;
        }));

        /*      LoadAudioAsync($"{playername.ToUpper()}_ATTACK_{value}", (clip) =>
              {
                  var result = clip.Result;



                  foreach (var item in Resources.Load<StanceInfo>(player.stanceResourcePath[0]).ConvertData().attackMoves)
                  {
                      if (item.isSpecial)
                      {
                          Debug.Log("Updated Special Attack");
                          item.soundEffects[0].sounds[0] = result;
                      }
                  } });
              LoadAudioAsync($"{playername.ToUpper()}_KO_{value}", (clip) =>
              {
                  var result = clip.Result;
                  player.koSound = result;
              });
              LoadAudioAsync($"{playername.ToUpper()}_START_{value}", (clip) =>
              {
                  var result = clip.Result;
                  player.openingSound = result;
              });*/
    }

    public IEnumerator PreloadAudio(string url,Action<AudioClip> callback)
    {
      

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip( Base_URL+url, AudioType.OGGVORBIS))
        {
            var request = www.SendWebRequest();
            float timer = 0f;
            float timeout = 10f; // seconds

            while (!request.isDone)
            {
                timer += Time.deltaTime;
                if (timer > timeout)
                {
                    Debug.LogError($"[PreloadAudio] Timeout for URL: {url}");
                    yield break;
                }
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
               callback?.Invoke(clip);
                OnAudioClipLoaded(clip);
            }
            else
            {
                Debug.LogError($"[PreloadAudio] Failed to preload audio for URL: {url}. Error: {www.error}");
            }
        }
    }

    public async void LoadAudioAsync(string key,Action<AsyncOperationHandle<AudioClip>> callback=null) { 
      

        while (Application.internetReachability == NetworkReachability.NotReachable)
            await Task.Delay(1000);

        bool success = false;
        while (!success)
        {
            var loadOp = Addressables.LoadAssetAsync<AudioClip>(key);
            await loadOp.Task;

            if (loadOp.Status == AsyncOperationStatus.Succeeded && loadOp.Result != null)
            {
                callback?.Invoke(loadOp);
                OnAudioClipLoaded(loadOp.Result);
                success = true;
                break;
            }
            else
            {
                Debug.LogWarning($"Audio load failed: {key}. Retrying...");
                Addressables.ClearDependencyCacheAsync(key);
                Addressables.Release(loadOp);
                await Task.Delay(1000);
            }
        }
    }

    private void OnAudioClipLoaded(AudioClip handle)
    {
     
        if(counter==6) 
        audioLoaded?.Invoke();
        counter++;
    }

    private void OnDestroy()
    {foreach (var handle in this.handles)
        if (handle.IsValid() && handle.IsDone)
            Addressables.Release(handle);
    }
}