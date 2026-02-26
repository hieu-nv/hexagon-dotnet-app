#!/usr/bin/env bash
set -euo pipefail

# Copy necessary skills from awesome-copilot and antigravity-awesome-skills to .github/skills
# Only copies skills relevant to .NET hexagonal architecture projects
#
# Usage: ./agent-skills-cleanup.sh [-f]
#   -f    Force copy, overwrite existing skills

# Source directories (searched in order, first match wins)
SOURCE_SKILLS_DIRS=(
    "../../awesome-copilot/skills"
    "../../antigravity-awesome-skills/skills"
)
DEST_SKILLS_DIR="../.github/skills"
FORCE=false

# Parse arguments
while getopts "f" opt; do
    case $opt in
        f) FORCE=true ;;
        *) echo "Usage: $0 [-f]" >&2; exit 1 ;;
    esac
done

# Skills to copy for .NET hexagonal architecture project
COPY_SKILLS=(
    "dotnet-architect"
    "dotnet-backend"
    "dotnet-backend-patterns"
    "dotnet-best-practices"
    "dotnet-design-pattern-review"
    "dotnet-upgrade"
    "csharp-pro"
    "csharp-docs"
    "csharp-async"
    "csharp-xunit"
    "csharp-mstest"
    "ef-core"
    "domain-driven-design"
    "ddd-tactical-patterns"
    "ddd-strategic-design"
    "ddd-context-mapping"
    "architecture-patterns"
    "architecture"
    "architecture-blueprint-generator"
    "api-design-principles"
    "api-security-best-practices"
    "api-security-testing"
    "aspnet-minimal-api-openapi"
    "aspire"
    "database"
    "database-design"
    "database-optimizer"
    "database-migrations-sql-migrations"
    "cosmosdb-datamodeling"
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
    "git-commit"
    "git-flow-branch-creator"
    "docker-expert"
    "multi-stage-dockerfile"
    "containerize-aspnetcore"
    "deployment-engineer"
    "deployment-procedures"
    "documentation"
    "documentation-writer"
    "create-readme"
    "create-specification"
    "create-implementation-plan"
    "create-architectural-decision-record"
    "create-technical-spike"
    "sql-optimization-patterns"
    "sql-pro"
    "postgresql-code-review"
    "postgresql-optimization"
    "nuget-manager"
    "editorconfig"
    "conventional-commit"
    "refactor"
    "review-and-refactor"
)

# Check if at least one source directory exists
found_source=false
for source_dir in "${SOURCE_SKILLS_DIRS[@]}"; do
    if [[ -d "$source_dir" ]]; then
        found_source=true
        break
    fi
done

if [[ "$found_source" == false ]]; then
    echo "Error: No source skills directories found:"
    for source_dir in "${SOURCE_SKILLS_DIRS[@]}"; do
        echo "  - $source_dir"
    done
    exit 1
fi

# Create destination directory if it doesn't exist
mkdir -p "$DEST_SKILLS_DIR"

echo "Copying skills to $DEST_SKILLS_DIR"
echo "Source directories:"
for source_dir in "${SOURCE_SKILLS_DIRS[@]}"; do
    echo "  - $source_dir"
done
echo "=================================================="
echo ""

copied_count=0
skipped_count=0
notfound_count=0

# Copy each skill
for skill in "${COPY_SKILLS[@]}"; do
    dest_skill="$DEST_SKILLS_DIR/$skill"
    source_skill=""
    
    # Find skill in source directories
    for source_dir in "${SOURCE_SKILLS_DIRS[@]}"; do
        if [[ -e "$source_dir/$skill" ]]; then
            source_skill="$source_dir/$skill"
            break
        fi
    done
    
    if [[ -z "$source_skill" ]]; then
        echo "✗ Not found: $skill"
        ((notfound_count++))
        continue
    fi
    
    # If destination already exists and not forcing, skip
    if [[ -e "$dest_skill" && "$FORCE" == false ]]; then
        echo "⊘ Skip:     $skill (already exists)"
        ((skipped_count++))
        continue
    fi
    
    # If forcing and destination exists, remove it first
    if [[ -e "$dest_skill" && "$FORCE" == true ]]; then
        rm -rf "$dest_skill"
    fi
    
    # Copy the skill (file or directory)
    cp -r "$source_skill" "$dest_skill"
    echo "✓ Copy:     $skill"
    ((copied_count++))
done

echo ""
echo "=================================================="
echo "Copy Complete!"
echo "  Copied:   $copied_count skills"
echo "  Skipped:  $skipped_count skills"
echo "  Not found: $notfound_count skills"
echo "=================================================="
