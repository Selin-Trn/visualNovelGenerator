using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// this is mostly for handling the background for NPCs
// DALL-E is unable to create images with alpha value (transparency)
// therefore, I created a logic where the background is generated white and
// an algorithm detects the white background and erases it
public class ImageProcessor : MonoBehaviour
{
    private Color backgroundColor = new Color(255 / 255f, 255 / 255f, 255 / 255f);

    /*
    * This removes a specified background color from an image by making pixels with that color transparent 
    * it starts by creating a new texture identical to the original and uses a flood fill algorithm to traverse and process pixels
    * it enqueues all edge pixels and checks if their color matches the target color within a defined tolerance
    * if a pixel matches, it is set to transparent, and its neighboring pixels are added to the queue for further processing
    * this continues until all relevant pixels are processed
    * finally, the changes are applied to the new texture, which is then returned with the background color removed
    */
    public Texture2D RemoveBackgroundColor(Texture2D originalTexture)
    {
        // Define the target color and tolerance for background removal
        Color targetColor = backgroundColor;
        int tolerance = 40; // Adjust this tolerance as needed

        // Create a copy of the texture to modify with the same format and mipmaps
        Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, originalTexture.format, originalTexture.mipmapCount > 0);
        Graphics.CopyTexture(originalTexture, newTexture);

        // Flood fill from the edges
        Queue<Vector2Int> pixels = new Queue<Vector2Int>();

        // Add edge pixels to the queue
        for (int x = 0; x < newTexture.width; x++)
        {
            pixels.Enqueue(new Vector2Int(x, 0)); // Top edge
            pixels.Enqueue(new Vector2Int(x, newTexture.height - 1)); // Bottom edge
        }
        for (int y = 0; y < newTexture.height; y++)
        {
            pixels.Enqueue(new Vector2Int(0, y)); // Left edge
            pixels.Enqueue(new Vector2Int(newTexture.width - 1, y)); // Right edge
        }

        // Process the queue and set the target color pixels to transparent
        while (pixels.Count > 0)
        {
            Vector2Int point = pixels.Dequeue();
            Color pixelColor = newTexture.GetPixel(point.x, point.y);
            if (IsTargetColor(pixelColor, targetColor, tolerance))
            {
                newTexture.SetPixel(point.x, point.y, Color.clear);
                EnqueueIfValid(pixels, point.x - 1, point.y, newTexture.width, newTexture.height);
                EnqueueIfValid(pixels, point.x + 1, point.y, newTexture.width, newTexture.height);
                EnqueueIfValid(pixels, point.x, point.y - 1, newTexture.width, newTexture.height);
                EnqueueIfValid(pixels, point.x, point.y + 1, newTexture.width, newTexture.height);
            }
        }

        newTexture.Apply(); // Apply all SetPixel changes
        return newTexture;
    }

    // Check if a pixel color matches the target color within a given tolerance
    private bool IsTargetColor(Color pixelColor, Color targetColor, int tolerance)
    {
        return Mathf.Abs(pixelColor.r - targetColor.r) * 255 <= tolerance &&
               Mathf.Abs(pixelColor.g - targetColor.g) * 255 <= tolerance &&
               Mathf.Abs(pixelColor.b - targetColor.b) * 255 <= tolerance;
    }

    // Add valid neighboring pixels to the queue
    private void EnqueueIfValid(Queue<Vector2Int> queue, int x, int y, int width, int height)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            queue.Enqueue(new Vector2Int(x, y));
        }
    }

    // Load a texture from a file
    public Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2); // Temporary size; LoadImage will replace it
        if (texture.LoadImage(fileData)) // LoadImage replaces the texture's dimensions
            return texture;
        return null;
    }

    // Save a texture to a file in PNG format
    public void SaveTexture(Texture2D texture, string filePath)
    {
        byte[] bytes = texture.EncodeToPNG(); // Encode the texture into PNG format
        File.WriteAllBytes(filePath, bytes); // Write to file
    }

    // Download an image from a URL and save it to a file
    public IEnumerator DownloadAndSaveImage(string imageUrl, string savePath, Action<Texture2D> onSuccess, Action<string> onFailure)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Save the downloaded texture and invoke the success callback
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            SaveTexture(texture, savePath);
            onSuccess?.Invoke(texture);
        }
        else
        {
            // Log the error and invoke the failure callback
            Debug.LogError($"Failed to download image from {imageUrl}. Error: {request.error}");
            onFailure?.Invoke(request.error);
        }
    }

    // Generate a transparent mask with the specified width and height
    public byte[] GenerateTransparentMask(int width, int height)
    {
        // Create a new transparent texture with the same dimensions as the original image
        Texture2D maskTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Color[] transparentPixels = new Color[width * height];
        for (int i = 0; i < transparentPixels.Length; i++)
        {
            transparentPixels[i] = new Color(0, 0, 0, 0);
        }
        maskTexture.SetPixels(transparentPixels);
        maskTexture.Apply();

        return maskTexture.EncodeToPNG();
    }

}
