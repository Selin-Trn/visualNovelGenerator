using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    private NarrativeManager narrativeManager;
    void Start()
    {
        narrativeManager = FindObjectOfType<NarrativeManager>();

    }

    void Update()
    {
        if (Input.GetKeyDown("right"))
        {
            narrativeManager.DisplayNextLine();
        }
    }
}
