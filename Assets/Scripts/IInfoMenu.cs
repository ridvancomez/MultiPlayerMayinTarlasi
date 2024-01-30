using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using Photon.Realtime;

interface IInfoMenu
{
    // Info Paneli kapatma metodu GameManagerMultiplayer Scriptinde
    InfoData Info { get; set; }

    /// <summary>
    /// Bu metod, bir oyuncunun bilgilerini alır ve ekranda bir bilgi panelini etkinleştirir.
    /// </summary>
    public void ShowPlayerInfo();
    /// <summary>
    /// PUNRPC ile işaretlenmiş bu metod, bir oyuncunun verilerini talep eder.
    /// </summary>
    public void RequestUserData(Player thisPlayer);
    /// <summary>
    /// PUNRPC ile işaretlenmiş bu metod, başka bir oyuncuya Player Data verileri talebinde bulunur.
    /// </summary>
    public void SendPlayerDataRequest(string playerName, int avatarBodyIndex, int avatarFaceIndex, int avatarHairIndex, int avatarKitIndex, int heartAmount, int buyutecAmount, int yonDegistirmeAmount);
}
