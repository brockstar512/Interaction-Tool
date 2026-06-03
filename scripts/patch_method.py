#!/usr/bin/env python3
"""
patch_method.py  <file>  '<signature-anchor>'   (new method body on stdin)

Replaces a whole C# method with a new version, robustly:
  - finds the UNIQUE line containing the signature anchor
  - brace-matches from that method's opening { to its closing }
  - replaces signature-line..closing-brace with the new method text (stdin)
Aborts (non-zero, no write) if the anchor is missing, not unique, or braces
don't balance. Safe to re-run: the method signature (the anchor) survives
patching, so re-running simply replaces the already-wrapped method with the
identical version again (idempotent) — verified to keep braces balanced and
not double-wrap.
"""
import sys

def die(msg):
    sys.stderr.write("  ABORT: " + msg + "\n")
    sys.exit(1)

if len(sys.argv) != 3:
    die("usage: patch_method.py <file> '<signature-anchor>'  (new body on stdin)")

path, anchor = sys.argv[1], sys.argv[2]
new_method = sys.stdin.read().rstrip("\n")

try:
    src = open(path, "r", encoding="utf-8").read()
except FileNotFoundError:
    die("file not found: " + path)

i = src.find(anchor)
if i == -1:
    die("signature not found (already patched, or file differs): " + anchor)
if src.find(anchor, i + len(anchor)) != -1:
    die("signature is not unique in file: " + anchor)

# opening brace of the method
b = src.find("{", i)
if b == -1:
    die("no opening brace after signature: " + anchor)

depth = 0
j = b
end = -1
while j < len(src):
    c = src[j]
    if c == "{":
        depth += 1
    elif c == "}":
        depth -= 1
        if depth == 0:
            end = j
            break
    j += 1
if end == -1:
    die("could not brace-match method: " + anchor)

line_start = src.rfind("\n", 0, i) + 1   # start of the signature line (keep its indent in new_method)
new_src = src[:line_start] + new_method + "\n" + src[end + 1:]
open(path, "w", encoding="utf-8").write(new_src)
print("  patched: " + anchor + "  in  " + path)
