namespace qASIC.Console.Parsers
{
    public class QuashParser : CommandParser
    {
        public override string[] ParseString(string cmd)
        {
            List<string> args = new List<string>();

            bool isAdvanced = false;
            string currentString = "";

            cmd = cmd.Trim();

            for (int i = 0; i < cmd.Length; i++)
            {
                if (isAdvanced)
                {
                    if (cmd[i] == '"' && (cmd.Length <= i + 1 || cmd[i + 1] == ' '))
                    {
                        isAdvanced = false;
                        continue;
                    }

                    currentString += cmd[i];
                    continue;
                }

                if (cmd[i] == ' ')
                {
                    args.Add(currentString);
                    currentString = "";
                    continue;
                }

                if (cmd[i] == '"' && (i != 0 && cmd[i - 1] == ' ' || i == 0) && currentString == "")
                {
                    isAdvanced = true;
                    continue;
                }

                currentString += cmd[i];
            }

            args.Add(currentString);
            return args.ToArray();
        }
    }
}