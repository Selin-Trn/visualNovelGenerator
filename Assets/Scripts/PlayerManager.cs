using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.IO;

// In-game player image manager
public class PlayerManager : MonoBehaviour
{
    [SerializeField] public Image playerPortait;
    [SerializeField] public GameObject playerPortaitObject;

    /// <summary>
    /// Initializes the component by loading the player's image.
    /// </summary>
    void Start()
    {
        LoadPlayerImage();
    }

    /// <summary>
    /// Loads the player's image from the specified path and sets it to the portrait.
    /// </summary>
    private void LoadPlayerImage()
    {
        string imagePath = Path.Combine(ChosenStoryManager.chosenStorySaveFolderPath, "images", $"player.png");
        Texture2D texture = LoadTexture(imagePath);
        if (texture != null)
        {
            playerPortait.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        else
        {
            Debug.LogError("Failed to load player texture at: " + imagePath);
        }
    }

    /// <summary>
    /// Loads a texture from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the file to load the texture from.</param>
    /// <returns>The loaded texture, or null if the load failed.</returns>
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
}
