using System.Collections;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private ServerManager serverManager;

    [SerializeField] private TMP_InputField newNick;
    [SerializeField] private TMP_InputField changeNick;

    [SerializeField] private Animator changeNickAnimator;
    [SerializeField] private GameObject newNickPanel;
    [SerializeField] private List<GameObject> panels;
    [SerializeField] private SrollSettings scrollSettingsMain;
    [SerializeField] private SrollSettings scrollSettingsShop;

    [Header("Aktif Karakter Eşyası")]
    [SerializeField, Tooltip("Aktif Karakter Eşyası")] private List<GameObject> items; // 0 = body / 1 = face / 2 = hair / 3 = kit

    [Header("Karakter Eşyaları")] // sadece UI için
    [SerializeField] private List<Products> bodies;
    [SerializeField] private List<Products> faces;
    [SerializeField] private List<Products> hairs;
    [SerializeField] private List<Products> kits;
    [SerializeField] private Transform middlePanel;
    [SerializeField] private Transform scrollView;
    [SerializeField] private ScrollRect scrollRectShop;
    [SerializeField] private RectTransform scrollRectShopContent;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI playerKitShopPriceText;
    [SerializeField] private Button priceButton;
    [SerializeField] private int characterProductNumber;

    private int middlePanelIndex;
    private int scrollViewIndex;
    [SerializeField] private float maxWidth;
    [SerializeField] private float contentCurrentPosX;
    // maxWidth = scrollRectShopContent.anchoredPosition.x;
    [SerializeField] private int productIndex;

    [Header("Skill Market Ayarları")]
    [Header("Yetenek Fiyatları")]
    [SerializeField] private int heartPrice;
    [SerializeField] private int buyutecPrice;
    [SerializeField] private int yonDegistirmePrice;

    [Header("Yetenek Fiyatı Textleri")]
    [SerializeField] private TextMeshProUGUI heartPriceText;
    [SerializeField] private TextMeshProUGUI buyutecPriceText;
    [SerializeField] private TextMeshProUGUI yonDegistirmePriceText;

    [Header("Oyuncu Parası")]
    [SerializeField] private TextMeshProUGUI playerSkillShopPriceText;

    [Header("Yetenek Miktarı")]
    [SerializeField] private TextMeshProUGUI heartAmountText;
    [SerializeField] private TextMeshProUGUI buyutecAmountText;
    [SerializeField] private TextMeshProUGUI yonDegistirmeAmountText;

    [Header("Yetenek Satın Alma Butonları")]
    [SerializeField] private Button heartBuyButton;
    [SerializeField] private Button buyutecBuyButton;
    [SerializeField] private Button yonDegistirmeBuyButton;




    private PlayerData playerData;

    [System.Serializable]
    private class SrollSettings
    {
        public ScrollRect ScrollRect;
        public float StartSize = -400;
        public float StepSize = 805f; // Adım büyüklüğü
        public float SmoothTime = 0.3f; // Pürüzsüz kaydırma süresi
        public float SnapSpeed;

        public float SnapForce;

        public float CurrentPosition;
    }

    [System.Serializable]
    private class Products
    {
        public int Price;
        public GameObject Image;

        public Sprite GetImageSprite => Image.GetComponent<Image>().sprite;
    }
    private void Awake()
    {

        if (!System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, "playerDatas.json")))
        {
            newNickPanel.SetActive(true);
            middlePanelIndex = middlePanel.GetSiblingIndex();
            scrollViewIndex = scrollView.GetSiblingIndex();
            return;
        }
        playerData = TextFileHandler.ReadPlayerData();

        UpdateCharacterFutures();

        if (PlayerPrefs.HasKey("GameDifficulty"))
            PlayerPrefs.DeleteKey("GameDifficulty");

        middlePanelIndex = middlePanel.GetSiblingIndex();
        scrollViewIndex = scrollView.GetSiblingIndex();

        for (int i = 0; i < playerData.OpenedBodysIndex.Count; i++)
            bodies[playerData.OpenedBodysIndex[i]].Image.transform.GetChild(0).gameObject.SetActive(false);

        for (int i = 0; i < playerData.OpenedFacesIndex.Count; i++)
            faces[playerData.OpenedFacesIndex[i]].Image.transform.GetChild(0).gameObject.SetActive(false);


        for (int i = 0; i < playerData.OpenedHairsIndex.Count; i++)
            hairs[playerData.OpenedHairsIndex[i]].Image.transform.GetChild(0).gameObject.SetActive(false);

        for (int i = 0; i < playerData.OpenedKitsIndex.Count; i++)
            kits[playerData.OpenedKitsIndex[i]].Image.transform.GetChild(0).gameObject.SetActive(false);



        WritePlayerMoney();

        //Skillerin ücretlerini texte yazar
        heartPriceText.text = heartPrice.ToString();
        buyutecPriceText.text = buyutecPrice.ToString();
        yonDegistirmePriceText.text = yonDegistirmePrice.ToString();

        WriteSkillText();
    }

    private void Update()
    {
        if (scrollSettingsMain.ScrollRect.gameObject.activeSelf)
            GradualTransition(scrollSettingsMain);


        if (scrollSettingsShop.ScrollRect.gameObject.activeSelf)
            GradualTransition(scrollSettingsShop);
    }
    
    /// <summary>
    /// Karakterin Parasını Yazar
    /// </summary>
    private void WritePlayerMoney()
    {
        playerKitShopPriceText.text = "Para: " + playerData.Score.ToString() + " $";
        playerSkillShopPriceText.text = "Para: " + playerData.Score.ToString() + " $";
    }

    private void GradualTransition(SrollSettings scrollSettings)
    {
        if (scrollSettings.ScrollRect.velocity.magnitude < 200)
        {
            scrollSettings.ScrollRect.velocity = Vector2.zero;

            scrollSettings.SnapSpeed = scrollSettings.SnapForce * Time.deltaTime;

            scrollSettings.CurrentPosition = scrollSettings.ScrollRect.content.anchoredPosition.x - scrollSettings.StartSize;

            int pos = (int)System.Math.Round(scrollSettings.CurrentPosition / scrollSettings.StepSize);


            scrollSettings.CurrentPosition = pos * scrollSettings.StepSize;

            scrollSettings.CurrentPosition += scrollSettings.StartSize;

            scrollSettings.ScrollRect.content.anchoredPosition = new Vector2(Mathf.MoveTowards(
                scrollSettings.ScrollRect.content.anchoredPosition.x,
                scrollSettings.CurrentPosition, scrollSettings.SnapSpeed), scrollSettings.ScrollRect.content.anchoredPosition.y);

            contentCurrentPosX = scrollRectShopContent.anchoredPosition.x;

            productIndex = (int)(maxWidth - contentCurrentPosX) / 200;

            switch (characterProductNumber)
            {
                case 1:
                    WritePriceText(bodies, 1);
                    break;
                case 2:
                    WritePriceText(faces, 2);
                    break;
                case 3:
                    WritePriceText(hairs, 3);
                    break;
                case 4:
                    WritePriceText(kits, 4);

                    break;
                default:
                    priceText.text = "------";
                    priceButton.interactable = false;
                    break;
            }

        }
        if (scrollSettings.ScrollRect.velocity.magnitude > 200)
        {
            priceText.text = "------";
            priceButton.interactable = false;
            scrollSettings.SnapSpeed = 0;
        }
    }

    private void WritePriceText(List<Products> product, int productNumber) // 1 = body, 2 = face, 3 = hairs, 4 = kits
    {
        if (product.Count > productIndex && productIndex > -1)
        {
            switch (productNumber)
            {
                case 1: // Body
                    if (playerData.OpenedBodysIndex.Contains(productIndex))
                    {

                        if (playerData.ActiveBodyIndex == productIndex)
                        {
                            priceText.text = "Aktif";
                            priceButton.interactable = false;

                            UpdateCharacterFutures(1);
                        }
                        else
                        {
                            priceText.text = "Giyilsin Mi";
                            priceButton.interactable = true;
                        }
                    }
                    else
                    {
                        priceText.text = $"Fiyat {product[productIndex].Price}";

                        priceButton.interactable = playerData.Score >= product[productIndex].Price;
                    }
                    break;
                case 2: // Face
                    if (playerData.OpenedFacesIndex.Contains(productIndex))
                    {

                        if (playerData.ActiveFaceIndex == productIndex)
                        {
                            Debug.Log("Aktif");
                            priceText.text = "Aktif";
                            priceButton.interactable = false;

                            UpdateCharacterFutures(2);
                        }
                        else
                        {
                            priceText.text = "Giyilsin Mi";
                            priceButton.interactable = true;
                        }
                    }
                    else
                    {
                        priceText.text = $"Fiyat {product[productIndex].Price}";
                        priceButton.interactable = true;
                    }
                    break;
                case 3: // Hair
                    if (playerData.OpenedHairsIndex.Contains(productIndex))
                    {

                        if (playerData.ActiveHairIndex == productIndex)
                        {
                            Debug.Log("Aktif");
                            priceText.text = "Aktif";
                            priceButton.interactable = false;

                            UpdateCharacterFutures(3);
                        }
                        else
                        {
                            priceText.text = "Giyilsin Mi";
                            priceButton.interactable = true;
                        }
                    }
                    else
                    {
                        priceText.text = $"Fiyat {product[productIndex].Price}";
                        priceButton.interactable = true;
                    }
                    break;
                case 4: // Kit
                    if (playerData.OpenedKitsIndex.Contains(productIndex))
                    {
                        UpdateCharacterFutures(4);
                        if (playerData.ActiveKitIndex == productIndex)
                        {
                            priceText.text = "Aktif";
                            priceButton.interactable = false;


                        }
                        else
                        {
                            priceText.text = "Giyilsin Mi";
                            priceButton.interactable = true;
                        }
                    }
                    else
                    {
                        priceText.text = $"Fiyat {product[productIndex].Price}";
                        priceButton.interactable = true;
                    }
                    break;
            }



        }
        else
        {
            priceText.text = "------";
            priceButton.interactable = false;
        }
    }
    /// <summary>
    /// Tüm karakter eşyalarını günceller
    /// </summary>
    private void UpdateCharacterFutures()
    {
        items[3].GetComponent<Image>().sprite = kits[playerData.ActiveKitIndex].GetImageSprite;
        items[2].GetComponent<Image>().sprite = hairs[playerData.ActiveHairIndex].GetImageSprite;
        items[1].GetComponent<Image>().sprite = faces[playerData.ActiveFaceIndex].GetImageSprite;
        items[0].GetComponent<Image>().sprite = bodies[playerData.ActiveBodyIndex].GetImageSprite;
    }

    /// <summary>
    /// Sayıyı girdiğiniz karakter eşyasını günceller
    /// 1 = Body
    /// 2 = Face
    /// 3 = Hair
    /// 4 = Kits
    /// </summary>
    private void UpdateCharacterFutures(int futuresType)
    {
        switch (futuresType) 
        {
            case 1:
                items[0].GetComponent<Image>().sprite = bodies[playerData.ActiveBodyIndex].GetImageSprite;
                break;
            case 2:
                items[1].GetComponent<Image>().sprite = faces[playerData.ActiveFaceIndex].GetImageSprite;
                break;
            case 3:
                items[2].GetComponent<Image>().sprite = hairs[playerData.ActiveHairIndex].GetImageSprite;
                break;
            case 4:
                items[3].GetComponent<Image>().sprite = kits[playerData.ActiveKitIndex].GetImageSprite;
                break;
        }
    }

    private void HideAllPanels() => panels.ForEach(panel => panel.SetActive(false));

    public void ShowAllPanels() => panels.ForEach(panel => panel.SetActive(true));

    /// <summary>
    /// Skillerin elimizde ne kadar olduğunu doğru text elemanına yazan metot
    /// </summary>
    private void WriteSkillText()
    {
        playerData = TextFileHandler.ReadPlayerData();

        heartAmountText.text = playerData.Heart.ToString();
        buyutecAmountText.text = playerData.CyberMagnifyingGlass.ToString();
        yonDegistirmeAmountText.text = playerData.PassiveMove.ToString();

    }

    /// <summary>
    /// Skille para yetmeze butonu pasifleştiren parası yeterse aktifleştiren metot
    /// </summary>
    private void IsBuySkill()
    {
        playerData = TextFileHandler.ReadPlayerData();

        heartBuyButton.interactable = playerData.Score >= heartPrice;
        buyutecBuyButton.interactable = playerData.Score >= buyutecPrice;
        yonDegistirmeBuyButton.interactable = playerData.Score >= yonDegistirmePrice;
    }

    public void Buy()
    {
        switch (characterProductNumber)
        {
            case 1:
                if (!playerData.OpenedBodysIndex.Contains(productIndex))
                {
                    if(playerData.Score >= bodies[productIndex].Price)
                    {
                        playerData.OpenedBodysIndex.Add(productIndex);
                        bodies[productIndex].Image.transform.GetChild(0).gameObject.SetActive(false);
                        playerData.OpenedBodysIndex.Sort();

                        playerData.Score -= bodies[productIndex].Price;
                    }
                    else
                    {
                        //Uyarı Mesajı burada verilecek
                    }
                }
                else
                    playerData.ActiveBodyIndex = productIndex;
                break;
            case 2:
                if (!playerData.OpenedFacesIndex.Contains(productIndex))
                {
                    if(playerData.Score >= faces[productIndex].Price)
                    {
                        playerData.OpenedFacesIndex.Add(productIndex);
                        faces[productIndex].Image.transform.GetChild(0).gameObject.SetActive(false);
                        playerData.OpenedFacesIndex.Sort();

                        playerData.Score -= faces[productIndex].Price;
                    }
                    else
                    {
                        //Uyarı Mesajı burada verilecek
                    }
                }
                else
                    playerData.ActiveFaceIndex = productIndex;
                break;
            case 3:
                if (!playerData.OpenedHairsIndex.Contains(productIndex))
                {
                    if(playerData.Score >= hairs[productIndex].Price)
                    {
                        playerData.OpenedHairsIndex.Add(productIndex);
                        hairs[productIndex].Image.transform.GetChild(0).gameObject.SetActive(false);
                        playerData.OpenedHairsIndex.Sort();

                        playerData.Score -= hairs[productIndex].Price;
                    }
                    else
                    {
                        //Uyarı Mesajı burada verilecek
                    }

                }
                else
                    playerData.ActiveHairIndex = productIndex;
                break;
            case 4:
                if (!playerData.OpenedKitsIndex.Contains(productIndex))
                {
                    if(playerData.Score >= kits[productIndex].Price)
                    {
                        playerData.OpenedKitsIndex.Add(productIndex);
                        kits[productIndex].Image.transform.GetChild(0).gameObject.SetActive(false);
                        playerData.OpenedKitsIndex.Sort();

                        playerData.Score -= kits[productIndex].Price;
                    }
                    else
                    {
                        //Uyarı Mesajı burada verilecek
                    }
                }
                else
                    playerData.ActiveKitIndex = productIndex;
                break;
        }

        TextFileHandler.WritePlayerData(playerData);

        playerData = TextFileHandler.ReadPlayerData();

        playerKitShopPriceText.text = "Para: " + playerData.Score.ToString() + " $";
    }

    public void SkillBuy(int skillId) // 0 = Heart, 1 = Buyutec, 2 = Yon Degistirme
    {
        switch (skillId)
        {
            case 0: // Heart
                Debug.Log("Girdi");
                playerData.Heart++;
                playerData.Score -= heartPrice;
                break;
            case 1: // Buyutec
                playerData.CyberMagnifyingGlass++;
                playerData.Score -= buyutecPrice;
                break;
            case 2: // Yon Degistirme
                playerData.PassiveMove++;
                playerData.Score -= yonDegistirmePrice;
                break;
        }
        TextFileHandler.WritePlayerData(playerData);
        

        WriteSkillText();
        IsBuySkill();
        WritePlayerMoney();
    }

    public void ShowPanel(int index)
    {
        HideAllPanels();

        panels[index].SetActive(true);
        if (index == 4)
        {
            serverManager.ListRoomButtons();
        }

    }

    public void ShowCharacterProduct(int _characterProductNumber) //1 = Vücut / 2 = Yüz / 3 = Saç / 4 = Kıyafet
    {

        characterProductNumber = _characterProductNumber;
        bodies.ForEach(body => body.Image.SetActive(false));
        faces.ForEach(face => face.Image.SetActive(false));
        hairs.ForEach(hair => hair.Image.SetActive(false));
        kits.ForEach(kit => kit.Image.SetActive(false));

        middlePanel.SetSiblingIndex(middlePanelIndex);
        scrollView.SetSiblingIndex(scrollViewIndex);

        RectTransform rectTransform = scrollSettingsShop.ScrollRect.content.GetComponent<RectTransform>();
        items.ForEach(item => item.SetActive(true));

        switch (_characterProductNumber)
        {
            case 1: //Vücut 3 elemanlı
                scrollSettingsShop.StartSize = 200;
                bodies.ForEach(body => body.Image.SetActive(true));
                rectTransform.sizeDelta = new Vector2(200 * bodies.Count + 400, 370.65f);
                items[0].SetActive(false);

                middlePanel.SetSiblingIndex(scrollViewIndex);
                scrollView.SetSiblingIndex(middlePanelIndex);
                break;
            case 2: // Yüz 3 elemanlı
                scrollSettingsShop.StartSize = 200;
                faces.ForEach(face => face.Image.SetActive(true));
                rectTransform.sizeDelta = new Vector2(200 * faces.Count + 400, 370.65f);
                items[1].SetActive(false);

                middlePanel.SetSiblingIndex(middlePanelIndex);
                scrollView.SetSiblingIndex(scrollViewIndex);
                break;
            case 3: //Saç 10 elemanlı
                scrollSettingsShop.StartSize = 100;
                hairs.ForEach(hair => hair.Image.SetActive(true));
                rectTransform.sizeDelta = new Vector2(200 * hairs.Count + 400, 370.65f);
                items[2].SetActive(false);

                middlePanel.SetSiblingIndex(middlePanelIndex);
                scrollView.SetSiblingIndex(scrollViewIndex);
                break;
            case 4: //Kıyafet 17 elemanlı
                scrollSettingsShop.StartSize = 200;
                kits.ForEach(kit => kit.Image.SetActive(true));
                rectTransform.sizeDelta = new Vector2(200 * kits.Count + 400, 370.65f);
                items[3].SetActive(false);

                middlePanel.SetSiblingIndex(middlePanelIndex);
                scrollView.SetSiblingIndex(scrollViewIndex);
                break;
        }

        maxWidth = (scrollRectShopContent.rect.width - scrollRectShop.viewport.rect.width) / 2;

    }

    // 0,1,2 olarak üç değer alır bunun nedeni bir enum kullandığım için. 0 = Kolay, 1 = Orta, 2 = Zor
    public void WriteSoloGameDifficulty(int gameDifficulty)
    {
        PlayerPrefs.SetInt("GameDifficulty", gameDifficulty);

        SceneManager.LoadScene(1);
    }

    public void NewNick()
    {
        
        PlayerData playerData = new(newNick.text, 1000);
        TextFileHandler.WritePlayerData(playerData);
        this.playerData = TextFileHandler.ReadPlayerData();

        for (int i = 0; i < playerData.OpenedBodysIndex.Count; i++)
            bodies[playerData.OpenedBodysIndex[i]].Image.transform.GetChild(0).gameObject.SetActive(false);

        for (int i = 0; i < playerData.OpenedFacesIndex.Count; i++)
            faces[playerData.OpenedFacesIndex[i]].Image.transform.GetChild(0).gameObject.SetActive(false);


        for (int i = 0; i < playerData.OpenedHairsIndex.Count; i++)
            hairs[playerData.OpenedHairsIndex[i]].Image.transform.GetChild(0).gameObject.SetActive(false);

        for (int i = 0; i < playerData.OpenedKitsIndex.Count; i++)
            kits[playerData.OpenedKitsIndex[i]].Image.transform.GetChild(0).gameObject.SetActive(false);


        UpdateCharacterFutures();
        WritePlayerMoney();

        newNickPanel.SetActive(false);


    }

    public void ChangeNick()
    {
        PlayerData playerData = new(changeNick.text, TextFileHandler.ReadPlayerData().Score);

        TextFileHandler.WritePlayerData(playerData);

        StartCoroutine(ChangeNickPanel());
    }

    public void Quit() => Application.Quit();

    private IEnumerator ChangeNickPanel()
    {
        changeNickAnimator.SetBool("Show", true);

        yield return new WaitForSeconds(2);

        changeNickAnimator.SetBool("Show", false);
    }
}
