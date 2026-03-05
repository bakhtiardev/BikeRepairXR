using System;
using UnityEngine;

public class SwitchBetweenPanels : MonoBehaviour
{
    public GameObject[] panels;
    private int currentPanel = 0;

    void Start()
    {
        Debug.Log("SwitchBetweenPanels Start");
        ShowPanel(0);
    }

    public void NextPanel()
    {
        if (currentPanel < panels.Length - 1)
        {
            Debug.Log("NextPanel method hit correctly");
            currentPanel++;
            ShowPanel(currentPanel);
        }
    }

    public void PrevPanel()
    {
        if (currentPanel > 0)
        {
            Debug.Log("PrevPanel method hit correctly");
            currentPanel--;
            ShowPanel(currentPanel);
        }
    }

    void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }
    }
}