#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DIAGRAM_DIR="$ROOT_DIR/docs/reports/assets/diagrams"
CONFIG="$DIAGRAM_DIR/mermaid-config.json"
CLI_VERSION="11.4.2"

find "$DIAGRAM_DIR" -maxdepth 1 -type f -name '*.mmd' | sort | while IFS= read -r source; do
  base="${source%.mmd}"
  echo "Rendering ${base#$ROOT_DIR/}.png"
  npx -y "@mermaid-js/mermaid-cli@$CLI_VERSION" -i "$source" -o "$base.png" -c "$CONFIG" -b white --scale 2
  echo "Rendering ${base#$ROOT_DIR/}.svg"
  npx -y "@mermaid-js/mermaid-cli@$CLI_VERSION" -i "$source" -o "$base.svg" -c "$CONFIG" -b white
done
