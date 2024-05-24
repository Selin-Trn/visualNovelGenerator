using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using static NarrativeParser;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Linq;

public class NarrativeManager : MonoBehaviour
{

    NarrativeParser narrativeParser;

    [Header("Managers")]
    [SerializeField] public PlayerManager playerManager;
    [SerializeField] public NPCManager npcManager;
    [SerializeField] public BackgroundManager backgroundManager;
    private string storyTextJson;
    private string storyDataJson;
    private StoryData.StorySaveData storyData;
    private Story story;

    [Header("Story Position")]
    public string currentKey;
    private int currentKeyIndex;
    private List<string> currentBranchKeys;
    private List<string> storyAllBranchesKeys;
    private Branch currentBranch;
    public int currentBranchIndex;


    [Header("Dialogue Box")]
    [SerializeField] public GameObject dialogueBox;

    [Header("Text Field")]
    [SerializeField] public TextMeshProUGUI playerSpeechTextField;
    [SerializeField] private TextMeshProUGUI playerNameField;
    [SerializeField] public TextMeshProUGUI npcSpeechTextField;
    [SerializeField] private TextMeshProUGUI npcNameField;
    [SerializeField] public TextMeshProUGUI narrativeTextField;

    [Header("Choices")]
    [SerializeField] public GameObject choiceObjects;
    [SerializeField] public VerticalLayoutGroup choiceButtonContainer;
    [SerializeField] public Button choiceButtonPrefab;


    void Start()
    {
        narrativeParser = GetComponent<NarrativeParser>();
        if (narrativeParser == null)
        {
            Debug.LogError("NarrativeParser component not found on the GameObject");
            return;
        }
        StartStory();
    }

    private void StartStory()
    {
        GetStoryData();
        GetStoryText();

        npcManager.CreateNPCs(storyData.NPCs);
        // initialize game from the first branch and line
        SetAllBranchesKeys();
        SetCurrentBranch(0);
        SetCurrentBranchKeys();
        SetCurrentKey(0);

        DisplayNextLine(); // display first line
        RefreshChoiceView();
    }

    public void GetStoryData()
    {
        try
        {
            var storyDataPath = Path.Combine(ChosenStoryManager.chosenStorySaveFolderPath, "data.json");
            storyDataJson = File.ReadAllText(storyDataPath);
            storyData = Newtonsoft.Json.JsonConvert.DeserializeObject<StoryData.StorySaveData>(storyDataJson);
            Debug.Log(storyData);
            Debug.Log("JSON read and parsed successfully!");
        }
        catch (Exception ex)
        {
            Debug.Log("Error reading or parsing JSON: " + ex.Message);
        }
    }
    public void GetStoryText()
    {
        try
        {
            var storyTextPath = Path.Combine(ChosenStoryManager.chosenStorySaveFolderPath, "story.json");
            storyTextJson = File.ReadAllText(storyTextPath);
            story = Newtonsoft.Json.JsonConvert.DeserializeObject<Story>(storyTextJson);
            Debug.Log(story.a._0);
            Debug.Log(story);
            Debug.Log("JSON read and parsed successfully!");
        }
        catch (Exception ex)
        {
            Debug.Log("Error reading or parsing JSON: " + ex.Message);
        }
    }


    public List<string> ExtractBranchKeys(string branchKey)
    {
        var keys = new List<string>();

        try
        {
            JObject jsonObject = JObject.Parse(storyTextJson);

            // Access the branch by name
            var branch = jsonObject[branchKey] as JObject;
            if (branch == null)
            {
                throw new System.ArgumentException("Branch not found: " + branchKey);
            }

            // Iterate over the properties in the branch and add their names to the list
            foreach (var property in branch.Properties())
            {
                keys.Add(property.Name);
            }
        }
        catch (System.Exception ex)
        {
            // Handle any parsing errors
            System.Diagnostics.Debug.WriteLine("Error parsing JSON: " + ex.Message);
        }

        return keys;
    }
    private void JumpToNewBranch(string branchName)
    {

        PropertyInfo propertyInfo = story.GetType().GetProperty(branchName);

        if (propertyInfo != null && propertyInfo.PropertyType == typeof(Branch))
        {
            currentBranch = (Branch)propertyInfo.GetValue(story);
            // Find the index of the branch name
            List<string> branchNames = GetBranchNames();
            currentBranchIndex = branchNames.IndexOf(branchName);
        }
        else
        {
            Debug.LogError("Property not found or is not of type 'Branch'");
        }
    }
    private List<string> GetBranchNames()
    {
        return story.GetType().GetProperties()
                    .Where(p => p.PropertyType == typeof(Branch))
                    .Select(p => p.Name)
                    .ToList();
    }
    private void SetAllBranchesKeys()
    {
        var keys = new List<string>();
        PropertyInfo[] properties = story.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            var value = property.GetValue(story);
            if (ValueIsValid(value))
            {
                keys.Add(property.Name);
                Debug.Log(property.Name);
            }
        }
        storyAllBranchesKeys = keys;

    }
    private void SetCurrentBranchKeys()
    {
        var keys = ExtractBranchKeys(storyAllBranchesKeys[currentBranchIndex]);
        currentBranchKeys = keys;
    }

    private void SetCurrentBranch(int index)
    {
        if (index < 0 || index >= storyAllBranchesKeys.Count)
        {
            Debug.LogError("Index out of range");
            return;
        }

        string branchName = storyAllBranchesKeys[index];
        PropertyInfo propertyInfo = story.GetType().GetProperty(branchName);

        if (propertyInfo != null && propertyInfo.PropertyType == typeof(Branch))
        {
            currentBranch = (Branch)propertyInfo.GetValue(story);
            currentBranchIndex = index; // Update the index
        }
        else
        {
            Debug.LogError("Property not found or is not of type 'Branch'");
        }
    }
    private void SetCurrentKey(int index)
    {
        if (index < 0 || index >= currentBranchKeys.Count)
        {
            Debug.LogError("Index out of range");
            return;
        }

        currentKeyIndex = index;
        currentKey = currentBranchKeys[currentKeyIndex];
    }

    private bool ValueIsValid(object value)
    {
        if (value == null)
            return false;

        Type type = value.GetType();
        if (type.IsValueType)
            return !value.Equals(Activator.CreateInstance(type));

        return true;
    }

    public void DisplayNextLine()
    {
        if (currentBranch != null && !string.IsNullOrEmpty(currentKey))
        {
            if (currentBranchKeys.Count - 1 > currentKeyIndex)
            {
                if (narrativeParser.functionsKeys.Contains(currentKey))
                {
                    // APR/DSPR MANAGEMENT

                    object value = getCurrentKeyValue();
                    if (value is string npcName) // Safe cast to NpcText
                    {
                        if (currentKey.Contains("apr")) // Example key check
                        {
                            npcManager.ShowNPC(npcName); // Use actual logic to determine which NPC to show
                        }
                        else if (currentKey.Contains("dspr"))
                        {
                            npcManager.HideNPC(npcName); // Similarly, determine which NPC to hide
                        }
                    }
                    SetCurrentKey(currentKeyIndex + 1);
                    DisplayNextLine();
                }
                else if (currentKey.EndsWith(narrativeParser.backgroundSwitchKey))
                {
                    // BACKGROUND MANAGEMENT
                    object value = getCurrentKeyValue();
                    if (value is string backgroundName) // Safe cast to NpcText
                    {
                        backgroundManager.SwitchBackgroundImage(backgroundName);
                    }
                    SetCurrentKey(currentKeyIndex + 1);
                    DisplayNextLine();
                }
                else if (currentKey.EndsWith(narrativeParser.npcSpeechKey))
                {
                    ClearTextFields();
                    // NPC SPEECH MANAGEMENT
                    object value = getCurrentKeyValue();
                    if (value is NpcText npcText) // Safe cast to NpcText
                    {
                        HandleTextFields("npc", npcText.name, npcText.txt);
                    }
                    SetCurrentKey(currentKeyIndex + 1);
                }
                else if (currentKey.EndsWith(narrativeParser.playerSpeechKey))
                {
                    ClearTextFields();
                    // PLAYER SPEECH MANAGEMENT
                    object value = getCurrentKeyValue();
                    HandleTextFields("player", storyData.player.name, value.ToString());
                    SetCurrentKey(currentKeyIndex + 1);
                }
                else
                {
                    ClearTextFields();
                    // NARRATIVE TEXT MANAGEMENT
                    object value = getCurrentKeyValue();
                    HandleTextFields("narrative", "", value.ToString());
                    SetCurrentKey(currentKeyIndex + 1);
                }
            }
            else if (currentBranchKeys.Count - 1 == currentKeyIndex)
            {
                if (currentKey == narrativeParser.jumpKey)
                {
                    if (!choiceObjects.activeInHierarchy)
                    {
                        // JUMP MANAGEMENT 
                        Jump jumpChoices = currentBranch._jmp;
                        CreateChoiceButtons(jumpChoices);
                    }
                    else
                    {
                        Debug.Log("Make a choice!");
                    }
                }
                else if (currentKey == narrativeParser.endKey)
                {
                    // END MANAGEMENT
                }
                else if (currentKey == narrativeParser.diedKey)
                {
                    // DIED MANAGEMENT
                }
            }
        }
        else
        {
            playerSpeechTextField.text = "Story branch or key marker is not set";
        }
    }

    void CreateChoiceButtons(Jump jumpChoices)
    {
        foreach (Choice choice in jumpChoices.GetAllChoices())
        {
            Button choiceButton = Instantiate(choiceButtonPrefab);
            choiceButton.transform.SetParent(choiceButtonContainer.transform, false);

            TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = choice.txt;

            choiceButton.onClick.AddListener(() => OnClickChoiceButton(choice));

        }
        choiceObjects.SetActive(true);
    }
    void OnClickChoiceButton(Choice choice)
    {
        Debug.Log(choice.br);
        JumpToNewBranch(choice.br);
        SetCurrentBranchKeys();
        SetCurrentKey(0);
        DisplayNextLine();
        RefreshChoiceView();
    }
    void RefreshChoiceView()
    {
        if (choiceButtonContainer != null)
        {
            foreach (var button in choiceButtonContainer.GetComponentsInChildren<Button>())
            {
                Destroy(button.gameObject);
            }
        }
        choiceObjects.gameObject.SetActive(false);

    }

    public void ClearTextFields()
    {
        narrativeTextField.text = "";
        playerSpeechTextField.text = "";
        playerNameField.text = "";
        npcSpeechTextField.text = "";
        npcNameField.text = "";

    }
    public void HandleTextFields(string speaker, string speakerName, string text)
    {
        ClearTextFields();
        switch (speaker)
        {
            case "player":
                playerManager.playerPortaitObject.SetActive(true);
                playerSpeechTextField.text = text;
                playerNameField.text = speakerName;
                break;
            case "npc":
                playerManager.playerPortaitObject.SetActive(false);
                npcSpeechTextField.text = text;
                npcNameField.text = speakerName;
                break;
            case "narrative":
                playerManager.playerPortaitObject.SetActive(false);
                narrativeTextField.text = text;
                break;
            default:
                break;

        }

    }
    private object getCurrentKeyValue()
    {
        PropertyInfo propertyInfo = currentBranch.GetType().GetProperty(currentKey);
        object value = propertyInfo.GetValue(currentBranch);
        return value;
    }
}
