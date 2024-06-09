using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Manager for circles that turn when an image is being generated/retouched or novel text is generated
public class LoadingCircleManager : MonoBehaviour
{
    public Image loadingCircleNovel;
    public Image loadingCirclePlayer;
    public Image loadingCirclePlayerRetouch;
    public Image loadingCircleFinishGame;
    public float rotationSpeed = 200f;

    private Coroutine rotationCoroutine;

    /// <summary>
    /// Initializes the loading circles by setting them to inactive.
    /// </summary>
    void Start()
    {
        SetLoading(false, "novel");
        SetLoading(false, "player");
        SetLoading(false, "playerRetouch");
        SetLoading(false, "finish");
    }

    /// <summary>
    /// Sets the loading state for a specified area, starting or stopping the rotation of the loading circle.
    /// </summary>
    /// <param name="isLoading">Indicates whether loading is in progress.</param>
    /// <param name="area">The area for which the loading state is being set ("novel", "player", "playerRetouch", "finish").</param>
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

    /// <summary>
    /// Coroutine to rotate the loading circle image.
    /// </summary>
    /// <param name="loadingCircleImage">The loading circle image to rotate.</param>
    /// <returns>An IEnumerator for coroutine support.</returns>
    private IEnumerator RotateLoadingCircle(Image loadingCircleImage)
    {
        while (true)
        {
            loadingCircleImage.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
