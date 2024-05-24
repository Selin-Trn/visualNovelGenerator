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
    private float offScreenX;
    private float onScreenX;
    private readonly float animationSpeed = 0.5f;

    private void Awake()
    {
        npcImage = GetComponent<Image>();
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
        }
        else
        {
            Debug.LogError("Failed to load texture at path: " + imagePath);
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

    private void SetAnimationValues()
    {
        switch (Position)
        {
            case NPCPosition.Left:
                onScreenX = Screen.width * 0.25f;
                offScreenX = -Screen.width * 0.25f;
                break;
            case NPCPosition.Center:
                onScreenX = Screen.width * 0.5f;
                offScreenX = -Screen.width * 0.25f;
                break;
            case NPCPosition.Right:
                onScreenX = Screen.width * 0.75f;
                offScreenX = Screen.width * 1.25f;
                break;
        }
    }

    private void HideInstantly()
    {
        transform.position = new Vector3(offScreenX, transform.position.y, transform.localPosition.z);
    }

    public void Show()
    {
        transform.position = new Vector3(offScreenX, transform.position.y, transform.localPosition.z);
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
