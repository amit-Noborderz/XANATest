using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UFE3D;
using UnityEngine.AddressableAssets;
using System;

namespace BD
{

	public class DefaultLoadingBattleScreen : LoadingBattleScreen
	{
		#region public instance properties
		[Range(0, 100)]
		public float BGM_Percent, Music_Percent,SoundFX,Dialog;
		public AudioClip onLoadSound;
		public AudioClip music;
		public float delayBeforeMusic = .1f;
		public float delayBeforePreload = .5f;
		public float delayAfterPreload = .5f;
		public TextMeshProUGUI namePlayer1;
		public TextMeshProUGUI namePlayer2;
		public CharacterClothes clothesP1;
		public CharacterClothes clothesP2;
		public Text nameStage;
		public Image portraitPlayer1;
		public Image portraitPlayer2;
		public Image screenshotStage;
		public bool stopPreviousSoundEffectsOnLoad = false;
		public Transform Player1pos;
		public Animator anim;
		public  Transform Player2pos;
		public RuntimeAnimatorController controller;
        public RuntimeAnimatorController controller1;
        bool once;
		#endregion

		#region public override methods
		public override void OnShow()
        {
			UFE.BGM_Percent = BGM_Percent/100;
			UFE.Music_Percent = Music_Percent/100;
			UFE.SoundFX = SoundFX / 100;
			UFE.Dialog = Dialog/100; 
			Debug.LogError($"bgmpercent = {UFE.BGM_Percent}, music = {UFE.Music_Percent}, soundFX {UFE.SoundFX}, dialog {UFE.Dialog}");
			UFE.setSOundDefault();
          var  P1SelectedChar = FightingGameManager.instance.profiles[FightingGameManager.instance.id1];
			
            var P2SelectedChar = FightingGameManager.instance.profiles[FightingGameManager.instance.id2];
       //     P2SelectedChar.characterName = FightingGameManager.instance.P2Name.ToUpper();
            //		}
            if (UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.VersusMode)
            {

                //if (ProfileSelector._instance.CurrentProfile == -1) {
                //	ProfileSelector._instance.CurrentProfile = UnityEngine.Random.Range(0, FightingGameManager.instance.profiles.Length);
                //}
               

                P1SelectedChar = FightingGameManager.instance.profiles[ProfileSelector.Instance.CurrentProfile];
				FightingGameManager.instance.P1Name = P1SelectedChar.characterName.ToUpper();
				P2SelectedChar = FightingGameManager.instance.profiles[PlayerPrefs.GetInt("OpponentID")];
				FightingGameManager.instance.P2Name = P2SelectedChar.characterName.ToUpper();
				int audioindex = UnityEngine.Random.Range(1, 4);
				audioindex = 1;
				string audioclip = $"{P1SelectedChar.characterName.ToUpper()}_VS_{P2SelectedChar.characterName.ToUpper()}_{audioindex}";
				FightingGameManager.instance.audioManager.audioLoaded += onAudioLoaded;

				if (FightingGameManager.instance.audioManager.CheckForKeyAvailability(audioclip))
				{
					StartCoroutine( FightingGameManager.instance.audioManager.PreloadAudio(audioclip+".OGG", (clip) =>
                    {
                        var result = clip;
                        UFE.config.stages[0].music = result;
                    }));
					FightingGameManager.instance.audioManager.LoadAudioplayer(P1SelectedChar);FightingGameManager.instance.audioManager.LoadAudioplayer(P2SelectedChar);


                }
				else 
				{
                    audioclip = $"{P2SelectedChar.characterName.ToUpper()}_VS_{P1SelectedChar.characterName.ToUpper()}_{audioindex}";
					if (FightingGameManager.instance.audioManager.CheckForKeyAvailability(audioclip))
					{
						StartCoroutine( FightingGameManager.instance.audioManager.PreloadAudio(audioclip+".OGG", (clip) =>
                        {
                            var result = clip;
                            UFE.config.stages[0].music = result;
                        }));
                        FightingGameManager.instance.audioManager.LoadAudioplayer(P1SelectedChar); FightingGameManager.instance.audioManager.LoadAudioplayer(P2SelectedChar);
                    }
					else { 
						Debug.LogError("No Audio Found...."+ audioclip);
					}
                }


                //PlayerStats.userName;
                //  P1SelectedChar.characterName = PlayerStats.userName.ToUpper();
                /* int randomprofile2 = UnityEngine.Random.Range(0, FightingGameManager.instance.profiles.Length);
                  while (randomprofile2 == ProfileSelector.Instance.CurrentProfile)
                  {
                      randomprofile2 = UnityEngine.Random.Range(0, FightingGameManager.instance.profiles.Length);
                  }*/
                /*if (FightingGameManager.instance.isAITestingMode)
				{
					randomprofile2 = FightingGameManager.instance.AIProfileNumber;
				}*/
                /*if (FightingGameManager.instance.P2Name.IsNullOrEmpty())
				{
					//FightingGameManager.instance.profiles[randomprofile2].characterName =
					FightingGameManager.instance.P2Name = FightingGameManager.instance.AINames[UnityEngine.Random.Range(0, FightingGameManager.instance.AINames.Count)];

                    P2SelectedChar = FightingGameManager.instance.profiles[0];
				}*/

                /* if (FightingGameManager.instance.P2Name.IsNullOrEmpty())
                 {
                     //  FightingGameManager.instance.profiles[randomprofile2].characterName = FightingGameManager.instance.AINames[UnityEngine.Random.Range(0, FightingGameManager.instance.AINames.Count)];
                 //    FightingGameManager.instance.P2Name = FightingGameManager.instance.AINames[UnityEngine.Random.Range(0, FightingGameManager.instance.AINames.Count)]; //FightingGameManager.instance.profiles[randomprofile2].characterName.ToUpper();

                     P2SelectedChar = FightingGameManager.instance.profiles[PlayerPrefs.GetInt("OpponentID")];
                 }*/
            }
			//"Tamaki".ToUpper();
			// print("<color=red>DO UNCOMMENT ABOVE CODE</color>");
			///////////////////////////////////////////////////////////////////////////////////////////////
			UFE.SetPlayer1(P1SelectedChar);
            UFE.SetPlayer2(P2SelectedChar);

			var p1object = Instantiate(P1SelectedChar.characterPrefab, Player1pos);
            var p2object = Instantiate(P2SelectedChar.characterPrefab, Player2pos);
			p1object.GetComponent<Animator>().runtimeAnimatorController = controller;
			p2object.GetComponent<Animator>().runtimeAnimatorController = controller1;
			p1object.layer = 10; p2object.layer = 10;
			p1object.transform.localScale = Vector3.one;p2object.transform.localScale = Vector3.one;
			p1object.transform.localPosition = Vector3.zero; p2object.transform.localPosition = Vector3.zero;
			foreach (var item in p1object.transform.GetAllChildren())
			{
				item.gameObject.layer = 10;
			}
            foreach (var item in p2object.transform.GetAllChildren())
            {
                item.gameObject.layer = 10;
            }
            base.OnShow();
			if (!once)
			{
				if (this.music != null)
				{
					UFE.DelayLocalAction(delegate () { UFE.PlayMusic(this.music); }, this.delayBeforeMusic);
				}

				if (this.stopPreviousSoundEffectsOnLoad)
				{
					UFE.StopSounds();
				}

				if (this.onLoadSound != null)
				{
					UFE.DelayLocalAction(delegate () { UFE.PlaySound(this.onLoadSound); }, this.delayBeforeMusic);
				}

				if (UFE.config.player1Character != null)
				{
					if (this.portraitPlayer1 != null)
					{
						this.portraitPlayer1.sprite = Sprite.Create(
							UFE.config.player1Character.profilePictureBig,
							new Rect(0f, 0f, UFE.config.player1Character.profilePictureBig.width, UFE.config.player1Character.profilePictureBig.height),
							new Vector2(0.5f * UFE.config.player1Character.profilePictureBig.width, 0.5f * UFE.config.player1Character.profilePictureBig.height)
						);
					}

					if (this.namePlayer1 != null)
					{
						this.namePlayer1.text = TextLocalization.GetLocaliseTextByKey(FightingGameManager.instance.P1Name);
                        //this.clothesP1.SetClothes(1);
                    }
				}

				if (UFE.config.player2Character != null)
				{
					if (this.portraitPlayer2 != null)
					{
						this.portraitPlayer2.sprite = Sprite.Create(
							UFE.config.player2Character.profilePictureBig,
							new Rect(0f, 0f, UFE.config.player2Character.profilePictureBig.width, UFE.config.player2Character.profilePictureBig.height),
							new Vector2(0.5f * UFE.config.player2Character.profilePictureBig.width, 0.5f * UFE.config.player2Character.profilePictureBig.height)
						);
					}

					if (this.namePlayer2 != null)
					{
						this.namePlayer2.text = TextLocalization.GetLocaliseTextByKey( FightingGameManager.instance.P2Name);
					//	this.clothesP2.SetClothes(2);
					}
				}

				if (UFE.config.selectedStage != null)
				{
					if (this.screenshotStage != null)
					{
						this.screenshotStage.sprite = Sprite.Create(
							UFE.config.selectedStage.screenshot,
							new Rect(0f, 0f, UFE.config.selectedStage.screenshot.width, UFE.config.selectedStage.screenshot.height),
							new Vector2(0.5f * UFE.config.selectedStage.screenshot.width, 0.5f * UFE.config.selectedStage.screenshot.height)
						);

						Animator anim = this.screenshotStage.GetComponent<Animator>();
						if (anim != null)
						{
							anim.enabled = UFE.gameMode != GameMode.StoryMode;
						}
					}

					/*if (this.nameStage != null){
						this.nameStage.text = UFE.config.selectedStage.stageName;
					}*/
				}

				
				Debug.Log("Loging Screen Debug.......");
				// If network synchornization is needed in this screen, use this instead
				//UFE.DelaySynchronizedAction(UFE.PreloadBattle, this.delayBeforePreload);
				//UFE.DelaySynchronizedAction(this.StartBattle, this.delayBeforePreload + UFE.config.preloadingTime + this.delayAfterPreload);
				once = true;
			}
		}

        private void onAudioLoaded()
        {
            UFE.DelayLocalAction(UFE.PreloadBattle, this.delayBeforePreload);
            UFE.DelayLocalAction(this.StartBattle, UFE.config._preloadingTime);
        }
        #endregion
    }
}