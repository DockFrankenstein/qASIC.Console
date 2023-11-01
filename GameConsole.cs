using GameLog = qASIC.Log;
using System.Diagnostics;
using qASIC.Console.Commands;
using System.Reflection;
using qASIC.Console.Parsing.Arguments;

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
        public ArgumentsParser? CommandParser { get; set; }

        public GameConsoleTheme Theme { get; set; } = GameConsoleTheme.Default;

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
        public void Execute(ConsoleArgument[] args)
        {
            if (CommandList == null)
                throw new Exception("Cannot execute commands with no command list!");

            if (args.Length == 0)
                return;

            if (!CommandList.TryGetCommand(args[0].arg, out IGameCommand? command))
            {
                LogError($"Command {args[0].arg.ToLower()} doesn't exist");
                return;
            }

            try
            {
                var commandArgs = new CommandArgs()
                {
                    args = args,
                    console = this,
                };

                var output = command!.Run(commandArgs);

                if (output != null)
                    Log($"Command returned '{output}'");
            }
            catch (GameCommandException e)
            {
                LogError(e.ToString(IncludeStackTraceInCommandExceptions));
            }
            catch (Exception e)
            {
                LogError(IncludeStackTraceInUnknownCommandExceptions ?
                    $"There was an error while executing command '{args[0].arg.ToLower()}': {e}" :
                    $"There was an error while executing command '{args[0].arg.ToLower()}'.");
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

                var method = stackFrame?.GetMethod();
                var declaringType = method?.DeclaringType;

                if (TryGetColorAttributeOfTrace(method, declaringType, out var colorAttr))
                {
                    log.colorTag = colorAttr!.ColorTag;
                    log.color = colorAttr!.Color;
                }

                if (TryGetPrefixAttributeOfTrace(method, declaringType, out var prefixAttr))
                {
                    log.message = prefixAttr!.FormatMessage(log.message);
                }
            }

            Logs.Add(log);
            OnLog?.Invoke(log);
        }

        public Color GetLogColor(GameLog log) =>
            Theme.GetLogColor(log);

        static bool TryGetPrefixAttributeOfTrace(MethodBase? method, Type? declaringType, out LogPrefixAttribute? attribute)
        {
            attribute = null;

            if (method != null &&
                ConsoleReflections.PrefixAttributeMethods.TryGetValue(ConsoleReflections.CreateMethodId(method), out var methodAttr))
            {
                attribute = methodAttr!;
                return true;
            }

            if (declaringType != null &&
                ConsoleReflections.PrefixAttributeDeclaringTypes.TryGetValue(ConsoleReflections.CreateTypeId(declaringType), out var declaringTypeAttr))
            {
                attribute = declaringTypeAttr!;
                return true;
            }

            return false;
        }

        static bool TryGetColorAttributeOfTrace(MethodBase? method, Type? declaringType, out LogColorAttribute? attribute)
        {
            attribute = null;

            if (method != null &&
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