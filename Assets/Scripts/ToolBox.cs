using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public enum Positions { Position1, Position2, Position3, Position4 }
public class ToolBox : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    protected static class Coordinats
    {
        //Sıralama Empty, Flag Spade, X olarak gidiyor
        public static List<Vector2> Position1 = new List<Vector2> { new Vector2(-1, 1), new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1) };

        public static List<Vector2> Position2 = new List<Vector2> { new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1) };

        public static List<Vector2> Position3 = new List<Vector2> { new Vector2(1, -1), new Vector2(-1, -1), new Vector2(1, 1), new Vector2(-1, 1) };

        public static List<Vector2> Position4 = new List<Vector2> { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) };
    }

    [SerializeField] protected FeatureManager featureManager;
    [SerializeField] protected RectTransform toolBoxButtonTransform;
    [SerializeField] protected Positions positions = Positions.Position1;
    [SerializeField] protected List<GameObject> buttons;
    [SerializeField] protected GameObject toolBoxMain;
    [SerializeField] protected int posX;
    [SerializeField] protected int posY;

    public void ClosedToolBox() => toolBoxMain.SetActive(false);

    public virtual void BoxMarked() {}

    public virtual void OpenTheBox() { }

    /// <summary>
    /// ToolBoxu açıp kapatır
    /// </summary>
    public void ToogleToolBoxMain(bool isActive) => toolBoxMain.SetActive(isActive);
}