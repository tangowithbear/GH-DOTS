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

namespace DOTS {
    public class WindowViewContentComponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public WindowViewContentComponent()
          : base("WindowWiewContent", "WinView",
            "Window View Content",
            "DOTS", "Vision") {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {

            pManager.AddGenericParameter("Spatial Unit", "SU", "Spatial unit to test", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Obstacles", "O", "Building geometry excluding the windows", GH_ParamAccess.list);
            pManager.AddRectangleParameter("Windows", "W", "Windows", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Buildings", "B", "Exteior obstacles", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Ground", "G", "Ground", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddGenericParameter("SpatialUnit", "SU", "Generated Spatial Unit", GH_ParamAccess.item);
            pManager.AddPointParameter("TestPoint", "TP", "SU location", GH_ParamAccess.item);
            pManager.AddPointParameter("WindowCentroid", "WP", "Centroid of the closest window", GH_ParamAccess.item);
            pManager.AddRectangleParameter("WindowRectangle", "WR", "Window rectangular frame", GH_ParamAccess.item);
            pManager.AddPointParameter("P", "P", "grid", GH_ParamAccess.list);
            pManager.AddPointParameter("BuildingPoints", "BP", "BuildingPoints", GH_ParamAccess.list);
            pManager.AddPointParameter("GroundPoints", "GP", "GroundPoints", GH_ParamAccess.list);
            pManager.AddTextParameter("Properties data", "D", "Show properties with their values", GH_ParamAccess.list);
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
            List<Rectangle3d> windows= new List<Rectangle3d>();
            List<GeometryBase> buildings= new List<GeometryBase>();
            GeometryBase ground = null;

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.


            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();

            if (!DA.GetData(0, ref testSU)) return;
            if (!DA.GetDataList<GeometryBase>(1, obstacles)) return;
            if (!DA.GetDataList<Rectangle3d>(2, windows)) return;
            if (!DA.GetDataList<GeometryBase>(3, buildings)) return;
            if (!DA.GetData(4, ref ground)) return;

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
            ComputeClosestWindowPoint(testSU, obstacles, windows, out Rectangle3d closestWindow, out Point3d closestWindowPoint);
            List<Point3d> windowGridPoints = CompouteWindowGridPoints(closestWindow, 10, 10);

            List<Ray3d> rays = ComputeRays(testSU.Gen_Point3d, windowGridPoints);
            ComputeRayIntersections(rays, buildings, ground, out int buildingScore, out int groundScore,
                                    out List<Point3d> allbuildingIntersectionPoints, out List<Point3d> allgroundIntersectionPoints); 

            testSU.ViewContent_BuiltPercentage = buildingScore;
            testSU.ViewContent_GroundPercentage = groundScore;
            testSU.ViewContent_SkyPercentage = 100 - buildingScore - groundScore;

            List<string> data = AggregateProperties(testSU);

            DA.SetData(0, testSU);
            DA.SetData(1, testSU.Gen_Point3d);
            DA.SetData(2, closestWindowPoint);
            DA.SetData(3, closestWindow);
            DA.SetDataList(4, windowGridPoints);
            DA.SetDataList(5, allbuildingIntersectionPoints);
            DA.SetDataList(6, allgroundIntersectionPoints);
            DA.SetDataList(7, data);
        }


        /// .........................COMPUTE FLOOR TO FLOOR HEIGHT................................
        public Point3d ComputeClosestWindowPoint(SpatialUnit SU, List<GeometryBase> obstacles, List<Rectangle3d> windows, out Rectangle3d closestWindow, out Point3d closestWindowCentroid) {
            closestWindowCentroid = new Point3d();
            double minDistanceSquared = Double.MaxValue;
            closestWindow = new Rectangle3d();

            foreach (Rectangle3d window in windows) {
                Point3d windowCentroid = window.Center;
                //Point3d windowCentroid = window.GetBoundingBox(true).PointAt(0.5, 0.5, 0.5); // for brep
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

        /// .........................COMPUTE WINDOW GRID POINTS................................

        public List<Point3d> CompouteWindowGridPoints (Rectangle3d window, double colCount, double rowCount) {
            List<Point3d> windowGridPoints = new List<Point3d>();
            Plane windowPlane = window.Plane;

            double cellSizeU = window.Width / colCount;
            double cellSizeV = window.Height / rowCount;

            for (double u = -(cellSizeU * 4); u <= (cellSizeU * 4); u += cellSizeU) {
                for (double v = -(cellSizeV * 4); v <= (cellSizeV * 4); v += cellSizeV) {
                    //double u = windowPlane.OriginX + j * cellSizeX + 0.5 * cellSizeX;
                    //double w = windowPlane.OriginY + i * cellSizeY + 0.5 * cellSizeY;
                    //Point3d gridPoint = new Point3d(x, y, windowPlane.OriginZ);

                    Point3d gridPoint = windowPlane.PointAt(u, v);
                    windowGridPoints.Add(gridPoint);
                }
            }
            return windowGridPoints;
        }

        /// .........................COMPUTE RAYS................................

        public List<Ray3d> ComputeRays(Point3d testPoint, List<Point3d> windowGridPoints) {
            List<Ray3d> rays = new List<Ray3d>();
            foreach (Point3d windowGridPoint in windowGridPoints) {
                Vector3d vector = windowGridPoint - testPoint;
                Ray3d ray = new Ray3d(testPoint, vector);
                rays.Add(ray);                     
            }
            return rays;
        }

        /// .........................COMPUTE INTERSECTIONS................................

        public void ComputeRayIntersections(List<Ray3d> rays, List<GeometryBase> buildings, GeometryBase ground, out int buildingScore, out int groundScore,
                                            out List<Point3d> allbuildingIntersectionPoints, out List<Point3d> allgroundIntersectionPoints) {
            buildingScore = 0;
            groundScore = 0;

            allbuildingIntersectionPoints = new List<Point3d>();
            allgroundIntersectionPoints = new List<Point3d>();

            List<GeometryBase> grounds = new List<GeometryBase>();
            grounds.Add(ground);


            foreach (Ray3d ray in rays) {

                Point3d[] buildingIntersectionPoints = Intersection.RayShoot(ray, buildings, 1);
                Point3d[] groundIntersectionPoints = Intersection.RayShoot(ray, grounds, 1);

                if (buildingIntersectionPoints.Count() > 0) {
                    allbuildingIntersectionPoints.Add(buildingIntersectionPoints[0]);
                    buildingScore += 1;
                } 
                else if (groundIntersectionPoints.Count() > 0 && buildingIntersectionPoints.Count() == 0) {
                    groundScore += 1;
                    allgroundIntersectionPoints.Add(groundIntersectionPoints[0]);
                }
            }
        }
    

        public List<string> AggregateProperties(SpatialUnit testSU) {

            List<string> result = new List<string>();

            Type t = testSU.GetType();
            PropertyInfo[] props = t.GetProperties();
            foreach (var property in props) {

                //string propString = string.Format("{0} : {1}", property.Name, property.GetValue(testSU));
                string propString = $"{property.Name} : {property.GetValue(testSU)}";

                if (propString.Contains("ViewContent") || propString.Contains("SUID")) {

                    result.Add(propString);
                }
            }

            return result;
        }






        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("2618FB5C-BE57-470F-9335-E104D893189E");

    }
}
