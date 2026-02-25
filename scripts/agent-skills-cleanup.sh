#!/usr/bin/env bash
set -euo pipefail

# Cleanup unnecessary skills from ~/.agent/skills/
# Keeps only skills relevant to .NET hexagonal architecture projects

SKILLS_DIR="$HOME/.agent/skills"

# Skills to keep for .NET hexagonal architecture project
KEEP_SKILLS=(
    "dotnet-architect"
    "dotnet-backend"
    "dotnet-backend-patterns"
    "csharp-pro"
    "domain-driven-design"
    "ddd-tactical-patterns"
    "ddd-strategic-design"
    "ddd-context-mapping"
    "architecture-patterns"
    "architecture"
    "api-design-principles"
    "api-security-best-practices"
    "api-security-testing"
    "database"
    "database-design"
    "database-optimizer"
    "database-migrations-sql-migrations"
    "datadog-automation"
    "clean-code"
    "code-review-excellence"
    "code-review-checklist"
    "tdd-workflow"
    "tdd-workflows-tdd-cycle"
    "tdd-workflows-tdd-green"
    "tdd-workflows-tdd-red"
    "tdd-workflows-tdd-refactor"
    "test-driven-development"
    "debugging-strategies"
    "debugging-toolkit-smart-debug"
    "security-audit"
    "security-auditor"
    "security"
    "security-scanning-security-dependencies"
    "security-scanning-security-hardening"
    "git-advanced-workflows"
    "docker-expert"
    "deployment-engineer"
    "deployment-procedures"
    "documentation"
    "sql-optimization-patterns"
    "sql-pro"
)

# Check if skills directory exists
if [[ ! -d "$SKILLS_DIR" ]]; then
    echo "Error: Skills directory not found at $SKILLS_DIR"
    exit 1
fi

echo "Starting cleanup of $SKILLS_DIR"
echo "=================================================="
echo ""

removed_count=0
kept_count=0

# Function to check if skill should be kept
should_keep() {
    local skill="$1"
    for keep in "${KEEP_SKILLS[@]}"; do
        if [[ "$skill" == "$keep" ]]; then
            return 0
        fi
    done
    return 1
}

# Process each directory in skills folder
cd "$SKILLS_DIR"
for dir in */; do
    # Remove trailing slash
    dir_name="${dir%/}"
    
    # Skip if it's a file or special directory
    if [[ ! -d "$dir_name" ]]; then
        continue
    fi
    
    # Check if directory should be kept
    if should_keep "$dir_name"; then
        echo "✓ Keep:   $dir_name"
        ((kept_count++))
    else
        echo "✗ Remove: $dir_name"
        rm -rf "$dir_name"
        ((removed_count++))
    fi
done

echo ""
echo "=================================================="
echo "Cleanup Complete!"
echo "  Kept:    $kept_count skills"
echo "  Removed: $removed_count skills"
echo "=================================================="
