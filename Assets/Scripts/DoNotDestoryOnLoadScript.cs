using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestoryOnLoadScript : MonoBehaviour
{
    public DoNotDestoryOnLoadScript instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
        DontDestroyOnLoad(instance);
    }
}
