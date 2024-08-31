using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviourPunCallbacks, IInfoMenu
{
    private GameManagerMultiplayer gameManager;
    private PlayerData playerData;

    [Header("Yetenekler")]
    [SerializeField] private List<Sprite> eventSprites; // 0 = heart / 1 = buyutec / 2 = pasif hamle / 3 = foul icon

    [Header("Karakter Eşyaları")]
    [SerializeField] private List<Image> characterItems; // 0 = body / 1 = face / 2 = hair / 3 = kit
    private GameObject playerObject;

    public PhotonView Pw;
    //Oyunda hangi kutuda yer almamız gerektiğini tutuyor
    public int PlayerNumber = 0;
    //Sıramızı tutuyor
    public int PlayerNumberForGame;
    //IINfo Menu interface inden gelen property - field  lar
    public InfoData Info { get; set; }

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerMultiplayer>();
        Pw = GetComponent<PhotonView>();
        if (Pw.IsMine)
        {
            playerData = TextFileHandler.ReadPlayerData();
            if (PhotonNetwork.CurrentRoom.MaxPlayers == 2)
                Pw.RPC("SetPlayerMain", RpcTarget.All, PhotonNetwork.PlayerList.Length + 1, Pw.Owner.NickName);
            else
                Pw.RPC("SetPlayerMain", RpcTarget.All, PhotonNetwork.PlayerList.Length, Pw.Owner.NickName);

            Pw.RPC("SetPlayerItems", RpcTarget.All, playerData.ActiveBodyIndex, playerData.ActiveFaceIndex, playerData.ActiveHairIndex, playerData.ActiveKitIndex);
        }

        //IInfo interfaceinden gelen değişkenlere veri ataması
        Info = GameObject.FindGameObjectWithTag("InfoData").GetComponent<InfoData>();
    }
    private void CloseEventImage() => Pw.RPC("ShowEventImage", RpcTarget.All, -1);


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (Pw.IsMine)
        {
            Pw.RPC("SetPlayerMain", RpcTarget.Others, PlayerNumber, Pw.Owner.NickName);

            Pw.RPC("SetPlayerItems", RpcTarget.Others, playerData.ActiveBodyIndex, playerData.ActiveFaceIndex, playerData.ActiveHairIndex, playerData.ActiveKitIndex);
        }
    }

    /// <summary>
    /// Kendini oyuncuların kapsayıcı objelerinden doğru olana atar ve diğerlerine bildirir
    /// </summary>
    [PunRPC]
    public void SetPlayerMain(int playerNumber, string nickName)
    {
        PlayerNumber = playerNumber;
        playerObject = GameObject.FindGameObjectWithTag("Player" + PlayerNumber);
        transform.SetParent(playerObject.transform);
        GetComponent<RectTransform>().localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Zamanı azaltır ve bunu diğer oyunculara gösterir
    /// </summary>
    [PunRPC]
    // time e -1 veriyorum çünkü bazı yerlerde zaman verecem bazı yerlerde vermeyeceğim. kullanıyorum çünkü
    public void SetPlayerModeText(int playerNumber, int turnNumber, bool maxPlayer, float time)
    {
        if (playerNumber == PlayerNumber && GetComponent<PhotonView>().IsMine)
            if (maxPlayer)
                if (playerNumber == turnNumber)
                {
                    if (time > -1)
                        GetComponent<Image>().fillAmount = time;
                    GetComponent<Image>().color = new Color32(255, 200, 0, 255);
                }
                else
                {
                    GetComponent<Image>().fillAmount = 1;
                    GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                }
            else
            {
                GetComponent<Image>().fillAmount = 1;
                GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
        else
            if (playerNumber == turnNumber)
        {
            if (time > -1)
                GetComponent<Image>().fillAmount = time;
            GetComponent<Image>().color = new Color32(255, 200, 0, 255);
        }
        else
        {
            GetComponent<Image>().fillAmount = 1;
            GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
    }

    /// <summary>
    /// Karakter eşyaların giyer ve bunu diğer oyunculara gösterir
    /// </summary>
    [PunRPC]
    public void SetPlayerItems(int bodyIndex, int faceIndex, int hairIndex, int kitIndex)
    {
        GameManagerMultiplayer gameManagerScript = FindObjectOfType<GameManagerMultiplayer>();
        characterItems[0].sprite = gameManagerScript.Bodies[bodyIndex];
        characterItems[1].sprite = gameManagerScript.Faces[faceIndex];
        characterItems[2].sprite = gameManagerScript.Hairs[hairIndex];
        characterItems[3].sprite = gameManagerScript.Kits[kitIndex];
    }

    /// <summary>
    /// Hangi feature kullanılıyorsa o feturenin logosunu gösterir ve bunu diğer oyunculara da gösterir
    /// </summary>
    [PunRPC]
    public void ShowEventImage(int eventIndex)
    {
        if (eventIndex == -1)
        {
            transform.GetChild(1).GetComponent<Image>().sprite = null;
            transform.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 0);
        }
        else
        {
            transform.GetChild(1).GetComponent<Image>().sprite = eventSprites[eventIndex];
            transform.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
    }

    /// <summary>
    /// Hangi feature kullanılıyorsa o feturenin logosunu gösterir ve bunu diğer oyunculara da gösterir ve time süresi bittikten sonra kendini kapatır
    /// </summary>
    [PunRPC]
    public void ShowEventImage(int eventIndex, float time)
    {
        if (eventIndex == -1)
        {
            transform.GetChild(1).GetComponent<Image>().sprite = null;
            transform.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 0);
        }
        else
        {
            transform.GetChild(1).GetComponent<Image>().sprite = eventSprites[eventIndex];
            transform.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            Invoke("CloseEventImage", time);
        }

    }

    //IInfo panelinden gelen Metotlar
    //Click eventinde
    public void ShowPlayerInfo()
    {
        Info.InfoPanel.SetActive(true);
        Pw.RPC("RequestUserData", Pw.Owner, gameManager.ThisPlayerManager.Pw.Owner);
    }

    [PunRPC]
    public void RequestUserData(Player thisPlayer)
    {
        Pw.RPC("SendPlayerDataRequest", thisPlayer, Pw.Owner.ToString(), playerData.ActiveBodyIndex, playerData.ActiveFaceIndex, playerData.ActiveHairIndex, playerData.ActiveKitIndex, playerData.Heart, playerData.CyberMagnifyingGlass, playerData.PassiveMove);
    }

    [PunRPC]
    public void SendPlayerDataRequest(string playerName, int avatarBodyIndex, int avatarFaceIndex, int avatarHairIndex, int avatarKitIndex, int heartAmount, int buyutecAmount, int yonDegistirmeAmount)
    {
        Info.PlayerNameText.text = playerName;

        GameManagerMultiplayer gameManagerScript = FindObjectOfType<GameManagerMultiplayer>();
        Info.PlayerBodyImage.sprite = gameManager.Bodies[avatarBodyIndex];
        Info.PlayerFaceImage.sprite = gameManager.Faces[avatarFaceIndex];
        Info.PlayerHairImage.sprite = gameManager.Hairs[avatarHairIndex];
        Info.PlayerKitImage.sprite = gameManager.Kits[avatarKitIndex];

        Info.HeartAmount.text = heartAmount.ToString();
        Info.BuyutecAmount.text = buyutecAmount.ToString();
        Info.YonDegistirmeAmount.text = yonDegistirmeAmount.ToString();
    }
}