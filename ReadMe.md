# Designator Shapes

More shapes, fewer clicks. Custom polygons, parallel lines, rounded rectangles, grids, flood fill, and undo/redo. Use a custom palette or add shapes to vanilla drawing.

<!-- merthsoft-download -->
## Download

[Download designator-shapes.zip](https://merthsoft.com/RimWorld/designator-shapes.zip) — RimWorld 1.7 release.

The archive includes older supported version folders where available. Extract the mod folder into RimWorld's `Mods` directory.
<!-- /merthsoft-download -->

Adds a shape palette for building, mining, zoning, and other designation tools.

## Shapes

- Lines, including diagonal, freeform, L-shaped corners, and parallel lines
- Rectangles and rounded rectangles
- Grids and inverse grids, with adjustable spacing. Great for strip mining
- Checkerboards, with adjustable tile width and height
- Ellipses and circles
- Quarter arcs and semicircles
- Pentagons, hexagons, and octagons
- Midpoint circles, pentagons, hexagons, and octagons, drawn from the center outward
- Custom polygons: click to choose your own corners

Many shapes include both outline and filled versions. Rectangles and ellipses also have fixed-size tools: enter a width and height, then place the shape from its center.

Also includes filled and outline stamps for sun lamps and orbital trade beacons.

See the Shape Dictionary in Mod Settings for descriptions and individual tools!

There's also a flood-fill tool, plus undo/redo while the game is paused. Ctrl+Z undoes and Ctrl+Y redoes by default.

## To Use

1) Select the task you want to perform: build, mine, zone, etc.
2) Select a shape from the palette.
3) Click or drag to draw. For custom polygons, click each corner, then finish the polygon.

A "+" opens a submenu; a "-" goes back out. You can drag the palette wherever it's most comfortable.

Use the rotation keys to rotate supported shapes. Shape rotation must be enabled in Mod Settings.

Use F, or your configured corner-fill key, to toggle corner filling.

Hold RimWorld's "force square dimensions" key (Ctrl by default) to constrain supported drag-based shapes to equal width and height. For example, an ellipse becomes a circle. This uses your existing RimWorld binding.

For tools with input values, click the small "+" beside the values to edit them; "-" returns to the readout. The increase/decrease thickness bindings affect supported lines and outlines; parallel lines have their own Thickness input.

## Drawing Tools

### Drag Between Corners

For ordinary rectangles, ellipses, pentagons, hexagons, and octagons, click and hold at one corner of the desired bounds, drag to the opposite corner, then release. Outline versions select the edge; filled versions select the interior too.

### Midpoint Shapes

Click and hold at the center, drag outward to set the radius, then release. The larger horizontal or vertical distance from the center sets the radius. Midpoint circles, pentagons, hexagons, and octagons use this interaction.

### Fixed-Size Shapes

Choose a tool under Fixed rectangles or Fixed ellipses. Enter W and H, then click to place the shape centered on that cell. These values set the whole shape's size; you don't drag out its bounds.

### Freeform Lines

Hold the mouse button and draw like a pen, then release to finish. The stroke follows your cursor instead of drawing a straight line between the endpoints.

### L Corners

Drag a bounding rectangle. L corner draws its left and bottom sides; Opposite L corner draws its top and right sides. Rotate to choose a different corner. The corner follows the bounds, so changing which end you click first doesn't change its orientation.

### Arcs

Drag a bounding rectangle to size the curve. By default, the quarter arc curves around the lower-left corner, joining the bottom-right and top-left corners. The semicircle arc joins the bottom-left and bottom-right corners with an arch above them. Rotate to turn either arc.

Arcs stretch to fit the bounds: equal width and height gives a circular quarter arc; a circular half-arc needs bounds roughly twice as wide as they are tall. Other proportions make ellipse arcs.

### Grids And Inverse Grids

Drag an area to cover it with a grid. Presets set the distance between grid lines; Custom grid lets you enter horizontal spacing (W) and vertical spacing (H). With the default one-cell thickness, spacing 3 means one grid line every three cells, leaving two cells between lines.

Inverse grids select the blocks between the grid lines, leaving the channels unselected. Their W/H inputs also control spacing. Grid spacing is different from the width and height of a fixed-size shape.

### Checkerboards

Drag an area to alternate selected and empty tiles. W/H sets each tile's width and height in cells; 1x1 gives the usual checkerboard, while 2x3 makes alternating tiles two cells wide and three tall.

"Starting cell selected" includes the first clicked cell; "Starting cell skipped" leaves it empty. The pattern stays anchored to that first click in every drag direction.

### Parallel Lines

Drag an area to draw repeating stripes. Thickness sets how many rows or columns are selected per stripe; Skip sets how many are left empty between stripes. For example, Thickness 2 and Skip 3 selects two, skips three, and repeats. Skip can be zero.

The longer drag axis determines whether the lines run horizontally or vertically. A square selection keeps the last direction. Rotation swaps the direction. The pattern stays anchored to your first click.

### Rounded Rectangles

Choose an outline or filled tool under Rounded rectangles, set Radius, then drag the bounds. Radius sets the corner radius in cells and is limited to fit the dragged area; zero gives square corners.

### Coverage Stamps

Choose a sun lamp or orbital trade beacon stamp, then click its center. The active designation tool is applied to that building's coverage footprint: filled selects the area, outline selects its boundary. Use the default thickness of 1 to match the normal coverage radius.

### Custom Polygons

1) Click to add each corner. The preview updates as you move the cursor.
2) Click the first corner again, or press Enter, to finish.

Backspace removes the last corner. Escape or right-click cancels the polygon while keeping the tool selected. Nothing is designated until you finish.

Both outline and filled versions support concave shapes. You need at least three corners that don't all lie on one straight line. Polygons support up to 128 corners and 512 cells per axis.

### Flood Fill

Select flood fill and click a connected region, such as the inside of a room. Clicking a wall or mineable rock instead targets connected cells of that wall or rock type. The active designation tool still determines which cells can be selected. The fill stops at boundaries, configured barriers, or the cell limit.

## Customize The Vanilla Drawing Palette

Prefer RimWorld's default drawing controls? You can add shapes directly to the vanilla drawing palette:

1) Open Mod Settings → Designator Shapes → Shape Dictionary.
2) Browse to the shape you want.
3) Check "Include in vanilla palette" beneath its description.
4) Collapse the Designator Shapes palette to use the vanilla drawing controls.

Changes apply immediately. Uncheck a shape to remove this mod's entry from the vanilla palette.

You can choose individual shapes, including separate outline, filled, and midpoint versions. Built-in RimWorld shapes remain available. Tools with inputs show their controls beside the vanilla drawing controls.

## Optional Controls

In Mod Settings, you can:

- Remember the last shape used for each designation tool
- Assign Ctrl+Alt shape shortcuts and a menu-toggle shortcut
- Bind "Previous shape" to switch between your last two shapes; it is unbound by default
- Cancel a drag stroke with Escape or right-click while keeping the tool selected
- Adjust the menu's size, position, and navigation
- Enable a collapse button to switch back to vanilla drawing

The Alt-key toggle is off by default. Custom polygon cancellation always keeps the tool selected.

## Flood-Fill Settings

Flood fill has configurable barriers and an optional confirmation before destructive actions.

You can also prevent it from spreading into fogged cells or unrevealed natural rock, including concealed ore. This restriction is optional and off by default.

## Menu Layout

- Lines includes freeform lines, L-shaped corners, and parallel lines.
- Rectangles includes Grids, Inverse Grids, Checkerboards, Fixed rectangles, and Rounded rectangles submenus.
- Ellipses includes circles, fixed-size ellipses, and the Arcs submenu.
- More Shapes contains Pentagons, Hexagons, Octagons, and Custom polygons submenus. Each regular polygon submenu includes its midpoint versions.

## Bounding Box

Choose whether the selection bounds follow the mouse or the actual drawn shape, or enable a button to switch between them. Useful for flood fill, stamps, and midpoint shapes!

## Vanilla Drawing Palette

These tools are also available in the default drawing palette:

- Freeform lines
- Midpoint circles, outline and filled
- Hexagons and midpoint hexagons, outline and filled
- Sun lamp outlines and fills
- Orbital trade beacon outlines and fills

Use the Shape Dictionary to add the other tools you want.

## Want Individual Tools?

These standalone mods add tools to the vanilla drawing palette without requiring Designator Shapes:

Grids:
https://steamcommunity.com/sharedfiles/filedetails/?id=3819995093

Freeform Lines:
https://steamcommunity.com/sharedfiles/filedetails/?id=3819995485

Trade Beacon:
https://steamcommunity.com/sharedfiles/filedetails/?id=3533891331

Sun Lamp:
https://steamcommunity.com/sharedfiles/filedetails/?id=3533863758

Flood Fill:
https://steamcommunity.com/sharedfiles/filedetails/?id=3534423645

Midpoint Circles:
https://steamcommunity.com/sharedfiles/filedetails/?id=3533973266

Hexagons:
https://steamcommunity.com/sharedfiles/filedetails/?id=3538863086

Don't enable those standalone versions alongside Designator Shapes.

## Rotation And Mod Compatibility

If you use RimFactory conveyor belts, SOS2 ship corners, or other objects with their own rotation controls:

1) Shapes without rotation pass rotation input through to the selected object.
2) Disable shape rotation keys in Mod Settings if you always want those keys to rotate objects.
3) Rebind the shape rotation keys if you want separate controls for both.

## Credits And Links

Original art by Halno. Freeform line tool by Jacbo. Chinese translation by 祂. Space fix by Lisanhu.

If you like my work, consider buying me a coffee:
https://ko-fi.com/merthsoft

Source:
https://github.com/merthsoft/designator-shapes
