using UnityEngine;

public class DontDestroyOnLoadHandler : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}