using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InfoData : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI heartAmount;
    [SerializeField] private TextMeshProUGUI buyutecAmount;
    [SerializeField] private TextMeshProUGUI yonDegistirmeAmount;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerBodyImage;
    [SerializeField] private Image playerFaceImage;
    [SerializeField] private Image playerHairImage;
    [SerializeField] private Image playerKitImage;


    public Player InfoPlayer { get; set; }
    public GameObject InfoPanel { get => infoPanel; set => infoPanel = value; }
    public TextMeshProUGUI HeartAmount { get => heartAmount; set => heartAmount = value; }
    public TextMeshProUGUI BuyutecAmount { get => buyutecAmount; set => buyutecAmount = value; }

    public TextMeshProUGUI YonDegistirmeAmount { get => yonDegistirmeAmount; set => yonDegistirmeAmount = value; }
    public TextMeshProUGUI PlayerNameText { get => playerNameText; set => playerNameText = value; }
    public Image PlayerBodyImage { get => playerBodyImage; set => playerBodyImage = value; }
    public Image PlayerFaceImage { get => playerFaceImage; set => playerFaceImage = value; }
    public Image PlayerHairImage { get => playerHairImage; set => playerHairImage = value; }
    public Image PlayerKitImage { get => playerKitImage; set => playerKitImage = value; }
}