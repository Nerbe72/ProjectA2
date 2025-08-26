using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Slots : MonoBehaviour
{
    [SerializeField] private GameObject slot;

    private List<Slot> slots = new List<Slot>();

    public void SetSlot(int _total, int _activated)
    {
        int count = slots.Count;
        for (int i = 0; i < count; i++)
        {
            if (i < _total)
            {
                if (i < _activated)
                    slots[i].SetActivated();
                else
                    slots[i].SetDeactivated();

                slots[i].gameObject.SetActive(true);
            }
            else
                slots[i].gameObject.SetActive(false);
        }

        for (int i = count; i < _total; i++)
        {
            Slot newSlot = Instantiate(slot, transform).GetComponent<Slot>();
            slots.Add(newSlot);
            if (i < _activated)
                newSlot.SetActivated();
            else
                newSlot.SetDeactivated();
        }
    }
}
