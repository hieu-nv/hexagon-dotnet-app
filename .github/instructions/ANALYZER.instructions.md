# Code Analysis Configuration

This document describes the comprehensive code analysis and quality configuration for the Hexagon .NET application.

## Overview

The project is configured with multiple layers of static code analysis to ensure high code quality, security, and maintainability:

- **Microsoft .NET Analyzers** - Built-in .NET code quality and security rules
- **SonarAnalyzer for C#** - Industry-standard code quality and security analysis
- **Microsoft BannedAPI Analyzers** - Prevents usage of deprecated or dangerous APIs
- **AsyncUsage Analyzers** - Ensures proper async/await patterns
- **Security Code Scan** - Additional security vulnerability detection

## Configuration Files

### 1. Directory.Build.props

- **Location**: `src/Directory.Build.props`
- **Purpose**: Central configuration for all projects
- **Features**:
  - Enables all analyzer packages
  - Sets analysis levels and modes
  - Configures nullable reference types
  - Central analyzer package management

### 2. .editorconfig

- **Location**: `.editorconfig`
- **Purpose**: Code style and formatting rules
- **Features**:
  - C# coding conventions
  - Naming conventions
  - Formatting preferences
  - Specific analyzer rule configurations

### 3. Global Configuration

- **Location**: `.globalconfig`
- **Purpose**: Modern analyzer rule configuration using EditorConfig format
- **Features**:
  - Security rules set to Error level
  - Performance rules set to Warning level
  - Code quality rules categorized by severity
  - Enhanced tooling support and IDE integration

### 4. GlobalSuppressions.cs

- **Location**: `src/GlobalSuppressions.cs`
- **Purpose**: Assembly-level suppressions with justifications
- **Features**:
  - Documented suppressions for legitimate cases
  - Test-specific suppressions
  - Framework-specific suppressions

### 5. SonarQube Configuration

- **Location**: `sonar-project.properties`
- **Purpose**: SonarCloud/SonarQube analysis configuration
- **Features**:
  - Project metadata
  - Coverage reporting configuration
  - Exclusion patterns

## Analyzer Packages

| Package                                    | Version       | Purpose                          |
| ------------------------------------------ | ------------- | -------------------------------- |
| Microsoft.CodeAnalysis.NetAnalyzers        | 10.0.100      | Core .NET analyzers              |
| SonarAnalyzer.CSharp                       | 10.5.0.116594 | Code quality & security          |
| Microsoft.CodeAnalysis.BannedApiAnalyzers  | 3.11.0        | API usage restrictions           |
| Microsoft.VisualStudio.Threading.Analyzers | 17.12.19      | Threading & async best practices |
| SecurityCodeScan.VS2019                    | 5.6.7         | Security vulnerability detection |

## Rule Categories

### Security Rules (Error Level)

- **S2068**: Credentials should not be hard-coded
- **S2092**: Cookies should be secure
- **S4426**: Cryptographic keys should be robust
- **S4784**: Regular expression usage security
- **S5332**: Clear-text protocol usage
- **CA2100**: SQL injection vulnerabilities
- **CA2300+**: Insecure deserialization patterns

### Performance Rules (Warning Level)

- **CA1304-1310**: Culture and string comparison
- **CA1825-1847**: Memory and performance optimizations
- **S3267**: LINQ expression simplification
- **S1643**: String concatenation optimization

### Code Quality Rules (Warning Level)

- **S1144**: Unused code removal
- **S1186**: Empty method detection
- **S2292**: Auto-property preferences
- **S3776**: Cognitive complexity monitoring
- **S3168**: Async void detection

## CI/CD Integration

### GitHub Actions Workflow

- **Location**: `.github/workflows/code-analysis.yml`
- **Features**:
  - Static analysis with CodeQL
  - Security scanning
  - SonarCloud integration
  - Quality gate enforcement

### CodeQL Configuration

- **Location**: `.github/codeql/codeql-config.yml`
- **Features**:
  - Security-focused queries
  - Path exclusions for generated code
  - Custom query filters

## Usage

### Local Development

1. **Build with Analysis**:

   ```bash
   dotnet build src/App.sln --verbosity normal
   ```

2. **Run Tests with Coverage**:

   ```bash
   dotnet test src/App.sln --collect:"XPlat Code Coverage"
   ```

3. **Format Code**:
   ```bash
   csharpier format .
   ```

### Analyzing Results

#### Build Output

- Analyzer warnings appear in build output
- Categorized by severity level
- Include links to rule documentation

#### IDE Integration

- Real-time analysis in Visual Studio/VS Code
- IntelliSense integration
- Quick fix suggestions

#### CI/CD Reports

- SARIF files for security analysis
- Coverage reports in multiple formats
- Quality gate status in pull requests

## Customization

### Adding New Rules

1. Update `.globalconfig` with new rule configuration
2. Add specific rule configurations to `.editorconfig` if needed
3. Document suppressions in `GlobalSuppressions.cs` with justification

### Excluding Files or Patterns

1. Add patterns to `.editorconfig` sections
2. Update `sonar-project.properties` exclusions
3. Modify CodeQL configuration filters

### Severity Adjustments

1. Edit rule severity in `.globalconfig`
2. Add specific overrides in `.editorconfig`
3. Use `#pragma warning` for method-level suppressions

## Best Practices

1. **Review analyzer warnings regularly** - Address warnings promptly to maintain code quality
2. **Document suppressions** - Always provide clear justifications for suppressed rules
3. **Test rule changes** - Verify analyzer configuration changes don't break builds
4. **Monitor quality metrics** - Track trends in code coverage and quality scores
5. **Keep analyzers updated** - Regularly update analyzer packages for latest rules

## Troubleshooting

### Common Issues

1. **Too many warnings**: Adjust rule severity levels in `.globalconfig`
2. **Performance impact**: Enable/disable specific analyzers in Directory.Build.props
3. **False positives**: Add targeted suppressions with documentation
4. **CI/CD failures**: Check quality gate thresholds and exclusion patterns

### Useful Commands

```bash
# View all analyzer warnings
dotnet build --verbosity detailed 2>&1 | grep warning

# Disable specific analyzer for project
dotnet_diagnostic.RULE_ID.severity = none

# Run analysis without build
msbuild /p:RunCodeAnalysis=true
```

## Resources

- [.NET Code Analysis Rules](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-quality-rules/)
- [SonarAnalyzer Rules](https://rules.sonarsource.com/csharp)
- [EditorConfig Reference](https://editorconfig.org/)
- [Security Code Scan Rules](https://security-code-scan.github.io/#rules)
