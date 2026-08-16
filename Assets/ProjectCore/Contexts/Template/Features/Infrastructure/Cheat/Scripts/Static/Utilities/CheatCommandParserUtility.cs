using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace ProjectCore.Template
{
    public static class CheatCommandParserUtility
    {
        public static Result<CheatCommandData> Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Result.Failure<CheatCommandData>(CheatErrors.InvalidInput("Cheat input is empty."));

            Result<IReadOnlyList<string>> tokenResult = Tokenize(input);
            if (tokenResult.IsFailure)
                return Result.Failure<CheatCommandData>(tokenResult.Errors);

            IReadOnlyList<string> tokens = tokenResult.Value;
            if (!CheatValidation.IsValidCommandName(tokens[0]))
            {
                return Result.Failure<CheatCommandData>(
                    CheatErrors.InvalidInput($"Invalid cheat command name '{tokens[0]}'."));
            }

            var arguments = new string[tokens.Count - 1];
            for (int i = 1; i < tokens.Count; i++)
                arguments[i - 1] = tokens[i];

            return Result.Success(new CheatCommandData(tokens[0], Array.AsReadOnly(arguments)));
        }

        private static Result<IReadOnlyList<string>> Tokenize(string input)
        {
            var tokens = new List<string>();
            var token = new StringBuilder();
            bool isInsideQuotes = false;
            bool isEscaped = false;
            bool isTokenStarted = false;

            foreach (char character in input)
            {
                if (isEscaped)
                {
                    token.Append(character);
                    isEscaped = false;
                    isTokenStarted = true;
                    continue;
                }

                if (character == '\\' && isInsideQuotes)
                {
                    isEscaped = true;
                    continue;
                }

                if (character == '"')
                {
                    isInsideQuotes = !isInsideQuotes;
                    isTokenStarted = true;
                    continue;
                }

                if (char.IsWhiteSpace(character) && !isInsideQuotes)
                {
                    if (isTokenStarted)
                    {
                        tokens.Add(token.ToString());
                        token.Clear();
                        isTokenStarted = false;
                    }

                    continue;
                }

                token.Append(character);
                isTokenStarted = true;
            }

            if (isInsideQuotes || isEscaped)
            {
                return Result.Failure<IReadOnlyList<string>>(
                    CheatErrors.InvalidInput("Cheat input contains an unterminated quoted argument."));
            }

            if (isTokenStarted)
                tokens.Add(token.ToString());

            return tokens.Count == 0
                ? Result.Failure<IReadOnlyList<string>>(CheatErrors.InvalidInput("Cheat input is empty."))
                : Result.Success<IReadOnlyList<string>>(tokens.AsReadOnly());
        }
    }
}
