using System;
using UnityEngine;

public class PlayerTransformFollower : MonoBehaviour
{
    public Transform playerTransform;
    [Range(0, 1)]
    public float speed = .8f;
    Vector3 lastPos;

    void OnEnable()
    {
        lastPos = transform.position;
        speed = .8f;
    }

    private void OnDisable()
    {
        speed = 0;
        playerTransform = null;
    }

    void Update()
    {
        if (playerTransform == null && GamificationComponentData.instance.playerControllerNew)
            playerTransform = GamificationComponentData.instance.playerControllerNew.transform;

        if (playerTransform && speed > 0)
        {
            transform.position = Vector3.Lerp(lastPos, playerTransform.position, speed);
            lastPos = transform.position;
        }
    }
}
