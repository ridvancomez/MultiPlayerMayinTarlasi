using System.Collections.Generic;

public class PlayerData
{
    public string PlayerName { get; set; }

    private int money;
    public int Money { get => money; set => money = value < 0 ? 0 : value; }
    /// <summary>
    /// Satın alınmış vücutlar
    /// </summary>
    public List<int> OpenedBodysIndex = new();
    /// <summary>
    /// Satın alınmış yüzler
    /// </summary>
    public List<int> OpenedFacesIndex = new();
    /// <summary>
    /// Satın alınmış saçlar
    /// </summary>
    public List<int> OpenedHairsIndex = new();
    /// <summary>
    /// Satın alınmış kıyafetler
    /// </summary>
    public List<int> OpenedKitsIndex = new();

    /// <summary>
    /// Aktif Vücut
    /// </summary>
    public int ActiveBodyIndex = 0;

    /// <summary>
    /// Aktif Yüz
    /// </summary>
    public int ActiveFaceIndex = 0;

    /// <summary>
    /// Aktif Saç
    /// </summary>
    public int ActiveHairIndex = 0;

    /// <summary>
    /// Aktif Kıyafet
    /// </summary>
    public int ActiveKitIndex = 0;

    private int heart;
    /// <summary>
    /// Bir özelliktir. Oyuncu hata yaptığında bu sayı eğer sıfırdan daha fazla ise bu hak otomatikman kullanılır
    /// </summary>
    public int Heart { get => heart; set => heart = value < 0 ? 0 : value; }

    private int cyberMagnifyingGlass;
    /// <summary>
    /// Bir özelliktir. Oyuncu bunu kullandığında bir kare seçiyor ve o karenin safe mi bomba mı olduğunu anlıyoruz
    /// </summaary>
    public int CyberMagnifyingGlass { get => cyberMagnifyingGlass; set => cyberMagnifyingGlass = value < 0 ? 0 : value; }


    private int passiveMove;
    /// <summary>
    /// Bir özelliktir. Oyuncu bunu kullandığında sırasını salıyor ve sıra diğer oyuncuya geçiyor
    /// </summary>
    public int PassiveMove { get => passiveMove; set => passiveMove = value < 0 ? 0 : value; }

    public PlayerData(string playerName, int score)
    {
        OpenedBodysIndex.Add(0);
        OpenedFacesIndex.Add(0);
        OpenedHairsIndex.Add(0);
        OpenedKitsIndex.Add(0);

        Heart = 5;
        CyberMagnifyingGlass = 5;
        PassiveMove = 5;

        PlayerName = playerName;
        Money = score;
    }
}