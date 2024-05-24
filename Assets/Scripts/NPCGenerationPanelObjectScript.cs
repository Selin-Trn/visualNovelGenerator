using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NPCGenerationPanelObjectScript : MonoBehaviour
{

    public ImageProcessor imageProcessor;
    public TextMeshProUGUI npcSummary;
    public GameObject npcImageObject;
    public Image npcImageComponent;
    public Button generateNpcPortraitButton;
    public Button retouchNpcPortraitButton;
    public TMP_InputField retouchNpcPortraitTextField;

    public string npcImagePrompt;
    public string npcImageCompletePrompt;
    public string npcNovelPrompt;

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
        generateNpcPortraitButton.onClick.AddListener(onGenerateNPCPortraitAsync);
    }

    public async void onGenerateNPCPortraitAsync()
    {
        try
        {
            string npcImageUrl = await generateImage(npcImageCompletePrompt, "600", "800");
            StartCoroutine(GetAndAssignNPCPortrait(npcImageUrl));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to generate NPC portrait: {ex.Message}");
        }
    }
    private async Task<string> generateImage(string prompt, string width, string height)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Keys.apiKey);

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
    private IEnumerator GetAndAssignNPCPortrait(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            // Assuming `imageProcessor.RemoveWhiteBackground` is implemented correctly
            Texture2D cleanedNpcTexture = imageProcessor.RemoveWhiteBackground(texture);
            Sprite sprite = Sprite.Create(cleanedNpcTexture, new Rect(0, 0, cleanedNpcTexture.width, cleanedNpcTexture.height), new Vector2(0.5f, 0.5f));
            npcImageComponent.sprite = sprite;
        }
        else
        {
            Debug.LogError($"Failed to load image: {request.error}");
        }
    }


}
