namespace qASIC.Console.Commands.BuiltIn
{
    public class ClearCommand : GameCommand
    {
        public override string CommandName => "clear";
        public override string[] Aliases => new string[] { "cls", "clr" };

        public override void Run(string[] args)
        {
            GameCommandUtility.CheckArgumentCount(args, 0, 0);
            GameConsole?.Log(Log.CreateNow(string.Empty, LogType.Clear, Color.Clear));
        }
    }
}