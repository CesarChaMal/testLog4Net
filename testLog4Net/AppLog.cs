/*'~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
'Developed By   :       Cesar Chavez
'File Name      :       AppLog.cs
'Purpose        :       This class is used for Logging the Debug and Error
                        Level Information
'Date           :       July-2016
'~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

# region NameSpaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;
using System.IO;
using log4net.Appender;
using log4net.Core;
using System.Configuration;
using System.Diagnostics;
using System.Collections;

# endregion

# region AppLog Class

public static class AppLog
{
    # region Global Variable
    private const string LOG_REPOSITORY = "Default"; // this should likely be set in the web config.
    private static ILog m_log;
    private const string LOG_FILE_NAME = "testlog4net.log";
    private static log4net.Repository.Hierarchy.Hierarchy h2 = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
    private static log4net.Repository.Hierarchy.Logger rootLogger_root = h2.Root;
    private static string loglevelDefault = rootLogger_root.Level.ToString();
    # endregion
    
    # region Init
    public static void Init()
    {
        // Jump up the stack frame one level and locate the calling method.
        System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(1);
        MethodBase callingMethod = stackFrame.GetMethod();

        // Build a string containing the namespace and method name
        string caller = callingMethod.DeclaringType.FullName + '.' + callingMethod.Name;

        log4net.Repository.ILoggerRepository RootRep;
        RootRep = log4net.LogManager.GetRepository();

        string sLogFileName = ConfigurationManager.AppSettings["LogFileName"];

        foreach (log4net.Appender.IAppender iApp in RootRep.GetAppenders())
        {
            log4net.Appender.RollingFileAppender fApp = (log4net.Appender.RollingFileAppender)iApp;
            if(!string.IsNullOrEmpty(sLogFileName))
                fApp.File = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sLogFileName);
            else
            fApp.File = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOG_FILE_NAME);

            fApp.ActivateOptions();
        }
    }
    # endregion

    # region Write
    /*
     * Main logging function
     * 
     * */
    public static void Write(string message, LogMessageType messageType)
    {
        DoLog(message, messageType, null, null);
    }
    public static void Write(string message, LogMessageType messageType, Type type)
    {
        DoLog(message, messageType, null, type);
    }
    public static void Write(string message, LogMessageType messageType, Exception ex)
    {
        DoLog(message, messageType, ex, null);
    }
    public static void Write(string message, LogMessageType messageType, Exception ex, Type type)
    {
        DoLog(message, messageType, ex, type);
    }
    # endregion

    # region Assert
    public static void Assert(bool condition, string message)
    {
        StackFrame frame = null;
        Type otype = null;
        try
        {
            frame = new StackFrame(1, false); // 1 frame up, false: no source data, like GetFileName and GetFileLineNumber
            otype = frame.GetMethod().DeclaringType;
        }
        catch (Exception e)
        {
            AppLog.WriteError("assert exception", e);
            otype = Type.GetType("System.Object");
        }
        Assert(condition, message, otype);
    }
    public static void Assert(bool condition, string message, Type type)
    {
        if (condition == false)
            Write(message, LogMessageType.Information);
    }
    # endregion

    # region DoLog
    private static void DoLog(string message, LogMessageType messageType, Exception ex, Type type)
    {
        StackFrame frame = null;
        Type otype = null;
        LoggingEvent levent = null;
        Level lev = null;
        String stackloc = "unkown";

        if (type == null)
        {
            try
            {
                frame = new StackFrame(1, true); // 3 frame up, false: no source data, like GetFileName and GetFileLineNumber
                otype = frame.GetMethod().DeclaringType;
                stackloc = frame.GetFileName().Split(new Char [] {'\\'}).DefaultIfEmpty("unknown").Last();
                stackloc += ":" + frame.GetFileLineNumber().ToString() + ":" + frame.GetFileColumnNumber().ToString();
            }
            catch (Exception)
            {
                // Not a problem, just use the system object 
                otype = Type.GetType("System.Object");
            }
            if (otype == null)
            {
                otype = Type.GetType("System.Object");
            }
            type = otype;
        }

        m_log = LogManager.GetLogger(type);

        switch (messageType)
        {
            case LogMessageType.Debug:
               // AppLog.m_log.Debug(message, ex);
                lev = Level.Debug;
                break;

            case LogMessageType.Information:
               // AppLog.m_log.Info(message, ex);
                lev = Level.Info;
                break;

            case LogMessageType.Warn:
              //  AppLog.m_log.Warn(message, ex);
                lev = Level.Warn;
                break;

            case LogMessageType.Error:
              //  AppLog.m_log.Error(message, ex);
                lev = Level.Error;
                break;

            case LogMessageType.Fatal:
              //  AppLog.m_log.Fatal(message, ex);
                lev = Level.Fatal;
                break;
            default:
                lev = Level.Info;
                break;
        }
        
        levent = new LoggingEvent(type, m_log.Logger.Repository, m_log.Logger.Name, lev, message, ex);
        levent.Properties["stackloc"] = stackloc;
        m_log.Logger.Log(levent);  
    }
    # endregion

    # region LogMessageType Enumeration
    public enum LogMessageType
    {
        Debug,
        Information,
        Warn,
        Error,
        Fatal
    }
    # endregion

    public static string getallAppenders()
    {
        string strFile = string.Empty;
        string path = string.Empty;
        //log4net.Repository.Hierarchy.Hierarchy hierarchy = LogManager.GetLoggerRepository() as log4net.Repository.Hierarchy.Hierarchy;
        log4net.Repository.Hierarchy.Hierarchy hierarchy = log4net.LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;


        foreach (IAppender app in hierarchy.Root.Appenders)
        {
            if (app is RollingFileAppender)
            {
                RollingFileAppender ap = (RollingFileAppender)app;
                strFile = ap.File;
                path = Path.GetDirectoryName(strFile);
            }
        }
        return path;

    }
    public static string SetLoglevel(string loglevel, string flag)
    {
        log4net.Repository.ILoggerRepository[] repositories = log4net.LogManager.GetAllRepositories();
        log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
        log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
        try
        {
            //Checking the flag as ON and OFF
            if (flag.ToUpper().Equals("ON"))
            {

                loglevel = loglevel.ToUpper();
                foreach (log4net.Repository.ILoggerRepository repository in repositories)
                {

                    repository.Threshold = repository.LevelMap[loglevel];
                    log4net.Repository.Hierarchy.Hierarchy hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
                    log4net.Core.ILogger[] loggers = hier.GetCurrentLoggers();
                    foreach (log4net.Core.ILogger logger in loggers)
                    {
                        ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[loglevel];
                    }
                }

                rootLogger.Level = h.LevelMap[loglevel];


            }
            else if (flag.ToUpper().Equals("OFF"))
            {

                foreach (log4net.Repository.ILoggerRepository repository in repositories)
                {

                    repository.Threshold = repository.LevelMap[loglevelDefault];
                    log4net.Repository.Hierarchy.Hierarchy hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
                    log4net.Core.ILogger[] loggers = hier.GetCurrentLoggers();
                    foreach (log4net.Core.ILogger logger in loggers)
                    {
                        ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[loglevelDefault];
                    }
                }
                rootLogger.Level = h.LevelMap[loglevelDefault];
                loglevel = loglevelDefault;


            }
            return "true";
        }

        catch (Exception ex)
        {
            AppLog.Write("Invalid log level.Log level should be {WARN, INFO, DEBUG, ERROR, FATAL} ", LogMessageType.Error, ex.InnerException);
            return "false";
        }

    }

    #region Write Helpers
    public static void ConsoleLog(string message, ConsoleColor color)
    {
        ConsoleColor temp = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = temp;
    }

    public static void WriteError(string message)
    {
        ConsoleLog(message, ConsoleColor.Red);
        Write(message, LogMessageType.Error);
    }
    public static void WriteError(string message, Type type)
    {
        ConsoleLog(message, ConsoleColor.Red);
        Write(message, LogMessageType.Error, type);
    }
    public static void WriteError(string message, Exception ex)
    {
        ConsoleLog(message + "\r\n" + ex.Message + ex.StackTrace, ConsoleColor.Red);
        Write(message, LogMessageType.Error, ex);
    }
    public static void WriteError(string message, Exception ex, Type type)
    {
        ConsoleLog(message + "\r\n" + ex.Message + ex.StackTrace, ConsoleColor.Red);
        Write(message, LogMessageType.Error, ex, type);
    }

    public static void WriteInfo(string message)
    {
        ConsoleLog(message, ConsoleColor.Green);
        Write(message, LogMessageType.Information);
    }
    public static void WriteInfo(string message, Type type)
    {
        ConsoleLog(message, ConsoleColor.Green);
        Write(message, LogMessageType.Information, type);
    }
    public static void WriteInfo(string message, Exception ex)
    {
        ConsoleLog(message + "\r\n" + ex.Message + ex.StackTrace, ConsoleColor.Green);
        Write(message, LogMessageType.Information, ex);
    }
    public static void WriteInfo(string message, Exception ex, Type type)
    {
        ConsoleLog(message + "\r\n" + ex.Message + ex.StackTrace, ConsoleColor.Green);
        Write(message, LogMessageType.Information, ex, type);
    }
    #endregion

}

# endregion
