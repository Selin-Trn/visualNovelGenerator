using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// simple class script for novel progression (pressing forward arrow button to move forward)
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
