#!/bin/bash
# Adds NHSOds.MCP to Claude Desktop's MCP server config.
# Run once from anywhere: bash add-to-claude.sh

set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG="$HOME/Library/Application Support/Claude/claude_desktop_config.json"

# Check dotnet is available
if ! command -v dotnet &>/dev/null; then
  echo "❌  dotnet not found. Install .NET 9 SDK from https://dotnet.microsoft.com/download"
  exit 1
fi

echo "✅  dotnet $(dotnet --version) found"
echo "📁  Project: $PROJECT_DIR"
echo "📄  Config:  $CONFIG"

# Create config file if it doesn't exist yet
if [ ! -f "$CONFIG" ]; then
  mkdir -p "$(dirname "$CONFIG")"
  echo '{}' > "$CONFIG"
  echo "📝  Created new config file"
fi

# Inject the server entry using Python (ships with macOS)
python3 - <<PYTHON
import json, sys

config_path = """$CONFIG"""
project_dir = """$PROJECT_DIR"""

with open(config_path) as f:
    config = json.load(f)

config.setdefault("mcpServers", {})

config["mcpServers"]["nhs-ods"] = {
    "command": "dotnet",
    "args": ["run", "--project", project_dir, "--no-launch-profile"],
    "env": {}
}

with open(config_path, "w") as f:
    json.dump(config, f, indent=2)

print("✅  nhs-ods server added to claude_desktop_config.json")
PYTHON

echo ""
echo "👉  Restart Claude Desktop for the change to take effect."
echo "    Then try: 'Find GP practices in Leeds'"
