#!/bin/bash
# Installs what a cloud session needs to actually build/test this repo instead of only
# trusting CI (see docs/adr, "threads start faster"). Web-session only.
set -euo pipefail

if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
	exit 0
fi

cd "$CLAUDE_PROJECT_DIR"

if ! command -v dotnet >/dev/null 2>&1; then
	sudo apt-get update -qq || true
	sudo apt-get install -y -qq dotnet-sdk-8.0
fi

dotnet restore StockManagement.sln

if ! pg_isready -h 127.0.0.1 -p 5432 >/dev/null 2>&1; then
	sudo service postgresql start
fi
sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'postgres';" >/dev/null

(cd StockManagement.Web && npm install)
