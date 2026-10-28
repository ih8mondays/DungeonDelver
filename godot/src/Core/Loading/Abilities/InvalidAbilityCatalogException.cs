using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonDelver.Core.Loading.Abilities;

/// Same idea as InvalidDungeonException: Problems = everything that's wrong, not just the first thing
public class InvalidAbilityCatalogException : Exception
{
    public IReadOnlyList<string> Problems { get; }

    public InvalidAbilityCatalogException(string source, IEnumerable<string> problems)
        : this(source, problems.ToList(), null)
    {
    }

    public InvalidAbilityCatalogException(string source, IEnumerable<string> problems, Exception inner)
        : this(source, problems.ToList(), inner)
    {
    }

    private InvalidAbilityCatalogException(string source, List<string> problems, Exception inner)
        : base($"Abilities '{source}' is invalid:\n - " + string.Join("\n - ", problems), inner)
    {
        Problems = problems;
    }
}
