namespace FolderRenameAssist.Class
{
    public static class Utilities
    {
        public static string Prepare_Keyword(string input)
        {
            if (!input.StartsWith("["))
                input = "[" + input;
            if (!input.EndsWith("]"))
                input = input + "]";
            return input;
        }
    }
}
