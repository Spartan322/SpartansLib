using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SpartansLib
{
    public static class Logger
    {
        [Flags]
        public enum LogLevel
        {
            VERBOSE =   0b1,
            DEBUG =     0b10,
            INFO =      0b100,
            WARN =      0b1000,
            ERROR =     0b10000,
            FATAL =     0b100000,
            All =       VERBOSE | DEBUG | INFO | WARN | ERROR | FATAL,
            None =      0,
            Default =   FATAL | ERROR | WARN | INFO
        }

        const int MAX_LOG_SIZE = 100;
        private static List<Tuple<LogLevel, string>> logHistory = new List<Tuple<LogLevel, string>>();
        private static LogLevel showFlags = LogLevel.Default;

        /// <summary>
        /// Gets the log history.
        /// </summary>
        /// <value>The log history for the past <see cref="MAX_LOG_SIZE"/> log calls.</value>
        public static IList<Tuple<LogLevel, string>> LogHistory => logHistory.AsReadOnly();

        /// <summary>
        /// Adds flag(s) to this <see cref="T:Pathfinder.Util.Logger"/>'s flags.
        /// </summary>
        /// <param name="levels">Log Level(s) to add.</param>
        public static void AddFlag(LogLevel levels) => showFlags |= levels;

        /// <summary>
        /// Sets showFlags to exact levels.
        /// </summary>
        /// <param name="levels">Log Levels to set this <see cref="T:Pathfinder.Util.Logger"/>'s showFlags to.</param>
        public static void SetFlags(LogLevel levels) => showFlags = levels;

        /// <summary>
        /// Removes a flag(s) from this <see cref="T:Pathfinder.Util.Logger"/>'s flags.
        /// </summary>
        /// <param name="levels">Log Level(s) to remove.</param>
        public static void RemoveFlag(LogLevel levels) => showFlags &= ~levels;

        /// <summary>
        /// Determines whether this <see cref="T:Pathfinder.Util.Logger"/> has the flag(s).
        /// </summary>
        /// <returns><c>true</c>, has flag, <c>false</c> otherwise.</returns>
        /// <param name="level">Log Level(s) to test for.</param>
        public static bool HasFlag(LogLevel level) => showFlags.HasFlag(level);

        public static bool IsError(LogLevel level) => level.HasFlag(LogLevel.ERROR | LogLevel.FATAL);

        private static string FormatPrefix(string memName, string path, int lineNum)
            => $"{Path.GetFileName(Path.GetDirectoryName(path)) + Path.DirectorySeparatorChar + Path.GetFileName(path)}#{lineNum}";

#if GODOT
        public static void PushWarning(params object[] input)
            => Godot.GD.PushWarning(string.Format(input[0].ToString(), input.Skip(1).ToArray()));

        public static void PushError(params object[] input)
            => Godot.GD.PushError(string.Format(input[0].ToString(), input.Skip(1).ToArray()));
#endif
        /// <summary>
        /// Logs the specified level and input.
        /// </summary>
        /// <param name="level">Log Level to log for.</param>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Log(LogLevel level,
            object arg0 = null,
            object arg1 = null,
            object arg2 = null,
            object arg3 = null,
            object arg4 = null,
            object arg5 = null,
            object arg6 = null,
            object arg7 = null,
            object arg8 = null,
            object arg9 = null,
            object arg10 = null,
            object arg11 = null,
            object arg12 = null,
            object arg13 = null,
            object arg14 = null,
            object arg15 = null,
            object arg16 = null,
            object arg17 = null,
            object arg18 = null,
            object arg19 = null,
            object arg20 = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        )
            => LogPrefixed(level,
                FormatPrefix(memberName, sourceFilePath, sourceLineNumber),
                arg0,
                arg1,
                arg2,
                arg3,
                arg4,
                arg5,
                arg6,
                arg7,
                arg8,
                arg9,
                arg10,
                arg11,
                arg12,
                arg13,
                arg14,
                arg15,
                arg16,
                arg17,
                arg18,
                arg19,
                arg20);


        public static void LogPrefixed(LogLevel level, string prefix = "", params object[] input)
        {
            if (!HasFlag(level)) return;
            input = input.Where(i => i != null).ToArray();
            Tuple<LogLevel, string> t;
            string time = "", fullPrefix = "";
            if (input.Length > 0) {
                var tdic = Godot.OS.GetDatetime(true);
                //time = $"{tdic["month"]}/{tdic["day"]}/{tdic["year"]}|{tdic["hour"]}:{tdic["minute"]}:{tdic["second"]}";
                fullPrefix = $"{tdic["month"]}/{tdic["day"]}/{tdic["year"]}:{tdic["hour"]}:{tdic["minute"]}:{tdic["second"]} [{level}] {prefix}: ";
            }
            switch (input.Length)
            {
                case 0: return;
                case 1:
                    t = new Tuple<LogLevel, string>(level, $"{fullPrefix}{input[0]}");
                    break;
                default:
                    t = new Tuple<LogLevel, string>(level, $"{fullPrefix}{string.Format(input[0].ToString(), input.Skip(1).ToArray())}");
                    break;
            }
            if (logHistory.Count >= MAX_LOG_SIZE)
                logHistory.RemoveRange(0, logHistory.Count - MAX_LOG_SIZE);
            logHistory.Add(t);
#if GODOT
            if (IsError(level)) Godot.GD.PrintErr(t.Item2);
            else Godot.GD.Print(t.Item2);
#else
            Console.WriteLine(t.Item2);
#endif
        }

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.VERBOSE"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Verbose(
            object arg0 = null,
            object arg1 = null,
            object arg2 = null,
            object arg3 = null,
            object arg4 = null,
            object arg5 = null,
            object arg6 = null,
            object arg7 = null,
            object arg8 = null,
            object arg9 = null,
            object arg10 = null,
            object arg11 = null,
            object arg12 = null,
            object arg13 = null,
            object arg14 = null,
            object arg15 = null,
            object arg16 = null,
            object arg17 = null,
            object arg18 = null,
            object arg19 = null,
            object arg20 = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        )
            => LogPrefixed(LogLevel.VERBOSE,
                FormatPrefix(memberName, sourceFilePath, sourceLineNumber),
                arg0,
                arg1,
                arg2,
                arg3,
                arg4,
                arg5,
                arg6,
                arg7,
                arg8,
                arg9,
                arg10,
                arg11,
                arg12,
                arg13,
                arg14,
                arg15,
                arg16,
                arg17,
                arg18,
                arg19,
                arg20);

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.DEBUG"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Debug(
            object arg0 = null,
            object arg1 = null,
            object arg2 = null,
            object arg3 = null,
            object arg4 = null,
            object arg5 = null,
            object arg6 = null,
            object arg7 = null,
            object arg8 = null,
            object arg9 = null,
            object arg10 = null,
            object arg11 = null,
            object arg12 = null,
            object arg13 = null,
            object arg14 = null,
            object arg15 = null,
            object arg16 = null,
            object arg17 = null,
            object arg18 = null,
            object arg19 = null,
            object arg20 = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        )
            => LogPrefixed(LogLevel.DEBUG,
                FormatPrefix(memberName, sourceFilePath, sourceLineNumber),
                arg0,
                arg1,
                arg2,
                arg3,
                arg4,
                arg5,
                arg6,
                arg7,
                arg8,
                arg9,
                arg10,
                arg11,
                arg12,
                arg13,
                arg14,
                arg15,
                arg16,
                arg17,
                arg18,
                arg19,
                arg20);

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.INFO"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Info(
            object arg0 = null,
            object arg1 = null,
            object arg2 = null,
            object arg3 = null,
            object arg4 = null,
            object arg5 = null,
            object arg6 = null,
            object arg7 = null,
            object arg8 = null,
            object arg9 = null,
            object arg10 = null,
            object arg11 = null,
            object arg12 = null,
            object arg13 = null,
            object arg14 = null,
            object arg15 = null,
            object arg16 = null,
            object arg17 = null,
            object arg18 = null,
            object arg19 = null,
            object arg20 = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        )
            => LogPrefixed(LogLevel.INFO,
                FormatPrefix(memberName, sourceFilePath, sourceLineNumber),
                arg0,
                arg1,
                arg2,
                arg3,
                arg4,
                arg5,
                arg6,
                arg7,
                arg8,
                arg9,
                arg10,
                arg11,
                arg12,
                arg13,
                arg14,
                arg15,
                arg16,
                arg17,
                arg18,
                arg19,
                arg20);

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.WARN"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Warn(
            object arg0 = null,
            object arg1 = null,
            object arg2 = null,
            object arg3 = null,
            object arg4 = null,
            object arg5 = null,
            object arg6 = null,
            object arg7 = null,
            object arg8 = null,
            object arg9 = null,
            object arg10 = null,
            object arg11 = null,
            object arg12 = null,
            object arg13 = null,
            object arg14 = null,
            object arg15 = null,
            object arg16 = null,
            object arg17 = null,
            object arg18 = null,
            object arg19 = null,
            object arg20 = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        )
            => LogPrefixed(LogLevel.WARN,
                FormatPrefix(memberName, sourceFilePath, sourceLineNumber),
                arg0,
                arg1,
                arg2,
                arg3,
                arg4,
                arg5,
                arg6,
                arg7,
                arg8,
                arg9,
                arg10,
                arg11,
                arg12,
                arg13,
                arg14,
                arg15,
                arg16,
                arg17,
                arg18,
                arg19,
                arg20);

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.ERROR"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Error(
            object arg0 = null,
            object arg1 = null,
            object arg2 = null,
            object arg3 = null,
            object arg4 = null,
            object arg5 = null,
            object arg6 = null,
            object arg7 = null,
            object arg8 = null,
            object arg9 = null,
            object arg10 = null,
            object arg11 = null,
            object arg12 = null,
            object arg13 = null,
            object arg14 = null,
            object arg15 = null,
            object arg16 = null,
            object arg17 = null,
            object arg18 = null,
            object arg19 = null,
            object arg20 = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        )
            => LogPrefixed(LogLevel.ERROR,
                FormatPrefix(memberName, sourceFilePath, sourceLineNumber),
                arg0,
                arg1,
                arg2,
                arg3,
                arg4,
                arg5,
                arg6,
                arg7,
                arg8,
                arg9,
                arg10,
                arg11,
                arg12,
                arg13,
                arg14,
                arg15,
                arg16,
                arg17,
                arg18,
                arg19,
                arg20);

        /// <summary>
        /// Logs on <see cref="T:Pathfinder.Util.Logger.LogLevel.FATAL"/> the specified input.
        /// </summary>
        /// <param name="input">Any stringable inputs. (if larger then one, must be in standard C# String.Format format)</param>
        public static void Fatal(
            object arg0 = null,
            object arg1 = null,
            object arg2 = null,
            object arg3 = null,
            object arg4 = null,
            object arg5 = null,
            object arg6 = null,
            object arg7 = null,
            object arg8 = null,
            object arg9 = null,
            object arg10 = null,
            object arg11 = null,
            object arg12 = null,
            object arg13 = null,
            object arg14 = null,
            object arg15 = null,
            object arg16 = null,
            object arg17 = null,
            object arg18 = null,
            object arg19 = null,
            object arg20 = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        )
            => LogPrefixed(LogLevel.FATAL,
                FormatPrefix(memberName, sourceFilePath, sourceLineNumber),
                arg0,
                arg1,
                arg2,
                arg3,
                arg4,
                arg5,
                arg6,
                arg7,
                arg8,
                arg9,
                arg10,
                arg11,
                arg12,
                arg13,
                arg14,
                arg15,
                arg16,
                arg17,
                arg18,
                arg19,
                arg20);
    }
}
