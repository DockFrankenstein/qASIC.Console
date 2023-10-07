﻿using GameLog = qASIC.Log;
using System.Diagnostics;
using qASIC.Console.Commands;

namespace qASIC.Console
{
    public class GameConsole
    {
        public GameConsole()
        {

        }

        public event Action<GameLog>? OnLog;

        public static List<GameLog> Logs { get; private set; } = new List<GameLog>();

        public GameCommandList? CommandList { get; set; }
        public CommandParser? CommandParser { get; set; }

        /// <summary>Determines if console should try looking for attributes that can change log messages and colors.</summary>
        public bool UseLogModifierAttributes { get; set; } = true;

        /// <summary>Determines if it should include exceptions when logging normal errors with executing commands.</summary>
        public bool IncludeStackTraceInCommandExceptions { get; set; } = true;

        /// <summary>Determines if it should include exceptions when logging unknown errors with executing commands.</summary>
        public bool IncludeStackTraceInUnknownCommandExceptions { get; set; } = true;

        /// <summary>Can the console execute commands using <see cref="Execute(string)"/>.</summary>
        public bool CanParseAndExecute =>
            CommandList != null;

        /// <summary>Can the console execute commands using <see cref="Execute(string[])"/>.</summary>
        public bool CanExecute =>
            CommandList != null;

        /// <summary>Executes a command.</summary>
        /// <param name="cmd">Command text that will be parsed and executed.</param>
        public void Execute(string cmd)
        {
            if (CommandParser == null)
                throw new Exception("Cannot parse commands with no parser!");

            var args = CommandParser.ParseString(cmd);
            Execute(args);
        }

        /// <summary>Executes a command.</summary>
        /// <param name="args">Parsed arguments.</param>
        public void Execute(string[] args)
        {
            if (CommandList == null)
                throw new Exception("Cannot execute commands with no command list!");

            if (args.Length == 0)
                return;

            if (!CommandList.TryGetCommand(args[0], out IGameCommand? command))
            {
                LogError($"Command {args[0].ToLower()} doesn't exist");
                return;
            }

            try
            {
                command!.GameConsole = this;
                command!.Run(args);
            }
            catch (GameCommandException e)
            {
                LogError(e.ToString(IncludeStackTraceInCommandExceptions));
            }
            catch (Exception e)
            {
                LogError(IncludeStackTraceInUnknownCommandExceptions ?
                    $"There was an error while executing command '{args[0].ToLower()}': {e}" :
                    $"There was an error while executing command '{args[0].ToLower()}'.");
            }
        }

        /// <summary>Logs a message to the console.</summary>
        /// <param name="message">Message to log.</param>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        public void Log(string message, int stackTraceIndex = 2) =>
            Log(GameLog.CreateNow(message, qDebug.DEFAULT_COLOR_TAG), stackTraceIndex, false);

        /// <summary>Logs a warning message to the console.</summary>
        /// <param name="message">Message to log.</param>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        public void LogWarning(string message, int stackTraceIndex = 2) =>
            Log(GameLog.CreateNow(message, qDebug.WARNING_COLOR_TAG), stackTraceIndex);

        /// <summary>Logs an error message to the console.</summary>
        /// <param name="message">Message to log.</param>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        public void LogError(string message, int stackTraceIndex = 2) =>
            Log(GameLog.CreateNow(message, qDebug.ERROR_COLOR_TAG), stackTraceIndex);

        /// <summary>Logs a message to the console with a color.</summary>
        /// <param name="message">Message to log.</param>
        /// <param name="color">Message color.</param>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        public void Log(string message, Color color, int stackTraceIndex = 2) =>
            Log(GameLog.CreateNow(message, color), stackTraceIndex);

        /// <summary>Logs a message to the console with a color.</summary>
        /// <param name="message">Message to log.</param>
        /// <param name="colorTag">Message color.</param>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        public void Log(string message, string colorTag, int stackTraceIndex = 2) =>
            Log(GameLog.CreateNow(message, colorTag), stackTraceIndex);

        /// <summary>Logs a log to the console.</summary>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        /// <param name="overwriteColor">If true, the console will not check for color attributes.</param>
        public void Log(GameLog log, int stackTraceIndex = 2, bool overwriteColor = true)
        {
            if (UseLogModifierAttributes && !overwriteColor)
            {
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(stackTraceIndex);

                if (TryGetColorAttributeOfTrace(stackFrame, out var colorAttr))
                {
                    log.colorTag = colorAttr!.ColorTag;
                    log.color = colorAttr!.Color;
                }
            }

            Logs.Add(log);
            OnLog?.Invoke(log);
        }

        static bool TryGetColorAttributeOfTrace(StackFrame? frame, out LogColorAttribute? attribute)
        {
            attribute = null;
            if (frame == null) return false;

            var method = frame.GetMethod();
            var declaringType = method?.DeclaringType;
            var methodBody = method?.GetMethodBody();

            if (methodBody != null &&
                ConsoleReflections.ColorAttributeMethods.TryGetValue(ConsoleReflections.CreateMethodId(method), out var methodAttr))
            {
                attribute = methodAttr!;
                return true;
            }

            if (declaringType != null &&
                ConsoleReflections.ColorAttributeDeclaringTypes.TryGetValue(ConsoleReflections.CreateTypeId(declaringType), out var declaringTypeAttr))
            {
                attribute = declaringTypeAttr!;
                return true;
            }

            return false;
        }
    }
}