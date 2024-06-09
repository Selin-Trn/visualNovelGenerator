using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is to communicate the chosen story UUID/path in-between scripts
// static variables shares value among all objects at the class level, therefore it remains constant
public class ChosenStoryManager : MonoBehaviour
{
    public static string chosenStorySaveFolderUUID;
    public static string chosenStorySaveFolderPath;
}
