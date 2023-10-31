namespace qASIC.Console
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; init; }
    }
}