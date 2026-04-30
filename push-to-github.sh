#!/bin/bash
# Creates the GitHub repo and pushes the project.
# Requires: gh CLI (https://cli.github.com) — install with: brew install gh
# Run from anywhere: bash push-to-github.sh

set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_NAME="NHSOds.MCP"

echo "📁  Project: $PROJECT_DIR"

# Check gh is available
if ! command -v gh &>/dev/null; then
  echo ""
  echo "❌  gh CLI not found. Install it with:"
  echo "    brew install gh"
  echo "    gh auth login"
  exit 1
fi

# Check gh is authenticated
if ! gh auth status &>/dev/null; then
  echo ""
  echo "❌  Not authenticated. Run:  gh auth login"
  exit 1
fi

echo "✅  gh CLI authenticated as $(gh api user --jq .login)"

cd "$PROJECT_DIR"

# Initialise git if not already done
if [ ! -d ".git" ]; then
  git init
  echo "📝  Git repository initialised"
fi

# Stage everything (respects .gitignore)
git add .
git status --short

# Initial commit
if git diff --cached --quiet; then
  echo "ℹ️   Nothing new to commit"
else
  git commit -m "feat: initial NHSOds.MCP server

C# .NET 9 MCP server wrapping the NHS Organisation Data Service.
Exposes GP practices, pharmacies, and NHS trusts as agentic tools
consumable by Claude Code, Claude Desktop, or any MCP-compatible client.

Tools: FindGpPractice, FindPharmacy, FindNhsTrust, GetOrganisation
Transport: stdio (Streamable HTTP ready via ModelContextProtocol.AspNetCore)
Auth: none — ODS API is fully open
CI: GitHub Actions — build on push, NuGet publish on release"
fi

# Create public GitHub repo (errors gracefully if it already exists)
echo ""
echo "🚀  Creating GitHub repository '$REPO_NAME'..."
gh repo create "$REPO_NAME" \
  --public \
  --description "C# .NET 9 MCP server wrapping the NHS Organisation Data Service — GP practices, pharmacies, and NHS trusts as agentic tools. No API key required." \
  --source . \
  --remote origin \
  --push 2>/dev/null || {
    echo "ℹ️   Repo may already exist — attempting push to existing remote..."
    git push -u origin main 2>/dev/null || git push -u origin master
  }

echo ""
echo "✅  Done! Your repo:"
gh repo view --web 2>/dev/null || echo "    https://github.com/$(gh api user --jq .login)/$REPO_NAME"
