# [Fougere](https://github.com/Tiniifan/Fougere/releases/latest) (Level-5 Animation Converter)

Fougere is a tool that allows you to edit and convert some Level-5 animation files to .json  

**Supported Files**
- XMTN (Bone Animation)
- XIMA (UV Animation)
- XMTM (Image Animation)

**Supported Versions**
- V1 (All games released before 2011)
- V2 (All games released between 2012 and 2016)

The tool comes in two versions: 
- a Graphical User Interface (GUI): perfect for file editing
- a Command-Line Interface (CMD): perfect for fast file conversion
  
## GUI Version

The GUI version provides a user-friendly interface, you just have to open a supported file,  
then you can edit your and save it as a json file or in another supported files.

![image](https://github.com/Tiniifan/Fougere/assets/30804632/d441acfc-8d4a-490a-a89b-b48c092120d6)

## CMD Version
The CMD version is designed for command-line use and requires invocation with the following syntax:

```bash
FougereGUI.py [option]
````

### Available option
-h: to show help menu  
-d [input_path]: decompress .mtn2/.imm2/.mtm2 to json  
-c [input_path] [output_path]: compress readable json to .mtn2/.imm2/.mtm2  

Example

  `FougereGUI.exe -d 000.imm2`  
  `FougereGUI.exe -c 000.json 000.imm2`  
