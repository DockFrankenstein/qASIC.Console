﻿namespace qASIC.Console
{
    public class GameCommandException : Exception
    {
        public override string ToString()
        {
            return $"{Message}\n{StackTrace}";
        }

        public string ToString(bool includeStackTrace) =>
            includeStackTrace ?
            ToString() :
            Message;
    }
}