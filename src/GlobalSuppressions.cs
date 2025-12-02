// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// General suppressions for the entire assembly
[assembly: SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification = "Exception handling in middleware should be broad to ensure stability"
)]

[assembly: SuppressMessage(
    "Naming",
    "CA1707:Identifiers should not contain underscores",
    Justification = "Database column names use underscore convention",
    Scope = "member",
    Target = "~P:*"
)]

[assembly: SuppressMessage(
    "Performance",
    "CA1822:Mark members as static",
    Justification = "Service methods should remain instance methods for dependency injection patterns"
)]

[assembly: SuppressMessage(
    "Design",
    "CA1062:Validate arguments of public methods",
    Justification = "Validation is handled by middleware and model binding"
)]

// Test-specific suppressions
[assembly: SuppressMessage(
    "Performance",
    "CA1861:Avoid constant arrays as arguments",
    Justification = "Test data arrays are acceptable in test methods",
    Scope = "namespaceanddescendants",
    Target = "~N:App.Core.Tests"
)]

[assembly: SuppressMessage(
    "Reliability",
    "CA2007:Consider calling ConfigureAwait on the awaited task",
    Justification = "ConfigureAwait(false) not needed in test projects",
    Scope = "namespaceanddescendants",
    Target = "~N:App.Core.Tests"
)]

// Entity Framework specific suppressions
[assembly: SuppressMessage(
    "Microsoft.Usage",
    "CA2227:CollectionPropertiesShouldBeReadOnly",
    Justification = "EF navigation properties require setters for proper functionality"
)]

// ASP.NET Core specific suppressions
[assembly: SuppressMessage(
    "Design",
    "CA1040:Avoid empty interfaces",
    Justification = "Marker interfaces are used for service registration patterns"
)]

[assembly: SuppressMessage(
    "Design",
    "CA1034:Nested types should not be visible",
    Justification = "Nested DTOs are appropriate for API endpoint organization"
)]

// Security suppressions (with careful justification)
[assembly: SuppressMessage(
    "Security",
    "CA2100:Review SQL queries for security vulnerabilities",
    Justification = "EF Core parameterization handles SQL injection protection"
)]
