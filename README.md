# [Fougere](https://github.com/Tiniifan/Fougere/releases/latest) (Level-5 Animation Converter)

Fougere is a tool that allows you to edit and convert some Level-5 animation files to .json and back  

**Supported Files**
- XMTN (Bone Animation) - `.mtn2`, `.mtn3`
- XIMA (UV Animation) - `.imm2`, `.imm3`
- XMTM (Image Animation) - `.mtm2`, `.mtm3`

**Supported Versions**
- V1 (All games released before 2011)
- V2 (All games released between 2012 and 2016)
- V3 (All games released from 2016)

V1 and V2 files both use the `2` extensions (`.mtn2`, `.imm2`, `.mtm2`), V3 files use the `3` extensions (`.mtn3`, `.imm3`, `.mtm3`).

Fougere is a single executable that works both as a GUI and as a command-line tool.

## GUI

Just open a supported file (or drag & drop it onto the window), edit it, then save it as a json file or in another supported format.

![image](https://github.com/Tiniifan/Fougere/assets/30804632/d441acfc-8d4a-490a-a89b-b48c092120d6)

You can also open a file directly by passing its path as an argument:

```bash
Fougere 000.mtn2
```

To convert an animation to another version, select the animation and change the `Version` property (1, 2 or 3).  
When you save, the extension also decides the version: saving to a `3` extension creates a V3 file, saving a V3 animation to a `2` extension creates a V2 file.

## Command-Line

```bash
Fougere [option]
```

### Available options
- `-h`, `--help`: show the help menu
- `-tj`, `--tojson <input> [--output <output>]`: convert an animation file (.mtn2/.imm2/.mtm2/.mtn3/.imm3/.mtm3) to json
- `-ta`, `--toanimation <input> [--output <output>]`: convert a json file back to its animation format

`--output` is optional. When omitted, the result is saved next to the input file, using the correct extension for its format and version.  
With `--toanimation`, the output extension decides the version the same way as in the GUI.

### Examples

```bash
Fougere --tojson 000.mtn2
```
Creates `000.json` next to `000.mtn2`.

```bash
Fougere -tj 000.mtn2 --output result.json
```
Creates `result.json`.

```bash
Fougere --toanimation 000.json
```
Creates the animation file next to `000.json`, with the extension matching its format and version (`.mtn2`, `.imm2`, `.mtm2`, `.mtn3`, `.imm3` or `.mtm3`).

```bash
Fougere -ta 000.json --output 000.mtn2
```
Creates `000.mtn2`.

```bash
Fougere -ta 000.json --output 000.mtn3
```
Creates `000.mtn3` as a V3 animation, whatever the version stored in `000.json`.

## Version Differences

All versions store the same information: an animation name, a frame count and up to 4 tracks (2 for XMTM). Each track has a type (location, rotation, scale, UV move...) and contains nodes (a bone or material name hash) with their key frames and values.  
What changes between versions is how this information is laid out.

| | V1 | V2 | V3 |
|---|---|---|---|
| Extensions | `.mtn2` `.imm2` `.mtm2` | `.mtn2` `.imm2` `.mtm2` | `.mtn3` `.imm3` `.mtm3` |
| Header | 8-byte magic + decompressed size + track counts | Same as V1 | New header (4-byte magic), the animation name, the frame count and the node offset table are stored uncompressed |
| Track type | Stored in every node | Stored once per track | Stored once per track |
| Name hashes | Stored in every node | One hash table, nodes use an index | One hash table per track (XMTN shares one table between location, rotation and scale) |
| Frame lookup table | One entry for every frame of the animation, for every node | None, only the key frames | None, only the key frames |
| Rotation values | 32-bit float | 32-bit float | 16-bit integer (`value / 32767`) |

### V1
- Every node carries its own header (type, data type, frame range) and a lookup table with one entry per frame of the animation, so the file size grows with the frame count even when the animation has few key frames.
- Every node must have a key frame on the last frame.

### V2
- The type and the value format are stored once per track, and the node names are grouped in a single hash table.
- Only the key frames are stored, which makes V2 files smaller than V1 files.

### V3
- The header has been rewritten: the node offset table is moved out of the compressed block.
- Rotations are stored as 16-bit integers instead of 32-bit floats, so rotation keys take half the space but are rounded to 1/32767.
- A track slot can be empty, and XIMA/XMTM store one name hash table per track.
- A V3 file can contain several animations. Fougere only supports V3 files with a single animation for now.

### Compatibility
- Games that support V2 never support V1: to use a V1 animation in a V2 or V3 game, convert it to V2 (or V3).
- Games that support V3 (from 2016) also support V2.
