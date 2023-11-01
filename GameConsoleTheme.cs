namespace qASIC.Console
{
    public class GameConsoleTheme
    {
        public static GameConsoleTheme Default =>
            new GameConsoleTheme();

        public Color defaultColor = Color.White;
        public Color warningColor = Color.Yellow;
        public Color errorColor = Color.Red;

        public Dictionary<string, Color> customColors = new Dictionary<string, Color>();

        public Color GetLogColor(Log log)
        {
            switch (log.colorTag)
            {
                case null:
                    return log.color;
                case qDebug.DEFAULT_COLOR_TAG:
                    return defaultColor;
                case qDebug.WARNING_COLOR_TAG:
                    return warningColor;
                case qDebug.ERROR_COLOR_TAG:
                    return errorColor;
                default:
                    if (customColors.ContainsKey(log.colorTag))
                        return customColors[log.colorTag];

                    break;
            }

            return defaultColor;
        }
    }
}