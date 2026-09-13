# [Fougere](https://github.com/Tiniifan/Fougere/releases/latest) (Level-5 Animation Converter)

Fougere is a tool that allows you to edit and convert some Level-5 animation files to .json  

**Supported Files**
- XMTN (Bone Animation)
- XIMA (UV Animation)
- XMTM (Image Animation)

**Supported Versions**
- V1 (All games released before 2011)
- V2 (All games released between 2012 and 2016)

Fougere is a single executable that works both as a GUI and as a command-line tool.

## GUI

Just open a supported file (or drag & drop it onto the window), edit it, then save it as a json file or in another supported format.

![image](https://github.com/Tiniifan/Fougere/assets/30804632/d441acfc-8d4a-490a-a89b-b48c092120d6)

You can also open a file directly by passing its path as an argument:

```bash
Fougere 000.mtn2
```

## Command-Line

```bash
Fougere [option]
```

### Available options
- `-h`, `--help`: show the help menu
- `-tj`, `--tojson <input> [--output <output>]`: convert an animation file (.mtn2/.imm2/.mtm2) to json
- `-ta`, `--toanimation <input> [--output <output>]`: convert a json file back to its animation format

`--output` is optional. When omitted, the result is saved next to the input file, using the correct extension.

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
Creates the animation file next to `000.json`, with the extension matching its format (`.mtn2`, `.imm2` or `.mtm2`).

```bash
Fougere -ta 000.json --output 000.mtn2
```
Creates `000.mtn2`.
