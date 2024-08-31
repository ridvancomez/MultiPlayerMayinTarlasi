using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManagerSinglePlayer : GameManager
{
    private void Awake()
    {
        FirstControl = true;
        gameDifficulty = (GameDifficulty)PlayerPrefs.GetInt("GameDifficulty");
        SelectDifficulty();
        FeatureButtonControl();
        flagNumber = bombNumber;

        boxLayoutGroup.constraintCount = columnNumber;

        GenerateBox();
        SelectBomb();

        flagText.text = flagNumber.ToString();
        stopPanel.SetActive(false);
    }

    /// <summary>
    /// Mayın tarlasındaki kareleri oluşturur
    /// </summary>
    private void GenerateBox()
    {
        bool firstColor = false;
        for (int i = 0; i < rowNumber; i++)
        {
            for (int j = 0; j < columnNumber; j++)
            {
                GameObject go = Instantiate(box, buttonsMainObject.transform);

                go.GetComponent<BoxSingle>().BoxNode.XCoordinat = j;
                go.GetComponent<BoxSingle>().BoxNode.YCoordinat = i;
                go.name = $"Image ({j},{i})";

                if (columnNumber % 2 == 0 && j == 0)
                    firstColor = !firstColor;

                go.GetComponent<BoxSingle>().BoxNode.IsFirstColor = firstColor;
                firstColor = !firstColor;
                boxes.Add(go);
            }
        }
    }

    /// <summary>
    /// Rastgele kare seçer ve onu bomba yapar
    /// </summary>
    private void SelectBomb()
    {
        for (int i = 0; i < bombNumber; i++)
        {
            GameObject box = boxes[Random.Range(0, boxes.Count)];

            BoxSingle boxScript = box.GetComponent<BoxSingle>();

            if (boxScript.BoxNode.Type == BoxType.Safe)
            {
                boxScript.BoxNode.Type = BoxType.Bomb;
                boxScript.BoxNode.IsBomb = true;
                boxScript.ChangeBombAnimal(Random.Range(0, animalSprites.Count));

                bombBoxes.Add(box);   
            }
            else
            {
                i--;
                continue;
            }
        }
    }

    protected override void Win() => StartCoroutine(ShowTheBombedBoxes(true));
    protected override void Lose() => StartCoroutine(ShowTheBombedBoxes(false));

    /// <summary>
    /// Yandığında veya kazandığında bombaları gösterir
    /// </summary>
    private IEnumerator ShowTheBombedBoxes(bool isWin)
    {
        foreach (GameObject box in bombBoxes)
        {
            box.GetComponent<BoxSingle>().ToggleBoxImage(1);
            yield return new WaitForSeconds(Random.Range(0, 1.1f));
        }

        List<GameObject> markedIncorrectlyBoxes = boxes.Where(box =>
        box.GetComponent<BoxSingle>().BoxNode.Type == BoxType.Marked &&
        !box.GetComponent<BoxSingle>().BoxNode.IsBomb).ToList();

        foreach (GameObject box in markedIncorrectlyBoxes)
        {
            box.GetComponent<BoxSingle>().ToggleBoxImage(2);
            yield return new WaitForSeconds(Random.Range(0, 1.1f));
        }
        losePanel.SetActive(!isWin);
        winPanel.SetActive(isWin);
    }

    protected override void ChangeBoxColor() => boxes.ForEach(box => box.GetComponent<BoxSingle>().ChangeBoxColor(isBuyutecFeature));
}