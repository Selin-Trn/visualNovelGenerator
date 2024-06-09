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
    // Public fields and references to other components
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

    // Serializable classes for portrait data
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

    /// <summary>
    /// Initializes the component.
    /// </summary>
    void Start()
    {
        // Instantiate the retouch screen
        GameObject npcRetouchScreenInstance = Instantiate(npcRetouchScreenPrefab, npcRetouchScreensContainer);
        npcRetouchScreenScript = npcRetouchScreenInstance.GetComponent<NPCRetouchScreenScript>();

        // Add listeners to buttons
        generateNpcPortraitButton.onClick.AddListener(onGenerateNPCPortraitAsync);
        npcRetouchScreenScript.retouchNpcPortraitButton.onClick.AddListener(onRetouchNPCPortraitAsync);
        npcRetouchScreenScript.retouchEraseAreasNpcPortraitButton.onClick.AddListener(EraseAreasOnImage);
        npcRetouchScreenScript.closeRetouchScreenButton.onClick.AddListener(CloseRetouchNPCImage);
        openRetouchScreenButton.onClick.AddListener(OpenRetouchNPCImage);
        npcRetouchScreenScript.npcRetouchScreen.SetActive(false);
        npcRetouchScreenInstance.SetActive(false);

        // Load and assign image
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

    /// <summary>
    /// Generates an NPC portrait asynchronously.
    /// </summary>
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

    /// <summary>
    /// Retouches an NPC portrait asynchronously.
    /// </summary>
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

    /// <summary>
    /// Generates an image using a prompt.
    /// </summary>
    /// <param name="prompt">The prompt to generate the image with.</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <returns>The URL of the generated image.</returns>
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

    /// <summary>
    /// Retouches an image using a prompt and mask.
    /// </summary>
    /// <param name="prompt">The prompt to retouch the image with.</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <returns>The URL of the retouched image.</returns>
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

    /// <summary>
    /// Gets and assigns the NPC portrait from a URL.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to get and assign.</param>
    /// <returns>An IEnumerator for coroutine support.</returns>
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

    /// <summary>
    /// Sets the loading state for the main loading circle.
    /// </summary>
    /// <param name="isLoading">Indicates whether loading is in progress.</param>
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
            loadingCircleNPC.gameObject.SetActive(false);
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }
        }
    }

    /// <summary>
    /// Sets the loading state for the retouch loading circle.
    /// </summary>
    /// <param name="isLoading">Indicates whether retouch loading is in progress.</param>
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

    /// <summary>
    /// Coroutine to rotate the main loading circle.
    /// </summary>
    /// <returns>An IEnumerator for coroutine support.</returns>
    private IEnumerator RotateLoadingCircle()
    {
        while (true)
        {
            loadingCircleNPC.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine to rotate the retouch loading circle.
    /// </summary>
    /// <returns>An IEnumerator for coroutine support.</returns>
    private IEnumerator RotateRetouchLoadingCircle()
    {
        while (true)
        {
            npcRetouchScreenScript.retouchLoadingCircleNPC.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// Loads and assigns an image to the NPC image component.
    /// </summary>
    /// <param name="npcName">The name of the NPC to load and assign the image for.</param>
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

    /// <summary>
    /// Gets the image bytes from a file.
    /// </summary>
    /// <param name="npcName">The name of the NPC to get the image for.</param>
    /// <returns>The image bytes as a byte array.</returns>
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

    /// <summary>
    /// Gets a Texture2D from a file.
    /// </summary>
    /// <param name="npcName">The name of the NPC to get the image for.</param>
    /// <returns>The image as a Texture2D.</returns>
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

    /// <summary>
    /// Handles NPC image click to zoom the image.
    /// </summary>
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

    /// <summary>
    /// Opens the NPC retouch screen.
    /// </summary>
    public void OpenRetouchNPCImage()
    {
        npcRetouchScreenScript.npcRetouchScreen.SetActive(true);
        npcRetouchScreenScript.npcMaskPainter.UpdateBaseTexture(GetImageTexture(npcName));
    }

    /// <summary>
    /// Closes the NPC retouch screen.
    /// </summary>
    public void CloseRetouchNPCImage()
    {
        npcRetouchScreenScript.npcRetouchScreen.SetActive(false);
    }

    /// <summary>
    /// Erases painted areas on the image.
    /// </summary>
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
