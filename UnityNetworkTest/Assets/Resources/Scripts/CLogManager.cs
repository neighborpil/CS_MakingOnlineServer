using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CLogManager
{
    public static void log(string format)
    {
        Debug.Log(string.Format("[{0}] {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, format));
    }

	
}
