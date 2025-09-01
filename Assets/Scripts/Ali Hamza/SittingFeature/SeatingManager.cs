using ExitGames.Client.Photon;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class SeatingManager : MonoBehaviourPunCallbacks
{
    public static SeatingManager instance;

    //[HideInInspector]
    public List<SeatingInteraction> SeatingList = new List<SeatingInteraction>();
    private Dictionary<int, bool[]> SeatingOccupancy = new Dictionary<int, bool[]>(); // Seat occupancy per seating area

    private int seatingIDCounter = 0; // Counter for unique seating IDs
    private const byte SeatUpdateEventCode = 1;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        Input.imeCompositionMode = IMECompositionMode.Off;
#endif
    }

    // Method to get the next unique seatingID
    public int GetNextSeatingID()
    {
        return seatingIDCounter++;
    }

    // Set seat occupancy dynamically based on seating area ID and position index
    public void SetSeatOccupied(int seatingID, int positionIndex, bool occupied)
    {
        if (!PhotonNetwork.InRoom) return;

        if (!SeatingOccupancy.ContainsKey(seatingID))
        {
            SeatingInteraction seating = GetSeatingByID(seatingID);
            int seatCount = seating.NoOfSeats.Length;
            SeatingOccupancy[seatingID] = new bool[seatCount];
        }

        SeatingOccupancy[seatingID][positionIndex] = occupied;
    }


    // Check if a seat is occupied
    public bool IsSeatOccupied(int seatingID, int positionIndex)
    {
        return SeatingOccupancy.ContainsKey(seatingID) && SeatingOccupancy[seatingID][positionIndex];
    }

    // Get SeatingInteraction by its unique ID from the seatingList
    public SeatingInteraction GetSeatingByID(int seatingID)
    {
        foreach (var seating in SeatingList)
        {
            if (seating.seatingID == seatingID)
            {
                return seating;
            }
        }
        return null;  // Return null if not found
    }

    public override void OnJoinedRoom()
    {
        LoadSeatingOccupancy();
        UpdateSeatingStatusForLateJoiners();
    }

    public void LoadSeatingOccupancy()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("SeatingOccupancy", out var seatingData))
        {
            SeatingOccupancy = (Dictionary<int, bool[]>)seatingData;
        }
    }

    private void UpdateSeatingStatusForLateJoiners()
    {
        //Debug.Log("[SeatingManager] UpdateSeatingStatusForLateJoiners called.");
        foreach (var seating in SeatingList)
        {
            for (int i = 0; i < seating.NoOfSeats.Length; i++)
            {
                bool isOccupied = IsSeatOccupied(seating.seatingID, i);
                //Debug.Log($"[SeatingManager] SeatingID={seating.seatingID}, SeatIndex={i}, isOccupied={isOccupied}");
                seating.SetPositionOccupied(i, isOccupied);
            }
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == SeatUpdateEventCode)
        {
            string jsonData = (string)photonEvent.CustomData;
            SeatData seatData = JsonConvert.DeserializeObject<SeatData>(jsonData);

            // Update local dictionary
            if (!SeatingOccupancy.ContainsKey(seatData.seatingID))
            {
                //Debug.LogError("Data Not Contain");
                SeatingOccupancy[seatData.seatingID] = new bool[10]; // Default 10 seats, change as needed
            }

            SeatingOccupancy[seatData.seatingID][seatData.positionIndex] = seatData.occupied;

           // Debug.LogError($"[Photon] Seat Updated - SeatingID: {seatData.seatingID}, Position: {seatData.positionIndex}, Occupied: {seatData.occupied}");
        }
    }
}

[System.Serializable]
public class SeatData
{
    public int seatingID;
    public int positionIndex;
    public bool occupied;

    public SeatData(int seatingID, int positionIndex, bool occupied)
    {
        this.seatingID = seatingID;
        this.positionIndex = positionIndex;
        this.occupied = occupied;
    }
}