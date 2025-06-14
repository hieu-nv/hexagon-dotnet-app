// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Generic type parameter naming rule
[assembly: SuppressMessage(
    "Style",
    "IDE1006:Naming Styles",
    Justification = "We use single-letter names for generic type parameters"
)]
