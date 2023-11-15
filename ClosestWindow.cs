using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Components;
using Rhino.Collections;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Input.Custom;
using Rhino.UI.ObjectProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace ISM {
    public class ClosestWindowComponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ClosestWindowComponent()
          : base("ClosestWindow", "ClosWin",
            "Closest Window centroid",
            "IndoorSpaceManager", "Vision") {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {

            pManager.AddGenericParameter("Spatial Unit", "SU", "Spatial unit to test", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Obstacles", "O", "Building geometry excluding the windows", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Windows", "W", "Windows", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddGenericParameter("SpatialUnit", "SU", "Generated Spatial Unit", GH_ParamAccess.item);
            pManager.AddPointParameter("TestPoint", "TP", "SU location", GH_ParamAccess.item);
            pManager.AddPointParameter("WindowCentroid", "WP", "Centroid of the closest window", GH_ParamAccess.item);
            pManager.AddGeometryParameter("WindowRectangle", "WR", "Window rectangular frame", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.


            SpatialUnit testSU = null;                             
            List<GeometryBase> obstacles = new List<GeometryBase>();    
            List<GeometryBase> windows= new List<GeometryBase>();


            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.


            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();

            if (!DA.GetData(0, ref testSU)) return;
            if (!DA.GetDataList<GeometryBase>(1, obstacles)) return;
            if (!DA.GetDataList<GeometryBase>(2, windows)) return;


            // We should now validate the data and warn the user if invalid data is supplied.

            if (obj == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Spatial Unit  is provided");
                return;
            }

            if (obstacles == null ) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No obstacles is provided");
                return;
            }

            if (windows == null ) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No windows is provided");
                return;
            }


            Point3d testPoint = testSU.Gen_Point3d;
            ComputeClosestWindowPoint(testSU, obstacles, windows, out Brep closestWindow, out Point3d closestWindowPoint);

            DA.SetData(0, testSU);
            DA.SetData(1, testSU.Gen_Point3d);
            DA.SetData(2, closestWindowPoint);
            DA.SetData(3, closestWindow);
        }


        /// .........................COMPUTE FLOOR TO FLOOR HEIGHT................................
        public Point3d ComputeClosestWindowPoint(SpatialUnit SU, List<GeometryBase> obstacles, List<GeometryBase> windows, out Brep closestWindow, out Point3d closestWindowCentroid) {
            closestWindowCentroid = new Point3d();
            double minDistanceSquared = Double.MaxValue;
            closestWindow = new Brep();

            foreach (Brep window in windows) {
                Point3d windowCentroid = window.GetBoundingBox(true).PointAt(0.5, 0.5, 0.5);
                if (SU.Gen_Point3d.DistanceToSquared(windowCentroid) < minDistanceSquared) {
                    LineCurve connection = new LineCurve(SU.Gen_Point3d, windowCentroid);
                    Curve[] overlapCurves;
                    Point3d[] obstacleIntersectPoints;
                    
                    foreach (Brep obstacle in obstacles) {

                        var Intersection = Rhino.Geometry.Intersect.Intersection.CurveBrep(connection, obstacle, 0.0, out overlapCurves, out obstacleIntersectPoints);
                        if (obstacleIntersectPoints.Count() == 0) {
                            minDistanceSquared = SU.Gen_Point3d.DistanceToSquared(windowCentroid);
                            closestWindowCentroid = windowCentroid;
                            closestWindow = window;
                        } 
                    }
                }
            }

            return closestWindowCentroid;
        }


        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("2618FB5C-BE57-470F-9335-E104D893189E");

    }
}
