using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryDataManager : MonoBehaviour
{
    /// <summary>
    /// Saves JSON content to a specified file path.
    /// </summary>
    /// <param name="jsonContent">The JSON content to save.</param>
    /// <param name="filePath">The file path to save the JSON content to.</param>
    public void SaveJsonToFile(string jsonContent, string filePath)
    {
        try
        {
            File.WriteAllText(filePath, jsonContent);
            Debug.Log($"JSON content successfully saved to {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save JSON content to {filePath}: {e.Message}");
        }
    }

    /// <summary>
    /// Moves a file from the source file path to the destination folder path.
    /// </summary>
    /// <param name="sourceFilePath">The source file path.</param>
    /// <param name="destinationFolderPath">The destination folder path.</param>
    public void MoveFile(string sourceFilePath, string destinationFolderPath)
    {
        try
        {
            // Ensure the destination directory exists
            if (!Directory.Exists(destinationFolderPath))
            {
                Directory.CreateDirectory(destinationFolderPath);
            }

            // Get the file name
            string fileName = Path.GetFileName(sourceFilePath);

            // Create the destination file path
            string destinationFilePath = Path.Combine(destinationFolderPath, fileName);

            // Move the file
            File.Move(sourceFilePath, destinationFilePath);
            Debug.Log($"File successfully moved to {destinationFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to move file from {sourceFilePath} to {destinationFolderPath}: {e.Message}");
        }
    }

    /// <summary>
    /// Deletes all contents in the temporary folder.
    /// </summary>
    public void DeleteAllContentsInTemp()
    {
        string folderPath = Path.Combine(Application.dataPath, "Saves", "temp");
        try
        {
            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    File.Delete(file);
                }

                string[] directories = Directory.GetDirectories(folderPath);
                foreach (string directory in directories)
                {
                    Directory.Delete(directory, true);
                }

                Debug.Log($"All contents in folder {folderPath} have been deleted.");
            }
            else
            {
                Debug.LogWarning($"Folder {folderPath} does not exist.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete contents of folder {folderPath}: {e.Message}");
        }
    }

    /// <summary>
    /// Creates a save folder with a unique UUID and returns the path to the new folder.
    /// </summary>
    /// <returns>The path to the new save folder.</returns>
    public string CreateSaveFolderWithUUID()
    {
        string parentFolderPath = Path.Combine(Application.dataPath, "Saves");
        try
        {
            // Generate a new UUID
            string uuid = Guid.NewGuid().ToString();
            GeneratedContentHolder.generatedStorySaveFolderUUID = uuid;

            string newFolderPath = Path.Combine(parentFolderPath, uuid);

            if (!Directory.Exists(parentFolderPath))
            {
                Directory.CreateDirectory(parentFolderPath);
            }

            // Create the new directory with the UUID name
            Directory.CreateDirectory(newFolderPath);

            // Create the "images" folder inside the new UUID folder
            string imagesFolderPath = Path.Combine(newFolderPath, "images");
            Directory.CreateDirectory(imagesFolderPath);

            Debug.Log($"Folder created at {newFolderPath}");

            return newFolderPath;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create folder with UUID: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Moves NPC images from the temporary save path to the images path.
    /// </summary>
    /// <param name="tempSavePath">The temporary save path.</param>
    /// <param name="imagesPath">The images path.</param>
    public void MoveNpcImages(string tempSavePath, string imagesPath)
    {
        try
        {
            foreach (string npcName in GeneratedContentHolder.npcNames)
            {
                string npcPortraitSavePath = Path.Combine(tempSavePath, npcName + ".png");
                MoveFile(npcPortraitSavePath, imagesPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to move NPC images: {e.Message}");
        }
    }

    /// <summary>
    /// Moves the player portrait from the temporary save path to the images path.
    /// </summary>
    /// <param name="tempSavePath">The temporary save path.</param>
    /// <param name="imagesPath">The images path.</param>
    public void MovePlayerPortrait(string tempSavePath, string imagesPath)
    {
        try
        {
            string playerPortraitSavePath = Path.Combine(tempSavePath, "player.png");
            MoveFile(playerPortraitSavePath, imagesPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to move player portrait: {e.Message}");
        }
    }

    /// <summary>
    /// Moves the story JSON file from the temporary save path to the UUID save path.
    /// </summary>
    /// <param name="tempSavePath">The temporary save path.</param>
    /// <param name="UUIDSavePath">The UUID save path.</param>
    public void MoveStoryJSON(string tempSavePath, string UUIDSavePath)
    {
        try
        {
            string storyJsonPath = Path.Combine(tempSavePath, "story.json");
            MoveFile(storyJsonPath, UUIDSavePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to move story.json: {e.Message}");
        }
    }
}
