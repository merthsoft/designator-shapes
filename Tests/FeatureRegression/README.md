# Approved-feature regression checks

Build the production 1.7 DLL with `./build-1.7.ps1`, then run:

`dotnet run --project Tests/FeatureRegression/FeatureRegression.csproj`

Requires the installed RimWorld 1.7 assemblies and the sibling flood-fill-shape 1.7 assembly. The checks exercise the actual production DLL, actual game designator types, standalone worker identity, bounded memory storage, drawable/action filtering, once-only transaction callbacks, circular freeform brush sampling and production lifecycle-reset callbacks. They do not open a game, perform GUI input, simulate a save round trip, or prove dialog focus/selection behavior in a running game. Those remain coordinator regression checks.

Freeform contributor history: Jacob Brandt (Jacbo), original freeform commit be5b5ae and later fixes eae0b4c/a53be75. The current engine preserves that feature's variable-width round-brush behavior while replacing its problematic mask bounds/path bookkeeping. Existing About.xml credit remains intact.

The geometry suite also compares actual standalone Grids source at `C:\code\rimworld\grid-shape` with integrated Primitives.Grid/InverseGrid (override StandaloneGridsDir if needed). No standalone repository is modified.
