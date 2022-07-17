using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorOnlyGO : MonoBehaviour
{
    void Start()
    {
        if (!EditorApplication.isPlaying)
        {
            gameObject.SetActive(false);
        }
    }
}
