using System;
using UnityEngine;

public abstract class BaseWindow : MonoBehaviour
{
    public virtual void Show()
    {
        gameObject.SetActiveOptimize(true);   
    }

    public virtual void Hide()
    {
        gameObject.SetActiveOptimize(false);   
    }

    private void OnValidate()
    {
        gameObject.name = GetType().ToString();
    }
}