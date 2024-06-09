using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ImageProcessor : MonoBehaviour
{
    private Color backgroundColor = new Color(255 / 255f, 255 / 255f, 255 / 255f);
    public Texture2D RemoveBackgroundColor(Texture2D originalTexture)
    {
        Color targetColor = backgroundColor;
        int tolerance = 40; // Adjust this tolerance as needed

        // Create a copy of the texture to modify with the same format and mipmaps
        Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, originalTexture.format, originalTexture.mipmapCount > 0);
        Graphics.CopyTexture(originalTexture, newTexture);

        // Flood fill from the edges
        Queue<Vector2Int> pixels = new Queue<Vector2Int>();

        // Add edge pixels
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

    private bool IsTargetColor(Color pixelColor, Color targetColor, int tolerance)
    {
        return Mathf.Abs(pixelColor.r - targetColor.r) * 255 <= tolerance &&
               Mathf.Abs(pixelColor.g - targetColor.g) * 255 <= tolerance &&
               Mathf.Abs(pixelColor.b - targetColor.b) * 255 <= tolerance;
    }

    private void EnqueueIfValid(Queue<Vector2Int> queue, int x, int y, int width, int height)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            queue.Enqueue(new Vector2Int(x, y));
        }
    }

    public Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2); // Temporary size; LoadImage will replace it
        if (texture.LoadImage(fileData)) // LoadImage replaces the texture's dimensions
            return texture;
        return null;
    }

    public void SaveTexture(Texture2D texture, string filePath)
    {
        byte[] bytes = texture.EncodeToPNG(); // Encode the texture into PNG format
        File.WriteAllBytes(filePath, bytes); // Write to file
    }

    public IEnumerator DownloadAndSaveImage(string imageUrl, string savePath, Action<Texture2D> onSuccess, Action<string> onFailure)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            SaveTexture(texture, savePath);
            onSuccess?.Invoke(texture);
        }
        else
        {
            Debug.LogError($"Failed to download image from {imageUrl}. Error: {request.error}");
            onFailure?.Invoke(request.error);
        }
    }

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
