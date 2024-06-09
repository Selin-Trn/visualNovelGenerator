using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is to communicate the generation variables in-between scripts
// static variables shares value among all objects at the class level, therefore it remains constant
public class GeneratedContentHolder : MonoBehaviour
{
    public static string generatedStorySaveFolderPath;
    public static string generatedStorySaveFolderUUID;
    public static string playerName;
    public static string playerPrompt;
    public static string novelName;
    public static string novelPrompt;
    public static string novelSettingPrompt;
    public static List<string> npcNames;
    public static List<string> npcPrompts;
    public static List<string> backgroundNames;

}
