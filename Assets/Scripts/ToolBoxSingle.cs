using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class ToolBoxSingle : ToolBox
{
    private BoxSingle box;
    private GameManagerSinglePlayer gameManager;
    PlayerData playerData;

    public Positions Positions { get => positions; set { positions = value; SelectPosition();} }

    private void Start()
    {
        playerData = TextFileHandler.ReadPlayerData();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerSinglePlayer>();

        toolBoxButtonTransform.sizeDelta = new Vector2(gameManager.BoxSize, gameManager.BoxSize);
        toolBoxMain.GetComponent<RectTransform>().sizeDelta = new Vector2(gameManager.BoxSize * 2, gameManager.BoxSize * 2);

        foreach (var button in buttons)
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(gameManager.BoxSize, gameManager.BoxSize);
    }

    private void SelectPosition()
    {
        switch (Positions)
        {
            case Positions.Position1:
                for (int i = 0; i < buttons.Count; i++)
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position1[i] * new Vector2(gameManager.BoxSize / 2, gameManager.BoxSize / 2);
                break;
            case Positions.Position2:
                for (int i = 0; i < buttons.Count; i++)
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position2[i] * new Vector2(gameManager.BoxSize / 2, gameManager.BoxSize / 2);
                break;
            case Positions.Position3:
                for (int i = 0; i < buttons.Count; i++)
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position3[i] * new Vector2(gameManager.BoxSize / 2, gameManager.BoxSize / 2);
                break;
            case Positions.Position4:
                for (int i = 0; i < buttons.Count; i++)
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position4[i] * new Vector2(gameManager.BoxSize / 2, gameManager.BoxSize / 2);
                break;
        }
    }

    private void FindBox()
    {
        GameObject _box = gameManager.Boxes.FirstOrDefault(x => x.GetComponent<BoxSingle>().BoxNode.XCoordinat == posX && x.GetComponent<BoxSingle>().BoxNode.YCoordinat == posY);

        if (_box != null)
            box = _box.GetComponent<BoxSingle>();
    }

    /// <summary>
    /// Sadece ilgili kutuyu toolBox a tanıtmak için kullanılan metot
    /// </summary>
    public void FindBox(int _posX, int _posY)
    {
        posX = _posX;
        posY = _posY;

        GameObject _box = gameManager.Boxes.FirstOrDefault(x => x.GetComponent<BoxSingle>().BoxNode.XCoordinat == posX && x.GetComponent<BoxSingle>().BoxNode.YCoordinat == posY);

        if (_box != null)
            box = _box.GetComponent<BoxSingle>();
    }

    public void ChangePosition(Vector2 boxPosition)
    {
        float halfBoxSize = gameManager.BoxSize / 2;
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

    public void SetCoordinats(int _posX, int _posY)
    {
        if (posX == _posX && posY == _posY)
            toolBoxMain.SetActive(!toolBoxMain.activeSelf);
        else
        {
            toolBoxMain.SetActive(true);
            posX = _posX;
            posY = _posY;
        }
        FindBox();
        buttons[2].SetActive(!(box.BoxNode.Type == BoxType.Marked));
    }

    //Tool Box Button Events
    public override void OpenTheBox()
    {
        ClosedToolBox();

        if (box.BoxNode.Type == BoxType.Safe)
            box.ClickedControl();
        else if (box.BoxNode.Type == BoxType.Bomb)
        {
            if (playerData.Heart > 0)
            {
                featureManager.Feature = FeaturesEnum.Heart;
                playerData.Heart--;
                TextFileHandler.WritePlayerData(playerData);
                BoxMarked();
            }
            else
            {
                gameManager.GameMode = GameMode.Lose;
                box.gameObject.GetComponent<Image>().color = Color.yellow;
            }
        }
    }

    public override void BoxMarked()
    {
        ClosedToolBox();
        box.ToggleBoxImage(0);

        if (box.BoxNode.Type != BoxType.Marked)
            box.BoxNode.Type = BoxType.Marked;

        else if (box.BoxNode.IsBomb)
            box.BoxNode.Type = BoxType.Bomb;

        else if (!box.BoxNode.IsBomb)
            box.BoxNode.Type = BoxType.Safe;
    }
}