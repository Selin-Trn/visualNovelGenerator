using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class NPCGenerationPanelManager : MonoBehaviour
{
    public GameObject imageZoomScreen;
    public Image zoomImage;
    [SerializeField] private NPCGenerationsManager npcGenerationsManager;
    [SerializeField] private GameObject npcGenerationPanelButtonPrefab;
    [SerializeField] private GameObject npcGenerationPanelFieldsObjectPrefab;
    [SerializeField] private Transform npcRetouchScreensContainer;
    [SerializeField] private Transform npcGenerationPanelButtonContainer;
    [SerializeField] private GameObject npcGenerationPanelContainer;

    private List<GameObject> npcGenerationPanelFieldsObjects = new List<GameObject>();
    private List<GameObject> npcGenerationPanelButtons = new List<GameObject>();

    /// <summary>
    /// Adds NPCs to the generation panel by creating buttons and fields for each NPC.
    /// </summary>
    public void AddNPCsToTheGenerationPanel()
    {
        foreach (GameObject npcForm in npcGenerationsManager.npcInputForms)
        {
            GameObject buttonObj = Instantiate(npcGenerationPanelButtonPrefab, npcGenerationPanelButtonContainer);
            buttonObj.name = npcForm.name + "_Button";  // Unique name for the button
            Button npcButton = buttonObj.GetComponent<Button>();
            npcButton.onClick.AddListener(() => ShowNPCFieldsObject(buttonObj.name));

            npcGenerationPanelButtons.Add(buttonObj);

            GameObject fieldsObject = Instantiate(npcGenerationPanelFieldsObjectPrefab, npcGenerationPanelContainer.transform);
            fieldsObject.name = buttonObj.name + "_Fields";
            npcGenerationPanelFieldsObjects.Add(fieldsObject);

            NPCGenerationPanelObjectScript fieldsScript = fieldsObject.GetComponent<NPCGenerationPanelObjectScript>();
            if (fieldsScript != null)
            {
                NPCGenerationOptions data = npcForm.GetComponent<NPCGenerationOptions>();
                if (data != null)
                {
                    fieldsScript.npcSummary.text = data.GetNPCNovelPrompt();
                    fieldsScript.npcImagePrompt = data.GetNPCImagePrompt();
                    fieldsScript.npcImageCompletePrompt = data.GetNPCImageCompletePrompt();
                    fieldsScript.npcName = data.GetNPCName();
                    fieldsScript.imageZoomScreen = imageZoomScreen;
                    fieldsScript.zoomImage = zoomImage;
                    fieldsScript.npcRetouchScreensContainer = npcRetouchScreensContainer;
                    npcButton.onClick.AddListener(() => ShowNPCFieldsObject(buttonObj.name));
                }
            }
        }

        DeactivateAllForms();
    }

    /// <summary>
    /// Shows the NPC fields object corresponding to the given button name.
    /// </summary>
    /// <param name="buttonName">The name of the button to show fields for.</param>
    private void ShowNPCFieldsObject(string buttonName)
    {
        // Deactivate all other forms first
        DeactivateAllForms();

        // Find or create the NPC input form
        GameObject fieldsObject = npcGenerationPanelFieldsObjects.Find(fieldObject => fieldObject.name == buttonName + "_Fields");
        if (fieldsObject == null)
        {
            // Instantiate a new form
            fieldsObject = Instantiate(npcGenerationPanelFieldsObjectPrefab, npcGenerationPanelContainer.transform);
            fieldsObject.name = buttonName + "_Fields"; // Unique name for the form

            // Initialize form fields
            InitializeFormFields(fieldsObject);

            npcGenerationPanelFieldsObjects.Add(fieldsObject);
        }

        // Set the form active
        fieldsObject.SetActive(true);
    }

    /// <summary>
    /// Initializes form fields by finding all input fields and resetting their content.
    /// </summary>
    /// <param name="form">The form to initialize.</param>
    private void InitializeFormFields(GameObject form)
    {
        var inputs = form.GetComponentsInChildren<TMP_InputField>();
        foreach (var input in inputs)
        {
            input.text = "";
        }
    }

    /// <summary>
    /// Deactivates all forms in the panel.
    /// </summary>
    private void DeactivateAllForms()
    {
        foreach (GameObject fieldsObject in npcGenerationPanelFieldsObjects)
        {
            fieldsObject.SetActive(false);
        }
    }

    /// <summary>
    /// Deletes all buttons and fields in the panel.
    /// </summary>
    public void DeleteAllButtons()
    {
        // Destroy all fields objects
        foreach (GameObject fieldsObject in npcGenerationPanelFieldsObjects)
        {
            Destroy(fieldsObject);
        }
        // Clear the list after destroying the GameObjects
        npcGenerationPanelFieldsObjects.Clear();

        // Destroy all buttons
        foreach (GameObject button in npcGenerationPanelButtons)
        {
            Destroy(button);
        }
        // Clear the list after destroying the GameObjects
        npcGenerationPanelButtons.Clear();
    }
}
