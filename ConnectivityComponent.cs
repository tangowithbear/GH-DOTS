﻿using Grasshopper;
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

namespace DOTS{
    public class ConnectivityComponent : GH_Component {
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
            "DOTS", "Vision") {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.

            //pManager.AddPlaneParameter("Plane", "P", "Base plane for spiral", GH_ParamAccess.item, Plane.WorldXY);
            //pManager.AddNumberParameter("Inner Radius", "R0", "Inner radius for spiral", GH_ParamAccess.item, 1.0);
            //pManager.AddNumberParameter("Outer Radius", "R1", "Outer radius for spiral", GH_ParamAccess.item, 10.0);
            //pManager.AddIntegerParameter("Turns", "T", "Number of turns between radii", GH_ParamAccess.item, 10);

            pManager.AddGenericParameter("Spatial Unit", "SU", "Spatial unit to test", GH_ParamAccess.item);
            pManager.AddGenericParameter("All Spatial Units", "allSUs", "A list of Spatial Units ", GH_ParamAccess.list);
            //pManager.AddIntegerParameter("Threshold", "T", "the percentage of the visible part to consider the object is visible, Default = 20", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Static obstacles", "IO", "opaque Building geometry including the exterieor walls", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Dynamic obstacles", "EO", "Opaque Exteriour geometry", GH_ParamAccess.list);



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

            //pManager.AddPointParameter("Target Points", "TP", "End points of the rays", GH_ParamAccess.item);
            //pManager.AddPointParameter("Intersection Points", "IP", "Intersections points with static obstacles", GH_ParamAccess.list);
            //pManager.AddPointParameter("Dynamic intersection Points", "DIP", "Intersections points witn dynamic obstacles", GH_ParamAccess.list);
            //pManager.AddBooleanParameter("results", "R", "test", GH_ParamAccess.list);
            pManager.AddNumberParameter("Percentage", "%", "Percentage of visible part of the target Geometry", GH_ParamAccess.item);
            pManager.AddNumberParameter("Number of visible units", "N", "Number of visible units from of the test unit", GH_ParamAccess.item);
            pManager.AddPointParameter("Visible spatial units test Points", "VSU", "Returns a list of visible SUID", GH_ParamAccess.list);
            pManager.AddNumberParameter("Through vision", "TV", "Number of gaze intersections", GH_ParamAccess.item);
           // pManager.AddGeometryParameter("Sphere", "S", "SPHERE", GH_ParamAccess.item);
           // pManager.AddLineParameter("Connections", "Con", "Connections", GH_ParamAccess.list);
            pManager.AddTextParameter("Properties data", "D", "Show properties with their values", GH_ParamAccess.list);

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
            //Plane plane = Plane.WorldXY;
            //double radius0 = 0.0;
            //double radius1 = 0.0;
            //int turns = 0;


            Point3d testPoint = Point3d.Unset;                                   // DIFFERENCE/ POINT3D VS POINT?
            List<SpatialUnit> allSUs = new List<SpatialUnit>();
            //int threshold = 20;
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
            if (!DA.GetDataList<SpatialUnit>(1, allSUs)) return;
            // if (!DA.GetData(2, ref threshold)) return;
            if (!DA.GetDataList<GeometryBase>(2, staticObstacles)) return;
            if (!DA.GetDataList<GeometryBase>(3, dynamicObstacles)) return;





            // We should now validate the data and warn the user if invalid data is supplied.

            if (obj == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Spatial Unit  is provided");
                return;
            }
            if (allSUs.Count <= 1) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No points to check");
                return;
            }
            if (staticObstacles == null || dynamicObstacles == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No obstacles is provided");
                return;
            }



            SpatialUnit testSU = obj.Value as SpatialUnit;

            if (testSU == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Test point is not valid");
                return;
            }

            testPoint = testSU.Gen_Point3d;


            var v = Params.Input[3].Sources[0];
            string bonusName = v.NickName;



            //object tiutout = null;
            //if (tiutout is Gen_Point3d point3d) {
            //}

            //else {
            //    List<object> tiutoutList = null;
            //    List<Gen_Point3d> myPoint3dList = tiutoutList.OfType<Gen_Point3d>().ToList();
            //}


            List<GeometryBase> obstacles = new List<GeometryBase>(staticObstacles);
            obstacles.AddRange(dynamicObstacles);



            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:


            HashSet<SpatialUnit> visibleSUs = new HashSet<SpatialUnit>();
            List<SpatialUnit> visibleSUList = visibleSUs.ToList();
            List<Point3d> intersectionPoints = new List<Point3d>();



            ComputeConnectivity(testSU, allSUs, obstacles, out visibleSUs, out intersectionPoints);
            int visibleSUNumber = visibleSUs.AsEnumerable().Count();

            List<Point3d> visibleSUTestPoints = new List<Point3d>();
            foreach (SpatialUnit SU in visibleSUs) {
                visibleSUTestPoints.Add(SU.Gen_Point3d);
            }
            double percentage = CalculatePercentage(visibleSUs, allSUs);
            int throughVision = CalculateThroughtVision(testSU, visibleSUTestPoints, obstacles) / 2;

            List<SpatialUnit> farthestVisibleSUs = CalculateFarthestVisibleSUs(testSU, visibleSUs);

            testSU.Connectivity_Percentage = percentage;
            testSU.Connectivity_NumberOfVisibleSUs = visibleSUNumber;
            testSU.Connectivity_FarthestVisibleSUs = farthestVisibleSUs;
            testSU.Connectivity_VisibleUnits = visibleSUs;
            testSU.Connectivity_ThroughVision = throughVision;


            List<string> data = AggregateProperties(testSU);


            //DA.SetDataList(0, intersectionPoints);
            DA.SetData(0, percentage);
            DA.SetData(1, visibleSUNumber);
            DA.SetDataList(2, visibleSUTestPoints);
            DA.SetData(3, throughVision);
            //DA.SetData(5, targetSphere);
            //DA.SetDataList(6, connections);
            DA.SetDataList(4, data);
        }





        /// .........................COMPUTE INTERSECTIONS AND INTERSECTION POINTS................................

        public HashSet<SpatialUnit> ComputeConnectivity(SpatialUnit testSU, List<SpatialUnit> targetSUs, List<GeometryBase> obstacles, out HashSet<SpatialUnit> visibleSUs,
                                                         out List<Point3d> intersectionPoints) {

            visibleSUs = new HashSet<SpatialUnit>();
            intersectionPoints = new List<Point3d>();
            foreach (SpatialUnit targetSU in targetSUs) {
                if (testSU.Gen_Point3d != targetSU.Gen_Point3d) {

                    Point3d theClosestPoint = targetSU.Gen_Point3d;
                    Line raytmp = new Rhino.Geometry.Line(testSU.Gen_Point3d, targetSU.Gen_Point3d);
                    Curve ray = raytmp.ToNurbsCurve();
                    List<Point3d> obstacleIntersectPoints = new List<Point3d>();

                    foreach (Brep obstacle in obstacles) {
                        Curve[] overlapCurves;
                        Point3d[] brepIntersectPoints;

                        var intersection = Rhino.Geometry.Intersect.Intersection.CurveBrep(ray, obstacle, 0.01, out overlapCurves, out brepIntersectPoints);
                        if (brepIntersectPoints.Count() > 0) {
                            obstacleIntersectPoints.AddRange(brepIntersectPoints);
                            Point3d currClosestPoint = Point3dList.ClosestPointInList(brepIntersectPoints, testSU.Gen_Point3d);
                            if (testSU.Gen_Point3d.DistanceToSquared(currClosestPoint) < testSU.Gen_Point3d.DistanceToSquared(theClosestPoint)) {
                                theClosestPoint = currClosestPoint;
                                intersectionPoints.Add(theClosestPoint);
                            }
                        }
                    }

                    if (obstacleIntersectPoints.Count() == 0) visibleSUs.Add(targetSU);
                }

            }
            return visibleSUs;
        }

        public static List<SpatialUnit> CalculateFarthestVisibleSUs (SpatialUnit testSU, HashSet<SpatialUnit> visibleSUS) {
            
            List<SpatialUnit> result = new List<SpatialUnit>();
            SpatialUnit farthestSU = null;
            Double maxDistanceSquared = 0.00;

            foreach (SpatialUnit SU in visibleSUS) { 
                if (testSU.Gen_Point3d.DistanceToSquared(SU.Gen_Point3d) > maxDistanceSquared) {
                    maxDistanceSquared = testSU.Gen_Point3d.DistanceToSquared(SU.Gen_Point3d);
                    farthestSU = SU;   
                }
            }

            foreach (SpatialUnit SU in visibleSUS) {
                if (testSU.Gen_Point3d.DistanceToSquared(SU.Gen_Point3d) == maxDistanceSquared) {
                    result.Add(SU);
                }
            }

            return result;
        }



        public static double CalculatePercentage(HashSet<SpatialUnit> visibleSUs, List<SpatialUnit> allSUs) {
            int trueCount = visibleSUs.Count + 1;
            int totalCount = allSUs.Count;
            var tmp = (trueCount / (double)totalCount) * 100;
            return Math.Ceiling(tmp);
        }


        public static int CalculateThroughtVision(SpatialUnit SU, List<Point3d> visiblePts, List<GeometryBase> obstacles) {

            Sphere xSphere = new Sphere(SU.Gen_Point3d, Math.Sqrt(SU.Gen_Area) / 2);
            Brep targetSphere = xSphere.ToBrep();
            //connections = new List <LineCurve>();
            int throughtVision = 0;
            Curve[] overlapCurves;
            Point3d[] brepIntersectPoints1;
            Point3d[] brepIntersectPoints;

            for (int j = 0; j < visiblePts.Count; j++) {
                for (int i = j + 1; i < visiblePts.Count; i++) {
                    LineCurve connection = new LineCurve(visiblePts[j], visiblePts[i]);

                    bool intersectedWithObstacles = false;
                    foreach (Brep obstacle in obstacles) {
                        Rhino.Geometry.Intersect.Intersection.CurveBrep(connection, obstacle, 0.01, out overlapCurves, out brepIntersectPoints1);  /// REPLACE BY SPHERE OBSTACLE INTERSECTION
                        if (brepIntersectPoints1.Count() > 0) {
                            intersectedWithObstacles = true;
                            break;
                        }
                    }
                    if (intersectedWithObstacles) {
                        continue;
                    }

                    Rhino.Geometry.Intersect.Intersection.CurveBrep(connection, targetSphere, 0.01, out overlapCurves, out brepIntersectPoints);
                    if (brepIntersectPoints.Count() > 1) {
                        throughtVision++;
                        //connections.Add(connection);
                    }
                }
            }
            return throughtVision;
        }



        public List<string> AggregateProperties(SpatialUnit testSU) {

            List<string> result = new List<string>();

            Type t = testSU.GetType();
            PropertyInfo[] props = t.GetProperties();
            List<string> listSUIDs = new List<string>();
            foreach (var property in props) {

                string propertyValue = null;

                if ((property.PropertyType == typeof(HashSet<SpatialUnit>)) || (property.PropertyType == typeof(List<SpatialUnit>))) {

                    var propertyValueList = (IEnumerable<SpatialUnit>)property.GetValue(testSU);
                    if (propertyValueList == null)
                        continue;
                    var listSUID = propertyValueList.Select(SU => SU.SUID);
                    propertyValue = string.Join(", ", listSUID);
                }
                //string propString = string.Format("{0} : {1}", property.Name, property.GetValue(testSU));

                else  propertyValue = $"{property.GetValue(testSU)}";

                string propString = $"{property.Name} : {propertyValue} ";

                if (propString.Contains("Connectivity") || propString.Contains("SUID")) {

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
