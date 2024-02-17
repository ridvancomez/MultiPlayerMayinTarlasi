using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class BoxSingle : Box
{
    private GameManagerSinglePlayer gameManager;
    private ToolBoxSingle toolBoxScript;
    private PlayerData playerData;
    [SerializeField] private FeatureManager featureManager;

    void Start()
    {
        playerData = TextFileHandler.ReadPlayerData();
        featureManager = GameObject.FindGameObjectWithTag("FeatureManager").GetComponent<FeatureManager>();

        FindTableWitdh();
        FindButtonLocation();

        images.ForEach(image => image.SetActive(false));

        rTransform = GetComponent<RectTransform>();

        toolBoxScript = FindObjectOfType<ToolBoxSingle>();

        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerSinglePlayer>();

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

                box = gameManager.Boxes.FirstOrDefault(x => x.GetComponent<BoxSingle>().BoxNode.XCoordinat + i == BoxNode.XCoordinat && x.GetComponent<BoxSingle>().BoxNode.YCoordinat + j == BoxNode.YCoordinat);


                if (box != null)
                    neighbors.Add(box);
            }
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
            if (box.GetComponent<BoxSingle>().BoxNode.Type == BoxType.Bomb)
            {
                bomb++;
            }
        }
        boxNode.BombNumber = bomb;

    }

    /// <summary>
    /// Kareye tıklanınca toolbox ın butonları hangi sırada ve nereye gideceğini söyleyen kod
    /// </summary>
    private void ToolBoxPositionChange()
    {
        if (BoxNode.XCoordinat == 0 && BoxNode.YCoordinat == 0)
        {
            toolBoxScript.Positions = Positions.Position1;
            toolBoxScript.ChangePosition(rTransform.position);
        }

        else if (BoxNode.XCoordinat != 0 && BoxNode.YCoordinat == 0)
        {
            toolBoxScript.Positions = Positions.Position2;
            toolBoxScript.ChangePosition(rTransform.position);
        }

        else if (BoxNode.XCoordinat != 0 && BoxNode.YCoordinat != 0)
        {
            toolBoxScript.Positions = Positions.Position3;
            toolBoxScript.ChangePosition(rTransform.position);
        }

        else if (BoxNode.XCoordinat == 0 && BoxNode.YCoordinat != 0)
        {
            toolBoxScript.Positions = Positions.Position4;
            toolBoxScript.ChangePosition(rTransform.position);
        }
    }

    /// <summary>
    /// Kareye tıklandıktan sonra çalışan metot
    /// </summary>
    public void Clicked()
    {
        if (gameManager.IsBuyutecFeature && !gameManager.IsBuyutecFeatureRun && boxNode.Type != BoxType.Marked)
        {
            gameManager.IsBuyutecFeatureRun = true;
            FindAnyObjectByType<FeatureManager>().BuyutecChangePosition((Vector2)transform.position, this);
        }
        else if(!gameManager.IsBuyutecFeature && !gameManager.IsBuyutecFeatureRun)
        {
            ToolBoxPositionChange();
            if (gameManager.GameMode == GameMode.Start)
            {
                gameManager.StartTimer();
                gameManager.GameMode = GameMode.Playing;
                ClickedControl();
            }
            else if (gameManager.GameMode == GameMode.Playing)
            {
                toolBoxScript.SetCoordinats(BoxNode.XCoordinat, BoxNode.YCoordinat);
            }
        }



    }
    /// <summary>
    /// Kendi karesini kontrol eder. Eğer komşu karelerin hiçbirinde bomba yoksa komşu kareleri de kontrol etmesi için tetikler
    /// </summary>
    public void ClickedControl()
    {
        if (BoxNode.Type == BoxType.Safe || BoxNode.Type == BoxType.Marked || GameManager.FirstControl)
        { // Kutu safe veya İşaretlenmişse veya ilk kontrol ise
            if (boxNode.IsFirstColor)
                GetComponent<Image>().sprite = buttonOpenedSpritesFirstColor[(int)imageLocation];
            else
                GetComponent<Image>().sprite = buttonOpenedSpritesSecondColor[(int)imageLocation];

            if (BoxNode.Type == BoxType.Marked)
                ToggleBoxImage(0);

            BoxNode.Type = BoxType.Clicked;
            GetComponent<Button>().interactable = false;

            if (boxNode.BombNumber != 0) //Etrafında bomba kare varsa
            {
                bombNumberText.text = boxNode.BombNumber.ToString();
            }//yoksa
            else
            {
                if(boxNode.IsBomb)
                {
                    toolBoxScript.FindBox(boxNode.XCoordinat, boxNode.YCoordinat);
                    toolBoxScript.BoxMarked();
                }

                foreach (var box in neighbors)
                {
                    Node boxNode = box.GetComponent<BoxSingle>().BoxNode;

                    if (boxNode.Type == BoxType.Safe || boxNode.Type == BoxType.Marked)
                        box.GetComponent<BoxSingle>().ClickedControl();
                }
                GameManagerSinglePlayer.FirstControl = false;
            }

            if (GameManagerSinglePlayer.FirstControl) // Eğer ilk kontrol ise yani burada sadece bomba olan veya etrafında bomba varsa buraya giriyor
            {
                GameManagerSinglePlayer.FirstControl = false;

                for (int i = 0; i < neighbors.Count; i++)
                {
                    BoxSingle boxScript = neighbors[i].GetComponent<BoxSingle>();
                    if (boxScript.boxNode.Type == BoxType.Safe || boxScript.boxNode.Type == BoxType.Marked)
                        boxScript.ClickedControl();
                }
            }

        }
        else if (BoxNode.Type == BoxType.Bomb)
        {//Kutu bomba ise
            if (playerData.Heart >= 0) // Can hakkın varsa
            {
                toolBoxScript.BoxMarked();
                featureManager.Feature = FeaturesEnum.Heart;
            }
            else
            {
                gameManager.GameMode = GameMode.Lose;
                GetComponent<Image>().color = Color.yellow;
            }
        }

        IsGameOver(gameManager);
    }

    /// <summary>
    /// Bayrak, Bomba veya çarpı işaretini açar veya kapar
    /// </summary>
    public void ToggleBoxImage(int index)
    {
        images[index].SetActive(!images[index].activeSelf);
        if (index == 0)
        {
            if (images[index].activeSelf)
                gameManager.FlagNumber--;
            else if (!images[index].activeSelf)
                gameManager.FlagNumber++;
        }
    }
}