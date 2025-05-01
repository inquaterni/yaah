#!/usr/bin/env bash
set -euo pipefail

# If the build user doesn't exist, create it and re-exec the script under that user
if [ "$(id -u)" -eq 0 ]; then
  useradd --create-home builder
  chown -R builder:builder "$(pwd)"
  exec su builder -c "$0"
fi

makepkg --printsrcinfo >.SRCINFO
makepkg --noconfirm -si
