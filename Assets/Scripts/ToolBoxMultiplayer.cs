using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using UnityEngine.UI;

public class ToolBoxMultiplayer : ToolBox
{

    [Header("Ses Ayarları")]
    [SerializeField] private List<AudioSource> auidos;
    private BoxMultiplayer box;
    private GameManagerMultiplayer gameManager;
    private PhotonView pw;

    private PlayerData playerData;

    public Positions Positions
    {
        get { return positions; }
        set
        {
            positions = value;
            SelectPosition();
        }
    }

    private void Start()
    {

        pw = GetComponent<PhotonView>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManagerMultiplayer>();

        playerData = TextFileHandler.ReadPlayerData();
    }

    /// <summary>
    /// Butonların yerlerini ayarlar
    /// </summary>
    private void SelectPosition()
    {
        switch (Positions)
        {
            case Positions.Position1:
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position1[i] * new Vector2(gameManager.BoxSize / 2, gameManager.BoxSize / 2);

                }
                break;
            case Positions.Position2:
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position2[i] * new Vector2(gameManager.BoxSize / 2, gameManager.BoxSize / 2);
                }
                break;
            case Positions.Position3:
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position3[i] * new Vector2(gameManager.BoxSize / 2, gameManager.BoxSize / 2);
                }
                break;
            case Positions.Position4:
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = Coordinats.Position4[i] * new Vector2(gameManager.BoxSize / 2, gameManager.BoxSize / 2);
                }
                break;
        }
    }

    /// <summary>
    /// Hangi kareye işlem yapacağını bulur
    /// </summary>
    private void FindBox()
    {
        GameObject _box = gameManager.Boxes.FirstOrDefault(x => x.GetComponent<BoxMultiplayer>().BoxNode.XCoordinat == posX && x.GetComponent<BoxMultiplayer>().BoxNode.YCoordinat == posY);

        if (_box != null)
        {
            box = _box.GetComponent<BoxMultiplayer>();
        }
    }

    /// <summary>
    /// butonların büyüklüğünü ayarlar
    /// </summary>
    public void ToolBoxLoadInformation()
    {
        toolBoxButtonTransform.sizeDelta = new Vector2(gameManager.BoxSize, gameManager.BoxSize);

        toolBoxMain.GetComponent<RectTransform>().sizeDelta = new Vector2(gameManager.BoxSize * 2, gameManager.BoxSize * 2);

        foreach (var button in buttons)
        {
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(gameManager.BoxSize, gameManager.BoxSize);

        }
    }

    /// <summary>
    /// Hangi kareye gideceğini bulur
    /// </summary>
    public void ChangePosition(Vector2 boxPosition)
    {
        float halfBoxSize = gameManager.BoxSize / 2;
        toolBoxButtonTransform.position = boxPosition;
        Vector2 anchoredPosition = toolBoxButtonTransform.anchoredPosition;
        switch (Positions)
        {
            case Positions.Position1: //+,-
                toolBoxButtonTransform.anchoredPosition = new Vector2(anchoredPosition.x + halfBoxSize, anchoredPosition.y - halfBoxSize);
                break;
            case Positions.Position2: //-,-
                toolBoxButtonTransform.anchoredPosition = new Vector2(anchoredPosition.x - halfBoxSize, anchoredPosition.y - halfBoxSize);
                break;
            case Positions.Position3: //-,+
                toolBoxButtonTransform.anchoredPosition = new Vector2(anchoredPosition.x - halfBoxSize, anchoredPosition.y + halfBoxSize);
                break;
            case Positions.Position4: //+,+
                toolBoxButtonTransform.anchoredPosition = new Vector2(anchoredPosition.x + halfBoxSize, anchoredPosition.y + halfBoxSize);
                break;
        }
    }



    /// <summary>
    /// Tıklanan kareyi bulması için onun koordinatlarını değişkene atar
    /// </summary>
    public void SetCoordinats(int _posX, int _posY, bool randomClicked = false)
    {

        if (gameManager.GameMode == GameMode.Start || randomClicked)
        {
            posX = _posX;
            posY = _posY;

            FindBox();
            OpenTheBox();
        }
        else
        {
            if (posX == _posX && posY == _posY)
            {
                ToogleToolBoxMain(!toolBoxMain.activeSelf);
            }
            else
            {
                ToogleToolBoxMain(true);
                posX = _posX;
                posY = _posY;
            }
            FindBox();
            buttons[2].SetActive(!(box.BoxNode.Type == BoxType.Marked));
        }

    }


    /// <summary>
    /// Kareyi açan metot
    /// </summary>
    //Tool Box Button Events
    public override void OpenTheBox()
    {
        gameManager.StopTimerCalc();

        if (gameManager.GameMode == GameMode.Start)
        {
            gameManager.IsTimeFlow = true;

            if (box.BoxNode.Type == BoxType.Safe)
                pw.RPC("OpenTheBoxPun", RpcTarget.All, box.BoxNode.XCoordinat, box.BoxNode.YCoordinat);
            else if (box.BoxNode.Type == BoxType.Bomb)
                pw.RPC("BoxMarkedPun", RpcTarget.All, box.BoxNode.XCoordinat, box.BoxNode.YCoordinat);
        }

        else if (gameManager.GameMode == GameMode.Playing)
        {
            if (box.BoxNode.Type == BoxType.Safe)
            {
                pw.RPC("OpenTheBoxPun", RpcTarget.All, box.BoxNode.XCoordinat, box.BoxNode.YCoordinat);
            }
            else if (box.BoxNode.Type == BoxType.Bomb)
            {
                if (playerData.Heart > 0)
                {
                    featureManager.Feature = FeaturesEnum.Heart;
                    pw.RPC("BoxMarkedPun", RpcTarget.All, box.BoxNode.XCoordinat, box.BoxNode.YCoordinat);
                }
                else
                {
                    pw.RPC("OpenTheBoxPun", RpcTarget.All, box.BoxNode.XCoordinat, box.BoxNode.YCoordinat);
                }
            }
        }

        gameManager.TurnNumber++;
        gameManager.Pw.RPC("FollowTurnNumber", RpcTarget.All, gameManager.TurnNumber);
    }

    /// <summary>
    /// Kareye bayrak ekleyen veya kaldıran metot
    /// </summary>
    public override void BoxMarked()
    {
        gameManager.StopTimerCalc();
        pw.RPC("BoxMarkedPun", RpcTarget.All, box.BoxNode.XCoordinat, box.BoxNode.YCoordinat);

        gameManager.TurnNumber++;
        gameManager.Pw.RPC("FollowTurnNumber", RpcTarget.All, gameManager.TurnNumber);
    }


    /// <summary>
    /// Kareyi diğer oyuncuların da açmasını söyleyen metot
    /// </summary>
    [PunRPC]
    public void OpenTheBoxPun(int _posX, int _posY)
    {
        BoxMultiplayer _box = gameManager.Boxes.FirstOrDefault(x => x.GetComponent<BoxMultiplayer>().BoxNode.XCoordinat == _posX && x.GetComponent<BoxMultiplayer>().BoxNode.YCoordinat == _posY).GetComponent<BoxMultiplayer>();

        if (_box != null)
        {
            ClosedToolBox();

            if (_box.BoxNode.Type == BoxType.Safe)
            {
                if (auidos[0].isPlaying)
                    auidos[0].Stop();

                auidos[0].Play();

                _box.ClickedControl();
            }
            else if (_box.BoxNode.Type == BoxType.Bomb)
            {
                _box.gameObject.GetComponent<Image>().color = Color.yellow;
                if (gameManager.PlayerNumber == gameManager.TurnNumber)
                {
                    gameManager.ChangeGameMode(true);

                    gameManager.Pw.RPC("SycnGameMode", RpcTarget.Others, false);
                }
            }
        }
    }

    /// <summary>
    /// Bayrağı diğer oyuncuların da koyup veya kaldırmasını söyleyen metot
    /// </summary>
    [PunRPC]
    public void BoxMarkedPun(int _posX, int _posY)
    {
        BoxMultiplayer _box = gameManager.Boxes.FirstOrDefault(x => x.GetComponent<BoxMultiplayer>().BoxNode.XCoordinat == _posX && x.GetComponent<BoxMultiplayer>().BoxNode.YCoordinat == _posY).GetComponent<BoxMultiplayer>();

        if (_box != null)
        {
            ClosedToolBox();
            _box.ToggleBoxImage(0, "Marked Pun");

            if (GameManagerMultiplayer.FirstControl)
                _box.ClickedControl();

            if (_box.BoxNode.Type != BoxType.Marked)
            {
                if (auidos[0].isPlaying)
                    auidos[0].Stop();

                auidos[0].Play();

                _box.BoxNode.Type = BoxType.Marked;
            }
            else if (_box.BoxNode.IsBomb)
            {
                if (auidos[0].isPlaying)
                    auidos[0].Stop();

                auidos[0].Play();

                _box.BoxNode.Type = BoxType.Marked;

                _box.BoxNode.Type = BoxType.Bomb;
            }
            else if (!_box.BoxNode.IsBomb)
            {
                if (auidos[0].isPlaying)
                    auidos[0].Stop();

                auidos[0].Play();

                _box.BoxNode.Type = BoxType.Safe;
            }
        }
    }

}