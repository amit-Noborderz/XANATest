using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace BD
{
    public class CharacterClothes : MonoBehaviour
    {
        public static Action clothChange;
        public enum CharacterScene { HOME,GamePlay,End}

        public CharacterScene scene;
        [SerializeField] private Texture[] redClothesTextures;
        [SerializeField] private Texture[] blackClothesTextures;

        [SerializeField] private GameObject redPlayerMesh;
        [SerializeField] private GameObject blackPlayerMesh;
        [SerializeField] private Texture mask;
        [SerializeField] private SkinnedMeshRenderer[] renderers;

        [SerializeField] private bool loadingScreenCharacter;
        [SerializeField] private BreakingDownCloth ClothsLoader;
        GameObject WearedCloth;
        private void OnEnable()
        {
            clothChange += reload;
        }
        /*        private void OnEnable()
                {
                    *//*if (loadingScreenCharacter)
                    {
                        SetClothes(2);
                        return;
                    }
                    if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Demo_Fighter3D - Type 2"))
                    {
                        var profile = FightingGameManager.instance.profiles[ProfileSelector.Instance.CurrentProfile]; ;
                        ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                        ClothsLoader.shirt = profile.characterPrefab;
                        StartCoroutine(ClothsLoader.StichItem());
                        return;
                    }

                    SetClothes(UFE.localPlayerNum);
        *//*


                    UFE.OnGameBegin += UFE_OnGameBegin;
                }*/
        
        public void reload()
        {
            
            if (scene == CharacterScene.HOME)
            {
                var profile = FightingGameManager.instance.profiles[ProfileSelector.Instance.CurrentProfile]; ;
                ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                Debug.Log("Cloth Wear .. " + profile.ClothingAsset.name);
                DestroyImmediate(ClothsLoader.shirt);
                ClothsLoader.shirt = profile.ClothingAsset;
                
                StartCoroutine(ClothsLoader.StichItem());
                return;
            }
            if (scene == CharacterScene.GamePlay)
            {
                if (transform.parent.TryGetComponent<ControlsScript>(out var controlsScript))
                {
                    //    SetClothes(controlsScript.playerNum);

                    if (controlsScript.playerNum == 1)
                    {
                        ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                        Debug.Log("Cloth Wear .. " + UFE.config.player1Character.ClothingAsset.name);
                        DestroyImmediate(ClothsLoader.shirt);
                        ClothsLoader.shirt = UFE.config.player1Character.ClothingAsset;//UFE.config.player1Character.ClothingAsset;


                        StartCoroutine(ClothsLoader.StichItem());
                        return;
                    }
                    else
                    {

                        ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                        Debug.Log("Cloth Wear .. " + UFE.config.player2Character.ClothingAsset.name);
                        DestroyImmediate(ClothsLoader.shirt);
                        ClothsLoader.shirt = UFE.config.player2Character.ClothingAsset;
                        
                        StartCoroutine(ClothsLoader.StichItem());
                        return;
                    }

                }
            }
            if (scene == CharacterScene.End)
            {
                if (UFE.winnerNumb == 1)
                {
                    ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                    Debug.Log("Cloth Wear .. " + UFE.config.player1Character.ClothingAsset.name);
                    DestroyImmediate(ClothsLoader.shirt);
                    ClothsLoader.shirt = UFE.config.player1Character.ClothingAsset;
                   
                    StartCoroutine(ClothsLoader.StichItem());
                    return;
                }
                else
                {

                    ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                    Debug.Log("Cloth Wear .. " + UFE.config.player2Character.ClothingAsset.name);
                    DestroyImmediate(ClothsLoader.shirt);
                    ClothsLoader.shirt = UFE.config.player2Character.ClothingAsset;

                    StartCoroutine(ClothsLoader.StichItem());
                    return;
                }
            }
        }
        private void Start()
        {
            if (scene == CharacterScene.HOME)
            {
                var profile = FightingGameManager.instance.profiles[ProfileSelector.Instance.CurrentProfile]; ;
                ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                Debug.Log("Cloth Wear .. " + profile.ClothingAsset.name);
                if (ClothsLoader.shirt != null) { DestroyImmediate(ClothsLoader.shirt); }
                ClothsLoader.shirt = profile.ClothingAsset;
                StartCoroutine(ClothsLoader.StichItem());
                return;
            }
            if(scene == CharacterScene.GamePlay)
            {
                if (transform.parent.TryGetComponent<ControlsScript>(out var controlsScript))
                {
                //    SetClothes(controlsScript.playerNum);
                
                    if(controlsScript.playerNum == 1)
                    {
                        ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                        if (ClothsLoader.shirt != null) { DestroyImmediate(ClothsLoader.shirt); }
                        ClothsLoader.shirt = UFE.config.player1Character.ClothingAsset;
                        StartCoroutine(ClothsLoader.StichItem());
                        return;
                    }
                    else
                    {

                        ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                        Debug.Log("Cloth Wear .. " + UFE.config.player2Character.ClothingAsset.name);
                        if (ClothsLoader.shirt != null) { DestroyImmediate(ClothsLoader.shirt); }
                        ClothsLoader.shirt = UFE.config.player2Character.ClothingAsset;
                        StartCoroutine(ClothsLoader.StichItem());
                        return;
                    }
                
                }
            }
           if(scene == CharacterScene.End)
            {
                if (UFE.winnerNumb== 1)
                {
                    ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                    Debug.Log("Cloth Wear .. " + UFE.config.player1Character.ClothingAsset.name);
                    if (ClothsLoader.shirt != null) { DestroyImmediate(ClothsLoader.shirt); }
                    ClothsLoader.shirt = UFE.config.player1Character.ClothingAsset;
                    StartCoroutine(ClothsLoader.StichItem());
                    return;
                }
                else
                {

                    ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                    Debug.Log("Cloth Wear .. " + UFE.config.player2Character.ClothingAsset.name);
                    if (ClothsLoader.shirt != null) { DestroyImmediate(ClothsLoader.shirt); }
                    ClothsLoader.shirt = UFE.config.player2Character.ClothingAsset;
                    StartCoroutine(ClothsLoader.StichItem());
                    return;
                }
            }
        }

        private void OnDisable()
        {
            clothChange -= reload;

            // UFE.OnGameBegin -= UFE_OnGameBegin;
        }

        private void UFE_OnGameBegin(ControlsScript player1, ControlsScript player2, UFE3D.StageOptions _)
        {
            if (transform.parent.TryGetComponent<ControlsScript>(out var controlsScript))
            {
                SetClothes(controlsScript.playerNum);
            }
            else
            {
               
                    var profile = FightingGameManager.instance.profiles[ProfileSelector.Instance.CurrentProfile]; ;
                    ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                    Debug.Log("Cloth Wear .. " +profile.ClothingAsset.name);
                    
                    ClothsLoader.shirt = profile.ClothingAsset;
                    StartCoroutine(ClothsLoader.StichItem());
                    return;
               
            }

        }

        public void SetClothes(int playerNum)
        {
            if (playerNum == 1)
            {
                ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                if (ClothsLoader.shirt != null) { DestroyImmediate(ClothsLoader.shirt); }
                ClothsLoader.shirt = UFE.config.player1Character.ClothingAsset;
                StartCoroutine(ClothsLoader.StichItem());
                return;
            }
            else
            {

                ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
                Debug.Log("Cloth Wear .. " + UFE.config.player2Character.ClothingAsset.name);
                if (ClothsLoader.shirt != null) { DestroyImmediate(ClothsLoader.shirt); }
                ClothsLoader.shirt = UFE.config.player2Character.ClothingAsset;
                StartCoroutine(ClothsLoader.StichItem());
                return;
            }
            /*if (playerNum == 1)
            {
                ClothsLoader.shirt = redPlayerMesh;
            }
            else
            {
                ClothsLoader.shirt = blackPlayerMesh;
            }
            ClothsLoader.Body.materials[0].SetTexture("_Shirt_Mask", mask);
            StartCoroutine(ClothsLoader.StichItem());*/
        }
    }
}
