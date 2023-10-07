namespace qASIC.Console.Commands
{
    public abstract class GameCommand : IGameCommand
    {
        public abstract string CommandName { get; }

        public virtual string[] Aliases => new string[0];

        public virtual string? Description => null;

        public virtual string? DetailedDescription => null;

        public GameConsole? GameConsole { get; set; }

        public abstract void Run(string[] args);
    }
}