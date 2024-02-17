using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] // butonun konumu bu enumların hangisi ise ona göre bir sprite atıyorum
public enum ButtonImageLocation { BottomLeft = 0, BottomRight, Middle, MiddleBottom, SideLeft, SideRight, TopLeft, TopRight, TopSide }
public class Box : MonoBehaviourPunCallbacks
{

    //mayın tarlasının büyüklüğünü vector2 formatında tutuyorum
    protected Vector2 tableWidth;
    //buton resimleri. Listenin sıralaması ile ButtonImageLocation enumunun sıralaması aynı yani (0. index sol üst karede kullanılan resmi temsil ediyor)
    [SerializeField] protected List<Sprite> buttonClosedSpritesFirstColor;
    [SerializeField] protected List<Sprite> buttonClosedSpritesSecondColor;

    [SerializeField] protected List<Sprite> buttonOpenedSpritesFirstColor;
    [SerializeField] protected List<Sprite> buttonOpenedSpritesSecondColor;

    [SerializeField] protected ButtonImageLocation imageLocation;

    //Ekranda etrafında kaç adet bomba yazdığını söyleyen text elemanı
    [SerializeField] protected TextMeshProUGUI bombNumberText;

    /// <summary>
    /// 0 = flag, 1 = bomb, 2 = false
    /// </summary>
    [SerializeField] protected List<GameObject> images;

    protected RectTransform rTransform;

    [SerializeField] protected Node boxNode = new();
    internal Node BoxNode { get { return boxNode; } private set { boxNode = value; } }

    //Bomba yerine kullanılacak olan hayvanların spriteları
    [SerializeField] protected List<Sprite> animalSprites;

    internal int AnimalSpriteIndex;

    //Komşu kareler
    [SerializeField] protected List<GameObject> neighbors = new();
    internal List<GameObject> Neighbors { get { return neighbors; } set { neighbors = value; } }

    /// <summary>
    /// Hangi kareye hangi sprite geleceğini buluyor
    /// </summary>
    protected void FindButtonLocation()
    {
        if (boxNode.XCoordinat == 0 && boxNode.YCoordinat == 0)
            imageLocation = ButtonImageLocation.TopLeft;

        else if (boxNode.XCoordinat == tableWidth.x && boxNode.YCoordinat == 0)
            imageLocation = ButtonImageLocation.TopRight;

        else if (boxNode.XCoordinat == 0 && boxNode.YCoordinat == tableWidth.y)
            imageLocation = ButtonImageLocation.BottomLeft;

        else if (boxNode.XCoordinat == tableWidth.x && boxNode.YCoordinat == tableWidth.y)
            imageLocation = ButtonImageLocation.BottomRight;


        else if ((boxNode.XCoordinat != 0 && boxNode.XCoordinat != tableWidth.x) && boxNode.YCoordinat == tableWidth.y)
            imageLocation = ButtonImageLocation.MiddleBottom;

        else if (boxNode.XCoordinat == tableWidth.x && (boxNode.YCoordinat != 0 && boxNode.YCoordinat != tableWidth.y))

            imageLocation = ButtonImageLocation.SideRight;

        else if (boxNode.XCoordinat != tableWidth.x && boxNode.YCoordinat != tableWidth.y && boxNode.XCoordinat != 0 && boxNode.YCoordinat != 0)
            imageLocation = ButtonImageLocation.Middle;

        else if (boxNode.XCoordinat == 0 && (boxNode.YCoordinat != 0 && boxNode.YCoordinat != tableWidth.y))
            imageLocation = ButtonImageLocation.SideLeft;

        else if ((boxNode.XCoordinat != 0 && boxNode.XCoordinat != tableWidth.x) && boxNode.YCoordinat == 0)
            imageLocation = ButtonImageLocation.TopSide;

        if (boxNode.IsFirstColor)
            GetComponent<Image>().sprite = buttonClosedSpritesFirstColor[(int)imageLocation];
        else
            GetComponent<Image>().sprite = buttonClosedSpritesSecondColor[(int)imageLocation];
    }

    /// <summary>
    /// Mayın tarlasının büyüklüğünü bulur
    /// </summary>
    protected void FindTableWitdh()
    {
        //Zorluk bilgisine ihtiyacım var çünkü butonImageLocationda kullanıyorum
        if (PlayerPrefs.HasKey("GameDifficulty"))
        {
            int gameDifficulty = PlayerPrefs.GetInt("GameDifficulty");

            switch (gameDifficulty)
            {
                case 0: //Kolay
                    tableWidth = new Vector2(9, 15);
                    break;
                case 1: //Orta
                    tableWidth = new Vector2(11, 17);
                    break;
                case 2: //Zor
                    tableWidth = new Vector2(13, 19);
                    break;
            }
        }
        else
        {
            Debug.LogError("Oyun Zorluğu Bulunamadı");
        }

    }

    /// <summary>
    /// Tüm kareler açılıp sadece bomba kareler kaldıysa Game Modunu Win yapacak
    /// </summary>
    protected void IsGameOver(GameManager gameManager)
    {
        bool finish = true;
        Node boxNode;
        for (int i = 0; i < gameManager.Boxes.Count; i++)
        {
            boxNode = gameManager.Boxes[i].GetComponent<Box>().BoxNode;
            if (boxNode.Type == BoxType.Safe || (!boxNode.IsBomb && boxNode.Type == BoxType.Marked))
                finish = false;
        }

        if (finish)
            gameManager.GameMode = GameMode.Win;
    }


    /// <summary>
    /// Büyüteç feature ile bağlı bir metot. Büyütecin hangi kareyi kontrol edebileceğini renklerle belirtmek için bir coroutune başlatan metot.
    /// büyüteç featuresi aktif ise isFeature true 
    /// </summary>

    public void ChangeBoxColor(bool isFeature)
    {
        if (boxNode.Type == BoxType.Clicked || boxNode.Type == BoxType.Marked)
        StartCoroutine(TransitionColor(isFeature));
    }

    /// <summary>
    /// Bomba sprite ını hayvan sprite ı ile değiştiriyor
    /// </summary>
    internal void ChangeBombAnimal(int index)
    {
        images[1].GetComponent<Image>().sprite = animalSprites[index];
    }

    /// <summary>
    /// Büyüteç feature si başlayınca karenin rengi yavaş şekilde değişmesi için gereken kod
    /// </summary>
    protected IEnumerator TransitionColor(bool isFeature)
    {
        float time = 0;

        time += Time.deltaTime / 2;

        while (time < 1)
        {
            time += Time.deltaTime;

            if (!isFeature)
            {
                GetComponent<Image>().color = Color.Lerp(GetComponent<Image>().color, new Color32(255, 255, 255, 255), time);
            }
            else
            {
                GetComponent<Image>().color = Color.Lerp(GetComponent<Image>().color, new Color32(157, 157, 157, 255), time);
            }
            yield return null;
        }

        if (!isFeature)
        {
            GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
        else
        {
            GetComponent<Image>().color = new Color32(157, 157, 157, 255);
        }
    }



    /// <summary>
    /// Eğer kare bomba karesi ise zamanı geldiğinde büyüteç feature bu coroutuneyi başlatır ve bomba simgesi kısa süreliğine gözükür
    /// </summary>
    public IEnumerator ShowTheBomb(bool isTutorial)
    {
        float startValue = 0f;
        float currentValue = startValue;

        while (currentValue < 1f)
        {
            currentValue += Time.deltaTime * 2;

            if (boxNode.IsBomb)
            {
                images[1].GetComponent<Image>().color = new Color(1, 1, 1, currentValue);
                images[1].SetActive(true);
            }


            yield return null;
        }

        currentValue = 1;

        if (boxNode.IsBomb)
            images[1].GetComponent<Image>().color = new Color(1, 1, 1, currentValue);


        yield return new WaitForSeconds(1);

        while (currentValue > 0f)
        {
            currentValue -= Time.deltaTime * 2;

            if (boxNode.IsBomb)
                images[1].GetComponent<Image>().color = new Color(1, 1, 1, currentValue);

            yield return null;
        }

        currentValue = 0;

        if (boxNode.IsBomb)
        {
            images[1].GetComponent<Image>().color = new Color(1, 1, 1, currentValue);
            images[1].SetActive(false);
        }

        if (isTutorial)
            FindAnyObjectByType<TutorialManager>().BuyutecChangePosition();

        else
            FindAnyObjectByType<FeatureManager>().BuyutecChangePosition();
    }
}
