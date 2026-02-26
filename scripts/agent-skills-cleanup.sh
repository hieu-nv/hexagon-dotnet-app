#!/usr/bin/env bash
set -euo pipefail

# Copy necessary skills from antigravity-awesome-skills to .github/skills
# Only copies skills relevant to .NET hexagonal architecture projects
#
# Usage: ./agent-skills-cleanup.sh [-f]
#   -f    Force copy, overwrite existing skills

SOURCE_SKILLS_DIR="../../antigravity-awesome-skills/skills"
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

# Check if source directory exists
if [[ ! -d "$SOURCE_SKILLS_DIR" ]]; then
    echo "Error: Source skills directory not found at $SOURCE_SKILLS_DIR"
    exit 1
fi

# Create destination directory if it doesn't exist
mkdir -p "$DEST_SKILLS_DIR"

echo "Copying skills from $SOURCE_SKILLS_DIR to $DEST_SKILLS_DIR"
echo "=================================================="
echo ""

copied_count=0
skipped_count=0
notfound_count=0

# Copy each skill
for skill in "${COPY_SKILLS[@]}"; do
    source_skill="$SOURCE_SKILLS_DIR/$skill"
    dest_skill="$DEST_SKILLS_DIR/$skill"
    
    if [[ ! -e "$source_skill" ]]; then
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
