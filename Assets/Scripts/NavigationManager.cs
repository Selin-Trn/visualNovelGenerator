using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigationManager : MonoBehaviour
{
    [SerializeField] NovelGenerationOptions novelOptions;
    [SerializeField] PlayerGenerationOptions playerOptions;
    [SerializeField] NPCGenerationOptions npcOptions;
    [SerializeField] GameObject generationCanvas;

    [SerializeField] Button novelButton;
    [SerializeField] Button playerButton;
    [SerializeField] Button npcsButton;
    [SerializeField] Button generationButton;

    [SerializeField] Color selectedButtonColor = Color.green;  // Color for selected button
    [SerializeField] Color defaultButtonColor = Color.white;   // Default color for buttons

    private Button currentSelectedButton;

    void Start()
    {
        novelButton.onClick.AddListener(() => onClickNovel());
        playerButton.onClick.AddListener(() => onClickPlayer());
        npcsButton.onClick.AddListener(() => onClickNPCs());
        generationButton.onClick.AddListener(() => onClickFinish());

        // Initialize the buttons with default settings
        SetButtonColors(novelButton, true);  // Start with the novel button selected
        onClickNovel();  // Open the novel tab initially
    }

    void UpdateButtonColors(Button selectedButton)
    {
        if (currentSelectedButton != null)
        {
            currentSelectedButton.GetComponent<Image>().color = defaultButtonColor;
        }
        currentSelectedButton = selectedButton;
        currentSelectedButton.GetComponent<Image>().color = selectedButtonColor;
    }

    public void onClickNovel()
    {
        novelOptions.gameObject.SetActive(true);
        playerOptions.gameObject.SetActive(false);
        npcOptions.gameObject.SetActive(false);
        generationCanvas.gameObject.SetActive(false);
        SetButtonColors(novelButton, true);
    }
    public void onClickPlayer()
    {
        novelOptions.gameObject.SetActive(false);
        playerOptions.gameObject.SetActive(true);
        npcOptions.gameObject.SetActive(false);
        generationCanvas.gameObject.SetActive(false);
        SetButtonColors(playerButton, true);
    }
    public void onClickNPCs()
    {
        novelOptions.gameObject.SetActive(false);
        playerOptions.gameObject.SetActive(false);
        npcOptions.gameObject.SetActive(true);
        generationCanvas.gameObject.SetActive(false);
        SetButtonColors(npcsButton, true);
    }
    public void onClickFinish()
    {
        novelOptions.gameObject.SetActive(false);
        playerOptions.gameObject.SetActive(false);
        npcOptions.gameObject.SetActive(false);
        generationCanvas.gameObject.SetActive(true);
        SetButtonColors(generationButton, true);
    }

    // Helper method to update the button colors based on selection
    private void SetButtonColors(Button selectedButton, bool isSelected)
    {
        if (isSelected)
        {
            UpdateButtonColors(selectedButton);
        }
    }
}
