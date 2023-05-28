using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsovistTest {
    internal class GenerateSU GH_Component {
            /// <summary>
            /// Each implementation of GH_Component must provide a public 
            /// constructor without any arguments.
            /// Category represents the Tab in which the component will appear, 
            /// Subcategory the panel. If you use non-existing tab or panel names, 
            /// new tabs/panels will automatically be created.
            /// </summary>
            public GenerateSU Component()
              : base("GenSU", "GenerateSU",
                "Cast to SU",
                "IndoorSpaceManager", "Map") {
            }

            /// <summary>
            /// Registers all the input parameters for this component.
            /// </summary>
            protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
                // Use the pManager object to register your input parameters.
                // You can often supply default values when creating parameters.
                // All parameters must have the correct access type. If you want 
                // to import lists or trees of values, modify the ParamAccess flag.


                pManager.AddPointParameter("Test Point", "P", "Test point for a spatial unit", GH_ParamAccess.item);
                pManager.AddPointParameter("All test points", "Ps", "A list of points ", GH_ParamAccess.list);


                // If you want to change properties of certain parameters, 
                // you can use the pManager instance to access them by index:
                //pManager[0].Optional = true;
            }

            /// <summary>
            /// Registers all the output parameters for this component.
            /// </summary>
            protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
                // Use the pManager object to register your output parameters.
                // Output parameters do not have default values, but they too must have the correct access type.
                //pManager.AddCurveParameter("Spiral", "S", "Spiral curve", GH_ParamAccess.item);

                pManager.AddPointParameter("Target Points", "TP", "End points of the rays", GH_ParamAccess.item);
                pManager.AddParameter("Interior intersection Points", "IEP", "Intersections points with interieor obstacles", GH_ParamAccess.list);


                //                                  HOW TO OUT PUT A TEXT/JSON/DICTIONARY?  

                // Sometimes you want to hide a specific parameter from the Rhino preview.
                // You can use the HideParameter() method as a quick way:
                //pManager.HideParameter(0);
            }

            /// <summary>
            /// This is the method that actually does the work.
            /// </summary>
            /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
            /// to store data in output parameters.</param>
            protected override void SolveInstance(IGH_DataAccess DA) {
                // First, we need to retrieve all data from the input parameters.
                // We'll start by declaring variables and assigning them starting values.


                Point3d testPoint = Point3d.Unset;                                   // DIFFERENCE/ POINT3D VS POINT?
                List<Point3d> allTestPoints = new List<Point3d>();
                int threshold = 20;
                List<GeometryBase> interiorObstacles = new List<GeometryBase>();    // HOW TO ASSIGN NULL TO POINTS / GEOMETRY
                List<GeometryBase> exteriorObstacles = new List<GeometryBase>();
                GeometryBase bonusViewGeometry = null;




                // Then we need to access the input parameters individually. 
                // When data cannot be extracted from a parameter, we should abort this method.

                //if (!DA.GetData(0, ref plane)) return;
                //if (!DA.GetData(1, ref radius0)) return;
                //if (!DA.GetData(2, ref radius1)) return;
                //if (!DA.GetData(3, ref turns)) return;

                if (!DA.GetData(0, ref testPoint)) return;
                if (!DA.GetDataList<Point3d>(1, allTestPoints)) return;
                if (!DA.GetData(2, ref threshold)) return;
                if (!DA.GetDataList<GeometryBase>(3, interiorObstacles)) return;
                if (!DA.GetDataList<GeometryBase>(4, exteriorObstacles)) return;
                if (!DA.GetData(5, ref bonusViewGeometry)) return;




                // We should now validate the data and warn the user if invalid data is supplied.

                if (testPoint == Point3d.Unset) {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Test point is provided");
                    return;
                }
                if (threshold <= 0.0) {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Threshold must be bigger than zero");
                    return;
                }
                if (bonusViewGeometry == null) {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Test object is provided");
                    return;
                }


                var v = Params.Input[4].Sources[0];
                string bonusName = v.NickName;



                //object tiutout = null;
                //if (tiutout is Point3d point3d) {
                //}

                //else {
                //    List<object> tiutoutList = null;
                //    List<Point3d> myPoint3dList = tiutoutList.OfType<Point3d>().ToList();
                //}


                List<GeometryBase> obstacles = new List<GeometryBase>(interiorObstacles);
                obstacles.AddRange(exteriorObstacles);



                // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
                // The actual functionality will be in a different method:


                List<bool> visibility = ContainsBonus(testPoint, obstacles, bonusViewGeometry, out List<Point3d> targetPoints);
                List<Curve> rays = ComputeRays(testPoint, targetPoints);
                List<Point3d> allIntersectionPoints = ComputeIntersectionPoints(testPoint, targetPoints, rays, obstacles);
                List<Point3d> interiorIntersectionPoints = ComputeIntersectionPoints(testPoint, targetPoints, rays, interiorObstacles);
                List<Point3d> exteriorIntersectionPoints = ComputeIntersectionPoints(testPoint, targetPoints, rays, exteriorObstacles);
                double percentage = CalculatePercentage(visibility);
                bool isThresholdPassed = IsThresholdPassed(threshold, percentage);




                DA.SetDataList(0, targetPoints);
                DA.SetDataList(1, interiorIntersectionPoints);
                DA.SetDataList(2, exteriorIntersectionPoints); ;
                DA.SetDataList(3, visibility);
                DA.SetData(4, percentage);
                DA.SetData(5, isThresholdPassed);
            }
        }
}
