# Designator Shapes

<!-- merthsoft-download -->
## Download

[Download designator-shapes.zip](https://merthsoft.com/RimWorld/designator-shapes.zip) — RimWorld 1.7 release.

The archive includes older supported version folders where available. Extract the mod folder into RimWorld's `Mods` directory.
<!-- /merthsoft-download -->

Allows you to build, mine, zone, etc. in various shapes, such as (both filled and outline):
- Line (including diagonal, freeform, and L-shaped corners)
- Rectangles, grids, inverse grids, and checkerboards
- Ellipse
- Circle, quarter arcs, and semicircles
- Pentagon
- Hexagon and octagon, including midpoint variants

Also includes these pre-defined object shapes (both filled and outline):
- Sun lamp
- Orbital trade beacon

See the shape dictionary in the mod settings! Each drawable shape has an **Include in vanilla palette** checkbox. Changes apply immediately and are saved in Mod Settings. Collapse the Shapes panel to use the vanilla palette. Existing mod additions remain enabled by default; other shapes are opt-in. Built-in shapes and standalone mods remain independent. Fixed-size shapes show width/height inputs beside the vanilla controls.

Hold RimWorld's **force square dimensions** key (Ctrl by default) to constrain rectangles, ellipses, grids, arcs, and other bounds-based shapes to equal width and height. This uses your existing RimWorld key binding. Center/radius shapes, freeform lines, stamps, and fixed-size inputs keep their normal behavior.

Flood fill has configurable barriers and optional reveal restrictions and destructive-action confirmation. Ctrl+Z undoes and Ctrl+Y redoes designations while paused.

Mod Settings supports Ctrl+Alt shape shortcuts and a configurable collapse shortcut (off by default). **Previous shape** switches between your last two shapes and is unbound by default. Custom grids are directly in their parent menus; checkerboard width/height controls set tile size.

## To use:
- Select the task you want to do (build, mine, zone, etc.)
- Select the shape you want to draw in the shapes menu
- Shapes with a "+" sign on them enter a sub-menu. Select the shape from within that menu
  - Some sub-menus have additional sub-menus. The "+" sign goes deeper into a sub-menu, the "-" sign goes back out
  - Within the rectangle and ellipse sub-menu are the "size input shapes" menu. From here you can define a width and height in the input menu and it will draw that shape in the given size from the middle.
- Use the rotate keys to rotate the shape
- Use the F key (or the key you have bound to "forbid item") to toggle filling of corners.
- There's a setting to allow you to add a "toggle" button that will collapse the menu and restore vanilla functionality.

There are settings you can adjust for flood-fill, the style of menu navigation, and the size and location of the shape menu. You can also drag the menu to put it wherever is most comfortable for you.

## MOD COMPATABILITY:
This mod is generally compatible with other mods no matter the load order. If you are using this with a mod that has rotatable elements (such as RimFactory's conveyor belts), you have three options to preserve RimFactory's rotation instead of Designator Shape's:
1) If you have a shape selected that isn't rotatable (lines, cutouts) then rotation is passed down to the other mod. There's nothing you need to do.
2) If you want the other-mod's rotation to work no matter the shape (e.g. rectangles), there is an option to disable rotation keys in the mod setting.
3) You can rebind the rotation keys to different keys if want both.

## Credits
Code by Merthsoft. Art by Halno. Freeform line tool by Jacbo. Chinese translation by 祂. Lisanhu for space fix.
