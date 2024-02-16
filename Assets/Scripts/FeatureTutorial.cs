using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureTutorial : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private GameObject playerFeatureIcon;
    private void SkipNewTutorial()
    {
        if(playerFeatureIcon != null )
            playerFeatureIcon.SetActive(false);
        tutorialManager.CloseInfoPanel();

    }
}
