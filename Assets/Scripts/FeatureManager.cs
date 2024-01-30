using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum FeaturesEnum { None, Heart, Buyutec, PasifHamle }

public class FeatureManager : MonoBehaviour
{
    [SerializeField] private bool isMultiPlayer;

    [SerializeField] private GameObject featurePanel;
    [SerializeField] private List<TextMeshProUGUI> featuresText; // Heart Text, BuyutecText, PasifHamleText

    [SerializeField] private List<Sprite> featureSprite;
    [SerializeField] private GameObject buyutec;
    [SerializeField] private float speed;
    [SerializeField] private Vector2 buyutecStartPosition;
    private GameManagerSinglePlayer gameManager;
    private GameManagerMultiplayer gameManagerMultiplayer;
    [SerializeField] private ToolBox toolBox;
    [SerializeField] private float buyutecSecond;
    [SerializeField] private float currentBuyutecSecond;
    [SerializeField] private bool isClicked;

    private PlayerData playerData;


    private Box box;

    /// <summary>
    /// 0 = heart, 1 = PasifHamle
    /// </summary>
    [SerializeField] private Animator[] featureAnimators;

    [SerializeField] private FeaturesEnum feature;
    public FeaturesEnum Feature
    {
        get
        { return feature; }
        set
        {
            feature = value;

            switch (feature)
            {
                case FeaturesEnum.None:
                    NoneFeature();
                    break;
                case FeaturesEnum.Heart:
                    HeartAnimation();
                    break;
                case FeaturesEnum.Buyutec:
                    BuyutecAnimation();
                    break;
                case FeaturesEnum.PasifHamle:
                    PasifHamleAnimation();
                    break;
            }
        }
    }

    //Kontrol edeceğim scriptler: gameManagerMultiplayer yazanları kontrol et

    private void Start()
    {
        var manager = GameObject.FindGameObjectWithTag("GameController");
        isMultiPlayer = manager.GetComponent<GameManagerSinglePlayer>() == null;

        if (isMultiPlayer)
            gameManagerMultiplayer = manager.GetComponent<GameManagerMultiplayer>();
        else
            gameManager = manager.GetComponent<GameManagerSinglePlayer>();

        Debug.Log($"isMultiplayer = {isMultiPlayer}");

        playerData = TextFileHandler.ReadPlayerData();
        buyutecStartPosition = buyutec.transform.position;

        ReadFeaturesAmount();
    }

    public void ChangeFeature(int index) // 1 = Buyutec, 2 = Pasif Hamle
    {
        if (isMultiPlayer)
        {
            if (gameManagerMultiplayer.GameMode == GameMode.Playing)
            {
                if(index == 1)
                {
                    gameManagerMultiplayer.IsBuyutecFeature = true;
                    toolBox.ToogleToolBoxMain(false);
                    currentBuyutecSecond = buyutecSecond;
                    StartCoroutine(Timer());
                    Feature = FeaturesEnum.Buyutec;
                }
                else if(index == 2)
                {
                    Feature = FeaturesEnum.PasifHamle;

                    playerData.PassiveMove--;
                    TextFileHandler.WritePlayerData(playerData);

                    //Burada sıra numarasını bir artırıp GameManagerMultiplayerdeki FollowTurnNumber metodunu çalıştırır. Diğer kişiye bu metot sayesinde geçer
                    gameManagerMultiplayer.TurnNumber++;
                    gameManagerMultiplayer.Pw.RPC("FollowTurnNumber", RpcTarget.All, gameManagerMultiplayer.TurnNumber);
                }
                
            }
        }
        else
        {
            gameManager.IsBuyutecFeature = true;
            toolBox.ToogleToolBoxMain(false);
            currentBuyutecSecond = buyutecSecond;
            StartCoroutine(Timer());
            Feature = FeaturesEnum.Buyutec;

        }



    }

    public void BuyutecChangePosition(Vector2 newPosition, Box box)
    {
        this.box = box;
        StartCoroutine(MoveToBuyutec(newPosition, true));
    }
    public void BuyutecChangePosition()
    {
        StartCoroutine(MoveToBuyutec(buyutecStartPosition, false));
    }

    private void NoneFeature()
    {
        if (isMultiPlayer)
            gameManagerMultiplayer.ThisPlayerManager.GetComponent<PhotonView>().RPC("ShowEventImage", RpcTarget.All, -1);
        TimeManage(true);
    }

    private void HeartAnimation()
    {
        TimeManage(false);
        featureAnimators[0].SetTrigger("Move");

        playerData.Heart--;
        TextFileHandler.WritePlayerData(playerData);

        if (isMultiPlayer)
            gameManagerMultiplayer.ThisPlayerManager.GetComponent<PhotonView>().RPC("ShowEventImage", RpcTarget.All, 0);

        ReadFeaturesAmount();
    }
    private void BuyutecAnimation()
    {
        TimeManage(false);

        if (isMultiPlayer)
            gameManagerMultiplayer.ThisPlayerManager.GetComponent<PhotonView>().RPC("ShowEventImage", RpcTarget.All, 1);

        playerData.CyberMagnifyingGlass--;
        TextFileHandler.WritePlayerData(playerData);

        ReadFeaturesAmount();
    }
    private void PasifHamleAnimation()
    {
        TimeManage(false);
        featureAnimators[1].SetTrigger("Move");
        gameManagerMultiplayer.ThisPlayerManager.GetComponent<PhotonView>().RPC("ShowEventImage", RpcTarget.All, 2);

        ReadFeaturesAmount();
    }

    /// <summary>
    /// Feature lerin miktarını textlere yazıyor
    /// </summary>
    private void ReadFeaturesAmount()
    {
        playerData = TextFileHandler.ReadPlayerData();
        featuresText[0].text = playerData.Heart.ToString();
        featuresText[1].text = playerData.CyberMagnifyingGlass.ToString();
        featuresText[2].text = playerData.PassiveMove.ToString();
    }

    private void TimeManage(bool isStart)
    {
        if (isMultiPlayer)
            gameManagerMultiplayer.IsTimeFlow = isStart;
    }

    public void ToogleFeaturePanel() => featurePanel.SetActive(!featurePanel.activeSelf);

    IEnumerator MoveToBuyutec(Vector2 targetPosition, bool first)
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
            StartCoroutine(box.ShowTheBomb());
        }
        else
        {
            isClicked = false;
            if (isMultiPlayer)
                gameManagerMultiplayer.IsBuyutecFeature = false;
            else
                gameManager.IsBuyutecFeature = false;

            TimeManage(true);
            Feature = FeaturesEnum.None;
        }
    }

    IEnumerator Timer()
    {
        while (currentBuyutecSecond > 0 && !isClicked)
        {
            currentBuyutecSecond -= Time.deltaTime;
            yield return null;
        }


        if (!isClicked)
        {
            currentBuyutecSecond = 0;
            if (isMultiPlayer)
                gameManagerMultiplayer.IsBuyutecFeature = false;
            else
                gameManager.IsBuyutecFeature = true;

            TimeManage(true);
            Feature = FeaturesEnum.None;
        }


    }
}