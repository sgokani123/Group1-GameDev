using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>

/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour
    where T : MonoBehaviour
{
    static T m_instance;

    public static T Instance
    {
        get
        {
            return m_instance;
        }


    }

    protected virtual void Awake()
    {
        m_instance = this as T;
    }
}
