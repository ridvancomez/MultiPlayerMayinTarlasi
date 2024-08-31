using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GameDifficulty { Easy, Medium, Hard }
public enum GameMode { Waiting, Start, Playing, Win, Lose }
public class GameManager : MonoBehaviourPunCallbacks
{
    protected PlayerData playerData;
    protected GameDifficulty gameDifficulty;
    protected int timer;
    protected List<GameObject> bombBoxes = new();

    [Header("Kutu Verileri")]
    [SerializeField] protected GameObject box;
    [SerializeField] protected List<GameObject> boxes = new();
    [SerializeField] protected Image buttonImageMain;
    [SerializeField] protected int boxSize;
    [SerializeField] protected GameObject buttonsMainObject;
    [SerializeField] protected GridLayoutGroup boxLayoutGroup;

    [Header("Hayvan SpriteLarı")]
    [SerializeField] protected List<Sprite> animalSprites;

    [Header("Zorluk Ayarları")]
    [SerializeField] protected int columnNumber;
    [SerializeField] protected int rowNumber;
    [SerializeField] protected int bombNumber;

    [Header("Bayrak Verileri")]
    [SerializeField] protected int flagNumber;


    [Header("Panel Ayarları")]
    [SerializeField] protected GameObject winPanel;
    [SerializeField] protected GameObject losePanel;
    [SerializeField] protected GameObject stopPanel;

    [Header("Text Ayarları")]
    [SerializeField] protected TextMeshProUGUI flagText;
    [SerializeField] protected TextMeshProUGUI timerText;

    [Header("Feature Ayarları")]
    [SerializeField] protected bool isBuyutecFeature;
    [SerializeField] protected bool isBuyutecFeatureRun;

    [Header("Oyun Modu")]
    [SerializeField] protected GameMode gameMode;

    /// <summary>
    /// 0 = Heart, 1= Büyüteç, 2 = Pasif Hamle
    /// </summary>
    [Header("Yetenek Butonları")]
    [SerializeField] protected List<Button> featureButtons;

    [Header("Karakter Eşyaları")]
    [SerializeField] protected List<Sprite> bodies;
    [SerializeField] protected List<Sprite> faces;
    [SerializeField] protected List<Sprite> hairs;
    [SerializeField] protected List<Sprite> kits;

    public List<Sprite> Bodies => bodies;
    public List<Sprite> Faces => faces;
    public List<Sprite> Hairs => hairs;
    public List<Sprite> Kits => kits;


    /// <summary>
    /// Bu First Control ilk tıklamada işe yarıyor. Default değeri False. Kareye ilk tıklandıktan sonra eğer kare bomba ise bayrak koy bir tur sayıları yaz sonra o açılanlarında bomba olup olmadığını söyle diyor, eğer bomba değil ama etrafında bomba varsa bomba olmayanları bir seferlik daha açmaya devam ediyor
    /// </summary>
    internal static bool FirstControl;

    internal int BoxSize { get { return boxSize; } }
    internal int FlagNumber { get { return flagNumber; } set { flagNumber = value; flagText.text = FlagNumber.ToString(); } }
    public bool IsBuyutecFeature { get { return isBuyutecFeature; } set { isBuyutecFeature = value; ChangeBoxColor(); } }
    public bool IsBuyutecFeatureRun { get => isBuyutecFeatureRun; set => isBuyutecFeatureRun = value;  }
    internal List<GameObject> Boxes { get { return boxes; } }
    internal GameMode GameMode
    {
        get { return gameMode; }
        set
        {
            if (gameMode == GameMode.Start)
            gameMode = value;

            else
                gameMode = value;

            switch (gameMode)
            {
                case GameMode.Win:
                    Win();
                    break;
                case GameMode.Lose:
                    Lose();
                    break;
            }
        }
    }

    public void ToogleStopMenu() => stopPanel.SetActive(!stopPanel.activeSelf);


    public void FeatureButtonControl()
    {
        playerData = TextFileHandler.ReadPlayerData();
        for (int i = 0; i < featureButtons.Count; i++)
        {
            switch (i)
            {
                case 0:
                    if (playerData.CyberMagnifyingGlass == 0)
                        featureButtons[0].interactable = false;
                    break;
                case 1:
                    if (playerData.PassiveMove == 0)
                        featureButtons[1].interactable = false;
                    break;
            }
        }
    }

    public virtual void TurnMainMainMenu()
    {

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        PhotonNetwork.LoadLevel(0);
    }

    /// <summary>
    /// Zorluğa göre kaç tane kare kaç tane bomba seçileceğini ayarlar
    /// </summary>
    public void SelectDifficulty()
    {
        if (PhotonNetwork.IsMasterClient)
            gameDifficulty = (GameDifficulty)PlayerPrefs.GetInt("GameDifficulty");

        switch (gameDifficulty)
        {
            case GameDifficulty.Easy:
                GameDifficultyLoadData(10, 16, 20, 28);
                break;

            case GameDifficulty.Medium:
                GameDifficultyLoadData(12, 18, 40, 23);
                break;

            case GameDifficulty.Hard:
                GameDifficultyLoadData(14, 20, 50, 20);
                break;
        }
    }

    public void StartTimer() => StartCoroutine(Timer());

    [PunRPC]
    public void StartTimerOnRpc() => StartTimer();

    private void GameDifficultyLoadData(int _columnNumber, int _rowNumber, int _bombNumber, int _boxSize)
    {
        columnNumber = _columnNumber;
        rowNumber = _rowNumber;
        bombNumber = _bombNumber;
        FlagNumber = bombNumber;
        boxSize = _boxSize;
        boxLayoutGroup.constraintCount = _columnNumber;
        boxLayoutGroup.cellSize = new Vector2(boxSize, boxSize);
    }

    protected virtual void Win() { }
    protected virtual void Lose() { }
    protected virtual void ChangeBoxColor() => StartCoroutine(TransitionColor(isBuyutecFeature));

    /// <summary>
    /// Oyun süresini başlatır
    /// </summary>
    protected IEnumerator Timer()
    {
        while (gameMode != GameMode.Win && gameMode != GameMode.Lose)
        {
            timer++;
            yield return new WaitForSeconds(1);
            timerText.text = timer.ToString("000");
        }
    }

    /// <summary>
    /// büyüteç feature ı çalıştığında ve bittiğinde kareleri kapsayan objenin  rengi değişmesi
    /// </summary>
    protected IEnumerator TransitionColor(bool isFeature)
    {
        float time = 0;
        time += Time.deltaTime / 2;

        while (time < 1)
        {
            time += Time.deltaTime;

            if (!isFeature)
                buttonImageMain.color = Color.Lerp(buttonImageMain.color, new Color32(222, 255, 250, 239), time);
            else
                buttonImageMain.color = Color.Lerp(buttonImageMain.color, new Color32(135, 149, 148, 255), time);

            yield return null;
        }

        if (!isFeature)
            buttonImageMain.color = new Color32(222, 255, 250, 239);
        else
            buttonImageMain.color = new Color32(135, 149, 148, 255);
    }
}