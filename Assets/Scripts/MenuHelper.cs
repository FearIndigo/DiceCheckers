using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHelper : MonoBehaviour
{
    public void OnPress1Player()
    {
        AiManager.Instance.SetTrainingEnabled(false);
        PlayerManager.Instance.Set1PlayerMode();
        SceneManagerManager.Instance.LoadGame();
    }

    public void OnPress2Player()
    {
        AiManager.Instance.SetTrainingEnabled(false);
        PlayerManager.Instance.Set2PlayerMode();
        SceneManagerManager.Instance.LoadGame();
    }
    
    public void OnPress0Player()
    {
        AiManager.Instance.SetTrainingEnabled(false);
        PlayerManager.Instance.Set0PlayerMode();
        SceneManagerManager.Instance.LoadGame();
    }
    
    public void OnPressTraining()
    {
        AiManager.Instance.SetTrainingEnabled(true);
        PlayerManager.Instance.Set0PlayerMode();
        SceneManagerManager.Instance.LoadGame();
    }
}
