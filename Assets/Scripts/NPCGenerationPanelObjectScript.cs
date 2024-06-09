using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NPCGenerationPanelObjectScript : MonoBehaviour
{

    public ImageProcessor imageProcessor;
    public TextMeshProUGUI npcSummary;
    public Image npcImageComponent;
    public Button generateNpcPortraitButton;
    public Button openRetouchScreenButton;

    public NPCRetouchScreenScript npcRetouchScreenScript;
    public Image loadingCircleNPC;
    public float rotationSpeed = 200f;

    public GameObject imageZoomScreen;
    public Image zoomImage;

    [SerializeField] private GameObject npcRetouchScreenPrefab;
    [SerializeField] public Transform npcRetouchScreensContainer;
    private Coroutine rotationCoroutine;
    private Coroutine retouchRotationCoroutine;

    public string npcImagePrompt;
    public string npcImageCompletePrompt;
    public string npcNovelPrompt;

    public string npcName;

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
    void Start()
    {
        GameObject npcRetouchScreenInstance = Instantiate(npcRetouchScreenPrefab, npcRetouchScreensContainer);
        npcRetouchScreenScript = npcRetouchScreenInstance.GetComponent<NPCRetouchScreenScript>();

        generateNpcPortraitButton.onClick.AddListener(onGenerateNPCPortraitAsync);
        npcRetouchScreenScript.retouchNpcPortraitButton.onClick.AddListener(onRetouchNPCPortraitAsync);
        npcRetouchScreenScript.retouchEraseAreasNpcPortraitButton.onClick.AddListener(EraseAreasOnImage);
        npcRetouchScreenScript.closeRetouchScreenButton.onClick.AddListener(CloseRetouchNPCImage);
        openRetouchScreenButton.onClick.AddListener(OpenRetouchNPCImage);
        npcRetouchScreenScript.npcRetouchScreen.SetActive(false);
        npcRetouchScreenInstance.SetActive(false);

        LoadAndAssignImage(npcName);
        SetLoading(false);
        SetRetouchLoading(false);

        var button = npcImageComponent.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnNpcImageClick);
        }


        npcRetouchScreenScript.retouchNpcPortraitButton.onClick.AddListener(onRetouchNPCPortraitAsync);
    }

    public async void onGenerateNPCPortraitAsync()
    {
        SetLoading(true);
        try
        {
            string npcImageUrl = await generateImage(npcImageCompletePrompt, "1024", "1024");
            StartCoroutine(GetAndAssignNPCPortrait(npcImageUrl));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to generate NPC portrait: {ex.Message}");
            SetLoading(false);
        }
    }
    public async void onRetouchNPCPortraitAsync()
    {
        SetRetouchLoading(true);
        try
        {
            string npcImageUrl = await retouchImage(npcRetouchScreenScript.retouchNpcPortraitTextField.text + " Make the image background RGB color (137, 144, 143) without any effects no matter what.", "1024", "1024");
            StartCoroutine(GetAndAssignNPCPortrait(npcImageUrl));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to retouch NPC portrait: {ex.Message}");
            SetRetouchLoading(false);
        }
    }
    private async Task<string> generateImage(string prompt, string width, string height)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", Keys.apiKey);

            string jsonContent = $"{{\"model\":\"dall-e-3\",\"prompt\":\"{prompt}\",\"size\":\"{width}x{height}\",\"quality\":\"standard\",\"n\":1}}";
            var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(
                "https://api.openai.com/v1/images/generations",
                requestContent);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                PortraitData portraitData = JsonUtility.FromJson<PortraitData>(responseString);
                return portraitData.data[0].url;
            }
            else
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }
        }
    }
    private async Task<string> retouchImage(string prompt, string width, string height)
    {

        byte[] imageBytes = GetImage(npcName);
        if (imageBytes == null)
        {
            throw new Exception("Image data is null.");
        }

        byte[] maskBytes = npcRetouchScreenScript.npcMaskPainter.GetMaskBytes();

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
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Request failed with status code {response.StatusCode} and message {errorContent}");
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }
        }
    }
    private IEnumerator GetAndAssignNPCPortrait(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Texture2D cleanedNpcTexture = imageProcessor.RemoveBackgroundColor(texture);

            npcRetouchScreenScript.npcMaskPainter.UpdateBaseTexture(cleanedNpcTexture);

            string path = Path.Combine(Application.dataPath, "Saves", "temp", npcName + ".png");
            imageProcessor.SaveTexture(cleanedNpcTexture, path);

            Sprite sprite = Sprite.Create(cleanedNpcTexture, new Rect(0, 0, cleanedNpcTexture.width, cleanedNpcTexture.height), new Vector2(0.5f, 0.5f));
            npcImageComponent.sprite = sprite;
            SetLoading(false);
            SetRetouchLoading(false);
        }
        else
        {
            Debug.LogError($"Failed to load image: {request.error}");
            SetLoading(false);
            SetRetouchLoading(false);
        }
    }


    public void SetLoading(bool isLoading)
    {

        if (isLoading)
        {

            loadingCircleNPC.gameObject.SetActive(true);
            if (rotationCoroutine == null)
            {
                rotationCoroutine = StartCoroutine(RotateLoadingCircle());
            }
        }
        else
        {
            // Hide the loading circle and stop rotating
            loadingCircleNPC.gameObject.SetActive(false);
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }
        }
    }

    public void SetRetouchLoading(bool isLoading)
    {
        if (isLoading)
        {
            npcRetouchScreenScript.retouchLoadingCircleNPC.gameObject.SetActive(true);
            if (retouchRotationCoroutine == null)
            {
                retouchRotationCoroutine = StartCoroutine(RotateRetouchLoadingCircle());
            }
        }
        else
        {
            if (retouchRotationCoroutine != null)
            {
                StopCoroutine(retouchRotationCoroutine);
                retouchRotationCoroutine = null;
            }
            npcRetouchScreenScript.retouchLoadingCircleNPC.gameObject.SetActive(false);
        }
    }

    private IEnumerator RotateLoadingCircle()
    {
        while (true)
        {
            loadingCircleNPC.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator RotateRetouchLoadingCircle()
    {
        while (true)
        {
            npcRetouchScreenScript.retouchLoadingCircleNPC.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
    public void LoadAndAssignImage(string npcName)
    {
        string filePath = Path.Combine(Application.dataPath, "Saves", "temp", npcName + ".png");

        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2); // Create a new Texture2D

            if (texture.LoadImage(fileData)) // Load the image data into the texture
            {
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Vector2 pivot = new Vector2(0.5f, 0.5f);
                Sprite sprite = Sprite.Create(texture, rect, pivot);

                npcImageComponent.sprite = sprite;  // Assign the sprite to the Image component
                npcRetouchScreenScript.npcMaskPainter.UpdateBaseTexture(texture);

                Debug.Log("Image loaded and assigned successfully.");
            }
            else
            {
                Debug.Log("No image found for the NPC.");
            }
        }
        else
        {
            Debug.Log($"File not found at path: {filePath}");
        }
    }
    public byte[] GetImage(string npcName)
    {
        string filePath = Path.Combine(Application.dataPath, "Saves", "temp", npcName + ".png");

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
    public Texture2D GetImageTexture(string npcName)
    {
        string filePath = Path.Combine(Application.dataPath, "Saves", "temp", npcName + ".png");

        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2); // Create a new Texture2D

            if (texture.LoadImage(fileData)) // Load the image data into the texture
            {
                return texture;
            }
            else
            {
                Debug.Log("No image found for the NPC.");
                return null;
            }
        }
        else
        {
            Debug.Log($"File not found at path: {filePath}");
            return null;
        }
    }
    private void OnNpcImageClick()
    {
        if (npcImageComponent.sprite != null)
        {
            zoomImage.sprite = npcImageComponent.sprite;
            imageZoomScreen.SetActive(true);
        }
        else
        {
            Debug.LogWarning("NPC image component has no sprite assigned.");
        }
    }
    public void OpenRetouchNPCImage()
    {
        npcRetouchScreenScript.npcRetouchScreen.SetActive(true);
        npcRetouchScreenScript.npcMaskPainter.UpdateBaseTexture(GetImageTexture(npcName));

    }
    public void CloseRetouchNPCImage()
    {
        npcRetouchScreenScript.npcRetouchScreen.SetActive(false);

    }

    public void EraseAreasOnImage()
    {
        Texture2D cleanedTexture = npcRetouchScreenScript.npcMaskPainter.ErasePaintedAreas();

        npcRetouchScreenScript.npcMaskPainter.UpdateBaseTexture(cleanedTexture);

        string path = Path.Combine(Application.dataPath, "Saves", "temp", npcName + ".png");
        imageProcessor.SaveTexture(cleanedTexture, path);

        Sprite sprite = Sprite.Create(cleanedTexture, new Rect(0, 0, cleanedTexture.width, cleanedTexture.height), new Vector2(0.5f, 0.5f));
        npcImageComponent.sprite = sprite;
    }

}
