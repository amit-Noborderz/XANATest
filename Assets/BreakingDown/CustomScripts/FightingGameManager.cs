using FPLibrary;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;
using UnityEngine.UI;

namespace BD
{
    public class FightingGameManager : MonoBehaviour
    {
        public static FightingGameManager instance;
        public Material newSkyboxMaterial;
        private Material originalSkyboxMaterial;
        public FightAudioManager audioManager;
        #region Inspector Fields

        [SerializeField] private bool startDirectly = false;

        public PhotonView PV;
        public bool isPause;
        public int players;
        public int otherPlayers;
        public bool isWon;

        public UFE3D.CharacterInfo P1SelectedChar;
        public UFE3D.CharacterInfo P2SelectedChar;

        public bool wonPlayer1;
        public ControlsScript p1ControlsScript;
        public ControlsScript p2ControlsScript;
        public int p1WonRound;
        public int p2WonRound;
        public Fix64 p1LifePercentage;
        public Fix64 p2LifePercentage;
        public HomeScreenPlayerAnim HomeScreenPlayerAnim;
       // public List<string> AINames = new List<string>() { "Pacman","Yuji T","Yuki","Gio","Ryu","Puppet","Gen","Roko","Tamaki" };
        public UFE3D.CharacterInfo Winner;
        //public Texture2D[] p1Textures;
        //public Texture2D[] p2Textures;
        //public Mesh p1Mesh;
        //public Mesh p2Mesh;
        public int WinnerId;

        public DateTime curTime;
        public string myName = ""; //Attizaz
        public string opponentName = "";

        private int opponentID = -1;

        public int OpponentID
        {
            set
            {
                opponentID = value;
            }

            get
            {
                if (opponentID == -1)
                {
                    Photon.Realtime.Player[] otherPlayers = PhotonNetwork.PlayerListOthers;
                    otherPlayers[0].CustomProperties.TryGetValue("userID", out object value);
                    opponentID = (int)value;
                }

                return opponentID;
            }
        }

        public AudioSource mainAudioSource,BGM;
        public AudioClip crowdSound, menuMusic;


        public UFE3D.CharacterInfo[] profiles;


        public Button OpenButton;
        [HideInInspector] public int id1;
        [HideInInspector] public int id2;
        [HideInInspector] public string P1Name;
        [HideInInspector] public string P2Name;
        public GameObject player1, player2, winnerAvatar;

        public bool isAITestingMode = false;
        [Tooltip("If above bool is on then you will be to select AI profile of your choice")] public int AIProfileNumber = 0;


        public GameObject buttonLayoutPanel/*,leaderBoardPanel*/;

        #endregion

        private void Awake()
        {
            ReferencesForGamePlay.instance.workingCanvas.GetComponent<CanvasGroup>().alpha = 0;
            ReferencesForGamePlay.instance.workingCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (instance == null)
            {
                instance = this;
            }
            APIManager.AUTH_TOKEN = ConstantsGod.AUTH_TOKEN;
            APIManager.WalletAddress = PlayerPrefs.GetString("publicID");
        }

        private void OnEnable()
        {
            originalSkyboxMaterial = RenderSettings.skybox;

            if (newSkyboxMaterial != null)
            {
                RenderSettings.skybox = newSkyboxMaterial;
            }

            GameObject camObject = GameObject.Find("EnvironmentRenderCamera");
            if (camObject != null)
            {
                camObject.SetActive(false);
            }
            EventManager.activateLayoutPanel += ActivatePanels;
        }

        private void OnDisable()
        {
            if (originalSkyboxMaterial != null)
            {
                RenderSettings.skybox = originalSkyboxMaterial;
            }
            else
            {
                RenderSettings.skybox = null;

            }


            GameObject camObject = GameObject.Find("EnvironmentRenderCamera");
            if (camObject != null)
            {
                camObject.SetActive(true);
            }
            ReferencesForGamePlay.instance.workingCanvas.GetComponent<CanvasGroup>().alpha = 1;
            ReferencesForGamePlay.instance.workingCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;

            EventManager.activateLayoutPanel -= ActivatePanels;
        }
        private void Start()
        {
            myName = "Player : " + UnityEngine.Random.Range(0, 20).ToString();
            PhotonNetwork.NickName = myName; // TODO: Change this to the actual player name from the API

            UFE.localPlayerNum = -1; // To reset after each battle. Should do this elsewhere.


            if (startDirectly)
            {
                print("Starting"); //kush
                UFE.SetPlayer1(P1SelectedChar);
                UFE.SetPlayer2(P2SelectedChar);
                UFE.StartLoadingBattleScreen();
            }
            BGM.volume = UFE.GetBGMVolume() ;
        }

        public void ActivatePanels(string panelID)
        {
            if (panelID == "layoutPanel")
                buttonLayoutPanel.SetActive(true);
            //else if (panelID == "leaderboardPanel")
            //    leaderBoardPanel.SetActive(true);
        }


        public float secd;
        private void Update()
        {
            players = PhotonNetwork.PlayerList.Length;
            otherPlayers = PhotonNetwork.PlayerListOthers.Length;
            if (p1ControlsScript != null)
            {
                p1LifePercentage = p1ControlsScript.currentLifePoints / p1ControlsScript.myInfo.lifePoints;
                p1WonRound = p1ControlsScript.roundsWon;
            }

            if (p2ControlsScript != null)
            {
                p2LifePercentage = p2ControlsScript.currentLifePoints / p2ControlsScript.myInfo.lifePoints;
                p2WonRound = p2ControlsScript.roundsWon;
            }

            if (wonPlayer1)
            {
                curTime = DateTime.Now;
                wonPlayer1 = false;
                /*p1ControlsScript.roundsWon = 2;
                if (p1ControlsScript.roundsWon > Mathf.Ceil(UFE.config.roundOptions.totalRounds / 2) || UFE.challengeMode != null)
                {
                    p1ControlsScript.SetMoveToOutro(1);
                    UFE.DelaySynchronizedAction(this.EndMatch, UFE.config.roundOptions._endGameDelay);
                    UFE.FireGameEnds(p1ControlsScript, p1ControlsScript.opControlsScript);
                }*/
            }
            TimeSpan diff = curTime - DateTime.Now;
            secd = MathF.Abs((float)diff.TotalSeconds);
        }

        public void EndMatch()
        {
            UFE.EndGame(false);
            UFE.cameraScript.killCamMove = true;
        }
        #region Attizaz's code
        public void CallRPC()
        {
            PV.RPC(nameof(PlayerSelection), RpcTarget.All);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (UFE.gameMode == UFE3D.GameMode.TrainingRoom)
                {

                }
                else if (UFE.gameMode == UFE3D.GameMode.VersusMode)
                {

                }
                else
                {
                    isPause = true;
                    curTime = DateTime.Now;
                    if (PhotonNetwork.InRoom)
                    {
                        if (PV.IsMine)
                        {
                            object[] passdata = new object[3];
                            passdata[0] = ProfileSelector.Instance.profiles[ProfileSelector.Instance.CurrentProfile] as object;
                            passdata[1] = PhotonNetwork.LocalPlayer.ActorNumber as object;
                            passdata[2] = ProfileSelector.Instance.CurrentProfile as object;
                            Debug.LogError("Pause RPC Fire");
                            if (PhotonNetwork.IsMasterClient)
                            {
                                Player[] players = PhotonNetwork.PlayerListOthers;
                                if (players.Length > 0)
                                {
                                    PhotonNetwork.SetMasterClient(players[0]);
                                }
                            }
                            PV.RPC(nameof(ApplicationPauseRPC), RpcTarget.AllBuffered, passdata);
                            PhotonNetwork.SendAllOutgoingCommands();
                        }
                    }
                }
            }
        }


        public void OnApplicationFocus(bool focus)
        {
            if (isPause && focus)
            {
                if (UFE.gameMode == UFE3D.GameMode.TrainingRoom)
                {

                }
                else if (UFE.gameMode == UFE3D.GameMode.VersusMode)
                {

                }
                else
                {
                    //riken
                    return;
                    TimeSpan diff = curTime - DateTime.Now;
                    float sec = MathF.Abs((float)diff.TotalSeconds);
                    //Riken
                    /*if (sec > 5f)
                    {
                        UFE.EndGame();
                        UFE.StartMainMenuScreen();
                    }
                    else*/
                    {
                        if (PhotonNetwork.InRoom)
                        {
                            if (PV.IsMine)
                            {
                                object[] passdata = new object[3];
                                passdata[0] = ProfileSelector.Instance.profiles[ProfileSelector.Instance.CurrentProfile] as object;
                                passdata[1] = PhotonNetwork.LocalPlayer.ActorNumber as object;
                                passdata[2] = ProfileSelector.Instance.CurrentProfile as object;
                                Debug.LogError("Focus RPC Fire");
                                PV.RPC(nameof(ApplicationFocusRPC), RpcTarget.AllBuffered, passdata);
                                PhotonNetwork.SendAllOutgoingCommands();

                            }
                        }
                    }
                }
                isPause = false;
            }
        }

        [PunRPC]
        public void ApplicationPauseRPC(object[] data)
        {
            Debug.LogError(data[0].ToString() + $" Actor {data[1]}   {PhotonNetwork.LocalPlayer.ActorNumber} " + " Profile: " + data[2].ToString() + " paused The Game");
            int index = (int)data[1];
            if (PhotonNetwork.LocalPlayer.ActorNumber != index)
            {
                //Riken
                return;
                isFocus = false;
                if (ForceWinOpponentPlayerCoroutine != null)
                {
                    StopCoroutine(ForceWinOpponentPlayerCoroutine);
                }
                ForceWinOpponentPlayerCoroutine = StartCoroutine(IEForceWinOpponentPlayer(index));
            }
        }

        public Coroutine ForceWinOpponentPlayerCoroutine;
        public bool isFocus;

        public IEnumerator IEForceWinOpponentPlayer(int num)
        {
            Debug.LogError("ForceWinOpponentPlayerCoroutine: " + num + "   " + PhotonNetwork.LocalPlayer.ActorNumber);
            int waitTime = 5;
            while (waitTime > 0)
            {
                yield return new WaitForSeconds(1);
                waitTime--;
            }
            if (!isFocus)
            {
                ControlsScript controlscr = num == 1 ? p1ControlsScript : p2ControlsScript;

                controlscr.opControlsScript.currentLifePoints = 0;
                controlscr.roundsWon = 2;
                /*if (controlscr.roundsWon > Mathf.Ceil(UFE.config.roundOptions.totalRounds / 2) || UFE.challengeMode != null)
                {
                    Debug.LogError($"Player Actor: {num} Won");
                    controlscr.SetMoveToOutro(1);
                    UFE.DelaySynchronizedAction(this.EndMatch, UFE.config.roundOptions._endGameDelay);
                    UFE.FireGameEnds(controlscr, controlscr.opControlsScript);
                }*/
                //isWon = true;
                UFE.fluxCapacitor.EndRound();
            }
        }

        [PunRPC]
        public void ApplicationFocusRPC(object[] data)
        {
            isFocus = true;
            Debug.LogError(data[0].ToString() + $" Actor {data[1]} " + " Profile: " + data[2].ToString() + " focus The Game");
        }

        [PunRPC]
        public void PlayerSelection()
        {
            print(this.GetType().Name + ": My nickName: " + PhotonNetwork.NickName);
            Photon.Realtime.Player[] otherPlayers = PhotonNetwork.PlayerListOthers;
            opponentName = otherPlayers[0].NickName;
            print(this.GetType().Name + ": Opponent nickName: " + opponentName);


            //UFE.SetPlayer1(P1SelectedChar);
            //UFE.SetPlayer2(P2SelectedChar);

            //UFE.StartLoadingBattleScreen();
        }


        public void RestartScene()
        {
            StartCoroutine(RestartingScene());

        }

        IEnumerator RestartingScene()
        {
            yield return new WaitForSeconds(0.2f);
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        /////Sound Settings
        public void PlayMenuMusic()
        {
            if (mainAudioSource != null)
            {
                mainAudioSource.Pause();
                mainAudioSource.clip = menuMusic;
                mainAudioSource.Play();
            }
        }

        public void PlayCrowdSound()
        {
            if (mainAudioSource != null)
            {
                mainAudioSource.Pause();
                mainAudioSource.clip = crowdSound;
                mainAudioSource.Play();
            }
        }

        //public bool testonly = false;
        //private void Update()
        //{
        //    if (testonly) {
        //        testonly = false;
        //        FindPlayersAndManageWin();
        //    }
        //}

        public void FindPlayersAndManageWin(bool isDraw = false)
        {
            player1 = GameObject.Find("Player1");
            player2 = GameObject.Find("Player2");


            player1.SetActive(false);
            player2.SetActive(false);

           /* if (!isDraw) // muneeb
            {*/
            SoundChanger soundChanger = FindObjectOfType<SoundChanger>();
            var p1object = Instantiate(winnerAvatar, soundChanger.WinnerAvatar.transform);
            p1object.GetComponent<Animator>().runtimeAnimatorController = soundChanger.Controller;
            p1object.layer = 9;
            p1object.transform.localScale = Vector3.one;
            p1object.transform.localPosition = Vector3.zero;
            foreach (var item in p1object.transform.GetAllChildren())
            {
                item.gameObject.layer = 9;
            }

            winnerAvatar = soundChanger.WinnerAvatar;

            winnerAvatar.SetActive(true);
          // }
        }

        #endregion

    }
}