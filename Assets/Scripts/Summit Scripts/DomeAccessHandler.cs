using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static XANASummitDataContainer;

public class DomeAccessHandler : MonoBehaviour
{
    public XANASummitDataContainer SummitDataContainer;

    public static (bool, string) CheckPlayerForSpecialAvtar(DomeAccessMailInfo[] domeAccessMailInfos)
    {

        if(domeAccessMailInfos != null)
        {
            for(int i=0;i<domeAccessMailInfos.Length;i++)
            {
                if (domeAccessMailInfos[i].isAccessGiven == 0)
                    return (false, null);

                if (!string.IsNullOrEmpty(domeAccessMailInfos[i].email) && PlayerPrefs.GetString("LoggedInMail") == domeAccessMailInfos[i].email)
                {

                    return (true, domeAccessMailInfos[i].avatar);
                }
            }
        }

        return (false, null);
    }
}
