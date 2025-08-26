using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class SkillIndicator : MonoBehaviour
{
    private Player player;

    [SerializeField] private Transform boundary;
    [SerializeField] private GameObject skillFramePrefab;
    
    private List<SkillFrame> skillFrames;

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        skillFrames = new List<SkillFrame>();
    }

    private void Start()
    {
        player = Singleton.Player;

        player.OnWeaponChanged += WeaponChanged;


        var weapon = player.GetCurrentWeapon();
        if (weapon != null)
        {
            WeaponChanged(weapon);
        }
    }

    private void OnDestroy()
    {
        Singleton.Player.OnWeaponChanged -= WeaponChanged;
    }

    private void WeaponChanged(WeaponItemInstance _weaponInstance)
    {
        var adapter = Singleton.Inventory.GetWeaponAdapter(_weaponInstance);
        var skills = adapter.GetEnchantedSkills();

        // ¼³Á¤
        for (int i = 0; i < skills.Count; i++)
        {
            if (skillFrames.Count > i)
            {
                skillFrames[i].SetItemID(skills[i]);
            }
            else
            {
                var obj = Instantiate(skillFramePrefab, boundary);
                var skillFrame = obj.GetComponent<SkillFrame>();
                skillFrame.SetItemID(skills[i]);

                skillFrames.Add(skillFrame);
            }

            skillFrames[i].gameObject.SetActive(true);
        }

        for (int i = skills.Count; i < skillFrames.Count; i++)
        {
            skillFrames[i].gameObject.SetActive(false);
        }
    }
}
