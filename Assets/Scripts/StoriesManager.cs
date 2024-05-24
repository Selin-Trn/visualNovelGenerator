using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System;
using UnityEngine.SceneManagement;

public class StoriesManager : MonoBehaviour
{
    [Header("Stories")]
    [SerializeField] public GameObject storyObjects;
    [SerializeField] public GridLayoutGroup storyButtonsContainer;
    [SerializeField] public Button storyButtonPrefab;
    [SerializeField] public TMP_Text storySummary;

    private string selectedStoryUUID;

    void Start()
    {
        LoadStories();
    }

    void LoadStories()
    {
        ChosenStoryManager.chosenStorySaveFolderUUID = null;
        ChosenStoryManager.chosenStorySaveFolderPath = null;

        string path = Path.Combine(Application.dataPath, "Saves");
        foreach (string storyDir in Directory.GetDirectories(path))
        {
            string jsonDataPath = Path.Combine(storyDir, "data.json");
            if (File.Exists(jsonDataPath))
            {
                try
                {
                    string jsonData = File.ReadAllText(jsonDataPath);
                    StoryData.StorySaveData storyData = JsonConvert.DeserializeObject<StoryData.StorySaveData>(jsonData);

                    Button newButton = Instantiate(storyButtonPrefab, storyButtonsContainer.transform);
                    newButton.GetComponentInChildren<TMP_Text>().text = storyData.story.name;
                    newButton.onClick.AddListener(() => UpdateStorySummary(storyData.story.generationPrompt, new DirectoryInfo(storyDir).Name));
                }
                catch (Exception e)
                {
                    Debug.Log("Problem getting data for the chosen story: " + e);
                }

            }
        }
    }

    void UpdateStorySummary(string prompt, string uuid)
    {
        storySummary.text = prompt;
        selectedStoryUUID = uuid;
    }

    public void PlayChosenStory()
    {

        if (selectedStoryUUID != null)
        {
            string saveFolderPath = Path.Combine(Application.dataPath, @"Saves");
            string selectedStorySaveFolderPath = Path.Combine(saveFolderPath, selectedStoryUUID);
            ChosenStoryManager.chosenStorySaveFolderUUID = selectedStoryUUID;
            ChosenStoryManager.chosenStorySaveFolderPath = selectedStorySaveFolderPath;
            Debug.Log("Loading story with UUID: " + selectedStoryUUID);
            SceneManager.LoadScene("Game");
        }
        else
        {
            UpdateStorySummary("Please choose a story!", null);
        }
    }
}

