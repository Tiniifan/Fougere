# [Fougere](https://github.com/Tiniifan/Fougere/releases/latest) (Level-5 Animation Converter)

Fougere is a tool that allows you to convert some Level-5 animation files (.mtn2;.imm2;.mtm2) to .json  

The tool comes in two versions: 
- a Graphical User Interface (GUI) version 
- a Command-Line Interface (CMD) version.
  
## GUI Version

The GUI version provides a user-friendly interface, you just have to open a supported file, then you can edit your file in the tool (text) and save it as a json file or in another supported files.

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
