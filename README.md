Overview

This project is an AutoCAD .NET (DLL) tool for fast and automated extraction of geometric and load-related data from mullion–transom curtain wall facades.
It is intended for systems behaving as single-span beams and supports efficient preparation of input data for structural analysis.

Workflow

The user first prepares the facade model in AutoCAD by defining:

Mullions and transoms along their axis lines
Wind load areas as surface regions

After running the script, the tool processes the geometry and generates structured output for each element.

Output Data
For every mullion and transom, the following data is extracted:
Tag
Width
Height
Distance to adjacent mullions and transoms
Wind load (pressure and suction)

The results are exported to an Excel-compatible file, where final load aggregation, member selection, and structural verification are performed.

Purpose

The tool significantly speeds up repetitive data extraction, reduces manual errors, and streamlines the workflow between CAD modeling and structural design.

Future Improvements:
Support for multi-span systems
Direct integration with structural solvers
Automated load calculation inside the tool
Parametric facade recognition
