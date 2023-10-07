namespace qASIC.Console.Commands
{
    public static class GameCommandUtility
    {
        public static void CheckArgumentCount(string[] args, int min, int max) =>
            CheckArgumentCount(args.Length, min, max);

        public static void CheckArgumentCount(int argsCount, int min, int max)
        {
            argsCount--;
            bool valid = min <= argsCount && argsCount <= max;

            if (!valid)
                throw new CommandArgsCountException(argsCount, min, max);
        }
    }
}