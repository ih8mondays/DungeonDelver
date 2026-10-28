using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonDelver.Core.Loading.Dungeon;

/// Problems = everything that's wrong, not just the first thing
public class InvalidDungeonException : Exception
{
    public IReadOnlyList<string> Problems { get; }

    // the public constructors just pass on to the private one.
    public InvalidDungeonException(string source, IEnumerable<string> problems)
        : this(source, problems.ToList(), null)
    {
    }

    public InvalidDungeonException(string source, IEnumerable<string> problems, Exception inner)
        : this(source, problems.ToList(), inner)
    {
    }

    // builds the message: "Map 'x' is invalid:" + one line per problem
    private InvalidDungeonException(string source, List<string> problems, Exception inner)
        : base($"Map '{source}' is invalid:\n - " + string.Join("\n - ", problems), inner)
    {
        Problems = problems;
    }
}
