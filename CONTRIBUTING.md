# Contributing to Hexagon .NET App

Thank you for your interest in contributing! We follow a structured approach to ensure code quality and architectural consistency.

## Getting Started

1.  **Prerequisites**: Install .NET 10 SDK and CSharpier.
2.  **Clone the Repo**:
    ```bash
    git clone https://github.com/hieu-nv/hexagon-dotnet-app.git
    cd hexagon-dotnet-app
    ```
3.  **Build**:
    ```bash
    dotnet build src/App.slnx
    ```

## Development Workflow

### Coding Standards
- We use **Hexagonal Architecture**. Business logic must live in `App.Core`.
- Infrastructure (Persistence, External Gateways) must live in `App.Data` and `App.Gateway`.
- Use **Minimal APIs** for all endpoints.
- Format your code using **CSharpier** before committing:
  ```bash
  csharpier format .
  ```

### Branching & Commits
- Use descriptive branch names: `feature/your-feature` or `fix/your-fix`.
- We follow **Conventional Commits**:
  - `feat`: New feature
  - `fix`: Bug fix
  - `docs`: Documentation changes
  - `test`: Adding or updating tests
  - `chore`: Maintenance tasks

### Testing
- All new features must include unit tests.
- Ensure solution-wide code coverage is above **80%**.
- Run tests with coverage:
  ```bash
  dotnet test src/App.slnx --collect:"XPlat Code Coverage"
  ```

## Pull Request Process

1.  Create a separate branch for each feature or fix.
2.  Ensure all tests pass and coverage requirements are met.
3.  Submit a Pull Request targeting the `main` or `develop` branch.
4.  A maintainer will review your changes for architectural consistency.
