namespace qASIC.Console.Commands
{
    public class GameCommandList
    {
        private List<RegisteredCommand> Commands { get; set; } = new List<RegisteredCommand>();

        public GameCommandList AddCommand(IGameCommand command)
        {
            var registeredCommand = new RegisteredCommand(command);
            Commands.Add(registeredCommand);
            return this;
        }

        /// <summary>Tries to find command</summary>
        /// <param name="commandName">Name of the command, doesn't need to be lowercase</param>
        /// <param name="command">Found command</param>
        /// <returns>Returns if it found a command</returns>
        public bool TryGetCommand(string commandName, out IGameCommand? command)
        {
            commandName = commandName.ToLower();

            var targets = Commands
                .Where(x => x.names.Contains(commandName))
                .Select(x => x.command);

            command = targets.FirstOrDefault();
            return command != null;
        }

        class RegisteredCommand
        {
            public RegisteredCommand(IGameCommand command)
            {
                this.command = command;

                names = command.Aliases
                    .Prepend(command.CommandName)
                    .Select(x => x.ToLower())
                    .ToArray();
            }

            public string[] names;
            public IGameCommand command;
        }
    }
}