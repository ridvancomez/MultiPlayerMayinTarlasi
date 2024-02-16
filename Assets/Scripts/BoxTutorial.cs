using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class BoxTutorial : Box
{

    private ToolBoxSingle toolBoxScript;
    private PlayerData playerData;
    [SerializeField] private TutorialManager tutorialManager;

    [SerializeField] private TutorialOrders proccessTutorialMode;
    internal TutorialOrders ProccessTutorialMode { get => proccessTutorialMode; set => proccessTutorialMode = value; }

    void Start()
    {
        playerData = TextFileHandler.ReadPlayerData();

        tableWidth = new Vector2(2, 2);


        FindButtonLocation();

        images.ForEach(image => image.SetActive(false));

        rTransform = GetComponent<RectTransform>();

        toolBoxScript = FindObjectOfType<ToolBoxSingle>();
        FindNeighbors();
        FindBombBoxes();
    }

    /// <summary>
    /// Komşu kareleri bulur
    /// </summary>
    private void FindNeighbors()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                GameObject box;

                box = tutorialManager.Boxes.FirstOrDefault(x => x.GetComponent<BoxTutorial>().BoxNode.XCoordinat + i == BoxNode.XCoordinat && x.GetComponent<BoxTutorial>().BoxNode.YCoordinat + j == BoxNode.YCoordinat);

                if (box != null)
                    neighbors.Add(box);
            }
        }
    }

    /// <summary>
    /// Kareye tıklanınca toolbox ın butonları hangi sırada ve nereye gideceğini söyleyen kod
    /// </summary>
    private void ToolBoxPositionChange()
    {
        if (BoxNode.XCoordinat == 0 && BoxNode.YCoordinat == 0)
        {
            tutorialManager.Positions = Positions.Position1;
            tutorialManager.ChangePosition(rTransform.position);
        }

        else if (BoxNode.XCoordinat != 0 && BoxNode.YCoordinat == 0)
        {
            tutorialManager.Positions = Positions.Position2;
            tutorialManager.ChangePosition(rTransform.position);
        }

        else if (BoxNode.XCoordinat != 0 && BoxNode.YCoordinat != 0)
        {
            tutorialManager.Positions = Positions.Position3;
            tutorialManager.ChangePosition(rTransform.position);
        }

        else if (BoxNode.XCoordinat == 0 && BoxNode.YCoordinat != 0)
        {
            tutorialManager.Positions = Positions.Position4;
            tutorialManager.ChangePosition(rTransform.position);
        }
    }

    /// <summary>
    /// Komşu karelerin kaçı bomba ise onları bulur
    /// </summary>
    private void FindBombBoxes()
    {
        int bomb = 0;
        if (BoxNode.Type == BoxType.Bomb)
            return;

        foreach (var box in neighbors)
        {
            if (box.GetComponent<BoxTutorial>().BoxNode.Type == BoxType.Bomb)
            {
                bomb++;
            }
        }
        boxNode.BombNumber = bomb;

    }

    /// <summary>
    /// Kareye tıklandıktan sonra çalışan metot ve oyunda 9 adet kare var
    /// </summary>
    public void Clicked()
    {
        bool isThisTutorial = proccessTutorialMode == tutorialManager.Tutorial;
        
        if (isThisTutorial && tutorialManager.Tutorial == TutorialOrders.MakeMove)
        {
            tutorialManager.StartCoroutine(tutorialManager.Timer());
            OpenBox();
            tutorialManager.CloseInfoPanel();
        }

        else if (isThisTutorial && tutorialManager.Tutorial == TutorialOrders.MakeAnotherMove)
        {
            ToolBoxPositionChange();
            GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            tutorialManager.SetCoordinats(BoxNode.XCoordinat, BoxNode.YCoordinat);
        }

        else if(isThisTutorial && tutorialManager.Tutorial == TutorialOrders.PlaceFlag)
        {
            ToolBoxPositionChange();
            GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            tutorialManager.SetCoordinats(BoxNode.XCoordinat, BoxNode.YCoordinat);
        }

        else if (proccessTutorialMode == TutorialOrders.PlaceFlag && tutorialManager.Tutorial == TutorialOrders.RemoveFlag)
        {
            GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            ToolBoxPositionChange();
            tutorialManager.SetCoordinats(BoxNode.XCoordinat, BoxNode.YCoordinat);
        }

        else if(isThisTutorial && tutorialManager.Tutorial == TutorialOrders.UseBuyutec && tutorialManager.BuyutecClicked && 
            !tutorialManager.IsClicked)
        {
            tutorialManager.BuyutecChangePosition(this, true);
        }

        else if(isThisTutorial && tutorialManager.Tutorial == TutorialOrders.UseHeart)
        {
            GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            ToolBoxPositionChange();
            tutorialManager.SetCoordinats(BoxNode.XCoordinat, BoxNode.YCoordinat);
        }
    }

    public void OpenBox()
    {
        Image image = GetComponent<Image>();
        GetComponent<Button>().interactable = false;
        image.color = new Color32(255, 255, 255, 255);
        if (boxNode.IsFirstColor)
            image.sprite = buttonOpenedSpritesFirstColor[(int)imageLocation];
        else
            image.sprite = buttonOpenedSpritesSecondColor[(int)imageLocation];

        bombNumberText.text = boxNode.BombNumber != 0 ? boxNode.BombNumber.ToString() : "";
    }

    /// <summary>
    /// Bayrak, Bomba veya çarpı işaretini açar veya kapar
    /// </summary>
    public void ToggleBoxImage(int index)
    {

        images[index].SetActive(!images[index].activeSelf);
    }
}