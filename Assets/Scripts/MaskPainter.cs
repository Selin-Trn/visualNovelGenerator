using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaskPainter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RawImage rawImage; // The UI element to display the texture
    public Texture2D baseTexture; // The base texture on which to paint
    private Texture2D combinedTexture; // The combined texture of base and mask
    private Texture2D maskTexture; // The mask texture for painting
    public Color paintColor = Color.black; // The color to paint with
    public int brushSize; // The size of the brush
    public Slider brushSizeSlider; // Slider to control brush size

    private bool isPainting = false; // Flag to track if painting is active

    void Start()
    {
        // Add listener to brush size slider
        if (brushSizeSlider != null)
        {
            brushSizeSlider.onValueChanged.AddListener(UpdateBrushSize);
            UpdateBrushSize(brushSizeSlider.value);  // Set initial brush size
        }
        CleanCanvas();
    }

    // Reset and initialize the textures
    public void CleanCanvas()
    {
        InitializeTextures();
        UpdateCombinedTexture();
        rawImage.texture = combinedTexture;
    }

    // Initialize base, mask, and combined textures
    private void InitializeTextures()
    {
        if (baseTexture == null)
        {
            return;
        }

        // Initialize the mask texture with transparent pixels
        maskTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false);
        Color[] pixels = new Color[maskTexture.width * maskTexture.height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(0, 0, 0, 0); // Transparent color
        }
        maskTexture.SetPixels(pixels);
        maskTexture.Apply();

        // Initialize the combined texture
        combinedTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false);
    }

    // Start painting when the pointer is pressed down
    public void OnPointerDown(PointerEventData eventData)
    {
        isPainting = true;
        Paint(eventData.position);
    }

    // Stop painting when the pointer is released
    public void OnPointerUp(PointerEventData eventData)
    {
        isPainting = false;
    }

    // Continue painting as the pointer is dragged
    public void OnDrag(PointerEventData eventData)
    {
        if (isPainting)
        {
            Paint(eventData.position);
        }
    }

    // Paint on the mask texture at the specified screen position
    private void Paint(Vector2 screenPosition)
    {
        // Convert screen position to local coordinates
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.rectTransform, screenPosition, null, out Vector2 localPoint);
        Vector2 textureCoord = new Vector2((localPoint.x + rawImage.rectTransform.rect.width / 2) / rawImage.rectTransform.rect.width,
                                           (localPoint.y + rawImage.rectTransform.rect.height / 2) / rawImage.rectTransform.rect.height);

        int x = Mathf.FloorToInt(textureCoord.x * baseTexture.width);
        int y = Mathf.FloorToInt(textureCoord.y * baseTexture.height);

        // Paint on the mask texture using the brush size and color
        for (int i = -brushSize; i < brushSize; i++)
        {
            for (int j = -brushSize; j < brushSize; j++)
            {
                int brushX = x + i;
                int brushY = y + j;

                if (brushX >= 0 && brushX < baseTexture.width && brushY >= 0 && brushY < baseTexture.height)
                {
                    maskTexture.SetPixel(brushX, brushY, paintColor);
                }
            }
        }

        maskTexture.Apply();
        UpdateCombinedTexture();
    }

    // Update the combined texture with the base and mask textures
    private void UpdateCombinedTexture()
    {
        for (int y = 0; y < baseTexture.height; y++)
        {
            for (int x = 0; x < baseTexture.width; x++)
            {
                Color baseColor = baseTexture.GetPixel(x, y);
                Color maskColor = maskTexture.GetPixel(x, y);
                combinedTexture.SetPixel(x, y, baseColor + maskColor);
            }
        }
        combinedTexture.Apply();
        rawImage.texture = combinedTexture;
    }

    // Erase painted areas on the base texture
    public Texture2D ErasePaintedAreas()
    {
        Color[] maskPixels = maskTexture.GetPixels();
        Color[] baseTexturePixels = baseTexture.GetPixels();
        for (int i = 0; i < maskPixels.Length; i++)
        {
            if (maskPixels[i].a != 0)
            {
                baseTexturePixels[i] = Color.clear; // Make painted areas transparent
            }
        }

        baseTexture.SetPixels(baseTexturePixels);
        baseTexture.Apply();
        return baseTexture;
    }

    // Get the mask texture as a PNG byte array
    public byte[] GetMaskBytes()
    {
        // Create a new texture for the mask with painted areas transparent
        Texture2D finalMaskTexture = new Texture2D(maskTexture.width, maskTexture.height, TextureFormat.ARGB32, false);
        Color[] maskPixels = maskTexture.GetPixels();
        for (int i = 0; i < maskPixels.Length; i++)
        {
            if (maskPixels[i] == paintColor)
            {
                maskPixels[i] = Color.clear;
            }
            else
            {
                maskPixels[i] = new Color(1, 1, 1, 1);
            }
        }
        finalMaskTexture.SetPixels(maskPixels);
        finalMaskTexture.Apply();
        return finalMaskTexture.EncodeToPNG();
    }

    // Save the mask texture to a file
    public void SaveMask()
    {
        byte[] maskBytes = GetMaskBytes();
        File.WriteAllBytes(Path.Combine(Application.dataPath, "Saves", "temp", "npcMask.png"), maskBytes);
    }

    // Update the base texture and reinitialize textures
    public void UpdateBaseTexture(Texture2D newTexture)
    {
        baseTexture = newTexture;
        InitializeTextures();
        UpdateCombinedTexture();
        rawImage.texture = combinedTexture;
    }

    // Update the brush size
    public void UpdateBrushSize(float newSize)
    {
        brushSize = Mathf.FloorToInt(newSize);
        Debug.Log("Updated brush size to: " + brushSize);
    }
}
