using UnityEngine;
using System.Collections.Generic;
using UFE3D;
using DG.Tweening;
using System.Collections;

namespace BD
{
	public class DefaultVersusModeAfterBattleScreen : VersusModeAfterBattleScreen
	{
		#region public instance properties
		public AudioClip onLoadSound;
		public AudioClip music;
		public AudioClip selectSound;
		public AudioClip cancelSound;
		public AudioClip moveCursorSound;
		public bool stopPreviousSoundEffectsOnLoad = false;
		public float delayBeforePlayingMusic = 0.1f;
		#endregion

		#region public override methods
		public override void DoFixedUpdate(
			IDictionary<InputReferences, InputEvents> player1PreviousInputs,
			IDictionary<InputReferences, InputEvents> player1CurrentInputs,
			IDictionary<InputReferences, InputEvents> player2PreviousInputs,
			IDictionary<InputReferences, InputEvents> player2CurrentInputs
		)
		{
			base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);

			this.DefaultNavigationSystem(
				player1PreviousInputs,
				player1CurrentInputs,
				player2PreviousInputs,
				player2CurrentInputs,
				this.moveCursorSound,
				this.selectSound,
				this.cancelSound,
				this.GoToMainMenu
			);
		}
		Coroutine co;
        public void OnClickBackBTN()
        {
            UFE.EndGame();
            if (Application.internetReachability == NetworkReachability.NotReachable && co == null)
            {
                //.interactable = false;
                AvatarSpawnerOnDisconnect.Instance.ConnectionErrorToast.gameObject.SetActive(true);
                AvatarSpawnerOnDisconnect.Instance.toastMessage.text = TextLocalization.GetLocaliseTextByKey("Connection Error: waiting for network...");
                AvatarSpawnerOnDisconnect.Instance.ConnectionErrorToast.DOAnchorPos(new Vector2(AvatarSpawnerOnDisconnect.Instance.ConnectionErrorToast.anchoredPosition.x, -245f), 0.5f).SetEase(Ease.InOutSine);
               co= StartCoroutine(WaitForInternet());
                return;
            }
			LoadingHandler.Instance.WithoutApproval = false;
			GamePlayButtonEvents.OnExitButtonXANASummit?.Invoke();
        }
        public IEnumerator WaitForInternet()
        {
            while (Application.internetReachability == NetworkReachability.NotReachable) { yield return new WaitForSeconds(1f); }
            AvatarSpawnerOnDisconnect.Instance.ConnectionErrorToast.gameObject.SetActive(false);
            GamePlayButtonEvents.OnExitButtonXANASummit?.Invoke();
        }

        public override void OnShow()
		{
			base.OnShow();

			if (this.music != null)
			{
				UFE.DelayLocalAction(delegate () { UFE.PlayMusic(this.music); }, this.delayBeforePlayingMusic);
			}

			if (this.stopPreviousSoundEffectsOnLoad)
			{
				UFE.StopSounds();
			}

			if (this.onLoadSound != null)
			{
				UFE.DelayLocalAction(delegate () { UFE.PlaySound(this.onLoadSound); }, this.delayBeforePlayingMusic);
			}
		}
		#endregion
	}
}