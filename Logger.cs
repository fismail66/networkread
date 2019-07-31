using System;
using System.IO;

public enum LogTarget
{
    File, Database, EventLog
}

public abstract class LogBase
{
    protected readonly object lockObj = new object();
    public abstract void Log(string message);
}

public class FileLogger : LogBase
{
    public string filePath = @"Log.txt";
    public override void Log(string message)
    {
        lock (lockObj)
        {
            using (StreamWriter streamWriter = new StreamWriter(filePath, append: true))
            {
                //streamWriter.WriteLine(DateTime.Now.ToLocalTime() + ": " + message);
                streamWriter.Write(message);
                streamWriter.Close();
            }
        }
    }
}

public static class LogHelper
{
    private static LogBase logger = null;
    public static void Log(LogTarget target, string message)
    {
        switch (target)
        {
            case LogTarget.File:
                logger = new FileLogger();
                logger.Log(message);
                break;
            default:
                return;
        }
    }
}