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
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public class GenerationManager : MonoBehaviour
{
    [Header("Player Panel")]
    public TextMeshProUGUI playerSummary;
    public GameObject playerImageObject;
    public Image playerImageComponent;
    public Button generatePlayerPortraitButton;
    public Button retouchPlayerPortraitButton;
    public Button retouchEraseAreasPlayerPortraitButton;
    public TMP_InputField retouchPlayerPortraitTextField;
    public GameObject playerRetouchScreen;
    public MaskPainter playerMaskPainter;

    [Header("Novel Panel")]
    public TextMeshProUGUI novelSummary;
    public TextMeshProUGUI generatedNovelText;
    public Button generateNovelButton;
    public Button retouchNovelButton;
    public TMP_InputField retouchNovelTextField;

    [Header("Finish Fade Screen")]
    public GameObject finishFadeScreen;

    [Header("Image Zoom")]
    public GameObject imageZoomScreen;
    public Image zoomImage;

    [Header("Managers/Options/Utils")]
    public NovelGenerationOptions novelGenerationOptions;
    public PlayerGenerationOptions playerGenerationOptions;
    public NPCGenerationsManager npcGenerationsManagers;
    public NPCGenerationPanelManager npcGenerationPanelManager;
    public LoadingCircleManager loadingCircleManager;
    public ImageProcessor imageProcessor;
    public GeneratedJsonUtil generatedJsonUtil;

    public StoryDataManager storyDataManager;

    private string playerPrompt;
    private string novelPrompt;

    private string novelName;
    private List<string> npcImagePrompts;
    private List<string> npcNovelPrompts;
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
        public Usage usage;
    }

    [System.Serializable]
    public class Choice
    {
        public int index;
        public Message message;
        public string finish_reason;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    /// <summary>
    /// Initialize the UI elements and add listeners for buttons.
    /// </summary>
    void Start()
    {
        imageZoomScreen.SetActive(false);
        finishFadeScreen.SetActive(false);
        playerRetouchScreen.SetActive(false);
        retouchEraseAreasPlayerPortraitButton.onClick.AddListener(EraseAreasOnImage);

        generateNovelButton.onClick.AddListener(OnGenerateNovelAsync);
        generatePlayerPortraitButton.onClick.AddListener(OnGeneratePlayerPortraitAsync);

        ZoomImageButtons();

        CleanLeftoverStaticData();
    }

    /// <summary>
    /// Add listeners to zoom image (when clicked on player image) buttons.
    /// </summary>
    private void ZoomImageButtons()
    {
        var playerImageZoomButton = playerImageComponent.GetComponent<Button>();
        if (playerImageZoomButton != null)
        {
            playerImageZoomButton.onClick.AddListener(OnPlayerImageClick);
        }

        var imageZoomScreenButton = imageZoomScreen.GetComponent<Button>();
        if (imageZoomScreenButton != null)
        {
            imageZoomScreenButton.onClick.AddListener(OnZoomScreenClick);
        }
    }

    /// <summary>
    /// This is called when clicking generation tab.
    /// </summary>
    public void OnClickGenerationButton()
    {
        npcGenerationPanelManager.DeleteAllButtons();
        PrepareNovelPromptsAndSummary();
        PreparePlayerPromptsAndSummary();
        PrepareNPCsPromptsAndSummary();
        npcGenerationPanelManager.AddNPCsToTheGenerationPanel();
    }

    /// <summary>
    /// Prepare the prompts and summary for the player's portrait generation.
    /// </summary>
    private void PreparePlayerPromptsAndSummary()
    {
        playerPrompt = playerGenerationOptions.GetPlayerPortraitCompletePrompt();
        playerSummary.text = playerGenerationOptions.GetPlayerNovelPrompt();
        GeneratedContentHolder.playerPrompt = playerPrompt;
        GeneratedContentHolder.playerName = playerGenerationOptions.GetPlayerName();
    }

    /// <summary>
    /// Prepare the prompts and summary for the novel generation.
    /// </summary>
    private void PrepareNovelPromptsAndSummary()
    {
        novelName = novelGenerationOptions.GetNovelName();
        novelPrompt = novelGenerationOptions.GetNovelPrompt();
        novelSummary.text = novelPrompt;
        GeneratedContentHolder.novelPrompt = novelPrompt;
        GeneratedContentHolder.novelName = novelGenerationOptions.GetNovelName();
        GeneratedContentHolder.novelSettingPrompt = novelGenerationOptions.GetCharacterPromptSettingDetail();
    }

    /// <summary>
    /// Prepare the prompts and summary for NPC generation.
    /// </summary>
    private void PrepareNPCsPromptsAndSummary()
    {
        npcImagePrompts = npcGenerationsManagers.GetAllNPCCompleteImagePrompts();
        npcNovelPrompts = npcGenerationsManagers.GetAllNPCNovelPrompts();
        GeneratedContentHolder.npcPrompts = npcNovelPrompts;
        GeneratedContentHolder.npcNames = npcGenerationsManagers.GetAllNPCNames();
        foreach (string prompt in npcNovelPrompts)
        {
            Debug.Log(prompt);
        }
    }

    /// <summary>
    /// This is called after all the generation is done and the user is ready to prepare the game for play.
    /// </summary>
    public async void OnClickFinishAndSaveGame()
    {
        finishFadeScreen.SetActive(true);

        // Create the save folder with a unique UUID
        GeneratedContentHolder.generatedStorySaveFolderPath = storyDataManager.CreateSaveFolderWithUUID();

        // Get the paths for moving process
        string tempSavePath = Path.Combine(Application.dataPath, "Saves", "temp");
        string imagesPath = Path.Combine(GeneratedContentHolder.generatedStorySaveFolderPath, "images");

        // Move images to the original folder
        storyDataManager.MoveNpcImages(tempSavePath, imagesPath);
        storyDataManager.MovePlayerPortrait(tempSavePath, imagesPath);

        // Backgrounds are directly generated into original save folder!
        SaveBackgroundNamesToHolder();
        await GenerateBackgroundImagesAsync();

        // Generate and save data.json
        GenerateAndSaveDataJSONFile();

        // Move story.json
        storyDataManager.MoveStoryJSON(tempSavePath, GeneratedContentHolder.generatedStorySaveFolderPath);

        // Clean temp save file
        storyDataManager.DeleteAllContentsInTemp();

        // Move to the game
        PlayGeneratedStory();
    }

    /// <summary>
    /// Clean the temp folder and the static data for new game generation.
    /// </summary>
    private void CleanLeftoverStaticData()
    {
        storyDataManager.DeleteAllContentsInTemp();
        GeneratedContentHolder.generatedStorySaveFolderPath = null;
        GeneratedContentHolder.npcNames = null;
        GeneratedContentHolder.npcNames = null;
        GeneratedContentHolder.backgroundNames = null;
        ChosenStoryManager.chosenStorySaveFolderUUID = null;
        ChosenStoryManager.chosenStorySaveFolderPath = null;
    }

    /// <summary>
    /// Loading the Game Scene after the generation is done.
    /// </summary>
    public void PlayGeneratedStory()
    {
        if (GeneratedContentHolder.generatedStorySaveFolderPath != null)
        {
            ChosenStoryManager.chosenStorySaveFolderUUID = GeneratedContentHolder.generatedStorySaveFolderUUID;
            ChosenStoryManager.chosenStorySaveFolderPath = GeneratedContentHolder.generatedStorySaveFolderPath;
            SceneManager.LoadScene("Game");
        }
        else
        {
            Debug.LogError($"Failed to load story.json");
        }
    }

    /// <summary>
    /// Generates the player's portrait asynchronously.
    /// </summary>
    public async void OnGeneratePlayerPortraitAsync()
    {
        loadingCircleManager.SetLoading(true, "player");
        string playerImageUrl = await GenerateImage(playerPrompt, "1024", "1024");
        StartCoroutine(GetAndAssignPlayerPortrait(playerImageUrl));
    }

    /// <summary>
    /// Opens the retouch screen for the player's portrait.
    /// </summary>
    public void OpenRetouchPlayerPortrait()
    {
        playerRetouchScreen.SetActive(true);
    }

    /// <summary>
    /// Closes the retouch screen for the player's portrait.
    /// </summary>
    public void CloseRetouchPlayerPortrait()
    {
        playerRetouchScreen.SetActive(false);
    }

    /// <summary>
    /// Retouches the player's portrait asynchronously.
    /// </summary>
    public async void OnRetouchPlayerPortraitAsync()
    {
        loadingCircleManager.SetLoading(true, "playerRetouch");
        string playerImageUrl = await RetouchImage(retouchPlayerPortraitTextField.text, "1024", "1024");
        StartCoroutine(GetAndAssignPlayerPortrait(playerImageUrl));
    }

    /// <summary>
    /// Generates the novel asynchronously.
    /// </summary>
    public void OnGenerateNovelAsync()
    {
        loadingCircleManager.SetLoading(true, "novel");
        string concatNpcNovelPrompts = "";
        int npcCounter = 1;
        foreach (string npcPrompt in npcNovelPrompts)
        {
            concatNpcNovelPrompts = $"{concatNpcNovelPrompts} NPC {npcCounter}: {npcPrompt} .";
            npcCounter++;
        }
        string novelPromptWithChars = $"{novelPrompt}. The player = {playerPrompt}. NPCs = {concatNpcNovelPrompts}";
        StartCoroutine(GenerateAndDisplayStory(novelPromptWithChars));
    }

    /// <summary>
    /// Generates an image using DALL-E 3.
    /// </summary>
    private async Task<string> GenerateImage(string prompt, string width, string height)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", Keys.apiKey);

        // Create the request content as JSON
        string jsonContent = $"{{\"model\":\"dall-e-3\",\"prompt\":\"{prompt}\",\"size\":\"{width}x{height}\",\"quality\":\"standard\",\"n\":1}}";
        var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/images/generations", requestContent);

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

    /// <summary>
    /// Retouches an image based on the provided prompt.
    /// </summary>
    private async Task<string> RetouchImage(string prompt, string width, string height)
    {
        // Get the image bytes from the local file
        byte[] imageBytes = GetImage();
        if (imageBytes == null)
        {
            throw new Exception("Image data is null.");
        }

        // Get the mask bytes created by the player mask painter
        byte[] maskBytes = playerMaskPainter.GetMaskBytes();

        using (var client = new HttpClient())
        {
            // Add the authorization header with the API key
            client.DefaultRequestHeaders.Add("Authorization", Keys.apiKey);

            // Prepare the multipart form data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(prompt), "prompt" }, // Add the prompt
                { new StringContent($"{width}x{height}"), "size" }, // Add the size
                { new StringContent("dall-e-2"), "model" }, // Add the model type
                { new StringContent("1"), "n" } // Specify the number of images to generate
            };

            // Add the image content as a PNG file
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/png");
            formData.Add(imageContent, "image", "image.png");

            // Add the mask content as a PNG file
            var maskContent = new ByteArrayContent(maskBytes);
            maskContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/png");
            formData.Add(maskContent, "mask", "mask.png");

            // Send the request to OpenAI's API
            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/images/edits", formData);

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Read and parse the response
                string responseString = await response.Content.ReadAsStringAsync();
                PortraitData portraitData = JsonUtility.FromJson<PortraitData>(responseString);
                return portraitData.data[0].url; // Return the URL of the retouched image
            }
            else
            {
                // Handle error by setting loading indicators off and logging the error
                loadingCircleManager.SetLoading(false, "player");
                loadingCircleManager.SetLoading(false, "playerRetouch");
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Request failed with status code {response.StatusCode} and message {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }
        }
    }

    /// <summary>
    /// Downloads and assigns the player portrait image from the given URL.
    /// </summary>
    private IEnumerator GetAndAssignPlayerPortrait(string imageUrl)
    {
        string path = Path.Combine(Application.dataPath, "Saves", "temp", "player.png");
        yield return imageProcessor.DownloadAndSaveImage(imageUrl, path,
            texture =>
            {
                playerMaskPainter.UpdateBaseTexture(texture);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                playerImageComponent.sprite = sprite;
                loadingCircleManager.SetLoading(false, "player");
                loadingCircleManager.SetLoading(false, "playerRetouch");
            },
            error =>
            {
                Debug.Log("failure during GetAndAssignPlayerPortrait: " + error);
                loadingCircleManager.SetLoading(false, "player");
                loadingCircleManager.SetLoading(false, "playerRetouch");
            });
    }

    /// <summary>
    /// Downloads and saves the background image from the given URL.
    /// </summary>
    private IEnumerator GetAndSaveBackgroundImage(string imageUrl, string name)
    {
        string path = Path.Combine(GeneratedContentHolder.generatedStorySaveFolderPath, "images", name + ".png");
        yield return imageProcessor.DownloadAndSaveImage(imageUrl, path,
            texture =>
            {
                imageProcessor.SaveTexture(texture, path);
                Debug.Log($"Background image for {name} saved successfully.");
            },
            error =>
            {
                Debug.LogError($"Failed to download background image for {name}. Error: {error}");
            });
    }

    /// <summary>
    /// Generates and displays the story based on the provided prompt.
    /// </summary>
    private IEnumerator GenerateAndDisplayStory(string prompt)
    {
        Task<string> generationTask = GenerateNovel(prompt);
        yield return new WaitUntil(() => generationTask.IsCompleted);

        if (generationTask.Exception != null)
        {
            Debug.LogError("Failed to generate story: " + generationTask.Exception);
            loadingCircleManager.SetLoading(false, "novel");
        }
        else
        {
            generatedStory = generationTask.Result;
            Debug.Log(generatedStory);
            generatedNovelText.text = generatedStory;  // Display the story in the UI
            loadingCircleManager.SetLoading(false, "novel");
        }
    }

    /// <summary>
    /// Generates a novel based on the provided prompt using OpenAI's API.
    /// </summary>
    private async Task<string> GenerateNovel(string prompt)
    {
        // Create a new HttpClient to send the request
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", Keys.apiKey); // Add the authorization header with the API key

        // Prepare the JSON payload for the request
        var payload = new JObject
        {
            ["model"] = "gpt-4o", // Specify the model to use
            ["messages"] = new JArray
            {
                new JObject
                {
                    ["role"] = "system", // Add the system role with the explanation content
                    ["content"] = GenerationPromptHelper.gptExplanation
                },
                new JObject
                {
                    ["role"] = "user", // Add the user role with the prompt content
                    ["content"] = prompt
                }
            },
            ["temperature"] = 0.7, // Set the temperature for the generation
            ["max_tokens"] = 4020 // Set the maximum number of tokens
        };

        // Convert the payload to a JSON string
        string jsonContent = payload.ToString();
        Debug.Log("JSON Payload: " + jsonContent);
        var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json"); // Create the request content

        // Send the request to OpenAI's API
        HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);

        // Check if the response is successful
        if (response.IsSuccessStatusCode)
        {
            // Read and parse the response
            string responseString = await response.Content.ReadAsStringAsync();
            Debug.Log(responseString);

            // Extracting the story text from the response JSON structure
            var jsonResponse = JsonUtility.FromJson<OpenAIResponse>(responseString);

            var storyJsonString = jsonResponse.choices[0].message.content; // Get the generated story content
            string fixedStoryJsonString = generatedJsonUtil.FixJsonString(storyJsonString); // fix the JSON string

            // Saving the story.json to the temp folder
            string path = Path.Combine(Application.dataPath, "Saves", "temp", "story.json");
            storyDataManager.SaveJsonToFile(fixedStoryJsonString, path); // Save the JSON string to a file

            return storyJsonString; // Return the generated story content
        }
        else
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            Debug.LogError($"Request failed with status code {response.StatusCode} and message {errorContent}");
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }
    }

    /// <summary>
    /// Generates background images asynchronously based on the novel prompt.
    /// </summary>
    public async Task GenerateBackgroundImagesAsync()
    {
        foreach (string bgImageName in GeneratedContentHolder.backgroundNames)
        {
            string backgroundPrompt = novelPrompt +
                "I want you to draw a background for this background with the given settings. The background = " +
                bgImageName;

            string bgImageUrl = await GenerateImage(backgroundPrompt, "1792", "1024");
            StartCoroutine(GetAndSaveBackgroundImage(bgImageUrl, bgImageName));
        }
    }

    /// <summary>
    /// Generates and saves the data.json file (meta data of the novel).
    /// </summary>
    private void GenerateAndSaveDataJSONFile()
    {
        var npcsArray = new JArray();
        for (int i = 0; i < GeneratedContentHolder.npcNames.Count; i++)
        {
            var npcObject = new JObject
            {
                ["name"] = GeneratedContentHolder.npcNames[i],
                ["generationPrompt"] = GeneratedContentHolder.npcPrompts[i]
            };

            npcsArray.Add(npcObject);
        }
        var dataJson = new JObject
        {
            ["story"] = new JObject
            {
                ["name"] = GeneratedContentHolder.novelName,
                ["generationPrompt"] = GeneratedContentHolder.novelPrompt,
            },
            ["player"] = new JObject
            {
                ["name"] = GeneratedContentHolder.playerName,
                ["generationPrompt"] = GeneratedContentHolder.playerPrompt,
            },
            ["NPCs"] = npcsArray,
            ["backgrounds"] = new JObject
            {
                ["names"] = new JArray(GeneratedContentHolder.backgroundNames)
            }
        };
        var dataJsonString = dataJson.ToString();
        string path = Path.Combine(GeneratedContentHolder.generatedStorySaveFolderPath, "data.json");
        storyDataManager.SaveJsonToFile(dataJsonString, path);
    }

    /// <summary>
    /// Saves the background names to the holder.
    /// </summary>
    public void SaveBackgroundNamesToHolder()
    {
        var backgroundNames = new List<string>();

        try
        {
            var storyTextPath = Path.Combine(Application.dataPath, "Saves", "temp", "story.json");
            string jsonString = File.ReadAllText(storyTextPath);
            // Parse the JSON string into a JObject
            var json = JObject.Parse(jsonString);

            // Recursively search for _number_bg keys and add their values to the list
            FindBackgroundNames(json, backgroundNames);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse JSON: {e.Message}");
        }

        GeneratedContentHolder.backgroundNames = backgroundNames;
    }

    /// <summary>
    /// Recursively searches for background names in the JSON structure and adds them to the list.
    /// </summary>
    private void FindBackgroundNames(JToken token, List<string> backgroundNames)
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                if (property.Name.EndsWith("_bg"))
                {
                    backgroundNames.Add(property.Value.ToString());
                }
                else
                {
                    FindBackgroundNames(property.Value, backgroundNames); // Recursively search nested objects
                }
            }
        }
        else if (token is JArray array)
        {
            foreach (var item in array)
            {
                FindBackgroundNames(item, backgroundNames); // Recursively search array elements
            }
        }
    }

    /// <summary>
    /// Handles the click event on the player image to zoom it.
    /// </summary>
    private void OnPlayerImageClick()
    {
        if (playerImageComponent.sprite != null)
        {
            zoomImage.sprite = playerImageComponent.sprite;
            imageZoomScreen.SetActive(true);
        }
        else
        {
            Debug.Log("NPC image component has no sprite assigned.");
        }
    }

    /// <summary>
    /// Handles the click event on the zoom screen to close it.
    /// </summary>
    private void OnZoomScreenClick()
    {
        imageZoomScreen.SetActive(false);
    }

    /// <summary>
    /// Retrieves the image bytes from the local file.
    /// </summary>
    /// <returns>The image bytes if the file exists, otherwise null.</returns>
    public byte[] GetImage()
    {
        string filePath = Path.Combine(Application.dataPath, "Saves", "temp", "player.png");

        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            return fileData;
        }
        else
        {
            Debug.Log($"File not found at path: {filePath}");
            return null;
        }
    }

    /// <summary>
    /// Erases the painted areas on the image and updates the player image component.
    /// </summary>
    public void EraseAreasOnImage()
    {
        // Erase the painted areas on the image and get the cleaned texture
        Texture2D cleanedTexture = playerMaskPainter.ErasePaintedAreas();

        // Update the raw image texture in the player mask painter with the cleaned texture
        playerMaskPainter.rawImage.texture = cleanedTexture;

        string path = Path.Combine(Application.dataPath, "Saves", "temp", "player.png");

        imageProcessor.SaveTexture(cleanedTexture, path);

        Sprite sprite = Sprite.Create(cleanedTexture, new Rect(0, 0, cleanedTexture.width, cleanedTexture.height), new Vector2(0.5f, 0.5f));

        // Update the player image component with the new sprite
        playerImageComponent.sprite = sprite;
    }
}
