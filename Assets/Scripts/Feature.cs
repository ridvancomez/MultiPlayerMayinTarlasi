using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feature : MonoBehaviour
{
    [SerializeField] private FeatureManager featureManager;

    //Aşağıdaki metot animation Eventine bağlıdır (Heart, Pasif Hamle) ve featuere enumunu none çeker
    private void TurnToDefault()
    {
        featureManager.Feature = FeaturesEnum.None;
    }
}
