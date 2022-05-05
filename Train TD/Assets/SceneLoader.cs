using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
    public static SceneLoader s;
    
    public SceneReference mainScene;
    
    
    [SerializeField]
    private SceneReference initialScene;

    public bool isLevelStarted = false;
    public bool isLevelFinished = false;

    public bool isLevelInProgress {
        get {
            return isLevelStarted && !isLevelFinished;
        }
    }
    public LevelDataJson currentLevel;

    public bool isProfileMenu = true;
    
    private void Awake() {
        if (s == null) {
            s = this;
            DontDestroyOnLoad(gameObject);

            if (SceneManager.GetActiveScene().path == initialScene.ScenePath) {
                LoadScene(mainScene);
            } else {
                loadingScreen.SetActive(false);
            }
        } else if(s!= this){
            loadingScreen.SetActive(false);
            Destroy(gameObject);
        }
    }


    public GameObject loadingScreen;
    public CanvasGroup canvasGroup;


    public void OpenProfileScreen() {
        isLevelFinished = false;
        isLevelStarted = false;
        isProfileMenu = true;
        currentLevel = null;
        LoadScene(mainScene);
    }
    
    public void BackToMenu() {
        isLevelFinished = false;
        isLevelStarted = false;
        isProfileMenu = false;
        LoadScene(mainScene);
    }

    public static float loadingProgress;
    public bool isLoading = false;
    private void LoadScene(SceneReference sceneReference) {
        if(!isLoading)
            StartCoroutine(StartLoad(sceneReference));
    }

    IEnumerator StartLoad(SceneReference sceneReference) {
        isLoading = true;
        loadingProgress = 0;
        loadingScreen.SetActive(true);
        yield return StartCoroutine(FadeLoadingScreen(0,1, 0.5f));

        var operation = SceneManager.LoadSceneAsync(sceneReference.ScenePath, LoadSceneMode.Single);
        while (!operation.isDone)
        {
            loadingProgress = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }

        yield return StartCoroutine(FadeLoadingScreen(1,0, 0.5f));
        loadingScreen.SetActive(false);
        isLoading = false;
    }

    IEnumerator FadeLoadingScreen(float startValue, float targetValue, float duration)
    {
        float time = 0;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = targetValue;
    }
}
