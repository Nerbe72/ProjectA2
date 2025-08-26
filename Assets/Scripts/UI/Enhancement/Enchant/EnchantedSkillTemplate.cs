using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using GameStuff;

public class EnchantedSkillTemplate : MonoBehaviour
{
    [SerializeField] private Image skillImage;
    [SerializeField] private TMP_Text skillName;
    [SerializeField] private TMP_Text skillDescription;

    public void SetData(int _skillID)
    {
        var abilityManager = Singleton.Get<AbilityManager>();

        skillImage.sprite = abilityManager.GetIcon(_skillID);
        skillName.text = abilityManager.GetName(_skillID);
        skillDescription.text = abilityManager.GetDescription(_skillID);
    }
}
