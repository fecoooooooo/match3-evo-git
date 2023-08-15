using UnityEngine;
using System.Text;

public class ConsolLog : MonoBehaviour
{
    public static ConsolLog Instance;

    public delegate void OnHandleLog();

    public static OnHandleLog onHandleLog;

    StringBuilder log = new StringBuilder();

    void Start()
    {
        Instance = this;
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Debug.Log("ConsolLogOnDisable");
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string _logString, string _stackTrace, LogType _type)
    {
        log.AppendLine(_logString);
        log.AppendLine(_stackTrace);

        if (onHandleLog != null)
            onHandleLog.Invoke();
    }

    public string GetLog()
    {
        return log.ToString();
    }
}
