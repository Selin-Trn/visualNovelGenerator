using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateAndPlayButtonsManager : MonoBehaviour
{
    public void goToGenerateStory()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameGenerationUI");
    }
}
