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

    [Header("Managers/Options")]
    public NovelGenerationOptions novelGenerationOptions;
    public PlayerGenerationOptions playerGenerationOptions;
    public NPCGenerationsManager npcGenerationsManagers;
    public NPCGenerationPanelManager npcGenerationPanelManager;
    public LoadingCircleManager loadingCircleManager;
    public ImageProcessor imageProcessor;

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


    void Start()
    {

        imageZoomScreen.SetActive(false);
        finishFadeScreen.gameObject.SetActive(false);
        playerRetouchScreen.SetActive(false);
        retouchEraseAreasPlayerPortraitButton.onClick.AddListener(EraseAreasOnImage);

        generateNovelButton.onClick.AddListener(onGenerateNovelAsync);
        generatePlayerPortraitButton.onClick.AddListener(onGeneratePlayerPortraitAsync);

        ZoomImageButtons();

        CleanLeftoverStaticData();
    }

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
    public void onClickGenerationButton()
    {
        npcGenerationPanelManager.DeleteAllButtons();
        prepareNovelPromptsAndSummary();
        preparePlayerPromptsAndSummary();
        prepareNPCsPromptsAndSummary();
        npcGenerationPanelManager.AddNPCsToTheGenerationPanel();
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
        novelName = novelGenerationOptions.GetNovelName();
        novelPrompt = novelGenerationOptions.GetNovelPrompt();
        novelSummary.text = novelPrompt;
        GeneratedContentHolder.novelPrompt = novelPrompt;
        GeneratedContentHolder.novelName = novelGenerationOptions.GetNovelName();
        GeneratedContentHolder.novelSettingPrompt = novelGenerationOptions.GetCharacterPromptSettingDetail();
    }
    private void prepareNPCsPromptsAndSummary()
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

    public async void onClickFinishAndSaveGame()
    {
        finishFadeScreen.gameObject.SetActive(true);
        GeneratedContentHolder.generatedStorySaveFolderPath = storyDataManager.CreateSaveFolderWithUUID();
        string tempSavePath = Path.Combine(Application.dataPath, "Saves", "temp");
        string imagesPath = Path.Combine(GeneratedContentHolder.generatedStorySaveFolderPath, "images");

        // Move images to the orignal folder
        storyDataManager.moveNpcImages(tempSavePath, imagesPath);
        storyDataManager.movePlayerPortrait(tempSavePath, imagesPath);

        // Backgrounds are directly generated into original save folder!
        saveBackgroundNamesToHolder();
        await generateBackgroundImagesAsync();

        // generate and save data.json
        generateAndSaveDataJSONFile();

        // move story.json
        storyDataManager.moveStoryJSON(tempSavePath, GeneratedContentHolder.generatedStorySaveFolderPath);

        // clean temp save file
        storyDataManager.DeleteAllContentsInTemp();

        // move to the game
        PlayGeneratedStory();
    }

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
    public async void onGeneratePlayerPortraitAsync()
    {
        loadingCircleManager.SetLoading(true, "player");
        string playerImageUrl = await generateImage(playerPrompt, "1024", "1024");
        StartCoroutine(GetAndAssignPlayerPortrait(playerImageUrl));

    }
    public void openRetouchPlayerPortrait()
    {
        playerRetouchScreen.SetActive(true);
    }
    public void closeRetouchPlayerPortrait()
    {
        playerRetouchScreen.SetActive(false);
    }
    public async void onRetouchPlayerPortraitAsync()
    {
        loadingCircleManager.SetLoading(true, "playerRetouch");
        string playerImageUrl = await retouchImage(retouchPlayerPortraitTextField.text, "1024", "1024");
        StartCoroutine(GetAndAssignPlayerPortrait(playerImageUrl));

    }
    public void onGenerateNovelAsync()
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
    private async Task<string> retouchImage(string prompt, string width, string height)
    {

        byte[] imageBytes = GetImage();
        if (imageBytes == null)
        {
            throw new Exception("Image data is null.");
        }

        byte[] maskBytes = playerMaskPainter.GetMaskBytes();

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", Keys.apiKey);

            var formData = new MultipartFormDataContent
        {
            { new StringContent(prompt), "prompt" },
            { new StringContent($"{width}x{height}"), "size" },
            { new StringContent("dall-e-2"), "model" },
            { new StringContent("1"), "n" }
        };

            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/png");
            formData.Add(imageContent, "image", "image.png");

            var maskContent = new ByteArrayContent(maskBytes);
            maskContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/png");
            formData.Add(maskContent, "mask", "mask.png");

            HttpResponseMessage response = await client.PostAsync(
                "https://api.openai.com/v1/images/edits",
                formData);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                PortraitData portraitData = JsonUtility.FromJson<PortraitData>(responseString);
                return portraitData.data[0].url;
            }
            else
            {
                loadingCircleManager.SetLoading(false, "player");
                loadingCircleManager.SetLoading(false, "playerRetouch");
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Request failed with status code {response.StatusCode} and message {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }
        }
    }

    private IEnumerator GetAndAssignPlayerPortrait(string imageUrl)
    {

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

            playerMaskPainter.UpdateBaseTexture(texture);

            string path = Path.Combine(Application.dataPath, "Saves", "temp", "player.png");
            imageProcessor.SaveTexture(texture, path);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            playerImageComponent.GetComponent<Image>().sprite = sprite;
            loadingCircleManager.SetLoading(false, "player");

        }
        else
        {
            print(request.error);
            loadingCircleManager.SetLoading(false, "player");
        }

    }

    private IEnumerator GetAndSaveBackgroundImage(string imageUrl, string name)
    {

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

            string path = Path.Combine(GeneratedContentHolder.generatedStorySaveFolderPath, "images", name + ".png");
            imageProcessor.SaveTexture(texture, path);
        }
        else
        {
            Debug.LogError($"Failed to download background image for {name}. Error: {request.error}");
        }
    }
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
    private async Task<string> GenerateNovel(string prompt)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", Keys.apiKey);

        // Prepare the JSON payload for the request
        var payload = new JObject
        {
            ["model"] = "gpt-4o",
            ["messages"] = new JArray
        {
            new JObject
            {
                ["role"] = "system",
                ["content"] = GenerationPromptHelper.gptExplanation
            },
            new JObject
            {
                ["role"] = "user",
                ["content"] = prompt
            }
        },
            ["temperature"] = 0.7,
            ["max_tokens"] = 4004
        };

        string jsonContent = payload.ToString();
        Debug.Log("JSON Payload: " + jsonContent);
        var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Send the request to OpenAI's API
        HttpResponseMessage response = await client.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            requestContent);

        if (response.IsSuccessStatusCode)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            Debug.Log(responseString);

            // Assuming the response JSON structure and extracting the story text
            var jsonResponse = JsonUtility.FromJson<OpenAIResponse>(responseString);

            var storyJsonString = jsonResponse.choices[0].message.content;
            string cleansedStoryJsonString = CleanseJsonString(storyJsonString);

            string path = Path.Combine(Application.dataPath, "Saves", "temp", "story.json");
            storyDataManager.SaveJsonToFile(cleansedStoryJsonString, path);


            return storyJsonString;
        }
        else
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            Debug.LogError($"Request failed with status code {response.StatusCode} and message {errorContent}");
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }
    }
    private string CleanseJsonString(string jsonString)
    {
        // Use a regular expression to find the first '{' and the last '}'
        int startIndex = jsonString.IndexOf('{');
        int endIndex = jsonString.LastIndexOf('}');

        if (startIndex != -1 && endIndex != -1)
        {
            jsonString = jsonString.Substring(startIndex, (endIndex - startIndex) + 1);
        }

        return jsonString.Trim();
    }

    public async Task generateBackgroundImagesAsync()
    {
        foreach (string bgImageName in GeneratedContentHolder.backgroundNames)
        {
            string backgroundPrompt = novelPrompt +
            "I want you to draw a background for this background with the given settings. The background = " +
            bgImageName;

            string bgImageUrl = await generateImage(backgroundPrompt, "1792", "1024");
            StartCoroutine(GetAndSaveBackgroundImage(bgImageUrl, bgImageName));

        }

    }
    private void generateAndSaveDataJSONFile()
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

    public void saveBackgroundNamesToHolder()
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
    private void OnZoomScreenClick()
    {

        imageZoomScreen.SetActive(false);

    }

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


    public void EraseAreasOnImage()
    {
        Texture2D cleanedTexture = playerMaskPainter.ErasePaintedAreas();

        playerMaskPainter.rawImage.texture = cleanedTexture;

        string path = Path.Combine(Application.dataPath, "Saves", "temp", "player.png");
        imageProcessor.SaveTexture(cleanedTexture, path);

        Sprite sprite = Sprite.Create(cleanedTexture, new Rect(0, 0, cleanedTexture.width, cleanedTexture.height), new Vector2(0.5f, 0.5f));
        playerImageComponent.sprite = sprite;
    }
}
