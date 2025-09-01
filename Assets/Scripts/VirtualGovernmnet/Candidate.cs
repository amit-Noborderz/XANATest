using AIFLogger;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candidate : MonoBehaviour
{
    public int CanidateId;
    public string CanidateName;
    public string CanidateNameJP;
    public string VoiceLink;
    public GameObject canididateCamera;
    [Space, Header("Breaking Down")]
    public int BD_ProfileID;

    private void OnEnable()
    {
        if (CandidateSpwanner.CandidateObjectReference != null  )
        {
            CandidateSpwanner.CandidateObjectReference.Add(gameObject);
        }
     
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponentInChildren<PhotonView>().IsMine && Vector3.Distance(other.gameObject.transform.position, transform.position) < GamePlayUIHandler.inst.NearestDistance)
        {
            GamePlayUIHandler.inst.NearestDistance = Vector3.Distance(other.gameObject.transform.position, transform.position);
            GamePlayUIHandler.inst.FightBtn.SetActive(true);
            Debug.Log("setId  "+ BD_ProfileID); ;
            PlayerPrefs.SetInt("OpponentID",BD_ProfileID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponentInChildren<PhotonView>().IsMine && PlayerPrefs.GetInt("OpponentID",-1) == BD_ProfileID)
        {
            GamePlayUIHandler.inst.NearestDistance = Mathf.Infinity;
            GamePlayUIHandler.inst.FightBtn.SetActive(false);
        }
    }

    void OnCandidateSpawnedHandler(GameObject candidateObject)
    {
        print("Candidate is added : "+ candidateObject);
        CandidateSpwanner.CandidateObjectReference.Add(candidateObject);
    }
}
