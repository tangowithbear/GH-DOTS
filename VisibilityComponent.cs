using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Components;
using Rhino.Collections;
using Rhino.Geometry;
using Rhino.UI.ObjectProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace IsovistTest {
    public class VisibilityComponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VisibilityComponent()
          : base("Visibility", "Visibility",
            "Check if you see an object",
            "IndoorSpaceManager", "Field of view") {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.


            pManager.AddGenericParameter("Spatial Unit", "SU", "Test point for a spatial unit", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Threshold", "T", "the percentage of the visible part to consider the object is visible, Default = 20", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Interieor obstacles", "IO", "opaque Building geometry including the exterieor walls", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Exterior obstacles", "EO", "Opaque Exteriour geometry", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Object to test", "B", "Geometry to check the view acces", GH_ParamAccess.item);


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
            pManager.AddPointParameter("Interior intersection Points", "IEP", "Intersections points with interieor obstacles", GH_ParamAccess.list);
            pManager.AddPointParameter("Exterior intersection Points", "EIP", "Intersections points witn exterior obstacles", GH_ParamAccess.list);
            pManager.AddBooleanParameter("results", "R", "test", GH_ParamAccess.list);
            pManager.AddNumberParameter("Percentage", "%", "Percentage of visible part of the target Geometry", GH_ParamAccess.item);
            pManager.AddBooleanParameter("IsVisible", "V", "Returns True if pass the threshold", GH_ParamAccess.item );
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
        protected override void SolveInstance(IGH_DataAccess DA) {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            //Plane plane = Plane.WorldXY;
            //double radius0 = 0.0;
            //double radius1 = 0.0;
            //int turns = 0;


            Point3d testPoint = Point3d.Unset;                                   // DIFFERENCE/ POINT3D VS POINT?
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

            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();

            if (!DA.GetData(0, ref obj)) return;
            if (!DA.GetData(1, ref threshold)) return;
            if (!DA.GetDataList<GeometryBase>(2, interiorObstacles)) return;
            if (!DA.GetDataList<GeometryBase>(3, exteriorObstacles)) return;
            if (!DA.GetData(4, ref bonusViewGeometry)) return;




            // We should now validate the data and warn the user if invalid data is supplied.

            if (obj == null) {
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

            testSU.Visibility_Threshold = threshold;
            testSU.Visibility_Percentage = percentage;
            testSU.Visibility_Visibility = isThresholdPassed;

            List<string> data = AggregateProperties(testSU);

            DA.SetDataList(0, targetPoints);
            DA.SetDataList(1, interiorIntersectionPoints);
            DA.SetDataList(2, exteriorIntersectionPoints); ;
            DA.SetDataList(3, visibility);
            DA.SetData(4, percentage);
            DA.SetData(5, isThresholdPassed);
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

        /// .........................COMPUTE INTERSECTION POINTS

        public List<Point3d> ComputeIntersectionPoints(Point3d testPoint, List<Point3d> endPoints, List<Curve> rays, List<GeometryBase> obstacles) {
            List<Point3d> intersectionPoints = new List<Point3d>();
            foreach (Curve ray in rays) {
                Point3d theClosestPoint = endPoints[0];
                foreach (Brep io in obstacles) {
                    Curve[] overlapCurves;
                    Point3d[] brepIntersectPoints;

                    var intersection = Rhino.Geometry.Intersect.Intersection.CurveBrep(ray, io, 0.0, out overlapCurves, out brepIntersectPoints);
                    if (brepIntersectPoints.Count() > 0) {
                        Point3d currClosestPoint = Point3dList.ClosestPointInList(brepIntersectPoints, testPoint);
                        if (testPoint.DistanceToSquared(currClosestPoint) < testPoint.DistanceToSquared(theClosestPoint)) {
                            theClosestPoint = currClosestPoint;
                        }
                    }
                }
                intersectionPoints.Add(theClosestPoint);
            }
            return intersectionPoints;
        }



        ///.........................COMPUTE VISIBILITY.....................................

        public List<bool> ContainsBonus(Point3d testPoint, List<GeometryBase> obstacles, GeometryBase bonusViewGeometry, out List<Point3d> targetPoints) {
            Brep bonusGeometry = Brep.TryConvertBrep(bonusViewGeometry);
            BoundingBox bonusBbox = bonusGeometry.GetBoundingBox(true);
            Point3d startPt = bonusBbox.PointAt(0.5, 0.5, 0.0);
            Point3d endPt = bonusBbox.PointAt(0.5, 0.5, 1.0);
            Curve[] brepContourCurves = Brep.CreateContourCurves(bonusGeometry, startPt, endPt, 2.0);

            targetPoints = new List<Point3d>();
            Point3d[] curveTargetPoints = new Point3d[21];
            foreach (Curve contourCurve in brepContourCurves) {
                contourCurve.DivideByCount(20, false, out curveTargetPoints);
                foreach (Point3d curveTargetPoint in curveTargetPoints) {
                    targetPoints.Add(curveTargetPoint);
                }
            }

            List<bool> visibility = new List<bool>();
            List<Curve> rays = ComputeRays(testPoint, targetPoints);
            foreach (Curve ray in rays) {
                bool isVisible = true;
                foreach (Brep obstacle in obstacles) {
                    Curve[] overlapCurves;
                    Point3d[] obstaclesIntersectPoints;

                    Curve[] bonusSelfOverlapCurves;
                    Point3d[] bonusSelfIntersectPoints;

                    var intersection = Rhino.Geometry.Intersect.Intersection.CurveBrep(ray, obstacle, 0.0, out overlapCurves, out obstaclesIntersectPoints);
                    var bonusIntersection = Rhino.Geometry.Intersect.Intersection.CurveBrep(ray, bonusGeometry, 0.0, out bonusSelfOverlapCurves, out bonusSelfIntersectPoints);
                    if (obstaclesIntersectPoints.Count() > 0 || bonusSelfIntersectPoints.Count() > 1 ) 
                    {
                        isVisible = false;
                        break;
                    }

                }
                visibility.Add(isVisible);
            }

            return visibility;
        }

        public static double CalculatePercentage (List<bool> visibility) {
            int trueCount = 0;
            int totalCount = visibility.Count;  

            foreach (bool value in visibility) {
                if (value == true) {
                trueCount++;
                }
            }

            var tmp  = (trueCount / (double)totalCount) * 100;
            return Math.Ceiling(tmp);
        }


        public static bool IsThresholdPassed (int threshold, double percentage)
        {
           if (threshold <= percentage)  return true; 
           else return false;

        }




        public List<string> AggregateProperties(SpatialUnit testSU)
        {

            List<string> result = new List<string>();

            Type t = testSU.GetType();
            PropertyInfo[] props = t.GetProperties();
            foreach (var property in props)
            {

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
        protected override System.Drawing.Bitmap Icon => Properties.Resources.visibility;



        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("83BD2709-B249-4ED5-8379-769A2FB7E888"); 

    }
}
