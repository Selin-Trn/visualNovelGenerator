using UnityEngine;
using UnityEngine.UI;
using System.IO;
using static NPCManager;

public class NPC : MonoBehaviour
{
    public string NPCName { get; set; }
    public NPCPosition Position { get; set; }
    public bool IsShowing { get; set; }

    private Image npcImage;
    private RectTransform npcRectTransform;
    private float offScreenX;
    private float onScreenX;
    private float screenY;
    private readonly float animationSpeed = 0.5f;

    private void Awake()
    {
        npcImage = GetComponent<Image>();
        npcRectTransform = GetComponent<RectTransform>();
        if (npcImage == null)
        {
            Debug.LogError("NPC Image component not found!");
        }
    }

    public void Init(string name, NPCPosition position)
    {
        NPCName = name;
        Position = position;
        LoadNPCImage(name);
        SetAnimationValues();
        HideInstantly(); // Start offscreen
    }

    private void LoadNPCImage(string imageName)
    {
        string imagePath = Path.Combine(ChosenStoryManager.chosenStorySaveFolderPath, "images", $"{imageName}.png");
        Texture2D texture = LoadTexture(imagePath);
        if (texture != null)
        {
            npcImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            SetNPCImageSize(texture);
        }
        else
        {
            Debug.Log("Failed to load NPC texture at path: " + imagePath);
        }
    }

    private Texture2D LoadTexture(string filePath)
    {
        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                return texture;
            }
        }
        return null;
    }


    private void SetNPCImageSize(Texture2D texture)
    {
        float screenHeight = Screen.height;
        float screenWidth = Screen.width;
        float maxHeight = screenHeight * 4.0f / 5.0f;
        float maxWidth = screenWidth / 2.2f;

        float textureAspectRatio = (float)texture.width / texture.height;
        float maxAspectRatio = maxWidth / maxHeight;

        float desiredWidth;
        float desiredHeight;

        if (textureAspectRatio > maxAspectRatio)
        {
            // Width is the limiting factor
            desiredWidth = maxWidth;
            desiredHeight = maxWidth / textureAspectRatio;
        }
        else
        {
            // Height is the limiting factor
            desiredWidth = maxHeight * textureAspectRatio;
            desiredHeight = maxHeight;
        }

        npcRectTransform.sizeDelta = new Vector2(desiredWidth, desiredHeight);
    }

    private void SetAnimationValues()
    {
        screenY = Screen.height * 0.0f;

        switch (Position)
        {
            case NPCPosition.Left:
                onScreenX = Screen.width * 0.25f;
                offScreenX = -Screen.width * 0.55f;
                break;
            case NPCPosition.Center:
                onScreenX = Screen.width * 0.5f;
                offScreenX = -Screen.width * 0.55f;
                break;
            case NPCPosition.Right:
                onScreenX = Screen.width * 0.75f;
                offScreenX = Screen.width * 1.55f;
                break;
        }
    }

    private void HideInstantly()
    {
        transform.position = new Vector3(offScreenX, screenY, transform.localPosition.z);
    }

    public void Show()
    {
        transform.position = new Vector3(offScreenX, screenY, transform.localPosition.z);
        LeanTween.moveX(gameObject, onScreenX, animationSpeed).setEase(LeanTweenType.linear).setOnComplete(() =>
        {
            IsShowing = true;
        });
    }

    public void Hide()
    {
        LeanTween.moveX(gameObject, offScreenX, animationSpeed).setEase(LeanTweenType.linear).setOnComplete(() =>
        {
            IsShowing = false;
        });
    }
}
