using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.Networking;


public class GenerationManager : MonoBehaviour
{

    [Header("Player Panel")]
    public TextMeshProUGUI playerSummary;
    public GameObject playerImageObject;
    public Image playerImageComponent;
    public Button generatePlayerPortraitButton;
    public Button retouchPlayerPortraitButton;
    public TMP_InputField retouchPlayerPortraitTextField;


    [Header("Novel Panel")]
    public TextMeshProUGUI novelSummary;
    public TextMeshProUGUI generatedNovelText;
    public Button generateNovelButton;
    public Button retouchNovelButton;
    public TMP_InputField retouchNovelTextField;


    [Header("NPCs Panel")]
    public GameObject npcGenerationPanelObjectPrefab;
    public HorizontalLayoutGroup npcGenerationPanelButtons;
    public Button npcGenerationPanelButtonPrefab;


    [Header("Managers/Options")]
    public NovelGenerationOptions novelGenerationOptions;
    public PlayerGenerationOptions playerGenerationOptions;
    public NPCGenerationsManager npcGenerationsManagers;
    public NPCGenerationPanelManager npcGenerationPanelManager;
    public ImageProcessor imageProcessor;

    private string playerPrompt;
    private string novelPrompt;
    private List<string> npcPrompts;
    private string settingDetailPrompt;
    private string generatedStory;

    [System.Serializable]
    public class PortraitData
    {
        public int created;
        public Portrait[] data;
    }

    [System.Serializable]
    public class Portrait
    {
        public string revised_prompt;
        public string url;
    }

    [System.Serializable]
    public class OpenAIResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public string text;
    }



    void Start()
    {
        generateNovelButton.onClick.AddListener(onGenerateNovelAsync);
        generatePlayerPortraitButton.onClick.AddListener(onGeneratePlayerPortraitAsync);
    }
    public void onClickGenerationButton()
    {
        npcGenerationPanelManager.DeleteAllButtons();
        npcGenerationPanelManager.AddNPCsToTheGenerationPanel();
        preparePlayerPromptsAndSummary();
        prepareNovelPromptsAndSummary();
        prepareNPCsPromptsAndSummary();
    }

    private void preparePlayerPromptsAndSummary()
    {
        playerPrompt = playerGenerationOptions.GetPlayerPortraitCompletePrompt();
        playerSummary.text = playerGenerationOptions.GetPlayerNovelPrompt();
        GeneratedContentHolder.playerPrompt = playerPrompt;
        GeneratedContentHolder.playerName = playerGenerationOptions.GetPlayerName();

    }
    private void prepareNovelPromptsAndSummary()
    {
        novelPrompt = novelGenerationOptions.GetNovelPrompt();
        novelSummary.text = novelPrompt;
        GeneratedContentHolder.novelPrompt = novelPrompt;
        GeneratedContentHolder.novelName = novelGenerationOptions.GetNovelName();
    }
    private void prepareNPCsPromptsAndSummary()
    {

        npcPrompts = npcGenerationsManagers.GetAllNPCCompleteImagePrompts();
        GeneratedContentHolder.npcPrompts = npcPrompts;
        GeneratedContentHolder.npcNames = npcGenerationsManagers.GetAllNPCNames();
        foreach (string prompt in npcPrompts)
        {
            Debug.Log(prompt);
        }
    }


    public async void onGeneratePlayerPortraitAsync()
    {
        string playerImageUrl = await generateImage(playerPrompt, "175", "175");
        StartCoroutine(GetAndAssignPlayerPortrait(playerImageUrl));

    }
    public void onGenerateNovelAsync()
    {
        string concatNpcPrompts = "";
        int npcCounter = 1;
        foreach (string npcPrompt in npcPrompts)
        {
            concatNpcPrompts = $"{concatNpcPrompts} NPC {npcCounter}: {npcPrompt} .";
            npcCounter++;
        }
        string novelPromptWithChars = $"{novelPrompt}. The player = {playerPrompt}. NPCs = {concatNpcPrompts}";
        string fullNovelPrompt = GenerationPromptHelper.novelFormatExplanation + novelPrompt + novelPromptWithChars + GenerationPromptHelper.novelExampleJson;
        StartCoroutine(GenerateAndDisplayStory(fullNovelPrompt));
    }
    private async Task<string> generateImage(string prompt, string width, string height)
    {
        var client = new HttpClient();

        client.DefaultRequestHeaders.Add("Authorization", Keys.apiKey);


        // Create the request content as JSON
        string jsonContent = $"{{\"model\":\"dall-e-3\",\"prompt\":\"{prompt}\",\"size\":\"{width}x{height}\",\"quality\":\"standard\",\"n\":1}}";
        var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");


        HttpResponseMessage response = await client.PostAsync(
            "https://api.openai.com/v1/images/generations",
            requestContent);

        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            Debug.Log(responseString);

            PortraitData portraitData = JsonUtility.FromJson<PortraitData>(responseString);

            Debug.Log("Created: " + portraitData.created);
            foreach (Portrait portrait in portraitData.data)
            {
                Debug.Log("Revised Prompt: " + portrait.revised_prompt);
                Debug.Log("URL: " + portrait.url);
            }
            return portraitData.data[0].url;
        }
        else
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }
    }

    private IEnumerator GetAndAssignPlayerPortrait(string imageUrl)
    {

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);


            GeneratedContentHolder.playerPortraitTexture = texture;
            playerImageComponent.GetComponent<Image>().sprite = sprite;

        }
        else
        {
            print(request.error);
        }

    }

    private IEnumerator GetAndSaveBackgroundImage(string imageUrl)
    {

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            GeneratedContentHolder.backgroundTextures.Add(texture);

        }
        else
        {
            print(request.error);
        }
    }
    private IEnumerator GenerateAndDisplayStory(string prompt)
    {
        Task<string> generationTask = GenerateNovel(prompt);
        yield return new WaitUntil(() => generationTask.IsCompleted);

        if (generationTask.Exception != null)
        {
            Debug.LogError("Failed to generate story: " + generationTask.Exception);
        }
        else
        {
            generatedStory = generationTask.Result;
            generatedNovelText.text = generatedStory;  // Display the story in the UI
        }
    }
    private async Task<string> GenerateNovel(string prompt)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", Keys.apiKey);

        // Prepare the JSON payload for the request
        string jsonContent = $@"{{
        ""model"": ""gpt-4o"",
        ""prompt"": ${prompt},
        ""temperature"": 0.7,
        ""max_tokens"": 10
    }}";
        var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Send the request to OpenAI's API
        HttpResponseMessage response = await client.PostAsync(
            "https://api.openai.com/v1/completions",
            requestContent);

        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            Debug.Log(responseString);

            // Assuming the response JSON structure and extracting the story text
            var jsonResponse = JsonUtility.FromJson<OpenAIResponse>(responseString);
            return jsonResponse.choices[0].text;
        }
        else
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }
    }
}
