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

    public int XCoordinat { get => xCoordinat; set { xCoordinat = value; } }
    public bool IsBomb { get => isBomb; set { isBomb = value; } }
    public int YCoordinat { get => yCoordinat; set { yCoordinat = value; } }
    public BoxType Type { get => type; set { type = value; } }
    public int BombNumber { get => bombNumber; set {  bombNumber = value; } }
    public bool IsFirstColor { get => isFirstColor; set { isFirstColor = value; } }
}
