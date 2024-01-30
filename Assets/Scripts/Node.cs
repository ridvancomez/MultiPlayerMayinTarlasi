using System;
using UnityEngine;

[Serializable]
public enum BoxType { Safe, Bomb, Clicked, Marked }

[Serializable]
public class Node
{
    [SerializeField] private int xCoordinat;
    [SerializeField] private int yCoordinat;
    [SerializeField] private int bombNumber;
    [SerializeField] private bool isBomb;
    [SerializeField] private bool isFirstColor;
    [SerializeField] private BoxType type = BoxType.Safe;

    public int XCoordinat { get { return xCoordinat; } set { xCoordinat = value; } }
    public bool IsBomb { get { return isBomb; } set { isBomb = value; } }
    public int YCoordinat { get { return yCoordinat; } set { yCoordinat = value; } }
    public BoxType Type { get { return type; } set { type = value; } }
    public int BombNumber { get {  return bombNumber; } set {  bombNumber = value; } }
    public bool IsFirstColor { get { return isFirstColor; } set { isFirstColor = value; } }
}
