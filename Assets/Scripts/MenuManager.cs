using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("MenuWindow")]
    [SerializeField] private GameObject menuWindow;
    [SerializeField] private GameObject menuWindowSmaller;

    private float offScreenX;
    private float onScreenX;

    public bool isActive;

    private readonly float animationSpeed = 0.6f;


    void Start()
    {
        SetAnimationValues();
        InitializeMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown("escape") && !isActive)
        {
            LeanTween.moveX(menuWindow, onScreenX, animationSpeed).setEase(LeanTweenType.linear).setOnComplete(()
                =>
            { isActive = true; });
        }
        else if (Input.GetKeyDown("escape") && isActive)
        {
            LeanTween.moveX(menuWindow, offScreenX, animationSpeed).setEase(LeanTweenType.linear).setOnComplete(()
                =>
            { isActive = false; });
        }
    }

    private void SetAnimationValues()
    {
        onScreenX = Screen.width * 0.5f;
        offScreenX = -Screen.width * 1.5f;
    }

    private void InitializeMenu()
    {
        LeanTween.moveX(menuWindow, offScreenX, animationSpeed).setEase(LeanTweenType.linear).setOnComplete(()
                =>
        { isActive = false; });
    }
    public void goToStories()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StoriesScene");
    }
    public void goToGenerationUI()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameGenerationUI");
    }
    public void exitGame()
    {
        Application.Quit();
    }
}
