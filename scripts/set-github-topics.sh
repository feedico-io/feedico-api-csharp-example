#!/usr/bin/env bash
# Set GitHub repository topics (requires GH_TOKEN or `gh auth login`).
set -euo pipefail

REPO="${1:-feedico-io/feedico-api-csharp-example}"
TOPICS='["feedico","coupon-api","affiliate-api","merchants","coupons","rest-api","csharp","dotnet","api-example","affiliate-marketing","publisher-api"]'

if command -v gh >/dev/null 2>&1 && gh auth status >/dev/null 2>&1; then
  gh api "repos/${REPO}/topics" -X PUT \
    -H "Accept: application/vnd.github+json" \
    -f "names=${TOPICS}"
  echo "Topics set on ${REPO} via gh."
  exit 0
fi

if [[ -z "${GH_TOKEN:-}" ]]; then
  echo "Set GH_TOKEN (repo scope) or run: gh auth login" >&2
  exit 1
fi

curl -fsSL -X PUT \
  -H "Authorization: Bearer ${GH_TOKEN}" \
  -H "Accept: application/vnd.github+json" \
  "https://api.github.com/repos/${REPO}/topics" \
  -d "{\"names\":${TOPICS}}"

echo "Topics set on ${REPO}."
