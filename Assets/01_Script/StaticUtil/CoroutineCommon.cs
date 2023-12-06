using System;
using System.Collections;
using UnityEngine;

public static class CoroutineCommon
{
    private static readonly MonoBehaviour Behaviour;

    static CoroutineCommon()
    {
        var gameObject = new GameObject("CoroutineCommon");
        GameObject.DontDestroyOnLoad(gameObject);
        Behaviour = gameObject.AddComponent<MonoBehaviour>();
    }

    public static void CallWaitForOneFrame(Action action)
    {
        Behaviour.StartCoroutine(DoCallWaitForOneFrame(action));
    }

    static IEnumerator DoCallWaitForOneFrame(Action action)
    {
        yield return null;
        action();
    }
    
    public  static  void CallWaitForSeconds( float seconds, Action act)
    {
        Behaviour.StartCoroutine(DoCallWaitForSeconds(seconds, act));
    }
    
    private  static IEnumerator DoCallWaitForSeconds( float seconds, Action act)
    {
        yield  return  new WaitForSeconds(seconds);
        act();
    }
}
