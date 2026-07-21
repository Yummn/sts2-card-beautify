#!/usr/bin/env python3
"""Structural regression checks for encyclopedia-only CardBeautify selectors."""

from __future__ import annotations

import argparse
import datetime as dt
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT if (ROOT / "CardBeautifyCode").is_dir() else ROOT / "src"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--report", type=Path)
    parser.add_argument("--binary", type=Path, action="append", default=[])
    args = parser.parse_args()

    patch = (PROJECT / "CardBeautifyCode/CardNodePortraitPatch.cs").read_text(encoding="utf-8")
    watcher = (PROJECT / "CardBeautifyCode/CardBeautifyLibraryWatcher.cs").read_text(encoding="utf-8")
    manifest = (PROJECT / "CardBeautify.json").read_text(encoding="utf-8")

    checks = {
        "manifest is v0.5.1": '"version": "v0.5.1"' in manifest,
        "watcher searches only the current scene": "FindVisibleLibrary(tree?.CurrentScene)" in watcher and "FindVisibleLibrary(GetTree()?.Root)" not in watcher,
        "watcher tracks encyclopedia exit": "_wasInLibrary" in watcher,
        "watcher strips selectors from pooled cards": "CleanupAllSelectors(tree.Root)" in watcher,
        "selector requires exact library grid ownership": "ReferenceEquals(ownedGrid, grid)" in patch,
        "selector requires current-scene ownership": "IsUnderCurrentScene(library)" in patch and "tree.CurrentScene" in patch,
        "selector cleanup hides before deferred free": "selector.Visible = false" in patch and "selector.MouseFilter = Control.MouseFilterEnum.Ignore" in patch,
        "recursive selector cleanup exists": "internal static void CleanupAllSelectors(Node root)" in patch,
        "card-detail popup invalidates selector scope": "HasVisibleCardOutsideGrid(grid)" in patch and "ReferenceEquals(root, grid)" in patch and "root is NCard card && IsVisibleInTreeStrict(card)" in patch,
    }
    for binary in args.binary:
        checks[f"compiled binary exists: {binary}"] = binary.is_file() and binary.stat().st_size > 20_000

    passed = [name for name, ok in checks.items() if ok]
    failed = [name for name, ok in checks.items() if not ok]
    lines = [
        "CardBeautify v0.5.1 encyclopedia-scope offline audit",
        f"Timestamp: {dt.datetime.now().astimezone().isoformat(timespec='seconds')}",
        "Mode: source and binary checks only",
        f"Passed: {len(passed)}",
        f"Failed: {len(failed)}",
        "",
        "PASS",
        *[f"  [OK] {name}" for name in passed],
    ]
    if failed:
        lines += ["", "FAIL", *[f"  [FAIL] {name}" for name in failed]]
    report = "\n".join(lines) + "\n"
    print(report, end="")
    if args.report:
        args.report.parent.mkdir(parents=True, exist_ok=True)
        args.report.write_text(report, encoding="utf-8")
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())
