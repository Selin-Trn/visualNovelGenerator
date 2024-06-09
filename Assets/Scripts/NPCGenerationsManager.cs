using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class NPCGenerationsManager : MonoBehaviour
{
    [SerializeField] private GameObject npcButtonPrefab;
    [SerializeField] private GameObject npcInputPrefab;
    [SerializeField] private Transform npcButtonContainer;
    [SerializeField] private Canvas npcCanvas;

    public List<GameObject> npcInputForms = new List<GameObject>();
    private List<GameObject> npcButtons = new List<GameObject>();
    private int npcCounter = 0;  // Unique counter for each NPC

    /// <summary>
    /// Adds a new NPC button and input form to the UI.
    /// </summary>
    public void AddNPC()
    {
        if (npcButtons.Count >= 3) return; // Limit to 3 NPCs

        // Increment the counter for a unique ID
        npcCounter++;

        // Create and setup the NPC button
        GameObject buttonObj = Instantiate(npcButtonPrefab, npcButtonContainer);
        buttonObj.name = "NPC_Button_" + npcCounter;  // Unique name for the button
        Button npcButton = buttonObj.GetComponent<Button>();
        npcButton.onClick.AddListener(() => ShowNPCInputForm(buttonObj.name));

        // Add to list
        npcButtons.Add(buttonObj);
    }

    /// <summary>
    /// Shows the NPC input form corresponding to the given button name.
    /// </summary>
    /// <param name="buttonName">The name of the button to show the input form for.</param>
    private void ShowNPCInputForm(string buttonName)
    {
        // Deactivate all other forms first
        DeactivateAllForms();

        // Find or create the NPC input form
        GameObject inputForm = npcInputForms.Find(form => form.name == buttonName + "_Form");
        if (inputForm == null)
        {
            // Instantiate a new form
            inputForm = Instantiate(npcInputPrefab, npcCanvas.transform);
            inputForm.name = buttonName + "_Form"; // Unique name for the form

            // Initialize form fields
            InitializeFormFields(inputForm);

            // Configure delete button inside the input form
            Button deleteButton = inputForm.GetComponentInChildren<Button>();
            deleteButton.onClick.AddListener(() => DeleteNPC(buttonName, inputForm));

            npcInputForms.Add(inputForm);
        }

        // Set the form active
        inputForm.SetActive(true);
    }

    /// <summary>
    /// Initializes the input fields of the form by resetting their content.
    /// </summary>
    /// <param name="form">The form to initialize.</param>
    private void InitializeFormFields(GameObject form)
    {
        // Find all input fields and reset their content
        var inputs = form.GetComponentsInChildren<TMP_InputField>();
        foreach (var input in inputs)
        {
            input.text = "";
        }

        // Reset dropdowns as well
        var dropdowns = form.GetComponentsInChildren<TMP_Dropdown>();
        foreach (var dropdown in dropdowns)
        {
            dropdown.value = 0; // Reset to the first option
        }
    }

    /// <summary>
    /// Deactivates all NPC input forms.
    /// </summary>
    private void DeactivateAllForms()
    {
        foreach (GameObject form in npcInputForms)
        {
            form.SetActive(false);
        }
    }

    /// <summary>
    /// Deletes the NPC corresponding to the given button and input form.
    /// </summary>
    /// <param name="buttonName">The name of the button to delete.</param>
    /// <param name="inputForm">The input form to delete.</param>
    private void DeleteNPC(string buttonName, GameObject inputForm)
    {
        // Cleanup logic
        Button deleteButton = inputForm.GetComponentInChildren<Button>();
        deleteButton.onClick.RemoveAllListeners();

        // Remove from lists and destroy the GameObjects
        GameObject buttonObj = npcButtons.Find(btn => btn.name == buttonName);
        npcButtons.Remove(buttonObj);
        npcInputForms.Remove(inputForm);
        Destroy(buttonObj);
        Destroy(inputForm);
    }

    /// <summary>
    /// Gets the novel prompts for all NPCs.
    /// </summary>
    /// <returns>A list of novel prompts for all NPCs.</returns>
    public List<string> GetAllNPCNovelPrompts()
    {
        List<string> prompts = new List<string>();
        foreach (GameObject form in npcInputForms)
        {
            NPCGenerationOptions options = form.GetComponent<NPCGenerationOptions>();
            if (options != null)
            {
                prompts.Add(options.GetNPCNovelPrompt());
            }
        }
        return prompts;
    }

    /// <summary>
    /// Gets the complete image prompts for all NPCs.
    /// </summary>
    /// <returns>A list of complete image prompts for all NPCs.</returns>
    public List<string> GetAllNPCCompleteImagePrompts()
    {
        List<string> prompts = new List<string>();
        foreach (GameObject form in npcInputForms)
        {
            NPCGenerationOptions options = form.GetComponent<NPCGenerationOptions>();
            if (options != null)
            {
                prompts.Add(options.GetNPCImageCompletePrompt());
            }
        }
        return prompts;
    }

    /// <summary>
    /// Gets the names of all NPCs.
    /// </summary>
    /// <returns>A list of names for all NPCs.</returns>
    public List<string> GetAllNPCNames()
    {
        List<string> names = new List<string>();
        foreach (GameObject form in npcInputForms)
        {
            NPCGenerationOptions options = form.GetComponent<NPCGenerationOptions>();
            if (options != null)
            {
                names.Add(options.GetNPCName());
            }
        }
        return names;
    }
}
