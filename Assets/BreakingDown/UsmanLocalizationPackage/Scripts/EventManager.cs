

using System;
namespace Localization
{
    public class EventManager
    {
        public delegate void VoidAction();
        public delegate void BackgroundMusicDelegate(string sceneName, float soundVolume); // Scene Name LINE : HOME OR GAME;


        // Langauge change event
        public static event VoidAction OnGameLanguageChangedComplete;

        public static event VoidAction ShowRestrictPopup;
        public static void OnInvokeShowRestrictPopup()
        {
            ShowRestrictPopup?.Invoke();
        }

        #region SoundManager
        public static event VoidAction ButtonSoundClickEvent;
        public static event VoidAction NextButtonClickEvent;
        public static event VoidAction OpenOTPScreenEvent;
        public static event VoidAction SingupButtonClickSoundEvent;
        public static event VoidAction InputFieldClickSoundEvent;
        public static event VoidAction InnerButtonClickSoundEvent;
        public static event VoidAction InputFieldChangeSoundEvent;
        public static event VoidAction InputFieldChangeSoundEvent1;
        public static event VoidAction OnScrollChangeSoundEvent;
        public static event VoidAction OnChangeSceneSoundEvent;

        public static event VoidAction OnSaveDeckSoundEvent;
        public static event VoidAction FuturisticGameSoundsPack16Event;
        public static event VoidAction ChristmasSwooshEvent;
        public static event VoidAction RobotVoiceEnemyDetectedEvent;
        public static event VoidAction DeckSaveOkButtonEvent;


        // Card audio Events
        public static event VoidAction CardClickSoundEvent;
        public static event VoidAction CardDragEndSoundEvent;
        public static event VoidAction SaveDeckSoundEvent;
        public static event VoidAction AddNewDeckSoundEvent;
        public static event VoidAction ArenaSoundEvent;

        //Game Play Audio events
        public static event VoidAction PlayerVsPlayerSoundEvent;
        public static event VoidAction RobotVoiceActivatedSoundEvent;
        public static event VoidAction GamePlayCardSelectionSoundEvent;
        public static event VoidAction GameStartSoundEvent;
        public static event VoidAction GameCardInfoSoundEvent;
        public static event VoidAction CardSingSoundEvent;
        public static event VoidAction CardGameUpgradesEvent;

        public static event VoidAction YourTurnEvent;
        public static event VoidAction YourTurnEndEvent;
        public static event VoidAction GrabGameCardSoundEvent;
        public static event VoidAction GrabGameAttackCardSoundEvent;
        public static event VoidAction NewCardAddSoundEvent;

        public static event VoidAction CardPlaceSoundEvent;
        public static event VoidAction CardPlaceSoundTwoEvent;
        public static event VoidAction CardReadySoundEvent;

        public static event VoidAction EnemyRobotSoundEvent;
        public static event VoidAction EnemyRobotDefeatSoundEvent;
        public static event VoidAction EnemyGirlIncomingSoundEvent;


        public static event VoidAction ReadyAimFireSoundEvent;
        public static event VoidAction FireSoundEvent;
        public static event VoidAction ReturnAttackEvent;
        public static event VoidAction CardDestorySoundEvent;
        public static event VoidAction CardPlayerDestorySoundEvent;
        public static event VoidAction EnemyUpFollowUpSoundEvent;
        public static event VoidAction EnemyHandsUpSoundEvent;
        public static event VoidAction FireSoundEvent1;


        public static event VoidAction PlayButtonClickEvent;
        public static event VoidAction PlayButtonClickEvent2;
        public static event VoidAction GamePopupCloseEvent;


        public static event BackgroundMusicDelegate BackgroundMusicDelegateEvent;

        public static event VoidAction PlayerCardDestorySoundEvent;

        public static event VoidAction CardPreviewPopupEvent;
        public static event VoidAction CardAttackExplosionEvent;

        public static event VoidAction CardAddGamePlayEvent;
        public static event VoidAction EnemyCardAddEvent;

        #endregion


        #region Invoker
        /// <summary>
        /// change langauge related 
        /// </summary>
        ///
        public static void OnInvokeCangeLangauge()
        {
            OnGameLanguageChangedComplete?.Invoke();
        }
        /// end - change langauge related
        public static void OnInvokeEnemyCardAdd()
        {
            EnemyCardAddEvent?.Invoke();
        }

        public static void OnInvokeCardAddGamePlayEvent()
        {
            CardAddGamePlayEvent?.Invoke();
        }

        public static void OnInvokeCardAttackExpo()
        {
            CardAttackExplosionEvent?.Invoke();
        }

        public static void OnInvokeCardPreview()
        {
            CardPreviewPopupEvent?.Invoke();
        }

        public static void OnInvokeCardGameUpgradesEvent()
        {
            CardGameUpgradesEvent?.Invoke();
        }

        public static void OnInvokePlayerCardDestorySoundEvent()
        {
            PlayerCardDestorySoundEvent?.Invoke();
        }


        public static void OnInvokeDeckSaveOkButtonEvent()
        {
            DeckSaveOkButtonEvent?.Invoke();
        }


        public static void OnInvokePlayButtonAudio()
        {
            PlayButtonClickEvent?.Invoke();
        }

        public static void OnInvokePlayButtonAudio2()
        {
            PlayButtonClickEvent2?.Invoke();
        }

        public static void OnBackgroundMusicDelegateEvent(string sceneName, float volume)
        {
            BackgroundMusicDelegateEvent?.Invoke(sceneName, volume);
        }

        /// <summary>
        /// Buttons click related invokers
        /// </summary>
        ///

        public static void OnInvokeGamePopupCloseEvent()
        {
            GamePopupCloseEvent?.Invoke();
        }

        public static void OnRobotVoiceEnemyDetectedEvent()
        {
            RobotVoiceEnemyDetectedEvent?.Invoke();
        }

        public static void OnFuturisticGameSoundsPack16Event()
        {
            FuturisticGameSoundsPack16Event?.Invoke();
        }
        public static void OnChristmasSwooshEvent()
        {
            ChristmasSwooshEvent?.Invoke();
        }


        public static void OnInvokeScrollButtonSoundEvent()
        {
            OnScrollChangeSoundEvent?.Invoke();
        }
        public static void OnInvokeButtonSoundClickEvent()
        {
            ButtonSoundClickEvent?.Invoke();
        }
        public static void OnInvokeSingupButtonClickSoundEvent()
        {
            SingupButtonClickSoundEvent?.Invoke();
        }
        public static void OnInvokeInputFieldClickSoundEvent()
        {
            InputFieldClickSoundEvent?.Invoke();
        }
        public static void OnInvokeInnerButtonClickSoundEvent()
        {
            InnerButtonClickSoundEvent?.Invoke();
        }
        public static void OnInvokeInputFieldChangeSoundEvent()
        {
            InputFieldChangeSoundEvent?.Invoke();
        }
        public static void OnInvokeInputFieldChangeSoundEvent1()
        {
            InputFieldChangeSoundEvent1?.Invoke();
        }
        public static void OnInvokeNextButtonClickEvent()
        {
            NextButtonClickEvent?.Invoke();
        }
        public static void OnInvokeOpenOTPScreenEvent()
        {
            OpenOTPScreenEvent?.Invoke();
        }


        public static void OnChangeSceneSound()
        {
            OnChangeSceneSoundEvent?.Invoke();
        }

        public static void OnSaveDeckSoundPopup()
        {
            OnSaveDeckSoundEvent?.Invoke();
        }


        /// <summary>
        /// Cards related invookers 
        /// </summary>
        public static void OnInvokeCardClickSoundEvent()
        {
            CardClickSoundEvent?.Invoke();
        }

        public static void OnInvokeCardDragEndSoundEvent()
        {
            CardDragEndSoundEvent?.Invoke();
        }


        public static void OnInvokeSaveDeckSoundEvent()
        {
            SaveDeckSoundEvent?.Invoke();
        }
        public static void OnInvokeAddNewDeckSoundEvent()
        {
            AddNewDeckSoundEvent?.Invoke();
        }
        public static void OnInvokeArenaSoundEvent()
        {
            ArenaSoundEvent?.Invoke();
        }

        /// <summary>
        /// Game Play related invokers 
        /// </summary>
        public static void OnInvokePlayerVsPlayerSoundEvent()
        {
            PlayerVsPlayerSoundEvent?.Invoke();
        }

        public static void OnInvokeRobotVoiceActivatedEvent()
        {
            RobotVoiceActivatedSoundEvent?.Invoke();
        }
        public static void OnInvokeGamePlayCardSelectionSound()
        {
            GamePlayCardSelectionSoundEvent?.Invoke();
        }

        public static void OnInvokeGameStartSound()
        {
            GameStartSoundEvent?.Invoke();
        }

        public static void OnInvokeCardInfoPopupSound()
        {
            GameCardInfoSoundEvent?.Invoke();
        }

        public static void OnInvokeCardSignSound()
        {
            CardSingSoundEvent?.Invoke();
        }

        public static void OnInvokeYourTurnSound()
        {
            YourTurnEvent?.Invoke();
        }

        public static void OnInvokeYourTurnEndSound()
        {
            YourTurnEndEvent?.Invoke();
        }

        public static void OnInvokeGrabGameCardSound()
        {
            GrabGameCardSoundEvent?.Invoke();
        }

        public static void OnInvokeNewCardEventSound()
        {
            NewCardAddSoundEvent?.Invoke();
        }

        public static void OnInvokeCardPlaceSoundEvent()
        {
            CardPlaceSoundEvent?.Invoke();
        }

        public static void OnInvokeCardPlaceSoundTwoEvent()
        {
            CardPlaceSoundTwoEvent?.Invoke();
        }

        public static void OnInvokeCardReadySoundEvent()
        {
            CardReadySoundEvent?.Invoke();
        }


        public static void OnInvokeEnemyRobotSoundEvent()
        {
            EnemyRobotSoundEvent?.Invoke();
        }

        public static void OnInvokeEnemyDefeatSoundEvent()
        {
            EnemyRobotDefeatSoundEvent?.Invoke();
        }

        public static void OnInvokeEnenmyGirlIncomingEvent()
        {
            EnemyGirlIncomingSoundEvent?.Invoke();
        }
        #endregion

        #region ATTACK
        public static void OnInvokeReadyAimFireSoundEvent()
        {
            ReadyAimFireSoundEvent?.Invoke();
        }

        public static void OnInvokeFireSoundEvent()
        {
            FireSoundEvent?.Invoke();
        }
        public static void OnInvokeFireSoundEvent1()
        {
            FireSoundEvent1?.Invoke();
        }

        public static void OnInvokeReturnAttackEvent()
        {
            ReturnAttackEvent?.Invoke();
        }

        public static void OnInvokeCardDestorySoundEvent()
        {
            CardDestorySoundEvent?.Invoke();
        }

        public static void OnInvokeEnemyFollowUpSoundEvent()
        {
            EnemyUpFollowUpSoundEvent?.Invoke();
        }

        public static void OnInvokeEnemyHandsUpSoundEvent()
        {
            EnemyHandsUpSoundEvent?.Invoke();
        }

        public static void OnInvokeGrabCardAttackSoundEvent()
        {
            GrabGameAttackCardSoundEvent?.Invoke();
        }

        public static void OnInvokeCardPlayerDestorySoundEvent()
        {
            CardPlayerDestorySoundEvent?.Invoke();
        }
        #endregion
    }
}