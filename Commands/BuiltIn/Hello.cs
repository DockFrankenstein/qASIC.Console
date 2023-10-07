namespace qASIC.Console.Commands.BuiltIn
{
    public class Hello : GameCommand
    {
        public override string CommandName => "helloworld";
        public override string[] Aliases => new string[] { "hello" };

        public override void Run(string[] args)
        {
            GameCommandUtility.CheckArgumentCount(args, 0, 0);
            GameConsole?.Log("Hello world :)", Color.Green);
        }
    }
}
