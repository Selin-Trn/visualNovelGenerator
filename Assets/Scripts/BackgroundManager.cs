using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Manager for loading/switching backgrounds in-game
public class BackgroundManager : MonoBehaviour
{
    [SerializeField] public Image backgroundImage;

    /// <summary>
    /// Takes the background image name, finds it in the save folder, and loads it.
    /// </summary>
    /// <param name="backgroundName">The name of the background image to load.</param>
    public void SwitchBackgroundImage(string backgroundName)
    {
        string imagePath = Path.Combine(ChosenStoryManager.chosenStorySaveFolderPath, "images", $"{backgroundName}.png");
        Texture2D texture = LoadTexture(imagePath);
        if (texture != null)
        {
            backgroundImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        else
        {
            Debug.Log("Failed to load background texture at path: " + imagePath);
        }
    }

    /// <summary>
    /// Utility function to load a texture from a file.
    /// </summary>
    /// <param name="filePath">The file path of the texture to load.</param>
    /// <returns>The loaded texture, or null if loading fails.</returns>
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
