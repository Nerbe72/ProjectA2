using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Alert : MonoBehaviour
{
    private AlertTemplate alertTemplate;

    private List<AlertTemplate> alerts = new List<AlertTemplate>();

    private void Awake()
    {
        if (Singleton.Add(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        alertTemplate = GetComponentInChildren<AlertTemplate>(true);
    }

    public void Show(string _message, Color _color)
    {
        if (alertTemplate != null)
        {
            alertTemplate.Show(0, _message, _color);
        }
    }
}
