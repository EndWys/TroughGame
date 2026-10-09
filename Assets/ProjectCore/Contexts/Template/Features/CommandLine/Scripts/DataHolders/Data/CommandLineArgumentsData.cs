using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProjectCore.Template
{
    public sealed class CommandLineArgumentsData
    {
        internal CommandLineArgumentsData(
            Dictionary<string, List<string>> values,
            HashSet<string> flags,
            List<string> positionalArguments)
        {
            var readOnlyValues = new Dictionary<string, IReadOnlyList<string>>(
                values.Count,
                StringComparer.Ordinal);

            foreach (KeyValuePair<string, List<string>> pair in values)
            {
                readOnlyValues.Add(
                    pair.Key,
                    Array.AsReadOnly(pair.Value.ToArray()));
            }

            Values = new ReadOnlyDictionary<string, IReadOnlyList<string>>(readOnlyValues);
            Flags = Array.AsReadOnly(new List<string>(flags).ToArray());
            PositionalArguments = Array.AsReadOnly(positionalArguments.ToArray());
        }

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Values { get; }
        public IReadOnlyList<string> Flags { get; }
        public IReadOnlyList<string> PositionalArguments { get; }
    }
}
