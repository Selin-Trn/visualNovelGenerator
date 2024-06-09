using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

// utility class for checking the generatd JSON for common mistakes
public class GeneratedJsonUtil : MonoBehaviour
{
    /// <summary>
    /// Fixes common issues in a JSON string, such as missing braces and commas.
    /// </summary>
    /// <param name="jsonString">The input JSON string to be fixed.</param>
    /// <returns>The fixed JSON string or an error message if fixing fails.</returns>
    public string FixJsonString(string jsonString)
    {
        try
        {

            if (CheckIfJsonParseable(jsonString))
            {
                return jsonString;
            }

            string cleansedString = CleanseJsonString(jsonString);
            string bracedString = AddMissingBraces(cleansedString);
            string fixedString = AddMissingCommas(bracedString);

            if (CheckIfJsonParseable(fixedString))
            {
                return fixedString;
            }
            return "Problem with the novel. Please regenerate!";
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing JSON: {ex.Message}");
            return "Problem with the novel. Please regenerate!";
        }
    }

    /// <summary>
    /// Cleanses the input JSON string by trimming unnecessary characters and ensuring it starts and ends with braces.
    /// </summary>
    /// <param name="jsonString">The input JSON string to be cleansed.</param>
    /// <returns>The cleansed JSON string.</returns>
    private string CleanseJsonString(string jsonString)
    {
        // Use a regular expression to find the first '{' and the last '}'
        int startIndex = jsonString.IndexOf('{');
        int endIndex = jsonString.LastIndexOf('}');

        if (startIndex != -1 && endIndex != -1)
        {
            jsonString = jsonString.Substring(startIndex, (endIndex - startIndex) + 1);
        }

        return jsonString.Trim();
    }

    /// <summary>
    /// Adds missing commas between JSON key-value pairs where necessary.
    /// </summary>
    /// <param name="jsonString">The input JSON string with potential missing commas.</param>
    /// <returns>The JSON string with missing commas added.</returns>
    private string AddMissingCommas(string jsonString)
    {
        var sb = new System.Text.StringBuilder();
        int jsonStringLength = jsonString.Length;

        for (int i = 0; i < jsonStringLength; i++)
        {
            char currentChar = jsonString[i];
            char nextChar = i < jsonStringLength - 1 ? jsonString[i + 1] : '\0';

            bool needsComma = false;
            int spaceCount = 0;

            if (currentChar == '}' || currentChar == '\"')
            {
                for (int j = 1; j <= 11 && (i + j) < jsonStringLength; j++)
                {
                    if (jsonString[i + j] == '\n')
                    {
                        if (jsonString[i + j + 1] == ' ')
                        {
                            spaceCount++;
                        }
                    }
                    else if (jsonString[i + j] == ' ')
                    {
                        spaceCount++;
                    }
                    else if (jsonString[i + j] == '\"' || jsonString[i + j] == '{')
                    {
                        if ((currentChar == '}' && spaceCount >= 2) ||
                            (currentChar == '\"' && spaceCount >= 4))
                        {
                            needsComma = true;
                        }
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            sb.Append(currentChar);

            if (needsComma)
            {
                sb.Append(',');
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Adds missing closing braces to the JSON string if necessary.
    /// </summary>
    /// <param name="jsonString">The input JSON string with potential missing braces.</param>
    /// <returns>The JSON string with missing braces added.</returns>
    private string AddMissingBraces(string jsonString)
    {
        int openBraces = 0;
        int closeBraces = 0;

        foreach (char c in jsonString)
        {
            switch (c)
            {
                case '{':
                    openBraces++;
                    break;
                case '}':
                    closeBraces++;
                    break;
            }
        }

        int missingCloseBraces = openBraces - closeBraces;

        if (missingCloseBraces > 0)
        {
            jsonString = jsonString.TrimEnd() + new string('}', missingCloseBraces);
        }

        return jsonString;
    }

    /// <summary>
    /// Checks if the JSON string is parseable and returns a properly formatted JSON string.
    /// </summary>
    /// <param name="jsonString">The input JSON string to be checked.</param>
    /// <returns>True if the JSON is parseable, false if not.</returns>
    private bool CheckIfJsonParseable(string jsonString)
    {
        try
        {
            var jsonObject = JObject.Parse(jsonString);

            // If parsing is successful
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing JSON: {ex.Message}");
            return false;
        }
    }
}
