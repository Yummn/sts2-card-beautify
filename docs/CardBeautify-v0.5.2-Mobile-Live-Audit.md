# CardBeautify v0.5.2 Android live audit

- Date: 2026-07-24
- Device: REDMI K80 Pro (`e02b65b6`)
- Game: Android v0.103.2
- Result: PASS

## Validation

1. Cold start loaded CardBeautify v0.5.2.
2. The card encyclopedia displayed card-art selectors normally (`cards=25`, `eligible=18`, `selectors=18`).
3. Leaving the encyclopedia logged synchronous pooled-card selector cleanup.
4. Continuing the existing combat after visiting the encyclopedia showed no card-art selector on any hand card.
5. Card portraits remained replaced; only the editing controls were removed outside the encyclopedia.

Evidence is retained locally under `live-fixed-20260724/`.

