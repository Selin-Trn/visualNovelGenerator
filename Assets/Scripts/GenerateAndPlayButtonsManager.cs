using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// simple script for loading Generation Scene
public class GenerateAndPlayButtonsManager : MonoBehaviour
{
    public void goToGenerateStory()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameGenerationUI");
    }
}
