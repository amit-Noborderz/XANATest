using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class MoveablePlatform : MonoBehaviourPun, IPunObservable
{
    [SerializeField] public Transform targetObject; // The child object to move or rotate

    public enum PlatformType { Move, Rotate }
    public PlatformType platformType;

    public enum RotationDirection { Clockwise, Counterclockwise }
    public RotationDirection rotationDirection;

    [Header("Movement Settings")]
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeed = 2f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 50f; // Degrees per second

    private Vector3 targetPosition;
    private bool movingToEnd = true;

    private Quaternion initialRotation;

    // Variables for smooth synchronization
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkVelocity;
    private float lastReceivedTime;
    PhotonView photonView;
    private void Start()
    {
        photonView = gameObject.GetComponent<PhotonView>();

        if (platformType == PlatformType.Move)
        {
            targetPosition = endPoint.position;
        }
        else if (platformType == PlatformType.Rotate)
        {
            initialRotation = targetObject.rotation;
        }

        // Initialize network position and rotation
        if (targetObject != null)
        {
            networkPosition = targetObject.position;
            networkRotation = targetObject.rotation;
        }

        // Ensure ownership
        if (!photonView.IsMine)
        {
            photonView.RequestOwnership();
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            // Local player controls the platform
            if (platformType == PlatformType.Move)
            {
                MovePlatform();
            }
            else if (platformType == PlatformType.Rotate)
            {
                RotatePlatform();
            }
        }
        else
        {
            // Smoothly interpolate position and rotation for remote players
            SmoothSync();
        }
    }

    private void MovePlatform()
    {
        if (targetObject == null) return;

        // Smoothly move the platform towards the target position
        targetObject.position = Vector3.MoveTowards(targetObject.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if the platform has reached the target position
        if (Vector3.Distance(targetObject.position, targetPosition) < 0.001f) // Use a very small threshold
        {
            // Snap to the target position to avoid floating-point inaccuracies
            targetObject.position = targetPosition;

            // Switch target position
            targetPosition = movingToEnd ? startPoint.position : endPoint.position;
            movingToEnd = !movingToEnd;
        }
    }

    private void RotatePlatform()
    {
        if (targetObject == null) return;

        // Determine rotation direction based on the enum
        float directionMultiplier = rotationDirection == RotationDirection.Clockwise ? 1f : -1f;
        float rotationStep = rotationSpeed * directionMultiplier * Time.deltaTime;

        // Smoothly rotate the platform
        Quaternion targetRotation = targetObject.rotation * Quaternion.Euler(0f, rotationStep, 0f);
        targetObject.rotation = Quaternion.RotateTowards(targetObject.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void SmoothSync()
    {
        if (targetObject == null) return;

        // Calculate the time since the last update
        float timeSinceLastUpdate = Time.time - lastReceivedTime;

        // Predict the position using velocity
        Vector3 predictedPosition = networkPosition + (networkVelocity * timeSinceLastUpdate);

        // Smoothly move towards the predicted position
        targetObject.position = Vector3.MoveTowards(targetObject.position, predictedPosition, moveSpeed * Time.deltaTime);

        // Smoothly rotate towards the predicted rotation
        targetObject.rotation = Quaternion.RotateTowards(targetObject.rotation, networkRotation, rotationSpeed * Time.deltaTime);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send position, rotation, and velocity to other clients
            stream.SendNext(targetObject.position);
            stream.SendNext(targetObject.rotation);
            stream.SendNext((targetObject.position - networkPosition) / (Time.time - lastReceivedTime)); // Calculate velocity
        }
        else
        {
            // Receive position, rotation, and velocity from other clients
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();

            // Update the last received time
            lastReceivedTime = Time.time;
        }
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    print("~~~~~~~~~~ OTHER "+other.gameObject.name);
    //    // Check if the object entering the trigger is the player
    //    if (other.CompareTag("PhotonLocalPlayer"))
    //    {
    //        // Make the player a child of the platform
    //        other.transform.SetParent(targetObject);
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    // Check if the object exiting the trigger is the player
    //    if (other.CompareTag("PhotonLocalPlayer"))
    //    {
    //        // Unparent the player from the platform
    //        other.transform.SetParent(null);
    //    }
    //}
}
#if UNITY_EDITOR
[CustomEditor(typeof(MoveablePlatform))]
public class MoveablePlatformEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target object
        MoveablePlatform platform = (MoveablePlatform)target;

        // Always show the targetObject field
        platform.targetObject = (Transform)EditorGUILayout.ObjectField("Target Object", platform.targetObject, typeof(Transform), true);

        // Draw the default enum field for PlatformType
        platform.platformType = (MoveablePlatform.PlatformType)EditorGUILayout.EnumPopup("Platform Type", platform.platformType);

        // Conditionally show fields based on the selected PlatformType
        if (platform.platformType == MoveablePlatform.PlatformType.Move)
        {
            EditorGUILayout.LabelField("Movement Settings", EditorStyles.boldLabel);
            platform.startPoint = (Transform)EditorGUILayout.ObjectField("Start Point", platform.startPoint, typeof(Transform), true);
            platform.endPoint = (Transform)EditorGUILayout.ObjectField("End Point", platform.endPoint, typeof(Transform), true);
            platform.moveSpeed = EditorGUILayout.FloatField("Move Speed", platform.moveSpeed);
        }
        else if (platform.platformType == MoveablePlatform.PlatformType.Rotate)
        {
            EditorGUILayout.LabelField("Rotation Settings", EditorStyles.boldLabel);
            platform.rotationSpeed = EditorGUILayout.FloatField("Rotation Speed", platform.rotationSpeed);
            platform.rotationDirection = (MoveablePlatform.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction", platform.rotationDirection);
        }

        // Apply changes to the serialized object
        if (GUI.changed)
        {
            EditorUtility.SetDirty(platform);
        }
    }
}
#endif
