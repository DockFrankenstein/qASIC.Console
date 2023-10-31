using System.Reflection;

namespace qASIC.Console.Commands
{
    public class GameAttributeCommand : IGameCommand
    {
        public GameAttributeCommand() : this(string.Empty) { }
        public GameAttributeCommand(string commandName)
        {
            CommandName = commandName;
        }

        public string CommandName { get; set; }

        public string[] Aliases { get; set; } = new string[0];

        public string? Description { get; set; }

        public string? DetailedDescription { get; set; }

        public List<MethodTarget> Targets { get; set; } = new List<MethodTarget>();

        public object? Run(CommandArgs args)
        {
            var maxArgLimit = Targets
                .Select(x => x.maxArgsCount)
                .Max();

            var minArgLimit = Targets
                .Select(x => x.minArgsCount)
                .Min();

            args.CheckArgumentCount(minArgLimit, maxArgLimit);

            ConsoleArgument[] cmdArgs = args.args
                .Skip(1)
                .ToArray();

            var targets = Targets
                .Where(x => cmdArgs.Length >= x.minArgsCount && cmdArgs.Length <= x.maxArgsCount)
                .ToArray();

            List<Type>[] supportedArgTypes = new List<Type>[maxArgLimit]
                .Select(x => new List<Type>())
                .ToArray();

            foreach (var target in targets)
                for (int i = 0; i < target.argTypes.Length; i++)
                    supportedArgTypes[i].Add(target.argTypes[i]);

            for (int i = 0; i < cmdArgs.Length; i++)
                cmdArgs[i].parsedValues = cmdArgs[i].parsedValues
                    .Where(x => supportedArgTypes[i].Contains(x.GetType()) || x is string)
                    .ToArray();

            object? returnValue = null;

            int closestMatchCorrectArgsCount = -1;
            MethodTarget? closestMatch = null;
            
            if (FindCommandAndTryRun(new List<object>()))
                return returnValue;

            throw new CommandParseException(closestMatch!.Value.argTypes[closestMatchCorrectArgsCount], args[closestMatchCorrectArgsCount + 1].arg);

            bool FindCommandAndTryRun(List<object> values, bool first = true)
            {
                if (values.Count < cmdArgs.Length)
                {
                    var index = values.Count;
                    values.Add(new object());
                    foreach (var value in cmdArgs[index].parsedValues)
                    {
                        values[index] = value;
                        FindCommandAndTryRun(values, false);

                        if (first && RunFromValues(values))
                            return true;
                    }
                }

                if (cmdArgs.Length == 0 && first && RunFromValues(new List<object>()))
                    return true;

                return false;
            }

            bool RunFromValues(List<object> values)
            {
                var valueTypes = values
                    .Select(x => x.GetType())
                    .ToArray();

                foreach (var target in targets)
                {
                    var finalValues = values;

                    var targetArgTypes = new Type[valueTypes.Length];
                    Array.Copy(target.argTypes, targetArgTypes, targetArgTypes.Length);

                    if (target.forwardCommandArgs)
                        finalValues.Insert(0, args);

                    var parameterCount = target.methodInfo.GetParameters().Length;

                    while (finalValues.Count() < parameterCount)
                        finalValues.Add(Type.Missing);

                    int argCount = 0;
                    for (; argCount < valueTypes.Length; argCount++)
                        if (valueTypes[argCount] != targetArgTypes[argCount]) 
                            break;

                    if (argCount > closestMatchCorrectArgsCount)
                    {
                        closestMatch = target;
                        closestMatchCorrectArgsCount = argCount;
                    }

                    if (argCount != valueTypes.Length)
                        continue;

                    returnValue = ExecuteTarget(target, finalValues.ToArray());
                    return true;
                }

                return false;
            }
        }

        object? ExecuteTarget(MethodTarget target, object[] values)
        {
            if (target.methodInfo.IsStatic)
                return target.methodInfo.Invoke(null, values);

            return null;
        }

        public struct MethodTarget
        {
            public MethodTarget(MethodInfo methodInfo)
            {
                this.methodInfo = methodInfo;
                var parameters = methodInfo.GetParameters();

                forwardCommandArgs = parameters.Length > 0 && parameters[0].ParameterType == typeof(CommandArgs);

                if (forwardCommandArgs)
                    parameters = parameters
                        .Skip(1)
                        .ToArray();

                minArgsCount = parameters
                    .Where(x => !x.IsOptional)
                    .Count();

                maxArgsCount = parameters.Length;

                argTypes = parameters
                    .Select(x => x.ParameterType)
                    .ToArray();
            }

            public MethodInfo methodInfo;
            public Type[] argTypes;
            public int minArgsCount;
            public int maxArgsCount;
            public bool forwardCommandArgs;
        }
    }
}