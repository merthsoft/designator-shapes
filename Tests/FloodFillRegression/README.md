# Flood-fill regression checks

Run `dotnet run --project Tests/FloodFillRegression/FloodFillRegression.csproj`.

This isolated .NET 10 executable compiles the actual FloodFill.cs with lightweight game API stubs. In particular, AllDesignationsAt returns one reused temporary list, matching RimWorld's behavior. It checks seed-state stability, physical plan barriers, blueprint handling, fog and concealed-rock traversal, constructed enclosures, resource-only ore, partial cover, and unchanged unrestricted behavior. Rock-link fixtures stand in for full natural-rock edifices in the exposure tests.

These checks do not replace in-game map, visibility, settings-persistence or mod-compatibility tests.
