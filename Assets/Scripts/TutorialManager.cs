using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TutorialOrders { None, Welcome, MainMenu, MakeMove, MakeAnotherMove, PlaceFlag, RemoveFlag, CloseFeaturePanel, OpenFeaturePanel, ExplainBuyutec, UseBuyutec, UseHeart, MultiplayerTutorial, ViewPlayerDetails, ExplainFouls, UsePassiveMode, OpenGamePausePanel, CloseGamePausePanel, Finish }
public class TutorialManager : ToolBox
{

    //Verileri inspector paneline gir
    [System.Serializable]
    private class TutorialOrder
    {
        [SerializeField] private TutorialOrders tutorial;
        [SerializeField, TextArea] private string tutorialContent;
        [SerializeField] private bool tutorialIsSpecificClicked;
        public TutorialOrders Tutorial => tutorial;
        public string TutorialContent => tutorialContent;
        public bool TutorialIsSpecificClicked => tutorialIsSpecificClicked;
        public TutorialOrder(TutorialOrders _tutorial, string _tutorialContent, bool _tutorialIsSpecificClicked)
        {
            tutorial = _tutorial;
            tutorialContent = _tutorialContent;
            tutorialIsSpecificClicked = _tutorialIsSpecificClicked;
        }
    }


    [Header("Avatar Eşyaları")]
    [SerializeField] private Image face;
    [SerializeField] private Image body;
    [SerializeField] private Image hair;
    [SerializeField] private Image kit;

    [Header("Tüm Eşyalar")]
    [SerializeField] private List<Sprite> faces;
    [SerializeField] private List<Sprite> bodies;
    [SerializeField] private List<Sprite> hairs;
    [SerializeField] private List<Sprite> kits;

    [Header("Bilgi Paneli")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private float writeTime;

    [Header("Top Panel Text")]
    [SerializeField] private TextMeshProUGUI flagText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Multiplayer Verileri")]
    [SerializeField] private GameObject botAvatar;
    [SerializeField] private GameObject playerInfoPanel;

    [Header("Hamle Butonları")]
    [SerializeField] private Image firstClickButton;
    [SerializeField] private Image anotherMakeButton;
    [SerializeField] private Image placeFlagButton;
    [SerializeField] private Image featureControlButton;

    [Header("Oyun Durduma Paneli")]
    [SerializeField] private GameObject stopTheGamePanel;

    [Header("Özellik Ayarları")]
    [SerializeField] private GameObject featuresPanel;
    [SerializeField] private GameObject buyutec;
    [SerializeField] private Vector2 buyutecStartPosition;
    [SerializeField] private bool isClicked;
    [SerializeField] private float speed;
    [SerializeField] private Animator heartAnimator;
    [SerializeField] private Animator passiveMoveAnimator;
    [SerializeField] private GameObject avatarFeatureIcon;
    [SerializeField] private TextMeshProUGUI heartText;
    [SerializeField] private TextMeshProUGUI buyutecText;
    [SerializeField] private TextMeshProUGUI passiveMoveText;

    [Header("Tüm Kareler")]
    [SerializeField] private List<GameObject> boxes;

    [Header("Genel Ayarlar")]
    [SerializeField] TutorialOrders tutorialOrder;
    [SerializeField] private bool skipAfterTutorial;

    [Header("Öğretici bilgileri")]
    [SerializeField] private List<TutorialOrder> tutorialOrders;
    //[Header("ToolBox Ayarları")]

    private PlayerData playerData;
    private string infoContentText;
    private BoxTutorial box;
    private int boxSize;
    private int bombAmount;
    private TutorialOrder activeTutorial;
    private bool buyutecClicked;

    public Positions Positions { get => positions; set { positions = value; SelectPosition(); } }
    internal bool SkipAfterTutorial => skipAfterTutorial;
    internal TextMeshProUGUI FlagText { get => flagText; set => flagText = value; }
    internal TutorialOrders Tutorial => tutorialOrder;
    internal List<GameObject> Boxes => boxes;

    internal bool IsClicked => isClicked;
    internal bool BuyutecClicked => buyutecClicked;

    void Start()
    {
        boxSize = 28;
        bombAmount = 1;

        buyutecStartPosition = (Vector2)buyutec.transform.position;
        toolBoxButtonTransform.sizeDelta = new Vector2(boxSize, boxSize);
        flagText.text = "1";
        tutorialOrder = TutorialOrders.Welcome;
        ChangeStep(true);
        playerData = TextFileHandler.ReadPlayerData();
        AvatarPlacement();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && skipAfterTutorial && !activeTutorial.TutorialIsSpecificClicked && tutorialOrder != TutorialOrders.Finish)
        {
            CloseInfoPanel();
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0) && skipAfterTutorial && tutorialOrder == TutorialOrders.Finish)
        {
            StartCoroutine(TurnTheMainMenu());
            
        }
    }

    private void AvatarPlacement()
    {
        face.sprite = faces[playerData.ActiveFaceIndex];
        body.sprite = bodies[playerData.ActiveBodyIndex];
        hair.sprite = hairs[playerData.ActiveHairIndex];
        kit.sprite = kits[playerData.ActiveKitIndex];
    }

    private void ChangeStep(bool firstRun = false)
    {
        if (!firstRun)
            tutorialOrder += 1;

        if (FindTutorialOrder())
        {
            infoContentText = activeTutorial.TutorialContent;
            RunInfoPanel();
            DoTheTutorialProccess();
        }
    }

    private bool FindTutorialOrder()
    {
        activeTutorial = tutorialOrders.FirstOrDefault(t => t.Tutorial == tutorialOrder);
        return activeTutorial != null;
    }

    private void DoTheTutorialProccess()
    {
        switch (tutorialOrder)
        {
            case TutorialOrders.MakeMove:
                firstClickButton.color = new Color32(193, 193, 193, 255);
                break;
            case TutorialOrders.MakeAnotherMove:
                anotherMakeButton.color = new Color32(193, 193, 193, 255);
                break;
            case TutorialOrders.PlaceFlag:
                placeFlagButton.color = new Color32(193, 193, 193, 255);
                break;
            case TutorialOrders.RemoveFlag:
                placeFlagButton.color = new Color32(193, 193, 193, 255);
                break;
            case TutorialOrders.ExplainBuyutec:
                placeFlagButton.color = Color.white;
                break;
            case TutorialOrders.UseHeart:
                featureControlButton.color = new Color32(193, 193, 193, 255);
                featureControlButton.gameObject.GetComponent<BoxTutorial>().ProccessTutorialMode = TutorialOrders.UseHeart;
                break;
            case TutorialOrders.MultiplayerTutorial:
                botAvatar.SetActive(true);
                break;
            case TutorialOrders.OpenGamePausePanel:
                break;
            case TutorialOrders.CloseGamePausePanel:
                break;
            case TutorialOrders.Finish:
                break;
        }


    }

    /// <summary>
    /// İnfoContentText değişkenindeki yazıyı ekrana yazmaya yarar
    /// </summary>
    private void RunInfoPanel()
    {
        infoPanel.GetComponent<Image>().color = new Color(1, 1, 1, 1);

        infoPanel.SetActive(true);
        StopCoroutine(OpenInfoPanel());
        StartCoroutine(OpenInfoPanel());

        StopCoroutine(WriteInfoPanel());
        StartCoroutine(WriteInfoPanel());
    }

    internal void CloseInfoPanel()
    {
        StopCoroutine(ClosingInfoPanel());
        StartCoroutine(ClosingInfoPanel());
    }

    internal void RunHeartAnimation()
    {
        heartAnimator.SetTrigger("MoveTutorial");
        heartText.text = "0";
    }

    private void SelectPosition()
    {
        switch (positions)
        {
            case Positions.Position1:
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position1[i] * new Vector2(boxSize / 2, boxSize / 2);
                }
                break;
            case Positions.Position2:
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position2[i] * new Vector2(boxSize / 2, boxSize / 2);
                }
                break;
            case Positions.Position3:
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position3[i] * new Vector2(boxSize / 2, boxSize / 2);
                }
                break;
            case Positions.Position4:
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position4[i] * new Vector2(boxSize / 2, boxSize / 2);
                }
                break;
        }
    }

    internal void SetCoordinats(int _posX, int _posY)
    {
        if (posX == _posX && posY == _posY)
        {
            toolBoxMain.SetActive(!toolBoxMain.activeSelf);
        }
        else
        {
            toolBoxMain.SetActive(true);
            posX = _posX;
            posY = _posY;
        }
        GameObject _box = boxes.FirstOrDefault(x => x.GetComponent<BoxTutorial>().BoxNode.XCoordinat == posX && x.GetComponent<BoxTutorial>().BoxNode.YCoordinat == posY);


        if (_box != null)
        {
            box = _box.GetComponent<BoxTutorial>();
        }
        buttons[2].SetActive(!(box.BoxNode.Type == BoxType.Marked));
    }

    internal void ChangePosition(Vector2 boxPosition)
    {
        float halfBoxSize = boxSize / 2;
        toolBoxButtonTransform.position = boxPosition;
        Vector2 anchoredPosition = toolBoxButtonTransform.anchoredPosition;
        switch (Positions)
        {

            case Positions.Position1: // +,-
                toolBoxButtonTransform.anchoredPosition = new Vector2(anchoredPosition.x + halfBoxSize, anchoredPosition.y - halfBoxSize);
                break;
            case Positions.Position2: // -,-
                toolBoxButtonTransform.anchoredPosition = new Vector2(anchoredPosition.x - halfBoxSize, anchoredPosition.y - halfBoxSize);
                break;
            case Positions.Position3: // -,+
                toolBoxButtonTransform.anchoredPosition = new Vector2(anchoredPosition.x - halfBoxSize, anchoredPosition.y + halfBoxSize);
                break;
            case Positions.Position4: // +,+
                toolBoxButtonTransform.anchoredPosition = new Vector2(anchoredPosition.x + halfBoxSize, anchoredPosition.y + halfBoxSize);
                break;
        }
    }

    internal void BuyutecChangePosition(BoxTutorial _box, bool firstMove)
    {
        box = _box;
        StartCoroutine(MoveToBuyutec(box.transform.position, firstMove));
    }
    public void BuyutecChangePosition()
    {
        StartCoroutine(MoveToBuyutec(buyutecStartPosition, false));
    }

    public void OpenTheStopGamePanel()
    {
        if (tutorialOrder == TutorialOrders.OpenGamePausePanel)
        {
            stopTheGamePanel.SetActive(true);
            CloseInfoPanel();
        }
    }

    public void CloseTheStopGamePanel()
    {
        if (tutorialOrder == TutorialOrders.CloseGamePausePanel)
        {
            stopTheGamePanel.SetActive(false);
            CloseInfoPanel();
        }
    }

    public void ToogleFeaturePanel()
    {
        if (tutorialOrder == TutorialOrders.CloseFeaturePanel || tutorialOrder == TutorialOrders.OpenFeaturePanel)
        {
            featuresPanel.SetActive(!featuresPanel.activeSelf);
            CloseInfoPanel();
        }
    }

    public void OpenPlayerInfoPanel()
    {
        if (tutorialOrder == TutorialOrders.ViewPlayerDetails)
        {
            playerInfoPanel.SetActive(true);
        }
    }

    public void ClosePlayerInfoPanel()
    {
        playerInfoPanel.SetActive(false);
        CloseInfoPanel();
    }

    public void StartPassiveMove()
    {
        avatarFeatureIcon.SetActive(true);
        passiveMoveText.text = "0";
        passiveMoveAnimator.SetTrigger("MoveTutorial");
    }

    public void PutFlag()
    {
        if (tutorialOrder == TutorialOrders.PlaceFlag && box.ProccessTutorialMode == TutorialOrders.PlaceFlag)
        {
            box.BoxNode.Type = BoxType.Marked;
            CloseInfoPanel();
            toolBoxMain.SetActive(false);
            box.ToggleBoxImage(0);
            bombAmount--;
            flagText.text = bombAmount.ToString();
        }
        else if (tutorialOrder == TutorialOrders.RemoveFlag && box.ProccessTutorialMode == TutorialOrders.PlaceFlag)
        {
            box.BoxNode.Type = BoxType.Safe;
            CloseInfoPanel();
            toolBoxMain.SetActive(false);
            box.ToggleBoxImage(0);
            bombAmount++;
            flagText.text = bombAmount.ToString();
        }
    }

    public void OpenBox()
    {
        if (tutorialOrder == TutorialOrders.MakeAnotherMove && box.ProccessTutorialMode == TutorialOrders.MakeAnotherMove)
        {
            box.OpenBox();
            CloseInfoPanel();
            toolBoxMain.SetActive(false);
        }
        else if (tutorialOrder == TutorialOrders.UseHeart && box.ProccessTutorialMode == TutorialOrders.UseHeart)
        {
            RunHeartAnimation();
            box.BoxNode.Type = BoxType.Marked;
            box.ToggleBoxImage(0);
            bombAmount--;
            flagText.text = bombAmount.ToString();
            toolBoxMain.SetActive(false);
        }
    }

    public void BuyutecFeature()
    {
        if (tutorialOrder != TutorialOrders.UseBuyutec)
            return;

        buyutecText.text = "0";
        buyutecClicked = true;
        featureControlButton.color = new Color32(193, 193, 193, 255);
    }

    private IEnumerator OpenInfoPanel()
    {

        float infoPanelAlpha = 0;
        while (infoPanelAlpha <= 1)
        {
            infoPanel.GetComponent<Image>().color = new Color(1, 1, 1, infoPanelAlpha);
            infoPanelAlpha += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ClosingInfoPanel()
    {
        skipAfterTutorial = false;
        float infoPanelAlpha = 1;
        while (infoPanelAlpha >= 0)
        {
            infoPanel.GetComponent<Image>().color = new Color(1, 1, 1, infoPanelAlpha);
            infoPanelAlpha -= Time.deltaTime;
            yield return null;
        }
        infoPanel.SetActive(false);

        ChangeStep();
    }

    private IEnumerator WriteInfoPanel()
    {
        string text = "";
        infoText.text = text;
        for (int i = 0; i < infoContentText.Length; i++)
        {
            text += infoContentText[i];
            infoText.text = text;
            yield return new WaitForSeconds(writeTime);
        }

        if (tutorialOrder != TutorialOrders.MakeAnotherMove)
            skipAfterTutorial = true;
    }

    private IEnumerator MoveToBuyutec(Vector2 targetPosition, bool first)
    {
        isClicked = true;
        // Başlangıç pozisyonunu kaydet
        Vector2 startPosition = buyutec.transform.position;


        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            buyutec.transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime);

            elapsedTime += Time.deltaTime * speed;

            yield return null;
        }

        buyutec.transform.position = targetPosition;

        yield return new WaitForSeconds(.5f);

        if (first)
        {
            //Tutorial olduğu için true veriyorum
            StartCoroutine(box.ShowTheBomb(true));
        }
        else
        {
            isClicked = false;
            buyutecClicked = false;
            featureControlButton.color = Color.white;
            CloseInfoPanel();
        }
    }

    internal IEnumerator Timer()
    {
        int time = 0;
        while (tutorialOrder != TutorialOrders.Finish)
        {
            time++;
            timerText.text = time.ToString();
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator TurnTheMainMenu()
    {
        yield return new WaitForSeconds(.5f);
        SceneManager.LoadScene(0);
    }
}