# Grid and checkerboard controls

Branch: grid-checkerboard-controls, based on 4e14bb4.

Only 1.7 definitions are changed. Custom grid tools now belong directly to Grids and Grids_Inverse; redundant groups are removed. Checkerboards use showSizeInputs (not useSizeInputs), so drawing remains draggable and W/H size individual tiles, not the footprint. Both phases share tile sizes, defaulting to 1x1, independently of the existing 6x6 grid/fixed-size values. Anchoring, the 512-cell drag cap, and thickness behavior are preserved.

## Integrating with workshop-reports

The only ShapeControlsWindow.cs integration hunk replaces the inputWidth/inputHeight field declaration with separate grid/checker storage, ref accessors selecting storage by the active custom/native tool, and CheckerTileSize. Keep the other branch's drag hitbox and keyboard/Alt changes. All existing DrawSizeInputs validation, buffers, Tab navigation and custom/native layout code are reused unchanged. No drag-handling edits are included.

Build output was directed to build-validation to avoid changing tracked assemblies. Rebuild the 1.7 assembly during integration. The installed mod and all 1.6 files were left untouched. No in-game GUI validation was performed.

Validation: GeometryRegression, FeatureRegression, FloodFillRegression; full Release build. Build has the existing RuntimeStringInterpolator SYSLIB0051 warning.
