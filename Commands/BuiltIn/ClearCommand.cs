﻿namespace qASIC.Console.Commands.BuiltIn
{
    [StandardConsoleCommand]
    public class ClearCommand : GameCommand
    {
        public override string CommandName => "clear";
        public override string? Description => "Clears the console.";
        public override string[] Aliases => new string[] { "cls", "clr" };

        public override object? Run(CommandArgs args)
        {
            args.CheckArgumentCount(0);
            args.console.Log(Log.CreateNow(string.Empty, LogType.Clear, Color.Clear));
            return null;
        }
    }
}