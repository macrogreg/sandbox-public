using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace ConcurrentLogWriterTest
{
    /// <summary>
    /// Users of this library can use this class as a leighweight redirect to whatever log technology is used for output.
    /// This allows to avoid creating complex logging abstractions (or taking dependencies on ILogger) for now.
    /// We copy this simply class to each assembly once, becasue we need to change the namespace to avoid ambuguity.
    /// 
    /// For example:
    /// 
    /// Library "Datadog.AutoInstrumentation.Profiler.Managed.dll" gets a copy of this file with the adjusted namespace:
    /// 
    /// <code>
    ///   namespace Datadog.AutoInstrumentation.Profiler.Managed
    ///   {
    ///       public static class Log
    ///       {
    ///       . . .
    ///       }
    ///   }
    /// </code>
    /// 
    /// Library "Datadog.AutoInstrumentation.Tracer.Managed.dll" gets a copy of this file with the adjusted namespace:
    /// 
    /// <code>
    ///   namespace Datadog.AutoInstrumentation.Tracer.Managed
    ///   {
    ///       public static class Log
    ///       {
    ///       . . .
    ///       }
    ///   }
    /// </code>  
    /// 
    /// Each librry can now make Log statements, for example:
    /// 
    /// <code>
    ///   Log.Info("DataExporter", "Data transport started", "size", _size, "otherAttribute", _otherAttribute);
    /// </code>  
    /// 
    /// Another composing library "Datadog.AutoInstrumentation.ProductComposer.dll" the uses the two above libraries uses some particular logging system.
    /// It wants to redirect the logs of its components accordingly.
    /// It creates a trivial adaper and configures the indirection:
    /// 
    /// <code>
    ///   namespace Datadog.AutoInstrumentation.ProductComposer
    ///   {
    ///       using ComposerLogAdapter = Datadog.AutoInstrumentation.ProductComposer.LogAdapter;
    ///       using ProfilerLog = Datadog.AutoInstrumentation.Profiler.Managed.Log;
    ///       using TracerLog = Datadog.AutoInstrumentation.Tracer.Managed.Log;
    ///       
    ///       internal static class LogAdapter
    ///       {
    ///           static LogAdapter()
    ///           {
    ///               // Redirect the logs from the libraries being composed to the coposer's processors:
    ///   
    ///               ProfilerLog.Configure.Error((component, msg, data) => ComposerLogAdapter.ErrorMessage("Profiler", component, msg, data));
    ///               ProfilerLog.Configure.Error((component, ex, data) => ComposerLogAdapter.ErrorException("Profiler", component, ex, data));
    ///               ProfilerLog.Configure.Info((component, msg, data) => ComposerLogAdapter.Info("Profiler", component, msg, data));
    ///               ProfilerLog.Configure.Debug((component, msg, data) => ComposerLogAdapter.Debug("Profiler", component, msg, data));
    ///               ProfilerLog.Configure.DebugLoggingEnabled(ComposerLogAdapter.IsDebugLoggingEnabled);
    ///   
    ///               TracerLog.Configure.Error((component, msg, data) => ComposerLogAdapter.ErrorMessage("Tracer", component, msg, data));
    ///               TracerLog.Configure.Error((component, ex, data) => ComposerLogAdapter.ErrorException("Tracer", component, ex, data));
    ///               TracerLog.Configure.Info((component, msg, data) => ComposerLogAdapter.Info("Tracer", component, msg, data));
    ///               TracerLog.Configure.Debug((component, msg, data) => ComposerLogAdapter.Debug("Tracer", component, msg, data));
    ///               TracerLog.Configure.DebugLoggingEnabled(ComposerLogAdapter.IsDebugLoggingEnabled);
    ///           }
    ///   
    ///           public const bool IsDebugLoggingEnabled = true;
    ///   
    ///           public static void ErrorMessage(string componentGroupName, string componentName, string message, params object[] dataNamesAndValues)
    ///           {
    ///               // Prepare a log line in any appropriate way. For example:
    ///               StringBuilder logLine = ProfilerLog.DefaultFormat.ConstructLogLine(
    ///                                               ProfilerLog.DefaultFormat.LogLevelMoniker_Error,
    ///                                               componentGroupName,
    ///                                               "::",
    ///                                               componentName,
    ///                                               useUtcTimestamp: false,
    ///                                               message,
    ///                                               dataNamesAndValues);
    ///               // Persist logLine to file...
    ///           }
    ///   
    ///           public static void ErrorException(string componentGroupName, string componentName, Exception exception, params object[] dataNamesAndValues)
    ///           {
    ///               // Prepare a log line and persist it to file...
    ///           }
    ///   
    ///            public static void Info(string componentGroupName, string componentName, string message, params object[] dataNamesAndValues)
    ///            {
    ///               // Prepare a log line and persist it to file...
    ///            }
    ///   
    ///           public static void Debug(string componentGroupName, string componentName, string message, params object[] dataNamesAndValues)
    ///           {
    ///               // Prepare a log line and persist it to file...
    ///           }
    ///       }
    ///   }
    /// </code>
    /// </summary>
    public static class Log
    {
        private static class DefaultHandlers
        {
            public const bool IsDebugLoggingEnabled = true;

            public static void ErrorMessage(string componentName, string message, params object[] dataNamesAndValues)
            {
                Console.WriteLine();
                Console.WriteLine(Log.DefaultFormat.ConstructLogLine(Log.DefaultFormat.LogLevelMoniker_Error, componentName, useUtcTimestamp: false, message, dataNamesAndValues)
                                                   .ToString());
            }

            public static void ErrorException(string componentName, Exception exception, params object[] dataNamesAndValues)
            {
                Log.Error(componentName, exception?.ToString(), dataNamesAndValues);
            }

            public static void Info(string componentName, string message, params object[] dataNamesAndValues)
            {
                Console.WriteLine();
                Console.WriteLine(Log.DefaultFormat.ConstructLogLine(Log.DefaultFormat.LogLevelMoniker_Info, componentName, useUtcTimestamp: false, message, dataNamesAndValues)
                                                   .ToString());
            }

            public static void Debug(string componentName, string message, params object[] dataNamesAndValues)
            {
                Console.WriteLine();
                Console.WriteLine(Log.DefaultFormat.ConstructLogLine(Log.DefaultFormat.LogLevelMoniker_Debug, componentName, useUtcTimestamp: false, message, dataNamesAndValues)
                                                   .ToString());
            }

        }  // class DefaultHandlers

        internal static class DefaultFormat
        {
            public const string TimestampLocal = "yyyy-MM-dd, HH:mm:ss.fff";
            public const string TimestampUTC = "yyyy-MM-dd, HH:mm:ss.fff UTC";

            public const string LogLevelMoniker_Error = "ERROR";
            public const string LogLevelMoniker_Info = "INFO ";
            public const string LogLevelMoniker_Debug = "DEBUG";

            private const string NullWord = "null";
            private const string DataValueNotSpecifiedWord = "unspecified";

            public static StringBuilder ConstructLogLine(string logLevelMoniker, string componentName, bool useUtcTimestamp, string message, params object[] dataNamesAndValues)
            {
                return ConstructLogLine(logLevelMoniker, componentName, null, null, useUtcTimestamp, message, dataNamesAndValues);
            }

            public static StringBuilder ConstructLogLine(string logLevelMoniker, 
                                                         string componentNamePart1, 
                                                         string componentNamePart2, 
                                                         string componentNamePart3,
                                                         bool useUtcTimestamp,
                                                         string message, 
                                                         params object[] dataNamesAndValues)
            {
                var logLine = new StringBuilder(capacity: 128);
                AppendLogLinePrefix(logLine, logLevelMoniker, useUtcTimestamp);
                AppendEventInfo(logLine, componentNamePart1, componentNamePart2, componentNamePart3, message, dataNamesAndValues);

                return logLine;
            }

            public static void AppendLogLinePrefix(StringBuilder targetBuffer, string logLevelMoniker, bool useUtcTimestamp)
            {
                if (targetBuffer == null)
                {
                    return;
                }

                targetBuffer.Append("[");

                if (useUtcTimestamp)
                {
                    targetBuffer.Append(DateTimeOffset.UtcNow.ToString(TimestampUTC));
                }
                else
                {
                    targetBuffer.Append(DateTimeOffset.Now.ToString(TimestampLocal));
                }

                targetBuffer.Append(" | ");

                if (logLevelMoniker != null)
                {
                    targetBuffer.Append(logLevelMoniker);
                }

                targetBuffer.Append("] ");
            }

            public static void AppendEventInfo(StringBuilder targetBuffer, 
                                               string componentNamePart1,
                                               string componentNamePart2,
                                               string componentNamePart3, 
                                               string message, 
                                               params object[] dataNamesAndValues)
            {
                bool hasComponentName = false;

                if (! String.IsNullOrWhiteSpace(componentNamePart1))
                {
                    targetBuffer.Append(componentNamePart1);
                    hasComponentName = true;
                }

                if (! String.IsNullOrWhiteSpace(componentNamePart2))
                {
                    targetBuffer.Append(componentNamePart2);
                    hasComponentName = true;
                }

                if (! String.IsNullOrWhiteSpace(componentNamePart3))
                {
                    targetBuffer.Append(componentNamePart3);
                    hasComponentName = true;
                }

                if (hasComponentName)
                {
                    targetBuffer.Append(": ");
                }

                if (!String.IsNullOrWhiteSpace(message))
                {
                    targetBuffer.Append(message);
                    targetBuffer.Append(". ");
                }

                if (dataNamesAndValues != null && dataNamesAndValues.Length > 0)
                {
                    targetBuffer.Append("{");
                    for (int i = 0; i < dataNamesAndValues.Length; i += 2)
                    {
                        if (i > 0)
                        {
                            targetBuffer.Append(", ");
                        }

                        targetBuffer.Append('[');
                        QuoteIfString(targetBuffer, dataNamesAndValues[i]);
                        targetBuffer.Append(']');
                        targetBuffer.Append('=');

                        if (i + 1 < dataNamesAndValues.Length)
                        {
                            QuoteIfString(targetBuffer, dataNamesAndValues[i + 1]);
                        }
                        else
                        {
                            targetBuffer.Append(DataValueNotSpecifiedWord);
                        }
                    }

                    targetBuffer.Append("}");
                }
            }

            private static void QuoteIfString<T>(StringBuilder targetBuffer, T val)
            {
                if (val == null)
                {
                    targetBuffer.Append(NullWord);
                }
                else
                {
                    if (val is string strValue)
                    {
                        targetBuffer.Append('"');
                        targetBuffer.Append(strValue);
                        targetBuffer.Append('"');
                    }
                    else
                    {
                        targetBuffer.Append(val.ToString());
                    }
                }
            }
        }  // class DefaultFormat

        public static class Configure
        {
            public static void Error(Action<string, string, object[]> logEventHandler)
            {
                s_errorMessageLogEventHandler = logEventHandler;
            }

            public static void Error(Action<string, Exception, object[]> logEventHandler)
            {
                s_errorExceptionLogEventHandler = logEventHandler;
            }

            public static void Info(Action<string, string, object[]> logEventHandler)
            {
                s_infoLogEventHandler = logEventHandler;
            }

            public static void Debug(Action<string, string, object[]> logEventHandler)
            {
                s_debugLogEventHandler = logEventHandler;
            }

            public static void DebugLoggingEnabled(bool isDebugLoggingEnabled)
            {
                s_isDebugLoggingEnabled = isDebugLoggingEnabled;
            }
        }

        private static Action<string, string, object[]> s_errorMessageLogEventHandler = DefaultHandlers.ErrorMessage;
        private static Action<string, Exception, object[]> s_errorExceptionLogEventHandler = DefaultHandlers.ErrorException;
        private static Action<string, string, object[]> s_infoLogEventHandler = DefaultHandlers.Info;
        private static Action<string, string, object[]> s_debugLogEventHandler = DefaultHandlers.Debug;
        private static bool s_isDebugLoggingEnabled = DefaultHandlers.IsDebugLoggingEnabled;

        /// <summary>
        /// Gets whether debug log messages should be processed or ignored.
        /// Consider wrapping debug message invocations into IF statements that check for this
        /// value in order to avoid unnecessarily constructing debug message strings.
        /// </summary>
        public static bool IsDebugLoggingEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return s_isDebugLoggingEnabled; }
        }

        /// <summary>
        /// Logs an error.
        /// These need to be persisted well, so that the info is available for support cases.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string componentName, string message, params object[] dataNamesAndValues)
        {
            Action<string, string, object[]> logEventHandler = s_errorMessageLogEventHandler;
            if (logEventHandler != null)
            {
                logEventHandler(componentName, message, dataNamesAndValues);
            }
        }


        /// <summary>
        /// Logs an error.
        /// These need to be persisted well, so that the info is available for support cases.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string componentName, Exception exception, params object[] dataNamesAndValues)
        {
            Action<string, Exception, object[]> logEventHandler = s_errorExceptionLogEventHandler;
            if (logEventHandler != null)
            {
                logEventHandler(componentName, exception, dataNamesAndValues);
            }
        }

        /// <summary>
        /// Logs an important info message.
        /// These need to be persisted well, so that the info is available for support cases.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(string componentName, string message, params object[] dataNamesAndValues)
        {
            Action<string, string, object[]> logEventHandler = s_infoLogEventHandler;
            if (logEventHandler != null)
            {
                logEventHandler(componentName, message, dataNamesAndValues);
            }
        }

        /// <summary>
        /// Logs a non-critical info message. Mainly used for for debugging during prototyping.
        /// These messages can likely be dropped in production.
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string componentName, string message, params object[] dataNamesAndValues)
        {
            if (IsDebugLoggingEnabled)
            { 
                Action<string, string, object[]> logEventHandler = s_debugLogEventHandler;
                if (logEventHandler != null)
                {
                    logEventHandler(componentName, message, dataNamesAndValues);
                }
            }
        }
    }
}
