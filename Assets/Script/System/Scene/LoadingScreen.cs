using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEditor;
// Explicitly alias namespaces for disambiguation
using UI = UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingScreen;
    public UI.Slider progressBar; // Use Slider from UnityEngine.UI
    public string gameSceneName;
    public UI.Image hint; // Use Image from UnityEngine.UI
    public TMPro.TMP_Text text;
    private bool isSceneActivationTriggered = false; // To track user input
    private float originalTimeScale = 1f; // To store the original time scale

    public int level = 0;

    private void Start()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(false); // Hide the loading screen at the start
    }

    public void StartGame()
    {
        SystemController sysCtrl = FindAnyObjectByType<SystemController>();
        sysCtrl.loadStatus = level;
        LoadScene();
        
    }

    public void ReturnMenu()
    {
        LoadScene("Cover");
    }

    public void LoadGame()
    {
        // Load t? SaveSystem m?i
        GameObject btn = EventSystem.current.currentSelectedGameObject;
        SystemController sysCtrl = FindAnyObjectByType<SystemController>();
        sysCtrl.loadStatus = btn.name switch
        {
            "Load_1" => 1,
            "Load_2" => 2,
            "Load_3" => 3,
            "Load_4" => 4,
            "Load_5" => 5,
            "Load_6" => 6,
            _ => 0,
        };
        Debug.Log("Loading save file " + sysCtrl.loadStatus + " " + btn.name);
        LoadScene();
    }

    public void LoadScene()
    {
        StartCoroutine(LoadAsynchronously(gameSceneName));
    }

    public void LoadScene(string scene)
    {
        StartCoroutine(LoadAsynchronously(scene));
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        // Freeze the game by setting time scale to 0 (except the loading process)
        originalTimeScale = Time.timeScale; // Save the current time scale
        //Time.timeScale = 0; // Freeze all gameplay elements

        // Start loading the scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Prevent automatic scene activation

        if (loadingScreen != null)
            loadingScreen.SetActive(true); // Show the loading screen

        while (!operation.isDone)
        {
            // Calculate progress manually
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // Normalize progress between 0 and 0.9

            if (progressBar != null)
                progressBar.value = progress;

            // Check if loading is complete
            if (operation.progress >= 0.9f)
            {
                if (!isSceneActivationTriggered)
                {
                    if (text != null)
                        text.SetText("Press Space to activate the scene");
                }

                // Check for space key press (works even when Time.timeScale = 0)
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space))
                {
                    isSceneActivationTriggered = true;
                    operation.allowSceneActivation = true; // Activate the scene

                    // Restore time scale before scene switches
                    Time.timeScale = originalTimeScale;
                }
            }

            yield return null; // Keep the loading process active
        }

        // Restore the original time scale and hide the loading screen
        Time.timeScale = originalTimeScale; // Unfreeze gameplay
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
}