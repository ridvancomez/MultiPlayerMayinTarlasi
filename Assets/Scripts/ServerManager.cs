using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ServerManager : MonoBehaviourPunCallbacks
{
    //Test için Değil   
    [SerializeField] private Button onlineButton;

    [Header("Yeni Oda Kurulması İçin Gerekenler")]
    [SerializeField] private TMP_InputField roomName;

    [Header("Odaları Listelemek İçin Gerekenker")]
    [SerializeField] private GameObject roomButton;
    [SerializeField] private GameObject scrollContent;
    [SerializeField] private GameObject listRooms;

    private static List<RoomInfo> m_roomList = new();

    private bool isConnected;
    private bool startInternetControl;
    private bool disConnectedFlag;
    private bool enteredDisConnected;
    [Header("UI Manager")]
    [SerializeField] private UIManager uiManager;

    void Start()
    {
        disConnectedFlag = Time.time < 5;
        startInternetControl = false;
        uiManager.ShowAllPanels();
        listRooms = GameObject.FindGameObjectWithTag("ListRooms");
        onlineButton = GameObject.FindGameObjectWithTag("OnlineButton").GetComponent<Button>();
        roomName = GameObject.FindGameObjectWithTag("RoomName").GetComponent<TMP_InputField>();
        scrollContent = GameObject.FindGameObjectWithTag("ScrollContent");
        uiManager.ShowPanel(0);

        StartCoroutine(CheckConnectionStatus());
    }

    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            onlineButton.interactable = true;

        Debug.Log("Servere Bağlandı");
        PhotonNetwork.JoinLobby();
        isConnected = true;

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        enteredDisConnected = true;
        Debug.Log(enteredDisConnected);
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            if (!startInternetControl)
            {
                if (uiManager.IsShowPanel(2) || uiManager.IsShowPanel(3) || uiManager.IsShowPanel(4))
                    uiManager.ShowPanel(0);

                onlineButton.interactable = false;
            }
        }
        if (!disConnectedFlag || isConnected)
        {
            switch (cause)
            {
                case DisconnectCause.ServerTimeout:
                    uiManager.RunErrorPanel("Sunucu zaman aşımına uğradı");
                    break;

                case DisconnectCause.DnsExceptionOnConnect:
                    uiManager.RunErrorPanel("İnternetinizi kontrol edin");
                    break;

                default:
                    uiManager.RunErrorPanel("Bağlantı yok");
                    break;
            }

            isConnected = false;
            disConnectedFlag = true;
        }
        if (!startInternetControl)
            StartCoroutine(CheckConnectionStatus());
    }

    public override void OnConnected()
    {

        if (enteredDisConnected)
        {
            uiManager.RunErrorPanel("Bağlantı geldi");
            enteredDisConnected = false;
        }

        startInternetControl = false;

    }

    private void JoinTargetRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        uiManager.RunErrorPanel("Odaya giriş yapılırken sorun oluştu. Lütfen tekrar deneyin.");
    }

    public void JoinRandomRoom()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        uiManager.RunErrorPanel("Rastgele odaya girilirken sorun oluştu. Yeni oda kuruluyor");

        StartCoroutine(WaitAndGo(2));

        float second = 0;

        while (second <= 1)
        {
            second += Time.deltaTime;
        }
    }


    public void CreateRoom(int gameDifficulty)
    {
        System.DateTime now = System.DateTime.Now;
        string dateTimeNow = now.ToString("ddMMyyyyHHmms");
        PlayerPrefs.SetInt("GameDifficulty", gameDifficulty);
        string roomNameString = roomName.text + dateTimeNow;
        int maxPlayerNumber = 2;

        PhotonNetwork.NickName = TextFileHandler.ReadPlayerData().PlayerName;

        PhotonNetwork.JoinOrCreateRoom(roomNameString, new RoomOptions { MaxPlayers = maxPlayerNumber, IsOpen = true, IsVisible = true }, TypedLobby.Default);
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
            if (room.IsOpen && room.PlayerCount > 0 && room.MaxPlayers != room.PlayerCount && room.IsOpen && room.IsVisible)
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



    public override void OnJoinedLobby()
    {
        Debug.Log("Lobiye Girildi");
    }


    public override void OnJoinedRoom()
    {

        StartCoroutine(LoadLevelAfterJoin());
    }

    private IEnumerator LoadLevelAfterJoin()
    {
        // PhotonNetwork.LoadLevel(2); çağrısını bir Coroutine içine alıyoruz
        PhotonNetwork.LoadLevel(2);

        // Sahne yüklenene kadar bekleyelim
        while (!PhotonNetwork.IsMasterClient || PhotonNetwork.LevelLoadingProgress < 1.0f)
        {
            yield return null;
        }
    }

    private IEnumerator WaitAndGo(float second)
    {
        yield return new WaitForSeconds(second);
        CreateRoom(0);
    }

    private IEnumerator CheckConnectionStatus()
    {
        startInternetControl = true;
        while (!isConnected)
        {
            ConnectToPhoton();
            yield return new WaitForSeconds(.5f);
        }

    }

}