using UnityEngine;

public class ObjPoolManager : MonoBehaviour
{
    public static ObjPoolManager Instance;
    //[SerializeField] ObjectPooler throwBallPool;
    //[SerializeField] ObjectPooler shurikenPool;

    //[SerializeField] ObjectPooler flagPool;
    //[SerializeField] ObjectPooler spawnEffectPool;
    [SerializeField] ObjectPooler addForcePool;
    [SerializeField] ObjectPooler collectiblePool;
    [SerializeField] ObjectPooler audioPool;
    [SerializeField] ObjectPooler quizPool;
    [SerializeField] ObjectPooler turnOffLightPool;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject GetVFXEffect(Constants.ItemComponentType type, Vector3 position, Quaternion rotation = default, int returnToPoolWaitTime = 2)
    {
        GameObject vfx = null;
        switch (type)
        {
            case Constants.ItemComponentType.AddForceComponent:
                vfx = addForcePool.GetObjectFromPool();
                break;
            case Constants.ItemComponentType.CollectibleComponent:
                vfx = collectiblePool.GetObjectFromPool();
                break;
            case Constants.ItemComponentType.AudioComponent:
                vfx = audioPool.GetObjectFromPool();
                break;
            case Constants.ItemComponentType.QuizComponent:
                vfx = quizPool.GetObjectFromPool();
                break;
            case Constants.ItemComponentType.BlindComponent:
            case Constants.ItemComponentType.SituationChangerComponent:
                vfx = turnOffLightPool.GetObjectFromPool();
                break;
            default:
                return null;
        }

        if(vfx == null) return null;
        
        vfx.transform.SetPositionAndRotation(position, rotation);
        vfx.SetActive(true);
        new Delayed.Action(() => ReturnVFXEffect(type, vfx), returnToPoolWaitTime);
        return vfx;
    }

    public void ReturnVFXEffect(Constants.ItemComponentType type, GameObject vfx)
    {
        switch (type)
        {
            case Constants.ItemComponentType.AddForceComponent:
                addForcePool.ReturnObjectToPool(vfx);
                break;
            case Constants.ItemComponentType.CollectibleComponent:
                collectiblePool.ReturnObjectToPool(vfx);
                break;
            case Constants.ItemComponentType.AudioComponent:
                audioPool.ReturnObjectToPool(vfx);
                break;
            case Constants.ItemComponentType.QuizComponent:
                quizPool.ReturnObjectToPool(vfx);
                break;
            case Constants.ItemComponentType.BlindComponent:
            case Constants.ItemComponentType.SituationChangerComponent:
                turnOffLightPool.ReturnObjectToPool(vfx);
                break;
        }
    }
}
