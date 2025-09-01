using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDashHandler : MonoBehaviour
{
    PlayerController _playerController;

    [SerializeField]
    private float _dashSpeed = 10f;
    [SerializeField]
    private float _dashWalk = 6f;
    [SerializeField]
    private float _dashTime = 0.5f;
    [SerializeField]
    private float _fovIncreasingSpeed = 1.65f;
    [SerializeField]
    private float _fovDecreasingSpeed = 0.7f;
    [SerializeField]
    private Image _dashBtnImageLandscape;
    [SerializeField]
    private Image _dashBtnImagePortrait;
    [SerializeField]
    private GameObject _dashEffect;
    [SerializeField]
    private bool _canDash = true;
    //[SerializeField]
    //private Color _buttonDisableColor;
    [SerializeField]
    private AudioSource _audioSource;

    private float _sprintTime = 4f;
    private float _actualSpringSpeed;
    private float _actualWalkSpeed;
    private float _actualCameraFOV;
    public int _isDashing = 0; // Tracks whether the player is currently dashing , 0 = OFF 1 = ON
    public bool IsPlayerInOtherState
    {
        get
        {
            if (!_playerController.animator.IsInTransition(0) && (_playerController.animator.GetCurrentAnimatorStateInfo(0).IsName("NormalStatus") || _playerController.animator.GetCurrentAnimatorStateInfo(0).IsName("Dwarf Idle")))
            {
                return false;
            }
            else
                return true;
        }
    }

    public Sprite onDashImage;
    public Sprite offDashImage;
    private void OnEnable()
    {
        BuilderEventManager.AfterPlayerInstantiated += SetDashCacheStatus;
    }

    private void OnDisable()
    {
        BuilderEventManager.AfterPlayerInstantiated -= SetDashCacheStatus;

    }

    private void Start()
    {
        _isDashing = PlayerPrefs.GetInt("DashEnabled"); // Default to enabled
        if (gameObject.TryGetComponent(out PlayerController playerController))
        {
            _playerController = playerController;
            _actualSpringSpeed = _playerController.sprintSpeed;
            _actualWalkSpeed = _playerController.movementSpeed;
            _actualCameraFOV = _playerController.cinemachineFreeLook.m_Lens.FieldOfView;
        }
        else
        {
            Debug.Log("PlayerController not found");
        }
    }
    private void SetDashCacheStatus()
    {
        StartCoroutine(WaitForAnimatorAndSetDashStatus());
    }

    private IEnumerator WaitForAnimatorAndSetDashStatus()
    {
        // Wait until _playerController and _playerController.animator are set
        while (_playerController == null || _playerController.animator == null)
        {
            yield return  new WaitForEndOfFrame(); // Wait for the next frame
        }
        // Check the cached dash state and activate dash if needed
        if (_isDashing == 1)
        {
            if (GetComponent<SwimmingController>().isInPool || _playerController.IsJumping)
                yield return null;
            EmoteReactionUIHandler.lastEmotePlayed = null;
            if (IsPlayerInOtherState)
                yield return null;
            UpdateDashButtonImage(onDashImage);
            //StartCoroutine(DashRoutine());
            _playerController.sprintSpeed = _dashSpeed;
            _playerController.movementSpeed = _dashWalk;
            _playerController.cinemachineFreeLook.m_Lens.FieldOfView = 77.90008f;

        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            DashButton();
        }
    }

    public void DashButton()
    {
        // Prevent dash if the player is in the pool or jumping
        if (GetComponent<SwimmingController>().isInPool || _playerController.IsJumping)
            return;

        EmoteReactionUIHandler.lastEmotePlayed = null;

        if (_playerController == null || IsPlayerInOtherState)
            return;

        // Cancel any ongoing dash-related coroutines
        StopAllCoroutines();

        if (_isDashing == 0) // Activating Dash
        {
            _isDashing = 1;
            PlayerPrefs.SetInt("DashEnabled", 1);
            StartCoroutine(DashRoutine());

            // Update button image to "onDashImage"
            UpdateDashButtonImage(onDashImage);
        }
        else // Disabling Dash
        {
            if (_playerController.desiredMoveDirection != Vector3.zero || _playerController.desiredMoveDirectionFPP != Vector3.zero)
            {
                StartCoroutine(DashEndRoutine());
            }
            else
            {
                StartCoroutine(IdleDashEndRoutine());
            }
            _isDashing = 0;
            PlayerPrefs.SetInt("DashEnabled", 0);
            // Update button image to "offDashImage"
            UpdateDashButtonImage(offDashImage);
        }
        PlayerPrefs.Save(); // Ensure the data is written to disk
    }

    private IEnumerator DashRoutine()
    {
        // Cancel dash if the player is in the pool
        if (GetComponent<SwimmingController>().isInPool || _playerController.IsJumping)
            yield break;

        // Wait until _playerController.animator is set
        while (_playerController.animator == null)
        {
            yield return null; // Wait for the next frame
        }

        float startTime = Time.time;
        float endTime = startTime + _dashTime;

        if (_dashEffect)
        {
            var ps = _dashEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Clear(); // Stop and clear existing particles
                ps.Play();  // Start playing from the beginning
            }
        }
        _dashEffect.SetActive(true);  // Enable speed lines effect on Camera

        if (_playerController.animator)
            _playerController.animator.SetBool("IsDashing", true);

        while (Time.time < endTime)
        {
            // Increment FOV but clamp it to a maximum value
            _playerController.cinemachineFreeLook.m_Lens.FieldOfView = Mathf.Min(
                _playerController.cinemachineFreeLook.m_Lens.FieldOfView + _fovIncreasingSpeed,
                77.9f // Maximum FOV value
            );

            _playerController.characterController.Move(_dashSpeed * Time.deltaTime * transform.forward);
            yield return null;
        }

        if (_playerController.animator)
            _playerController.animator.SetBool("IsDashing", false);

        _playerController.sprintSpeed = _dashSpeed;
        _playerController.movementSpeed = _dashWalk;
    }

    private IEnumerator DashEndRoutine()
    {
        if (GetComponent<SwimmingController>().isInPool || _playerController.IsJumping)
            yield break;
        // Wait until _playerController.animator is set
        while (_playerController.animator == null)
        {
            yield return null; // Wait for the next frame
        }
        while (_playerController.cinemachineFreeLook.m_Lens.FieldOfView >= 60f)
        {
            _playerController.cinemachineFreeLook.m_Lens.FieldOfView -= _fovDecreasingSpeed;
            yield return null;
        }
        if (_dashEffect)
            _dashEffect.SetActive(false);
        _playerController.sprintSpeed = _actualSpringSpeed;
        _playerController.movementSpeed = _actualWalkSpeed;
    }

    /// <summary>
    /// To stop dash effect
    /// </summary>
    public void TerminateDashEffect()
    {
        if (_playerController.animator)
            _playerController.animator.SetBool("IsDashing", false);
        _playerController.sprintSpeed = _actualSpringSpeed;
        _playerController.movementSpeed = _actualWalkSpeed;
        _isDashing = 0;
        _playerController.cinemachineFreeLook.m_Lens.FieldOfView = _actualCameraFOV;
        if (_dashEffect)
            _dashEffect.SetActive(false);
        // Update button image to "offDashImage"
        UpdateDashButtonImage(offDashImage);
        PlayerPrefs.SetInt("DashEnabled", 0);
        PlayerPrefs.Save();
    }
    private IEnumerator IdleDashEndRoutine()
    {
        if (GetComponent<SwimmingController>().isInPool || _playerController.IsJumping)
            yield break;
        // Wait until _playerController.animator is set
        while (_playerController.animator == null)
        {
            yield return null; // Wait for the next frame
        }
        while (_playerController.cinemachineFreeLook.m_Lens.FieldOfView >= 60f)
        {
            _playerController.cinemachineFreeLook.m_Lens.FieldOfView -= _fovDecreasingSpeed;
            yield return null;
        }
       // yield return new WaitForSeconds(0.32f);
        if (_dashEffect)
            _dashEffect.SetActive(false);
        _playerController.sprintSpeed = _actualSpringSpeed;
        _playerController.movementSpeed = _actualWalkSpeed;
    }
    private void UpdateDashButtonImage(Sprite newImage)
    {
        if (_dashBtnImageLandscape != null)
        {
            _dashBtnImageLandscape.sprite = newImage;
        }

        if (_dashBtnImagePortrait != null)
        {
            _dashBtnImagePortrait.sprite = newImage;
        }
    }

}

