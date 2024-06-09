using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// manager foor circles that turn when an iamge is being generated/retouched or novel text generated
public class LoadingCircleManager : MonoBehaviour
{
    public Image loadingCircleNovel;
    public Image loadingCirclePlayer;
    public Image loadingCirclePlayerRetouch;
    public Image loadingCircleFinishGame;
    public float rotationSpeed = 200f;

    private Coroutine rotationCoroutine;

    void Start()
    {

        SetLoading(false, "novel");
        SetLoading(false, "player");
        SetLoading(false, "playerRetouch");
        SetLoading(false, "finish");
    }

    public void SetLoading(bool isLoading, string area)
    {
        Image loadingCircleImage = null;
        switch (area)
        {
            case "novel":
                loadingCircleImage = loadingCircleNovel;
                break;
            case "player":
                loadingCircleImage = loadingCirclePlayer;
                break;
            case "playerRetouch":
                loadingCircleImage = loadingCirclePlayerRetouch;
                break;
            case "finish":
                loadingCircleImage = loadingCircleFinishGame;
                break;
            default:
                break;
        }


        if (isLoading)
        {

            loadingCircleImage.gameObject.SetActive(true);
            if (rotationCoroutine == null)
            {
                rotationCoroutine = StartCoroutine(RotateLoadingCircle(loadingCircleImage));
            }
        }
        else
        {
            // Hide the loading circle and stop rotating
            loadingCircleImage.gameObject.SetActive(false);
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }
        }
    }

    private IEnumerator RotateLoadingCircle(Image loadingCircleImage)
    {
        while (true)
        {
            loadingCircleImage.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
}