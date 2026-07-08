# P0 Prompt Manifest

本文件记录本批次新生成源图的提示词，方便后续复现或重做。

说明：下面的 `environment_tiles`、`environment_walls`、`environment_props` 是生成批次中的历史源图命名。当前可用成品已经归档到 `../02_环境拆分素材/tiles`、`../02_环境拆分素材/walls`、`../02_环境拆分素材/props`。

## environment_tiles/p0_environment_tiles_source_atlas.png

```text
Use case: stylized-concept
Asset type: 2D game environment tile atlas for a Unity top-down pixel toy style game
Input images: Use the two visible images immediately above as style references: the accepted Shadough v4 map contact sheet and the chibi Chinese lantern character sheet. Preserve the low-resolution chunky pixel, toy-like, coarse-grain look; slightly clearer than the map contact sheet but not as detailed as high-resolution concept art.
Primary request: Create a clean 3 by 2 atlas of six square environment tiles, no labels and no text. Each cell is a separate 2D top-down/isometric-like tile sample with clear padding and consistent scale.
Tiles in reading order: 1 stone path tile, 2 blue-gray brick interior floor tile, 3 dark blue water channel tile, 4 left water bank edge tile with grass/stone transition, 5 right water bank edge tile with grass/stone transition, 6 broken bridge edge tile.
Style/medium: retro pixel-inspired, chunky square pixels, low resolution, toy-like, top-down 3/4 game asset, matching the existing green-grid Shadough map style.
Composition/framing: atlas sheet, six equal square cells, orthographic top-down 3/4 angle, centered tile in each cell, generous margins, no perspective camera distortion.
Color palette: muted Chinese official residence palette, deep greens, cool blue water, gray stone, small warm amber accents only when needed.
Materials/textures: blocky stone slabs, simple water pixels, coarse outlines, low detail, tileable-looking surfaces.
Constraints: no characters, no enemies, no UI, no labels, no arrows, no callouts, no watermark, no text. Keep each tile visually simple and readable for gameplay. Do not make it realistic or highly detailed.
```

## environment_walls/timber_wall_source_chroma.png

```text
Use case: stylized-concept
Asset type: 2D game transparent environment wall sprite sheet
Input images: Use the two visible reference images above for style: Shadough v4 scene maps and the chibi Chinese lantern character. Match the chunky, low-resolution toy pixel look, slightly clearer than v2 map art, not high-detail concept art.
Primary request: Create a single reusable timber wall segment for an ancient Chinese official residence, shown as a 2D top-down 3/4 game asset. The wall should be a horizontal straight segment with chunky gray stone base, dark brown wooden beam, simple paper/wood structure hints, and muted teal/gray roof-edge influence. This is a collision boundary sprite.
Style/medium: retro pixel-inspired top-down 3/4 asset, coarse square pixels, toy-like proportions, clean readable silhouette.
Composition/framing: one centered horizontal wall segment, generous transparent-removal padding, no other props. Put the wall on a perfectly flat solid #ff00ff chroma-key background for background removal.
Color palette: gray stone, dark brown timber, muted teal-gray, small warm amber not necessary.
Constraints: no characters, no doors, no labels, no UI, no text, no watermark, no cast shadow. Background must be perfectly uniform #ff00ff with no gradients, no texture, no floor plane, and no shadows. Do not use #ff00ff in the subject.
```

## environment_props/p0_props_source_chroma_sheet.png

```text
Use case: stylized-concept
Asset type: 2D game transparent scene prop sheet
Input images: Use the two visible reference images above for style: Shadough v4 scene maps and the chibi Chinese lantern character. Match the chunky, low-resolution toy pixel look, Chinese official residence theme, warm lantern-compatible palette, coarse square pixels.
Primary request: Create a clean 2 by 2 prop sheet with four separate scene props, no labels and no text. Put each prop centered in its own quadrant with generous empty padding.
Props in reading order: 1 simple ancient tree trunk, clearly brown, blocky and stylized; 2 simple ancient tree canopy, separate from trunk, chunky green leaves, not too detailed; 3 heavy wooden beam or wooden bar source for a pressure-plate shadow puzzle; 4 ornate brass/wood key pattern ornament source for a lock-shadow puzzle, not a collectible key UI icon.
Style/medium: retro pixel-inspired 2D game prop sprites, top-down 3/4, toy-like, low-resolution, chunky pixels, readable silhouettes.
Composition/framing: 2x2 grid, each prop isolated and centered, no overlap, no shadows, no floor plane. Use a perfectly flat solid #ff00ff chroma-key background for background removal.
Color palette: brown trunk and timber, muted green canopy, warm brass key ornament, dark outlines, no neon except the removable background.
Constraints: no characters, no enemies, no UI, no labels, no arrows, no callouts, no watermark, no text. Background must be uniform #ff00ff with no gradients, texture, lighting variation, floor plane, or shadows. Do not use #ff00ff in the subjects.
```
