using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class BoxMultiplayer : Box
{
    private GameManagerMultiplayer gameManager;

    private ToolBoxMultiplayer toolBoxScriptMultiplayer;

    void Start()
    {
        FindTableWitdh(); // tablonun büyüklüğünü buluyor

        FindButtonLocation(); // karenin hangi pozisyonda olduğunu buluyor. Ona göre doğru spriteı alacak

        images.ForEach(image => image.SetActive(false));

        rTransform = GetComponent<RectTransform>();

        toolBoxScriptMultiplayer = FindObjectOfType<ToolBoxMultiplayer>();

        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerMultiplayer>();

        FindNeighbors();
        FindBombBoxes();
    }

    /// <summary>
    /// Karenin hangi ButtonImageLocation enumuna denk geldiğini bulan metot. Bulduktan sonra gerekli sprite ataması yapıyor
    /// </summary>
    

    /// <summary>
    /// Komşu kareleri bulan metot.1,1 koordinatındaki karenin komşuları, 0,0 - 0,1 - 0,2 - 1,0 - 1,2 - 0,2 - 1,2 - 1,3 tür
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

                box = gameManager.Boxes.FirstOrDefault(x => x.GetComponent<BoxMultiplayer>().BoxNode.XCoordinat + i == BoxNode.XCoordinat && x.GetComponent<BoxMultiplayer>().BoxNode.YCoordinat + j == BoxNode.YCoordinat);


                if (box != null)
                    neighbors.Add(box);
            }
        }
    }


    /// <summary>
    /// Kareye tıklandıktan sonra çalışacak metotlardan biri. Toolbox ın butonlarını hangi sıra ile dizileceklerini ve nereye gideceğini söyleyen metot
    /// </summary>
    private void ToolBoxPositionChange()
    {
        if (BoxNode.XCoordinat == 0 && BoxNode.YCoordinat == 0)
        {
            toolBoxScriptMultiplayer.Positions = Positions.Position1;
            toolBoxScriptMultiplayer.ChangePosition(rTransform.position);
        }

        else if (BoxNode.XCoordinat != 0 && BoxNode.YCoordinat == 0)
        {
            toolBoxScriptMultiplayer.Positions = Positions.Position2;
            toolBoxScriptMultiplayer.ChangePosition(rTransform.position);
        }

        else if (BoxNode.XCoordinat != 0 && BoxNode.YCoordinat != 0)
        {
            toolBoxScriptMultiplayer.Positions = Positions.Position3;
            toolBoxScriptMultiplayer.ChangePosition(rTransform.position);
        }

        else if (BoxNode.XCoordinat == 0 && BoxNode.YCoordinat != 0)
        {
            toolBoxScriptMultiplayer.Positions = Positions.Position4;
            toolBoxScriptMultiplayer.ChangePosition(rTransform.position);
        }
    }

    /// <summary>
    /// Komşu karelerin kaç tanesi bombalı kare olduğunu bulan metot
    /// </summary>
    internal void FindBombBoxes()
    {
        int bomb = 0;
        if (BoxNode.Type == BoxType.Bomb)
            return;

        foreach (var box in neighbors)
        {
            if (box.GetComponent<BoxMultiplayer>().BoxNode.Type == BoxType.Bomb)
            {
                bomb++;
            }
        }
        boxNode.BombNumber = bomb;

    }



    /// <summary>
    /// Kendi karesini kontrol eder. Eğer komşu karelerin hiçbirinde bomba yoksa komşu kareleri de kontrol etmesi için tetikler
    /// </summary>
    public void ClickedControl()
    {
        //Safe veya bomba kare ise
        if (BoxNode.Type == BoxType.Safe || BoxNode.Type == BoxType.Marked || GameManagerMultiplayer.FirstControl)
        {

            if (boxNode.IsFirstColor)
                GetComponent<Image>().sprite = buttonOpenedSpritesFirstColor[(int)imageLocation];
            else
                GetComponent<Image>().sprite = buttonOpenedSpritesSecondColor[(int)imageLocation];

            GetComponent<Button>().interactable = false;

            if (BoxNode.Type == BoxType.Marked)
                ToggleBoxImage(0, "ClickedControl");

            BoxNode.Type = BoxType.Clicked;

            //etrafında bomba kare varsa
            if (boxNode.BombNumber != 0)
            {
                bombNumberText.text = boxNode.BombNumber.ToString();
            }//yoksa
            else
            {
                foreach (var box in neighbors)
                {
                    Node boxNode = box.GetComponent<BoxMultiplayer>().BoxNode;

                    if (boxNode.Type == BoxType.Safe || boxNode.Type == BoxType.Marked)
                        box.GetComponent<BoxMultiplayer>().ClickedControl();
                }

                GameManagerMultiplayer.FirstControl = false;
            }

            if (GameManagerMultiplayer.FirstControl)
            {

                GameManagerMultiplayer.FirstControl = false;

                for (int i = 0; i < neighbors.Count; i++)
                {
                    BoxMultiplayer boxScript = neighbors[i].GetComponent<BoxMultiplayer>();
                    if (boxScript.boxNode.Type == BoxType.Safe || boxScript.boxNode.Type == BoxType.Marked)
                        boxScript.ClickedControl();
                }
            }
        }
        //Kare bomba ise
        else if (BoxNode.Type == BoxType.Bomb)
        {
            gameManager.GameMode = GameMode.Lose;
        }

        IsGameOver(gameManager);

    }

    /// <summary>
    /// Bayrak, Bomba veya çarpı işaretini açar veya kapar
    /// </summary>
    public void ToggleBoxImage(int index, string test)
    {
        Debug.Log(test);

        images[index].SetActive(!images[index].activeSelf);
        if (index == 0 && (gameManager.GameMode != GameMode.Win || gameManager.GameMode != GameMode.Lose))
        {
            if (images[index].activeSelf)
            {
                gameManager.FlagNumber--;
            }
            else if (!images[index].activeSelf)
            {
                gameManager.FlagNumber++;
            }
        }
    }

    //Button eventine bağlı. Kareye tıklayınca çalışan kod
    public void Clicked(bool randomClicked = false)
    {
        if (gameManager.IsBuyutecFeature && !gameManager.IsBuyutecFeatureRun)
        {
            gameManager.IsBuyutecFeatureRun = true;
            FindAnyObjectByType<FeatureManager>().BuyutecChangePosition((Vector2)transform.position, this);
        }
        else if (!gameManager.IsBuyutecFeature && !gameManager.IsBuyutecFeatureRun)
        {
            ToolBoxPositionChange();

            if (gameManager.GameMode == GameMode.Start || gameManager.GameMode == GameMode.Playing)
            {
                if (gameManager.GameMode == GameMode.Start)
                    gameManager.Pw.RPC("StartTimerOnRpc", Photon.Pun.RpcTarget.All);

                toolBoxScriptMultiplayer.SetCoordinats(BoxNode.XCoordinat, BoxNode.YCoordinat, randomClicked);
            }
        }

    }

    


}