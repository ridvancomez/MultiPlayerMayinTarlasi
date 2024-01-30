using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviourPunCallbacks
{
    //Test için Değil   
    [SerializeField] private Button onlineButton;

    [Header("Yeni Oda Kurulması İçin Gerekenler")]
    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private TMP_Dropdown playerNumberDropDown;

    [Header("Random Oda İçin Gerekenler")]
    [SerializeField] private Button randomButton;

    [Header("Odaları Listelemek İçin Gerekenker")]
    [SerializeField] private GameObject roomButton;
    [SerializeField] private GameObject scrollContent;
    [SerializeField] private GameObject listRooms;

    private static List<RoomInfo> m_roomList = new();
    public static List<PlayerManager> Players = new();


    [Header("Karakter Eşyaları")] // sadece oyun oynarken kullanılacak
    [SerializeField] private List<Sprite> bodies;
    [SerializeField] private List<Sprite> faces;
    [SerializeField] private List<Sprite> hairs;
    [SerializeField] private List<Sprite> kits;


    public List<Sprite> Bodies { get { return bodies; } }
    public List<Sprite> Faces { get { return faces; } }
    public List<Sprite> Hairs { get { return hairs; } }
    public List<Sprite> Kits { get { return kits; } }

    void Start()
    {
        
            DontDestroyOnLoad(gameObject);

        GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>().ShowAllPanels();

        listRooms = GameObject.FindGameObjectWithTag("ListRooms");

        onlineButton = GameObject.FindGameObjectWithTag("OnlineButton").GetComponent<Button>();

        roomName = GameObject.FindGameObjectWithTag("RoomName").GetComponent<TMP_InputField>();
        playerNumberDropDown = GameObject.FindGameObjectWithTag("PlayerNumberDropDown").GetComponent<TMP_Dropdown>();

        randomButton = GameObject.FindGameObjectWithTag("RandomButton").GetComponent<Button>();

        scrollContent = GameObject.FindGameObjectWithTag("ScrollContent");

        GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>().ShowPanel(0);

        PhotonNetwork.ConnectUsingSettings();

        

        
    }

    private void JoinTargetRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        PhotonNetwork.LoadLevel(2);

    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();

        PhotonNetwork.LoadLevel(2);
    }

    public void RefResh()
    {
        randomButton.interactable = PhotonNetwork.CountOfRooms > 0;
    }

    public void CreateRoom(int gameDifficulty)
    {
        PhotonNetwork.LoadLevel(2);
        System.DateTime now = System.DateTime.Now;
        string dateTimeNow = now.ToString("ddMMyyyyHHmms");
        PlayerPrefs.SetInt("GameDifficulty", gameDifficulty);
        string roomNameString = roomName.text + dateTimeNow;
        int playerNumber = System.Convert.ToInt32(playerNumberDropDown.options[playerNumberDropDown.value].text);

        PhotonNetwork.NickName = TextFileHandler.ReadPlayerData().PlayerName;

        PhotonNetwork.JoinOrCreateRoom(roomNameString, new RoomOptions { MaxPlayers = playerNumber, IsOpen = true, IsVisible = true }, TypedLobby.Default);
    }


    public void ListRoomButtons()
    {
        GameObject[] roomButtons = GameObject.FindGameObjectsWithTag("RoomButton");

        foreach (var button in roomButtons)
        {
            Destroy(button);

        }

        foreach (var room in m_roomList)
        {
            ExitGames.Client.Photon.Hashtable customPropertes = room.CustomProperties;
            bool isClosed = false;
            if (customPropertes.ContainsKey("IsGameStarted"))
            {
                isClosed = customPropertes["IsGameStarted"] is bool;
            }

            if (room.IsOpen && room.PlayerCount > 0 && !isClosed)
            {
                GameObject instantiatedRoomButton = Instantiate(roomButton, scrollContent.transform);
                instantiatedRoomButton.GetComponent<Button>().onClick.AddListener(() => JoinTargetRoom(room.Name));
                instantiatedRoomButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = room.Name + "\t" + room.MaxPlayers.ToString() + " / " + room.PlayerCount;
            }

        }
        float size = m_roomList.Count * 150;
        RectTransform scrollRectTransform = scrollContent.GetComponent<RectTransform>();

        if (scrollRectTransform.sizeDelta.y < size)
            scrollRectTransform.sizeDelta = new Vector2(scrollRectTransform.sizeDelta.x, size);
    }

    //Server İşlemleri
    public override void OnConnectedToMaster()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            onlineButton.interactable = true;

        Debug.Log("Servere Bağlandı");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Lobiye Girildi");
        RefResh();
    }

    //Oda Listeleme İşlemleri
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
            return;

        m_roomList = roomList;
        int scrollContentHeight = 150 * roomList.Count + 30;
        scrollContent.GetComponent<RectTransform>().sizeDelta = new Vector2(scrollContent.GetComponent<RectTransform>().sizeDelta.x, scrollContentHeight);

        if (listRooms.activeSelf)
            ListRoomButtons();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom Çalıştı");
        GameObject go = PhotonNetwork.Instantiate("PlayerPrefab", Vector2.zero, Quaternion.identity);
        go.GetComponent<PhotonView>().Owner.NickName = TextFileHandler.ReadPlayerData().PlayerName;

        if (PhotonNetwork.InRoom)
        {
            // Oyuncunun PhotonNetwork.PlayerList içindeki sırasını bulun
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[i])
                {
                    go.GetComponent<PlayerManager>().PlayerNumber = i;
                    break;
                }
            }
        }
    }
}