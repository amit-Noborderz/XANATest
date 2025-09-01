using Photon.Pun;
using UnityEngine;

public class SeatingInteraction : MonoBehaviour
{
    public Transform[] NoOfSeats;  // List of positions where players can sit
    [HideInInspector]
    public int seatingID; // Unique ID for seating

    private GameObject[] seatOccupants; // Tracks which player is sitting at each seat

    private void Start()
    {
        if (SeatingManager.instance != null)
        {
            // Assign a unique seatingID using the SeatingManager's unique ID counter
            seatingID = SeatingManager.instance.GetNextSeatingID();

            // Register this seating interaction in the manager's list if not already present
            if (!SeatingManager.instance.SeatingList.Exists(si => si.seatingID == seatingID))
            {
                SeatingManager.instance.SeatingList.Add(this);
            }
        }

        // Initialize seatOccupants array to match the number of seats
        seatOccupants = new GameObject[NoOfSeats.Length];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(GameplayEntityLoader.instance != null && GameplayEntityLoader.instance.isLocalPlayer)
            {
                return;
            }
            UpdateSeatingStatus(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UpdateSeatingStatus(false);
        }
    }

    public void UpdateSeatingStatus(bool _InRange)
    {
        GameObject player = ReferencesForGamePlay.instance.m_34player;
        if (player.GetComponent<PhotonView>().IsMine && player.GetComponent<PlayerSitting>())
        {
            player.GetComponent<PlayerSitting>().SetInRange(this, _InRange);
        }
    }

    public bool IsPositionOccupied(int positionIndex)
    {
        return SeatingManager.instance.IsSeatOccupied(seatingID, positionIndex);
    }

    public void SetPositionOccupied(int positionIndex, bool occupied, GameObject player = null)
    {
        SeatingManager.instance.SetSeatOccupied(seatingID, positionIndex, occupied);

        // Ensure seatOccupants array is initialized and sized correctly
        if (seatOccupants == null || seatOccupants.Length != NoOfSeats.Length)
            seatOccupants = new GameObject[NoOfSeats.Length];

        if (occupied)
            seatOccupants[positionIndex] = player;
        else
            seatOccupants[positionIndex] = null;
    }

    // Get the player GameObject sitting at a specific seat index
    public GameObject GetPlayerAtSeat(int positionIndex)
    {
        if (seatOccupants == null || positionIndex < 0 || positionIndex >= seatOccupants.Length)
            return null;
        return seatOccupants[positionIndex];
    }
}
