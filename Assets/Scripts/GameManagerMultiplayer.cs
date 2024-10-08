using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManagerMultiplayer : GameManager
{
    [SerializeField] private bool isTimeFlow = true;
    internal bool IsTimeFlow { get => isTimeFlow; set { Pw.RPC("TurnTimeMode", RpcTarget.All, value); isTimeFlow = value; } }
    private int leavePlayerLength = 0;
    [SerializeField] private ToolBoxMultiplayer toolBoxManager;
    [Header("Zaman Ayarları")]
    private readonly float moveTime = 5;
    [SerializeField] private float remainingMoveTime = 5;
    private bool isMadeMove = false;

    [Header("Ödül Ceza Miktarı")]
    [SerializeField] private int prizeAmount;
    [SerializeField] private int punishAmount;

    private Coroutine timerCalcCoroutine;
    [Header("Kendi PlayerManager Scriptimiz")]
    public PlayerManager ThisPlayerManager;

    [Header("Sıra İşlemleri")]
    public int PlayerNumber = 0;
    public int TurnNumber = 0;
    public PhotonView Pw;

    [Header("InfoPanel")]
    [SerializeField] private GameObject infoPanel;

    [Header("FoulAnimator")]
    [SerializeField] private Animator foulAnimator;

    private void Start()
    {
        FeatureButtonControl();
        playerData = TextFileHandler.ReadPlayerData();

        FirstControl = true;
        Pw = GetComponent<PhotonView>();
        stopPanel.SetActive(false);

        CreatePlayer();
        JoinedRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (SceneManager.GetActiveScene().buildIndex == 2)
            PhotonNetwork.LoadLevel(0);
    }


    /// <summary>
    /// Mayın tarlasındaki kareleri oluşturur
    /// </summary>
    private void CreateTable()
    {
        bool firstColor = false;
        if (boxes.Count > 0)
            return;

        for (int i = 0; i < rowNumber; i++)
        {
            for (int j = 0; j < columnNumber; j++)
            {
                GameObject instantiatedBox = Instantiate(box, buttonsMainObject.transform);
                instantiatedBox.transform.localScale = Vector3.one;

                if (columnNumber % 2 == 0 && j == 0)
                    firstColor = !firstColor;

                firstColor = !firstColor;

                instantiatedBox.GetComponent<BoxMultiplayer>().BoxNode.XCoordinat = j;

                instantiatedBox.GetComponent<BoxMultiplayer>().BoxNode.YCoordinat = i;
                instantiatedBox.name = $"Image ({j},{i})";
                instantiatedBox.GetComponent<BoxMultiplayer>().BoxNode.IsFirstColor = firstColor;

                boxes.Add(instantiatedBox);
            }
        }
    }

    protected override void Win() => StartCoroutine(ShowTheBombedBoxes(true));

    protected override void Lose() => StartCoroutine(ShowTheBombedBoxes(false));

    /// <summary>
    /// Rastgele kare seçer ve onu bomba yapar
    /// </summary>
    private void SelectBombedBox()
    {
        for (int i = 0; i < bombNumber; i++)
        {
            GameObject go = null;
            while (go == null)
            {
                go = boxes[Random.Range(0, boxes.Count)];

                if (bombBoxes.Count == 0)
                {
                    BoxMultiplayer boxScript = go.GetComponent<BoxMultiplayer>();

                    boxScript.BoxNode.Type = BoxType.Bomb;
                    boxScript.BoxNode.IsBomb = true;
                    bombBoxes.Add(go);
                    boxScript.gameObject.GetComponent<Image>().color = Color.blue;

                    //Burayı belki ayırabilirim
                    int animalSpriteIndex = Random.Range(0, animalSprites.Count);
                    boxScript.ChangeBombAnimal(animalSpriteIndex);
                    boxScript.AnimalSpriteIndex = animalSpriteIndex;
                }
                else
                {
                    if (bombBoxes.Any(x => x.GetComponent<BoxMultiplayer>().BoxNode.XCoordinat == go.GetComponent<BoxMultiplayer>().BoxNode.XCoordinat && x.GetComponent<BoxMultiplayer>().BoxNode.YCoordinat == go.GetComponent<BoxMultiplayer>().BoxNode.YCoordinat))
                    {
                        go = null;
                    }
                    else
                    {
                        bombBoxes.Add(go);

                        BoxMultiplayer boxScript = go.GetComponent<BoxMultiplayer>();

                        int animalSpriteIndex = Random.Range(0, 19);
                        boxScript.ChangeBombAnimal(animalSpriteIndex);
                        boxScript.AnimalSpriteIndex = animalSpriteIndex;

                        boxScript.BoxNode.Type = BoxType.Bomb;
                        boxScript.BoxNode.IsBomb = true;
                    }
                }
            }
        }
        boxes.ForEach(box => box.GetComponent<BoxMultiplayer>().FindBombBoxes());
    }

    /// <summary>
    /// Oyuncu faul yaparsa cezalandırmak için kullanılan metot
    /// </summary>
    private void PlayerPunishment()
    {
        timerCalcCoroutine = null;
        isMadeMove = false;
        remainingMoveTime = moveTime;

        foulAnimator.SetTrigger("Move");
        ThisPlayerManager.Pw.RPC("ShowEventImage", RpcTarget.All, 3, 1.1f);

        List<GameObject> safeBoxes = boxes.Where(x =>
        x.GetComponent<BoxMultiplayer>().BoxNode.Type == BoxType.Safe ||
        x.GetComponent<BoxMultiplayer>().BoxNode.Type == BoxType.Bomb).ToList();

        //Eğer safe kare varsa rastgele işaretle, yoksa bir tanesini kaldır o kaldırılana tıkla
        if (safeBoxes.Count > 0)
        {
            safeBoxes[Random.Range(0, safeBoxes.Count())].GetComponent<BoxMultiplayer>().Clicked(true);
        }
        else // Yoksa rastgele bir kareden bayrak kaldırır ve o kareyi açar
        {
            List<GameObject> markedBoxes = boxes.Where(x =>
            x.GetComponent<BoxMultiplayer>().BoxNode.Type == BoxType.Marked).ToList();

            GameObject processedBox = markedBoxes[Random.Range(0, markedBoxes.Count)];

            processedBox.GetComponent<BoxMultiplayer>().ToggleBoxImage(0, "Rastgele kaldıran");

            if (processedBox.GetComponent<BoxMultiplayer>().BoxNode.IsBomb)
                processedBox.GetComponent<BoxMultiplayer>().BoxNode.Type = BoxType.Bomb;
            else
                processedBox.GetComponent<BoxMultiplayer>().BoxNode.Type = BoxType.Safe;

            processedBox.GetComponent<BoxMultiplayer>().Clicked(true);

        }
    }


    /// <summary>
    /// Büyüteç featuresi başladığında karelere bildirir ve karelerin kapsayıcısının rengini değiştirir
    /// </summary>
    protected override void ChangeBoxColor()
    {
        boxes.ForEach(box => box.GetComponent<BoxMultiplayer>().ChangeBoxColor(isBuyutecFeature));
    }

    public override void TurnMainMainMenu()
    {
        if (gameMode != GameMode.Win && gameMode != GameMode.Lose)
            PunishPlayer(punishAmount);

        base.TurnMainMainMenu();
    }
    /// <summary>
    /// Playeri oluşturuyor
    /// </summary>
    private void CreatePlayer()
    {
        GameObject go = PhotonNetwork.Instantiate("PlayerPrefab", Vector2.zero, Quaternion.identity);
        go.GetComponent<PhotonView>().Owner.NickName = TextFileHandler.ReadPlayerData().PlayerName;

        ThisPlayerManager = go.GetComponent<PlayerManager>();
        ThisPlayerManager.PlayerNumber = PhotonNetwork.PlayerList.Length - 1;
    }


    /// <summary>
    /// eğer oyuncu sayısı tam ise oyunu başlatıyor
    /// </summary>
    public void StartGame()
    {
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.PlayerList.Length)
        {
            if (PlayerNumber == 0)
            {
                gameMode = GameMode.Start;
                timerCalcCoroutine = StartCoroutine(TimerCalc());
            }
            else
                ThisPlayerManager.GetComponent<PhotonView>().RPC("SetPlayerModeText", RpcTarget.All, PlayerNumber, TurnNumber, true, (float)-1);

        }
        else
            ThisPlayerManager.GetComponent<PhotonView>().RPC("SetPlayerModeText", RpcTarget.All, PlayerNumber, TurnNumber, false, (float)-1);
    }

    /// <summary>
    /// Oyunu kaybettin veya kazandın enumunu güncelliyor. Bunu değiştirebilirim
    /// </summary>
    public void ChangeGameMode(bool isLose)
    {
        if (isLose)
            GameMode = GameMode.Lose;
        else
            GameMode = GameMode.Win;
    }

    /// <summary>
    /// Bayrak koyulduğunda bayrak sayısını bir azaltıyor, kaldırıldığında bir artırıyor. Kontrol etmem gerek
    /// </summary>
    public void SelectBomb(bool isPut)
    {
        if (isPut)
            bombNumber--;
        else
            bombNumber++;

        flagText.text = bombNumber.ToString();
    }

    /// <summary>
    /// Oyuncunun zamanını sıfırlayan metot
    /// </summary>
    public void StopTimerCalc()
    {
        if (timerCalcCoroutine != null)
        {
            isMadeMove = true;
            StopCoroutine("TimerCalc");
            timerCalcCoroutine = null;
        }
    }

    /// <summary>
    /// InfoPanelini kapatır veya açar
    /// </summary>
    public void ToogleInfoPanel() => infoPanel.SetActive(!infoPanel.activeSelf);

    private void JoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {

            SelectDifficulty();
            CreateTable();
            toolBoxManager.ToolBoxLoadInformation();
        }

        if (PhotonNetwork.InRoom)
        {
            // Oyuncunun PhotonNetwork.PlayerList içindeki sırasını bulun
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[i])
                {
                    PlayerNumber = i;
                    break;
                }
            }
        }

        StartGame();

        ThisPlayerManager.PlayerNumberForGame = PlayerNumber;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Pw.RPC("SetGameDifficulty", RpcTarget.Others, PlayerPrefs.GetInt("GameDifficulty"));

        if (PhotonNetwork.PlayerList.Length == PhotonNetwork.CurrentRoom.MaxPlayers)
        {

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            if (PhotonNetwork.IsMasterClient)
            {
                SelectBombedBox();

                foreach (var item in bombBoxes)
                {
                    Pw.RPC("ScynBombedBox", RpcTarget.Others,
                        item.GetComponent<BoxMultiplayer>().BoxNode.XCoordinat,
                        item.GetComponent<BoxMultiplayer>().BoxNode.YCoordinat,
                        item.GetComponent<BoxMultiplayer>().AnimalSpriteIndex);
                }
            }
        }
        StartGame();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        leavePlayerLength++;
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.PlayerList.Length + leavePlayerLength && GameMode != GameMode.Win && GameMode != GameMode.Lose)
        {
            if (otherPlayer == ThisPlayerManager.Pw.Owner)
                Pw.RPC("PunishPlayer", Pw.Owner, 100);

            else
                GameMode = GameMode.Win;
        }
    }

    public void PunishPlayer(int punishAmount)
    {
        playerData.Money -= punishAmount;
        TextFileHandler.WritePlayerData(playerData);
    }


    /// <summary>
    /// Diğer gelen oyunculara zorluk bilgisini iletiyor
    /// </summary>
    [PunRPC]
    public void SetGameDifficulty(int _gameDifficulty)
    {
        gameDifficulty = (GameDifficulty)_gameDifficulty;
        Debug.Log(gameDifficulty);
        SelectDifficulty();
        CreateTable();
        toolBoxManager.ToolBoxLoadInformation();
    }

    /// <summary>
    /// Hangi karede bomba varsa o karede diğer oyuncularda da bomba olması gerektiğini bildirir
    /// </summary>
    [PunRPC]
    public void ScynBombedBox(int xcoordinat, int ycoordinat, int animalIndex)
    {
        GameObject box = Boxes.FirstOrDefault(x =>
        x.GetComponent<BoxMultiplayer>().BoxNode.XCoordinat == xcoordinat &&
        x.GetComponent<BoxMultiplayer>().BoxNode.YCoordinat == ycoordinat);

        if (box != null)
        {
            BoxMultiplayer boxScript = box.GetComponent<BoxMultiplayer>();

            boxScript.BoxNode.Type = BoxType.Bomb;
            boxScript.BoxNode.IsBomb = true;
            boxScript.ChangeBombAnimal(animalIndex);

            bombBoxes.Add(box);
            boxes.ForEach(box => { box.GetComponent<BoxMultiplayer>().FindBombBoxes(); });
        }
    }

    /// <summary>
    /// Sıra geçtikten sonra bir sonraki oyuncuya sırasını savar
    /// </summary>
    [PunRPC]
    public void FollowTurnNumber(int newTurnNumber)
    {
        TurnNumber = newTurnNumber;
        TurnNumber %= PhotonNetwork.PlayerList.Length;

        if (TurnNumber == PlayerNumber)
        {
            if (gameMode != GameMode.Lose && gameMode != GameMode.Win)
                gameMode = GameMode.Playing;

            remainingMoveTime = moveTime;
            timerCalcCoroutine = StartCoroutine(TimerCalc());
        }
        else
        {
            if (gameMode != GameMode.Lose && gameMode != GameMode.Win)
                gameMode = GameMode.Waiting;

            ThisPlayerManager.GetComponent<PhotonView>().RPC("SetPlayerModeText", RpcTarget.All, PlayerNumber, TurnNumber, true, (float)-1);
        }
    }

    /// <summary>
    /// Diğer oyunculara kazandın kaybettin bilgisini verir
    /// </summary>
    [PunRPC]
    public void SycnGameMode(bool isLose)
    {
        if (isLose)
            GameMode = GameMode.Lose;
        else
            GameMode = GameMode.Win;
    }

    /// <summary>
    /// Diğer oyunculara zamanın durduğunun bilgisini verir
    /// </summary>
    [PunRPC]
    public void TurnTimeMode(bool timeMode) => isTimeFlow = timeMode;

    /// <summary>
    /// Oyuncunun hamle yapması için kalan zamanı gösterir ve azaltır
    /// </summary>
    private IEnumerator TimerCalc()
    {
        while (remainingMoveTime > 0)
        {
            if (gameMode == GameMode.Win || gameMode == GameMode.Lose || isMadeMove)
            {
                isMadeMove = false;
                break;
            }

            if (IsTimeFlow)
                remainingMoveTime -= Time.deltaTime;

            if (remainingMoveTime < 0)
                remainingMoveTime = 0;

            ThisPlayerManager.GetComponent<PhotonView>().RPC("SetPlayerModeText", RpcTarget.All, PlayerNumber, TurnNumber, true, remainingMoveTime / moveTime);

            yield return null;
        }

        if (remainingMoveTime == 0 && TurnNumber == PlayerNumber)
            PlayerPunishment();
    }

    /// <summary>
    /// Bombaları teker teker gösterir
    /// </summary>
    private IEnumerator ShowTheBombedBoxes(bool isWin)
    {
        if (isWin)
        {
            playerData.Money += prizeAmount;
            TextFileHandler.WritePlayerData(playerData);
        }

        bool finish = true;

        for (int i = 0; i < Boxes.Count; i++)
        {
            Node boxNode = Boxes[i].GetComponent<BoxMultiplayer>().BoxNode;
            if (boxNode.Type == BoxType.Safe || (!boxNode.IsBomb && boxNode.Type == BoxType.Marked))
            {
                finish = false;
                break;
            }
        }


        List<GameObject> bombedBoxes = boxes.Where(x => x.GetComponent<BoxMultiplayer>().BoxNode.IsBomb).ToList();

        foreach (GameObject box in bombedBoxes)
        {
            if (box.GetComponent<BoxMultiplayer>().BoxNode.Type == BoxType.Bomb || finish)
            {
                box.GetComponent<BoxMultiplayer>().ToggleBoxImage(1, "Bitmediyse bombayı göster");
                yield return new WaitForSeconds(Random.Range(0, 1.1f));
            }
        }

        List<GameObject> markedIncorrectlyBoxes = boxes.Where(x => x.GetComponent<BoxMultiplayer>().BoxNode.Type == BoxType.Marked &&
        !x.GetComponent<BoxMultiplayer>().BoxNode.IsBomb).ToList();

        foreach (GameObject box in markedIncorrectlyBoxes)
        {
            box.GetComponent<BoxMultiplayer>().ToggleBoxImage(2, "Hatalı olanları göster");
            yield return new WaitForSeconds(Random.Range(0, 1.1f));
        }

        losePanel.SetActive(!isWin);
        winPanel.SetActive(isWin);
    }
}