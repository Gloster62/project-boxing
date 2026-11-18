using UnityEngine;

public abstract class Singelton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance{get; private set;}

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Destroy(gameObject);
            return;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}

public abstract class PersistenSingelton<T> : Singelton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}