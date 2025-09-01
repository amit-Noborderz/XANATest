using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummitSingletonClass : MonoBehaviour
{
    public XANASummitDataContainer XANASummitDataContainer;

    public static SummitSingletonClass Instance;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

}
