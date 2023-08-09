using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Components;
using Rhino.Collections;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.UI.ObjectProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace IsovistTest
{
    public class ConnectivityComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ConnectivityComponent()
          : base("Visual connectivity", "Connectivity",
            "Check the visual connections",
            "IndoorSpaceManager", "Field of view")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.

            //pManager.AddPlaneParameter("Plane", "P", "Base plane for spiral", GH_ParamAccess.item, Plane.WorldXY);
            //pManager.AddNumberParameter("Inner Radius", "R0", "Inner radius for spiral", GH_ParamAccess.item, 1.0);
            //pManager.AddNumberParameter("Outer Radius", "R1", "Outer radius for spiral", GH_ParamAccess.item, 10.0);
            //pManager.AddIntegerParameter("Turns", "T", "Number of turns between radii", GH_ParamAccess.item, 10);

            pManager.AddGenericParameter("Spatial Unit", "SU", "Spatial unit to test", GH_ParamAccess.item);
            pManager.AddPointParameter("All test points", "Ps", "A list of points ", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Threshold", "T", "the percentage of the visible part to consider the object is visible, Default = 20", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Static obstacles", "IO", "opaque Building geometry including the exterieor walls", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Dynamic obstacles", "EO", "Opaque Exteriour geometry", GH_ParamAccess.list);



            // If you want to change properties of certain parameters, 
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.
            //pManager.AddCurveParameter("Spiral", "S", "Spiral curve", GH_ParamAccess.item);

            pManager.AddPointParameter("Target Points", "TP", "End points of the rays", GH_ParamAccess.item);
            pManager.AddPointParameter("Static intersection Points", "SIP", "Intersections points with static obstacles", GH_ParamAccess.list);
            pManager.AddPointParameter("Dynamic intersection Points", "DIP", "Intersections points witn dynamic obstacles", GH_ParamAccess.list);
            pManager.AddBooleanParameter("results", "R", "test", GH_ParamAccess.list);
            pManager.AddNumberParameter("Number of visible units", "N", "Number of visible units from of the test unit", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Visible spatial units", "VSU", "Returns a list of visible SUID", GH_ParamAccess.list);
            pManager.AddTextParameter("Properties data", "D", "Show all properties with their values", GH_ParamAccess.list);

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
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            //Plane plane = Plane.WorldXY;
            //double radius0 = 0.0;
            //double radius1 = 0.0;
            //int turns = 0;


            Point3d testPoint = Point3d.Unset;                                   // DIFFERENCE/ POINT3D VS POINT?
            List<Point3d> allTestPoints = new List<Point3d>();
            int threshold = 20;
            List<GeometryBase> staticObstacles = new List<GeometryBase>();    // HOW TO ASSIGN NULL TO POINTS / GEOMETRY
            List<GeometryBase> dynamicObstacles = new List<GeometryBase>();





            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.

            //if (!DA.GetData(0, ref plane)) return;
            //if (!DA.GetData(1, ref radius0)) return;
            //if (!DA.GetData(2, ref radius1)) return;
            //if (!DA.GetData(3, ref turns)) return;

            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();

            if (!DA.GetData(0, ref obj)) return;
            if (!DA.GetDataList<Point3d>(1, allTestPoints)) return;
            if (!DA.GetData(2, ref threshold)) return;
            if (!DA.GetDataList<GeometryBase>(3, staticObstacles)) return;
            if (!DA.GetDataList<GeometryBase>(4, dynamicObstacles)) return;





            // We should now validate the data and warn the user if invalid data is supplied.

            if (testPoint == Point3d.Unset)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Test point is provided");
                return;
            }
            if (allTestPoints.Count <= 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No points to check");
                return;
            }
            if (staticObstacles == null || dynamicObstacles == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No obstacles is provided");
                return;
            }



            SpatialUnit testSU = obj.Value as SpatialUnit;

            if (testSU == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Test point is not valid");
                return;
            }

            testPoint = testSU.Point3d;


            var v = Params.Input[4].Sources[0];
            string bonusName = v.NickName;



            //object tiutout = null;
            //if (tiutout is Point3d point3d) {
            //}

            //else {
            //    List<object> tiutoutList = null;
            //    List<Point3d> myPoint3dList = tiutoutList.OfType<Point3d>().ToList();
            //}


            List<GeometryBase> obstacles = new List<GeometryBase>(staticObstacles);
            obstacles.AddRange(dynamicObstacles);



            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:


            HashSet<SpatialUnit> visibleSU = new HashSet<SpatialUnit>();


            List<string> data = AggregateProperties(testSU);

            

            DA.SetDataList(0, targetPoints);
            DA.SetDataList(1, staticIntersectionPoints );
            DA.SetDataList(2, dynamicIntersectionPoints);
            DA.SetDataList(3, results);
            DA.SetDataList(4, visibleSUNumber );
            DA.SetData(5, visibleSU);
            DA.SetDataList(6, data);
        }

        ///..........................COMPUTE RAYS ...........................................
       
        
       public List<Curve> ComputeRays(Point3d testPoint, List<Point3d> endPoints) {

            List<Curve> rays = new List<Curve>();
            List<double> distances = new List<double>();

            {
                foreach (Point3d endPoint in endPoints) {
                    Line ray = new Rhino.Geometry.Line(testPoint, endPoint);
                    Curve curveRay = ray.ToNurbsCurve();
                    rays.Add(curveRay);
                }
            }

            return rays;
        }


        /// .........................COMPUTE INTERSECTIONS AND INTERSECTION POINTS................................

        public HashSet<SpatialUnit> ComputeConnectuvity (SpatialUnit testSU, List<SpatialUnit> targetSUs, List<GeometryBase> obstacles,  HashSet<SpatialUnit> visibleSU) {
                     
            List<Point3d> intersectionPoints = new List<Point3d>();
            foreach (SpatialUnit targetSU in targetSUs) {
                Point3d theClosestPoint = targetSU.Point3d;
                Line raytmp = new Rhino.Geometry.Line(testSU.Point3d, targetSU.Point3d);
                Curve ray = raytmp.ToNurbsCurve();

                foreach (Brep obstacle in obstacles) {
                    Curve[] overlapCurves;
                    Point3d[] brepIntersectPoints;

                    var intersection = Rhino.Geometry.Intersect.Intersection.CurveBrep(ray, obstacle, 0.0, out overlapCurves, out brepIntersectPoints);
                    if (brepIntersectPoints.Count() == 0) visibleSU.Add(targetSU);

                    /*else (brepIntersectPoints.Count() > 0) {
                        Point3d currClosestPoint = Point3dList.ClosestPointInList(brepIntersectPoints, testPoint);
                        if (testSU.DistanceToSquared(currClosestPoint) < testPoint.DistanceToSquared(theClosestPoint)) {
                            theClosestPoint = currClosestPoint;
                        }
                    }*/
                }
                //intersectionPoints.Add(theClosestPoint);
            }
            return visibleSU;
        }


        public List<string> AggregateProperties(SpatialUnit testSU) {

            List<string> result = new List<string>();

            Type t = testSU.GetType();
            PropertyInfo[] props = t.GetProperties();
            foreach (var property in props) {

                //string propString = string.Format("{0} : {1}", property.Name, property.GetValue(testSU));
                string propString = $"{property.Name} : {property.GetValue(testSU)}";

                if (propString.Contains("Visibility") || propString.Contains("SUID")) {

                    result.Add(propString);
                }
            }

            return result;
        }


        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        /// protected override System.Drawing.Bitmap Icon => Properties.Resources.;

        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("CF1B8304-B8E9-4103-AFA2-B0E1EF1CF5BC");
        
    }
}
