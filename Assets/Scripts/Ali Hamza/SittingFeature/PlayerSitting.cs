using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;

public class PlayerSitting : MonoBehaviour
{
    public bool isSitting = false;  // Track if player is currently sitting
    public bool isIdleSitting = false;  // Track if player is already sitting
    public bool isInRange = false;  // Track if player is in range of the chair

    public float moveDuration = 1.0f; // Duration to move smoothly to the chair position

    private SeatingInteraction currentSeating;
    private int assignedSeatIndex = -1;  // The assigned seat position index, default is -1 (no seat)

    private CharacterController characterController;  // Player's CharacterController
    private Animator animator;
    private PhotonView photonView;
    private PhotonAnimatorView photonAnimatorView;
    private Button sitBtn;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        photonAnimatorView = GetComponent<PhotonAnimatorView>();
        animator = GetComponent<Animator>();
        characterController = ReferencesForGamePlay.instance.MainPlayerParent.GetComponent<CharacterController>();

        sitBtn = GamePlayUIHandler.inst.sitButton;
        sitBtn.gameObject.SetActive(false);
        sitBtn.onClick.AddListener(SitStandToggle);
    }

    // This method is called when the player enters or exits the seating range
    public void SetInRange(SeatingInteraction seating, bool inRange)
    {
        if (photonView.IsMine)
        {
            if (animator.GetBool("IsEmote"))
            {
                if (!inRange && sitBtn.gameObject.activeSelf)
                    sitBtn.gameObject.SetActive(false);
                return;
            }
            isInRange = inRange;
            currentSeating = seating;
            if (inRange && !isSitting)
            {
                assignedSeatIndex = FindAvailableSeatIndex();
                sitBtn.gameObject.SetActive(assignedSeatIndex >= 0);
            }
            else
            {
                sitBtn.gameObject.SetActive(false);
            }
        }
    }

    // This method finds an available seat position from the seating interaction
    private int FindAvailableSeatIndex()
    {
        for (int i = 0; i < currentSeating.NoOfSeats.Length; i++)
        {
            if (!currentSeating.IsPositionOccupied(i))
                return i;
        }
        return -1;
    }

    // This method is called when the player clicks the sit button
    private void SitStandToggle()
    {
        if (!photonView.IsMine)
            return;

        if (animator.GetBool("IsEmote")  || animator.GetBool("IsFalling") == true)
        {
            return;
        }

        PlayerController playerController = ReferencesForGamePlay.instance.MainPlayerParent.GetComponent<PlayerController>();
        if (playerController.horizontal != 0 || playerController.vertical != 0 || PlayerSelfieController.Instance.m_IsSelfieFeatureActive)
        {
            playerController.horizontal = 0;
            playerController.vertical = 0;
        }

        if (assignedSeatIndex < 0)
        {
            if (isInRange && !isSitting)
            {
                assignedSeatIndex = FindAvailableSeatIndex();
                sitBtn.gameObject.SetActive(assignedSeatIndex >= 0);
            }
        }

        sitBtn.gameObject.SetActive(isSitting);
        if (isSitting)
        {
            StandUp();
        }
        else if (isInRange)
        {
            if (assignedSeatIndex >= 0)
            {
                StartCoroutine(SitDownSmooth());
            }
            else
            {
                // Wait until a seat becomes available, then sit
                StartCoroutine(WaitForAvailableSeatAndSit());
            }
        }
    }

    // Coroutine to wait until a seat is available, then sit
    private IEnumerator WaitForAvailableSeatAndSit()
    {
        while (FindAvailableSeatIndex() < 0)
        {
            yield return null;
        }

        assignedSeatIndex = FindAvailableSeatIndex();
       
        StartCoroutine(SitDownSmooth());
    }

    // This coroutine moves the player smoothly to the seating position
    private IEnumerator SitDownSmooth()
    {
        if (characterController != null) characterController.enabled = false;

        Transform player = ReferencesForGamePlay.instance.MainPlayerParent.transform;
        Vector3 startPosition = player.position;
        Quaternion startRotation = player.rotation;

        Vector3 targetPosition = currentSeating.NoOfSeats[assignedSeatIndex].position;
        Quaternion targetRotation = currentSeating.NoOfSeats[assignedSeatIndex].rotation;

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            player.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            player.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        player.GetComponent<PlayerController>().sittingRotationForThirdPerson = targetRotation.eulerAngles.y;

        // Set final position and rotation to ensure exact match
        player.position = targetPosition;
        player.rotation = targetRotation;

        SitDown();
    }

    // This method is called once the player has reached the seat and is sitting
    private void SitDown()
    {
        isSitting = true;
        characterController.GetComponent<PlayerController>().isMovementAllowed = false;
        animator.SetBool("isSitting", true);

        currentSeating.SetPositionOccupied(assignedSeatIndex, true);
        photonView.RPC(nameof(RPC_SyncSeatState), RpcTarget.AllBuffered, currentSeating.seatingID, assignedSeatIndex, true, photonView.ViewID);

        // Sync the sitting animation state across all clients
        photonAnimatorView.SetParameterSynchronized("isSitting", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
        if (PlayerCameraController.instance != null)
        {
            PlayerCameraController.instance.SetOrbitRadius(1.75f, 3, 2.2f);
            PlayerCameraController.instance.SetOrbitHeight(2.47f, 1.555f, 1.8f);
        }
        if (AvatarSpawnerOnDisconnect.Instance != null)
        {
            AvatarSpawnerOnDisconnect.Instance.DisableUIElementsOnDisconnect[4].gameObject.SetActive(false);
        }
    }

    // This coroutine is called when the player stands up from the seat
    public void StandUp()
    {
       
       
        while (!isIdleSitting)
        {
            return;
        }
        isSitting = false;
        animator.SetBool("isSitting", false);
        photonAnimatorView.SetParameterSynchronized("isSitting", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
        Invoke(nameof(ResetMovementControl), 2.08f);
        Invoke(nameof(ActiveStandHeightAndRadius), 2f);
        
    }

    public void ResetMovementControl()
    {
        if (photonView.IsMine)
        {
            characterController.GetComponent<PlayerController>().isMovementAllowed = true;

            if (characterController != null)
            {
                characterController.enabled = true;
             
            }
        }
        else
        {
            Debug.Log("[ResetMovementControl] This PhotonView is NOT mine.");
        }

        if (assignedSeatIndex == -1)
        {
          
            assignedSeatIndex = 0;
            currentSeating.SetPositionOccupied(assignedSeatIndex, false);
        }
        photonView.RPC(nameof(RPC_SyncSeatState), RpcTarget.AllBuffered, currentSeating.seatingID, assignedSeatIndex, false,photonView.ViewID);
        assignedSeatIndex = -1;
        isIdleSitting = false;
    }

    // This RPC method syncs seat occupation status across all players
    [PunRPC]
    private void RPC_SyncSeatState(int seatingID, int positionIndex, bool isOccupied, int playerViewID)
    {
        StartCoroutine(RPC_SyncSeatState_Coroutine(seatingID, positionIndex, isOccupied, playerViewID));
    }

    private IEnumerator RPC_SyncSeatState_Coroutine(int seatingID, int positionIndex, bool isOccupied, int playerViewID)
    {

        // Wait until the SeatingManager instance is available
        while (SeatingManager.instance == null)
        {
            yield return null;
        }

        // Wait until the seating is found
        SeatingInteraction seating = null;
        while ((seating = SeatingManager.instance.GetSeatingByID(seatingID)) == null)
        {
            yield return null;
        }

        // Find the player GameObject by PhotonView ID
        GameObject playerObj = null;
        PhotonView targetView = PhotonView.Find(playerViewID);
        if (targetView != null)
            playerObj = targetView.gameObject;

        seating.SetPositionOccupied(positionIndex, isOccupied, playerObj);

        // If this is the local player, update their sitting state and animation
        if (playerObj != null)
        {
            var playerSitting = playerObj.GetComponent<PlayerSitting>();
            if (playerSitting != null)
            {
                if (playerSitting.animator == null || playerSitting.photonAnimatorView==null)
                {
                    playerSitting.animator = playerObj.GetComponent<Animator>();
                    playerSitting.photonAnimatorView = playerObj.GetComponent<PhotonAnimatorView>();
                }
                if (isOccupied)
                {
                    playerSitting.isSitting = true;
                    playerSitting.assignedSeatIndex = positionIndex;
                    playerSitting.currentSeating = seating;
                    playerSitting.animator.SetBool("isSitting", true);
                    playerSitting.photonAnimatorView.SetParameterSynchronized("isSitting", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
                }
                else
                {
                    playerSitting.isSitting = false;
                    playerSitting.assignedSeatIndex = -1;

                    playerSitting.animator.SetBool("isSitting", false);
                    playerSitting.photonAnimatorView.SetParameterSynchronized("isSitting", PhotonAnimatorView.ParameterType.Bool, PhotonAnimatorView.SynchronizeType.Discrete);
                }
            }
        }
        seating.SetPositionOccupied(positionIndex, isOccupied, playerObj);

        // --- NEW: Update sitBtn for all players in range of this seating ---
        foreach (var playerSitting in FindObjectsOfType<PlayerSitting>())
        {
            if (playerSitting.currentSeating == seating && playerSitting.isInRange && !playerSitting.isSitting)
            {
                // Defensive: check assignedSeatIndex is valid
                int idx = playerSitting.assignedSeatIndex;
                bool validIndex = idx >= 0 && seating.NoOfSeats != null && idx < seating.NoOfSeats.Length;

                if (validIndex && seating.IsPositionOccupied(idx))
                {
                    playerSitting.sitBtn.gameObject.SetActive(false);
                }
                else
                {
                    playerSitting.sitBtn.gameObject.SetActive(validIndex);
                }
            }
        }
    }


    public void ActiveIdleSitting()
    {
        isIdleSitting = true;
    }
    void ActiveStandHeightAndRadius()
    {
        if (PlayerCameraController.instance != null)
        {
            PlayerCameraController.instance.SetOrbitRadius(1.75f, 3, 1f);
            PlayerCameraController.instance.SetOrbitHeight(2.47f, 1.555f, 0.64f);
        }
        if (AvatarSpawnerOnDisconnect.Instance != null) // Minimap enable On Standup Position
        {
            AvatarSpawnerOnDisconnect.Instance.DisableUIElementsOnDisconnect[4].gameObject.SetActive(true);
        }
    }
}
