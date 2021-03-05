using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public void LoadMenuScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void LoadGameScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
