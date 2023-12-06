using System;
using System.Collections;
using UnityEngine;


/// <summary>
/// makes it possible to call lambda function
/// inside StartCoroutine
/// </summary>
public static class CoroutineUtil
{
    private static readonly MonoBehaviour Behaviour;

    static CoroutineUtil()
    {
        var gameObject = new GameObject("CoroutineCommon");
        GameObject.DontDestroyOnLoad(gameObject);
        Behaviour = gameObject.AddComponent<MonoBehaviour>();
    }
    
    public static void CallLambda(Action action)
    {
        Behaviour.StartCoroutine(EnumerateFunction(action));
    }
    
    static IEnumerator EnumerateFunction(Action action)
    {
        action();
        yield break;
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
