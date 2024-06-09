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
using UnityEngine.SceneManagement;

// This is the class for in-game narrative management
// The parsers and the texts are all managed here
public class NarrativeManager : MonoBehaviour
{
    // Narrative parser
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

    [Header("Ending Fade Screen")]
    public GameObject endingWindow;
    public Image endingPanel;
    public TextMeshProUGUI endingTypeText;

    /// <summary>
    /// Initializes the narrative parser and starts the story.
    /// </summary>
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

    /// <summary>
    /// Starts the story by initializing story data and text, and setting the first branch and key.
    /// </summary>
    private void StartStory()
    {
        endingWindow.SetActive(false);
        GetStoryData();
        GetStoryText();

        npcManager.CreateNPCs(storyData.NPCs);
        SetAllBranchesKeys();
        SetCurrentBranch(0);
        SetCurrentBranchKeys();
        SetCurrentKey(0);

        DisplayNextLine(); // Display the first line
        RefreshChoiceView();
    }

    /// <summary>
    /// Loads story data from JSON.
    /// </summary>
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

    /// <summary>
    /// Loads story text from JSON.
    /// </summary>
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

    /// <summary>
    /// Extracts keys from a specified branch in the story JSON.
    /// </summary>
    /// <param name="branchKey">The branch key to extract keys from.</param>
    /// <returns>A list of keys from the specified branch.</returns>
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

    /// <summary>
    /// Jumps to a new branch in the story.
    /// </summary>
    /// <param name="branchName">The name of the branch to jump to.</param>
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

    /// <summary>
    /// Gets the names of all branches in the story.
    /// </summary>
    /// <returns>A list of branch names.</returns>
    private List<string> GetBranchNames()
    {
        return story.GetType().GetProperties()
                    .Where(p => p.PropertyType == typeof(Branch))
                    .Select(p => p.Name)
                    .ToList();
    }

    /// <summary>
    /// Sets the keys of all branches in the story.
    /// </summary>
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

    /// <summary>
    /// Sets the keys of the current branch.
    /// </summary>
    private void SetCurrentBranchKeys()
    {
        var keys = ExtractBranchKeys(storyAllBranchesKeys[currentBranchIndex]);
        currentBranchKeys = keys;
    }

    /// <summary>
    /// Sets the current branch by index.
    /// </summary>
    /// <param name="index">The index of the branch to set as current.</param>
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

    /// <summary>
    /// Sets the current key by index.
    /// </summary>
    /// <param name="index">The index of the key to set as current.</param>
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

    /// <summary>
    /// Checks if the value is valid (not null and not default).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    private bool ValueIsValid(object value)
    {
        if (value == null)
            return false;

        Type type = value.GetType();
        if (type.IsValueType)
            return !value.Equals(Activator.CreateInstance(type));

        return true;
    }

    /// <summary>
    /// Displays the next line in the story.
    /// </summary>
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
                    endingWindow.SetActive(true);
                    endingTypeText.text = "The End.";
                    endingPanel.color = UnityEngine.Color.gray;
                }
                else if (currentKey == narrativeParser.diedKey)
                {
                    // DIED MANAGEMENT
                    endingWindow.SetActive(true);
                    endingTypeText.text = "You Died.";
                    endingPanel.color = UnityEngine.Color.black;
                }
            }
        }
        else
        {
            playerSpeechTextField.text = "Story branch or key marker is not set";
        }
    }

    /// <summary>
    /// Creates choice buttons for the player to make a decision.
    /// </summary>
    /// <param name="jumpChoices">The jump choices to create buttons for.</param>
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

    /// <summary>
    /// Handles choice button click.
    /// </summary>
    /// <param name="choice">The choice that was clicked.</param>
    void OnClickChoiceButton(Choice choice)
    {
        Debug.Log(choice.br);
        JumpToNewBranch(choice.br);
        SetCurrentBranchKeys();
        SetCurrentKey(0);
        DisplayNextLine();
        RefreshChoiceView();
    }

    /// <summary>
    /// Refreshes the choice view by clearing existing buttons.
    /// </summary>
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

    /// <summary>
    /// Clears all text fields.
    /// </summary>
    public void ClearTextFields()
    {
        narrativeTextField.text = "";
        playerSpeechTextField.text = "";
        playerNameField.text = "";
        npcSpeechTextField.text = "";
        npcNameField.text = "";
    }

    /// <summary>
    /// Handles updating text fields based on the speaker.
    /// </summary>
    /// <param name="speaker">The speaker type ("player", "npc", "narrative").</param>
    /// <param name="speakerName">The name of the speaker.</param>
    /// <param name="text">The text to display.</param>
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

    /// <summary>
    /// Gets the value of the current key from the current branch.
    /// </summary>
    /// <returns>The value of the current key.</returns>
    private object getCurrentKeyValue()
    {
        PropertyInfo propertyInfo = currentBranch.GetType().GetProperty(currentKey);
        object value = propertyInfo.GetValue(currentBranch);
        return value;
    }

    /// <summary>
    /// Replays the game by loading the game scene.
    /// </summary>
    public void ReplayGame()
    {
        SceneManager.LoadScene("Game");
    }

    /// <summary>
    /// Goes back to the novels scene.
    /// </summary>
    public void BackToNovels()
    {
        SceneManager.LoadScene("StoriesScene");
    }
}
