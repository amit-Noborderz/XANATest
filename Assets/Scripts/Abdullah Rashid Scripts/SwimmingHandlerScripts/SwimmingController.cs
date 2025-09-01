using Cinemachine;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SwimmingController : MonoBehaviour
{
    public PlayerController _playerController;
    private Coroutine swimCoroutine;
    [SerializeField] private GameObject[] objectsToEnableDisable;
    public GameObject swimJumpButtom;

    private Image swimJumpButtonImage; // Reference to the Image component
    public bool isInPool = false;
    private float originalJumpVelocity;
    private float swimJump = 6;
    private bool allowJump = false;

    [Header("Button Colors")]
    [SerializeField] private Color idleColor = Color.white; // Color when idle
    [SerializeField] private Color movingColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Gray (#808080) with 50% opacity

    private PlayerCameraController playerCameraController; // Reference to PlayerCameraController

    private void Start()
    {
        if (gameObject.TryGetComponent(out PlayerController playerController))
        {
            _playerController = playerController;
            originalJumpVelocity = _playerController.JumpVelocity;
            playerCameraController = _playerController.cinemachineFreeLook.GetComponent<PlayerCameraController>();
        }
        else
        {
            Debug.LogError("PlayerController not found");
        }

        // Get the Image component from swimJumpButtom
        if (swimJumpButtom != null)
        {
            swimJumpButtonImage = swimJumpButtom.GetComponent<Image>();
        }
    }

    private void Update()
    {
        // Check if the space bar is pressed
        if (Input.GetKeyDown(KeyCode.Space) && allowJump)
        {
            Jump();
        }
        // Update the color of the swimJumpButtom
        UpdateSwimJumpButtonColor();
    }

    public void StartSwimming()
    {
        //if (isInPool) return; // Prevent starting swimming if already in pool
        if (_playerController.isFirstPerson) // If is in FPS force fully change TPS
        {
            GamePlayButtonEvents.inst.OnSwitchCameraClick();
            _playerController.isFirstPerson = false;
            _playerController.desiredMoveDirectionFPP = Vector3.zero;
        }
        if (GamePlayUIHandler.inst != null)
        {
            GamePlayUIHandler.inst.CloseEmoteAndReactionPanelOnWheel();
        }
        if (GameplayEntityLoader.instance._uiReferences!=null)
        {
            GameplayEntityLoader.instance._uiReferences.Onfreecam.interactable = false;
            GameplayEntityLoader.instance._uiReferences.OffFreecam.interactable = false;
        }
        GamePlayUIHandler.inst.fPSOffButton.interactable = false;
        GamePlayUIHandler.inst.fPSOnButton.interactable = false;
        GetComponent<PlayerDashHandler>().TerminateDashEffect();
        isInPool = true;
        _playerController.animator.SetBool("IsEmote", false);
        UpdateUIelement(false);
        _playerController.JumpVelocity = swimJump;
        //swimJumpButtom.SetActive(true);
        _playerController.animator.SetBool("IsSwimIdle", true);
        playerCameraController.SetFreeLookForSwimStart();

        if (swimCoroutine == null)
        {
            swimCoroutine = StartCoroutine(SwimmingState());
        }
        GameplayEntityLoader.instance.PlayerCamera.GetComponent<CinemachineCollider>().m_MinimumDistanceFromTarget = 1.5f;//Set the Minimum Distance of Camera on Player Swimming
    }


    public void StopSwimming()
    {
        isInPool = false;
        _playerController.JumpVelocity = originalJumpVelocity;
        if (MutiplayerController.instance.connectionState.Equals(ServerConnectionStates.ConnectedToServer))
        {
            UpdateUIelement(true);
        }
        else
        {
            GameplayEntityLoader.instance.DashButton.SetActive(true);
            GamePlayUIHandler.inst.JumpUI.SetActive(true);
        }
        if(GamePlayUIHandler.inst.isHideButton)
            XanaChatSystem.instance.gameObject.SetActive(false);
        //swimJumpButtom.SetActive(false);
        _playerController.animator.SetBool("IsSwimIdle", false);
        _playerController.animator.SetBool("IsSwimming", false);
        if (swimCoroutine != null)
        {
            StopCoroutine(swimCoroutine);
            swimCoroutine = null;
        }
        if (GameplayEntityLoader.instance._uiReferences != null)
        {
            GameplayEntityLoader.instance._uiReferences.Onfreecam.interactable = true;
            GameplayEntityLoader.instance._uiReferences.OffFreecam.interactable = true;
        }
        GamePlayUIHandler.inst.fPSOffButton.interactable = true;
        GamePlayUIHandler.inst.fPSOnButton.interactable = true;
        GameplayEntityLoader.instance.PlayerCamera.GetComponent<CinemachineCollider>().m_MinimumDistanceFromTarget = 0.02f; //Set to Default on Stop Swimming
    }

    private IEnumerator SwimmingState()
    {
        bool isMoving;
        while (true)
        {
            while (_playerController.animator == null)
            {
                yield return null; // Wait for the next frame
            }
            if (_playerController.desiredMoveDirection != Vector3.zero || _playerController.desiredMoveDirectionFPP != Vector3.zero)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
            _playerController.animator.SetBool("IsSwimming", isMoving);
            yield return null;
        }
    }

    public void UpdateUIelement(bool enable)
    {
        objectsToEnableDisable?.SetActive(enable);
    }

    public void Jump()
    {
        if (_playerController.animator.GetBool("IsSwimming") || _playerController.animator.GetBool("IsSwimIdle"))
        {
            allowJump = false;
            _playerController.animator.SetBool("IsSwimIdle", false);
            _playerController.animator.SetBool("IsSwimming", false);
            _playerController.Jump();
            StartCoroutine(JumpEndRoutine());
        }
    }

    private IEnumerator JumpEndRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        if (isInPool)
        {
            _playerController.animator.SetBool("IsSwimIdle", true);
            allowJump = true;
        }
    }

    private void UpdateSwimJumpButtonColor()
    {
        if (swimJumpButtonImage != null && swimJumpButtom.activeSelf)
        {
            // Change the button color based on whether the player is moving
            bool isMoving = _playerController.desiredMoveDirection != Vector3.zero || _playerController.desiredMoveDirectionFPP != Vector3.zero;
            swimJumpButtonImage.color = isMoving ? movingColor : idleColor;
        }
    }

    public void SetSwimJumpState(bool flag) {
        swimJumpButtom.SetActive(flag);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SwimPoint"))
        {
            allowJump = true;
            SetSwimJumpState(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SwimPoint"))
        {
            allowJump = false;
            SetSwimJumpState(false);
        }
    }
}
