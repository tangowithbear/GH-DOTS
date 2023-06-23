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
    public class IsovistTestComponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public IsovistTestComponent()
          : base("IsovistTest", "IsoVist",
            "Construct an Isovist and evaluate data",
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

            //pManager.AddPlaneParameter("Plane", "P", "Base plane for spiral", GH_ParamAccess.item, Plane.WorldXY);
            //pManager.AddNumberParameter("Inner Radius", "R0", "Inner radius for spiral", GH_ParamAccess.item, 1.0);
            //pManager.AddNumberParameter("Outer Radius", "R1", "Outer radius for spiral", GH_ParamAccess.item, 10.0);
            //pManager.AddIntegerParameter("Turns", "T", "Number of turns between radii", GH_ParamAccess.item, 10);

            pManager.AddGenericParameter("Spatial Unit", "SU", "Test point for a spatial unit", GH_ParamAccess.item);
            pManager.AddIntegerParameter("View resolution", "VR", "View resolution, 1 for 360, 2 for 720", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("radius", "R", "Lenght of the Ray", GH_ParamAccess.item, 1000);
            pManager.AddGeometryParameter("Interieor obstacles", "IO", "opaque Building geometry including the exterieor walls", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Exterior obstacles", "EO", "Opaque Exteriour geometry", GH_ParamAccess.list);





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

            pManager.AddPointParameter("End Points", "EP", "End points of the rays", GH_ParamAccess.list);
            pManager.AddPointParameter("Interior intersection Points", "IEP", "Intersections points with interieor obstacles", GH_ParamAccess.list);
            pManager.AddPointParameter("Exterior intersection Points", "EIP", "Intersections points witn exterior obstacles", GH_ParamAccess.list);
            pManager.AddBrepParameter("Interieor IsoVist", "IIV", "A brep representing interior firld of view for a given test point", GH_ParamAccess.list);
            pManager.AddBrepParameter("Exterior Isovist", "EIV", "A brep representing a field of view for a given test point", GH_ParamAccess.list);
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
            int resolution = 1;
            int radius = 1000;
            List<GeometryBase> interiorObstacles = new List<GeometryBase>();    // HOW TO ASSIGN NULL TO POINTS / GEOMETRY
            List<GeometryBase> exteriorObstacles = new List<GeometryBase>();





            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.

            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();

            if (!DA.GetData(0, ref obj)) return;
            if (!DA.GetData(1, ref resolution)) return;
            if (!DA.GetData(2, ref radius)) return;
            if (!DA.GetDataList<GeometryBase>(3, interiorObstacles)) return;
            if (!DA.GetDataList<GeometryBase>(4, exteriorObstacles)) return;
 


            // We should now validate the data and warn the user if invalid data is supplied.

            if (obj == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Spatial Unit is provided");
                return;
            }
            if (radius <= 0.0) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Inner radius must be bigger than zero");
                return;
            }
            if (resolution <= 0 || resolution > 2) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Resolution parameter must 1 or 2");
                return;
            }

            //object tiutout = null;
            //if (tiutout is Point3d point3d) {
            //}

            //else {
            //    List<object> tiutoutList = null;
            //    List<Point3d> myPoint3dList = tiutoutList.OfType<Point3d>().ToList();
            //}


            SpatialUnit testSU = obj.Value as SpatialUnit;

            if (testSU == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Test point is not valid");
                return;
            }

            testPoint = testSU.Point3d;

            List<GeometryBase> obstacles = new List<GeometryBase>(interiorObstacles);
            obstacles.AddRange(exteriorObstacles);


            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:
            //Curve spiral = CreateSpiral(plane, radius0, radius1, turns);


            List<Point3d> endPoints = ComputeEndPoints(testPoint, radius, resolution);
            List<Curve> rays = ComputeRays(testPoint, endPoints);

            List<Point3d> allIntersectionPoints = ComputeIntersectionPoints(testPoint, endPoints, rays, obstacles);
            List<Point3d> interiorIntersectionPoints = ComputeIntersectionPoints(testPoint, endPoints, rays, interiorObstacles);
            List<Point3d> exteriorIntersectionPoints = ComputeIntersectionPoints(testPoint, endPoints, rays, obstacles);

            List<Point3d> interiorPerimeterPoints = ComputePerimeterPoints(interiorIntersectionPoints, endPoints);
            Curve interiorPerimeter = CreatePerimeterCurve(interiorPerimeterPoints);
            Brep[] interiorIsoVist = CreateIsoVist(interiorPerimeter);
            Double interiorIsovistArea = ComputeIsoVistArea(interiorIsoVist);

            List<Point3d> exteriorPerimeterPoints = ComputePerimeterPoints(allIntersectionPoints, endPoints);
            Curve exteriorPerimeter = CreatePerimeterCurve(exteriorPerimeterPoints);
            Brep[] exteriorIsoVist = CreateIsoVist(exteriorPerimeter);
            Double exteriorIsovistArea = ComputeIsoVistArea(exteriorIsoVist);





            //Brep interiorIsoVist = 
            //Brep exteriorIsovist =

            // Finally assign the spiral to the output parameter.
            // DA.SetData(0, spiral);

            testSU.Isovist_InteriorIsovist = interiorIsoVist;
            testSU.Isovist_IntIsovistArea = interiorIsovistArea;
            testSU.Isovist_ExteriorIsovist = exteriorIsoVist;
            testSU.Isivist_ExtIsovistArea = exteriorIsovistArea;

            List<string> data = AggregateProperties(testSU);


            DA.SetDataList(0, endPoints);
            DA.SetDataList(1, interiorIntersectionPoints);
            DA.SetDataList(2, allIntersectionPoints); ;
            DA.SetDataList(3, interiorIsoVist);
            DA.SetDataList(4, exteriorIsoVist);
            DA.SetDataList(5, data);
        }
        /// .........................COMPUTE ENDPOINTS.......................................
        public List<Point3d> ComputeEndPoints(Point3d testPoint, int radius, int resolution) {
            Plane plane = Plane.WorldXY;
            Circle c = new Circle(plane, testPoint, radius);

            int N = resolution * 360;
            List<Point3d> endPoints = new List<Point3d>();
            double angle = (2 * Math.PI) / N;

            for (int i = 0; i < N; i++) {
                double t = i * angle;
                endPoints.Add(c.PointAt(t));
            }

            return endPoints;
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

        public List<Point3d> ComputePerimeterPoints(List<Point3d> intersectionPoints, List<Point3d> endPoints) {
            List<Point3d> perimeterPoints = new List<Point3d>();
            foreach (Point3d pt in intersectionPoints) {
                if (pt != endPoints[0]) perimeterPoints.Add(pt);
            }
            perimeterPoints.Add(perimeterPoints[0]);
            return perimeterPoints;
        }

        public Curve CreatePerimeterCurve(List<Point3d> perimeterPoints) {
            Curve perimeterCurve = Curve.CreateInterpolatedCurve(perimeterPoints, 1, CurveKnotStyle.Uniform);
            perimeterCurve.MakeClosed(10.0);
            return perimeterCurve;
        }

        public Brep[] CreateIsoVist(Curve perimeterCurve) {
            Brep[] area = Brep.CreatePlanarBreps(perimeterCurve, 0.01);
            return area;
        }



        ///.........................COMPUTE ISOVIST AREA.....................................



        public Double ComputeIsoVistArea(Brep[] area)
        {
            AreaMassProperties mp = null;
            mp = AreaMassProperties.Compute(area, true, false, false, false);
            return mp.Area;
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

                if (propString.Contains("Visibility") || propString.Contains("SUID"))
                {

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
        protected override System.Drawing.Bitmap Icon => Properties.Resources.iconplugin;
      


        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8984EDFF-6F6C-4384-A818-D5316B589D88");
    }
}