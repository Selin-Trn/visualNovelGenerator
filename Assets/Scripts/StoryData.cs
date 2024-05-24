using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryData : MonoBehaviour
{
    [Serializable]
    public class StorySaveData
    {
        public StoryJsonData story;
        public PlayerJsonData player;
        public NPCJsonData[] NPCs;
        public BackgroundJsonData backgrounds;

    }

    [Serializable]
    public class StoryJsonData
    {
        public string name;
        public string generationPrompt;
    }

    [Serializable]
    public class PlayerJsonData
    {
        public string name;
        public string generationPrompt;
    }

    [Serializable]
    public class NPCJsonData
    {
        public string name;
        public string generationPrompt;
    }

    [Serializable]
    public class BackgroundJsonData
    {
        public string[] names;
    }
}
