namespace UnityEngine;
public static class Debug
{
    public static readonly List<object> Errors = [];
    public static void LogError(object message) => Errors.Add(message);
    public static void LogException(Exception exception) => Errors.Add(exception);
}
