> This is a repo of "GH DOTS" plug-in for Grasshopper
>
> Please read the paper to know more about it: https://github.com/tangowithbear/GH-DOTS/blob/master/Paper/Encoding_Spaces.pdf


##  Description
Voxel Grid Framework for Quantitative Evaluation of Visual Perception in Indoor Spaces. The DOTS plug-in is a Grasshopper-based toolkit.
Grasshopper is a visual programming tool within Rhinoceros 3D, enabling parametric design workflows.
#### Development Status :
The tool is currently in early development and is intended to be run in debug mode through Visual Studio.


##  Installation

#### Requirements:
Rhinoceros 3D (Rhino) installed <br>
Visual Studio installed 

#### How to Run :
1. Clone the repository to your local machine. <br>
2. Open the project in Visual Studio. <br>
3. Ensure the Grasshopper NuGet package matches your installed version of Grasshopper. Update the package if needed. <br>
4. Build and run the project in Debug Mode. <br>

## Usage
The plugin contains three main sections: Map, Harvest and Query <br>
- Map section consists of a single component that creates a set of SUs from a given set of 3d points. <br>
- Harvest includes several components to run calculations for a specific set of properties for the provided spatial units. <br>
- Query enables exploring the properties by applying filters or by reading data directly from the units. 

For more detailed information read the Paper.
