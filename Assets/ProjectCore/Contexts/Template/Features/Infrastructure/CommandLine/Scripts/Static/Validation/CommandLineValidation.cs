namespace ProjectCore.Template
{
    public static class CommandLineValidation
    {
        public static bool IsValidKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)
                || key.Length <= 2
                || key[0] != '-'
                || key[1] != '-'
                || key[2] == '-')
            {
                return false;
            }

            for (var i = 2; i < key.Length; i++)
            {
                char character = key[i];
                if (!char.IsLetterOrDigit(character)
                    && character != '-'
                    && character != '_'
                    && character != '.')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
