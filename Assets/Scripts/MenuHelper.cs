using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHelper : MonoBehaviour
{
    public void OnPress1Player()
    {
        PlayerManager.Instance.Set1PlayerMode();
        SceneManagerManager.Instance.LoadGame();
    }

    public void OnPress2Player()
    {
        PlayerManager.Instance.Set2PlayerMode();
        SceneManagerManager.Instance.LoadGame();
    }
    
    public void OnPress0Player()
    {
        PlayerManager.Instance.Set0PlayerMode();
        SceneManagerManager.Instance.LoadGame();
    }
    
    public void OnPressTraining()
    {
        PlayerManager.Instance.Set0PlayerMode();
        SceneManagerManager.Instance.LoadGame();
    }
}
