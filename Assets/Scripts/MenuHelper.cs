using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHelper : MonoBehaviour
{
    public void OnPress1Player()
    {
        SceneManagerManager.Instance.LoadGame1Player();
    }

    public void OnPress2Player()
    {
        SceneManagerManager.Instance.LoadGame2Player();
    }
    
    public void OnPress0Player()
    {
        SceneManagerManager.Instance.LoadGame0Player();
    }
}
