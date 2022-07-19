using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class SceneManagerManager : MonoBehaviour
{
    public static SceneManagerManager Instance;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void LoadGame1Player()
    {
        SceneManager.LoadScene("Game_1");
    }
    
    public void LoadGame2Player()
    {
        SceneManager.LoadScene("Game_2");
    }
    
    public void LoadGame0Player()
    {
        SceneManager.LoadScene("Game_0");
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
