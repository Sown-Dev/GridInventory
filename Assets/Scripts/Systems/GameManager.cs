using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public string PlayerScene;
    public string HomeScene;

    public string currentWorld;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartGame();
    }


    //Starts the game
    void StartGame()
    {
        if (PlayerPrefs.HasKey(GetSaveKey()))
        {
            InitializeGame();
        }
        else
        {
            LoadGame();
        }
    }



    public string GetSaveKey()
    {
        return "Save";
    }
    public void InitializeGame()
    {
        //1: load player scene regularly, to destroy any current scenes
        LoadScene(PlayerScene);
        //2: load whatever scene player was in additively to the player
        LoadScene(HomeScene);

    }

    public void LoadGame()
    {
        LoadScene(PlayerScene);
        if (PlayerPrefs.HasKey(GetSaveKey()))
        {
            string json = PlayerPrefs.GetString(GetSaveKey());
            GameState savedState = JsonUtility.FromJson<GameState>(json);
            LoadScene(savedState.currentWorld);
        }
    }

    public GameState convertToState()
    {
        GameState state = new GameState();
        state.currentWorld = currentWorld;
        return state;
    }
    public void SaveGame()
    {
        string json = JsonUtility.ToJson(convertToState());
        PlayerPrefs.SetString(GetSaveKey(), json);
        PlayerPrefs.Save();
    }

    public void LoadScene(string sceneID)
    {
        if (!currentWorld.Equals(""))
        {
            UnloadScene(currentWorld);
        }

        SceneManager.LoadScene(sceneID, LoadSceneMode.Additive);
        currentWorld = sceneID;
    }
    public void UnloadScene(string sceneID)
    {
       // SceneManager.UnloadSceneAsync(sceneID, LoadSceneMode.Additive);

    }

    public void OnApplicationQuit()
    {

        SaveGame();

    }

}

[Serializable]
public class GameState
{
    public string currentWorld;
}