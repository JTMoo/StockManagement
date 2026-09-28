#!/usr/bin/env bash
# Regenerates the ADR table in docs/decisions.md from docs/adr/*.md.
# Run after adding/renaming an ADR. CI fails if this changes anything (see Integration.yml).
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

adr_dir="docs/adr"
index_file="docs/decisions.md"
start_marker="<!-- ADR-INDEX:START -->"
end_marker="<!-- ADR-INDEX:END -->"

table=$(printf '| ADR | Decision | Status |\n|---|---|---|\n')

seen=""
for file in "$adr_dir"/[0-9][0-9][0-9][0-9]-*.md; do
	[ -e "$file" ] || continue
	base=$(basename "$file")
	number=${base%%-*}

	if [[ " $seen " == *" $number "* ]]; then
		echo "Duplicate ADR number: $number ($base)" >&2
		exit 1
	fi
	seen="$seen $number"

	title=$(sed -n "1s/^# ADR-$number: //p" "$file")
	status=$(sed -n 's/^- Status: //p' "$file" | head -1)

	if [ -z "$title" ] || [ -z "$status" ]; then
		echo "Malformed ADR (need '# ADR-$number: <title>' and '- Status: <status>'): $base" >&2
		exit 1
	fi

	table+=$(printf '\n| [%s](adr/%s) | %s | %s |' "$number" "$base" "$title" "$status")
done

awk -v start="$start_marker" -v end="$end_marker" -v table="$table" '
	$0 == start { print; print table; skipping = 1; next }
	$0 == end { skipping = 0 }
	skipping { next }
	{ print }
' "$index_file" > "$index_file.tmp"
mv "$index_file.tmp" "$index_file"
