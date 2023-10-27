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

namespace IsovistTest {
    public class IsovistComponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public IsovistComponent()
          : base("Isovist", "IsoVist",
            "Construct an Isovist and evaluate data",
            "IndoorSpaceManager", "Vision") {
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
            pManager.AddGeometryParameter("Facade Glazing", "FG", "Facade glazing", GH_ParamAccess.list);
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

            pManager.AddPointParameter("End Points", "EP", "End points of the rays", GH_ParamAccess.item);
            pManager.AddPointParameter("Interior intersection Points", "IEP", "Intersections points with interieor obstacles", GH_ParamAccess.list);
            pManager.AddPointParameter("Exterior intersection Points", "EIP", "Intersections points witn exterior obstacles", GH_ParamAccess.list);
            pManager.AddBrepParameter("Interieor IsoVist", "IIV", "A brep representing interior firld of view for a given test point", GH_ParamAccess.list);
            pManager.AddBrepParameter("Exterior Isovist", "EIV", "A brep representing a field of view for a given test point", GH_ParamAccess.list);
            pManager.AddPointParameter("Center of Gravity", "CG", "Center of gravity", GH_ParamAccess.item);
            pManager.AddTextParameter("Properties data", "D", "Show all properties with their values", GH_ParamAccess.list);
            pManager.AddPointParameter("Vertices", "V", "vertices", GH_ParamAccess.list);

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
            List<GeometryBase> interiorObstacles = new List<GeometryBase>();
            List<GeometryBase> glazing = new List<GeometryBase>();
            List<GeometryBase> exteriorObstacles = new List<GeometryBase>();





            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.

            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();

            if (!DA.GetData(0, ref obj)) return;
            if (!DA.GetData(1, ref resolution)) return;
            if (!DA.GetData(2, ref radius)) return;
            if (!DA.GetDataList<GeometryBase>(3, interiorObstacles)) return;
            if (!DA.GetDataList<GeometryBase>(4, glazing)) return;
            if (!DA.GetDataList<GeometryBase>(5, exteriorObstacles)) return;
 


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

            List<Point3d> allIntersectionPoints = ComputeExtIntersectionPoints(testPoint, endPoints, rays, obstacles, true);
            List<Point3d> interiorIntersectionPoints = ComputeIntIntersectionPoints(testPoint, endPoints, rays, interiorObstacles, false);
            ///List<Point3d> exteriorIntersectionPoints = ComputeIntersectionPoints(testPoint, endPoints, rays, obstacles, true);

            List<Point3d> interiorPerimeterPoints = ComputeIntPerimeterPoints(testPoint, interiorIntersectionPoints, endPoints);
            Curve interiorPerimeter = CreatePerimeterCurve(interiorPerimeterPoints);
            Brep[] interiorIsoVist = CreateIsoVist(interiorPerimeter);
            Double interiorIsovistArea = ComputeIsoVistArea(interiorIsoVist);

            List<Point3d> exteriorPerimeterPoints = ComputeExtPerimeterPoints(testPoint, allIntersectionPoints, endPoints);
            Curve exteriorPerimeter = CreatePerimeterCurve(exteriorPerimeterPoints);
            Brep[] exteriorIsoVist = CreateIsoVist(exteriorPerimeter);
            Double exteriorIsovistArea = ComputeIsoVistArea(exteriorIsoVist);

            // List<Point3d> vertices = ComputeVertices(interiorPerimeterPoints);
            List<Point3d> vertices = ComputeVertices(interiorPerimeterPoints);
            Point3d intGravityCentre = ComputeCentreOfGravity(vertices);

      
            Double extCompactness = ComputeCopmactness(exteriorPerimeter.GetLength(), exteriorIsovistArea);
            Double intCompactness = ComputeCopmactness(interiorPerimeter.GetLength(), interiorIsovistArea);

            Vector3d drift = IsovistDrift(testSU, intGravityCentre, out double driftMagnitude, out int driftAngle);


            // Finally assign the values to the properties.
            testSU.Isovist_Radius = radius;
            //testSU.Isovist_Ext_Isovist = exteriorIsoVist;
            testSU.Isovist_Ext_PerimeterCurve = exteriorPerimeter;
            testSU.Isovist_Ext_Area  = exteriorIsovistArea;
            testSU.Isovist_Ext_Perimeter = exteriorPerimeter.GetLength();
            testSU.Isovist_Ext_Compactness = extCompactness;
            //testSU.Isovist_Int_Isovist = interiorIsoVist;
            testSU.Isovist_Int_PerimeterCurve = interiorPerimeter;
            testSU.Isovist_Int_Area = interiorIsovistArea;
            testSU.Isovist_Int_Perimeter = interiorPerimeter.GetLength();
            testSU.Isovist_Int_CentreOfGravity = intGravityCentre;
            testSU.Isovist_Int_NumberOfVertices = vertices.Count;
            testSU.Isovist_Int_Compactness = intCompactness;
            testSU.Isovist_Int_DriftMagnitude = driftMagnitude;
            testSU.Isovist_Int_DriftDirection = driftAngle;

            List<string> data = AggregateProperties(testSU);


            // Finally assign the values to the output parameter.
            // DA.SetData(0, spiral);

            DA.SetDataList(0, endPoints);
            DA.SetDataList(1, interiorIntersectionPoints);
            DA.SetDataList(2, allIntersectionPoints); 
            DA.SetDataList(3, interiorIsoVist);
            DA.SetDataList(4, exteriorIsoVist);
            DA.SetData(5, intGravityCentre);
            DA.SetDataList(6, data);
            DA.SetDataList(7, vertices);
        }
        /// .........................COMPUTE ENDPOINTS.......................................
        public List<Point3d> ComputeEndPoints(Point3d testPoint, int radius, int resolution) {
            Plane plane = new Plane(testPoint, Vector3d.XAxis, Vector3d.YAxis);
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

        public List<Point3d> ComputeExtIntersectionPoints(Point3d testPoint, List<Point3d> endPoints, List<Curve> rays, List<GeometryBase> obstacles, bool includeEndPoints) /// x=0 if endPoins not included
        {
            List<Point3d> extIntersectionPoints = new List<Point3d>();
            foreach (Curve ray in rays) {
                Point3d theClosestPoint = ray.PointAtEnd;
                foreach (Brep obstacle in obstacles) {
                    Curve[] overlapCurves;
                    Point3d[] brepIntersectPoints;

                    var intersection = Rhino.Geometry.Intersect.Intersection.CurveBrep(ray, obstacle, 0.0, out overlapCurves, out brepIntersectPoints);

                    if (includeEndPoints) {
                        if (brepIntersectPoints.Count() > 0) {
                            Point3d currClosestPoint = Point3dList.ClosestPointInList(brepIntersectPoints, testPoint);
                            if (testPoint.DistanceToSquared(currClosestPoint) < testPoint.DistanceToSquared(theClosestPoint)) {
                                theClosestPoint = currClosestPoint;
                            }
                        }
                    }
                }
            extIntersectionPoints.Add(theClosestPoint);
            }
            return extIntersectionPoints;
        }


        public List<Point3d> ComputeIntIntersectionPoints(Point3d testPoint, List<Point3d> endPoints, List<Curve> rays, List<GeometryBase> obstacles, bool includeEndPoints) /// x=0 if endPoins not included
        {
            List<Point3d> intIntersectionPoints = new List<Point3d>();
            foreach (Curve ray in rays) {
                Point3d theClosestPoint = ray.PointAtEnd;
                foreach (Brep obstacle in obstacles) {
                    Curve[] overlapCurves;
                    Point3d[] brepIntersectPoints;

                    var intersection = Rhino.Geometry.Intersect.Intersection.CurveBrep(ray, obstacle, 0.0, out overlapCurves, out brepIntersectPoints);

                    if (brepIntersectPoints.Count() > 0) {
                        Point3d currClosestPoint = Point3dList.ClosestPointInList(brepIntersectPoints, testPoint);
                        if (testPoint.DistanceToSquared(currClosestPoint) < testPoint.DistanceToSquared(theClosestPoint)) {
                            theClosestPoint = currClosestPoint;
                        }  
                    }
                }
                if (theClosestPoint != ray.PointAtEnd) {
                    intIntersectionPoints.Add(theClosestPoint);
                }
            }     
            return intIntersectionPoints;
        }



    public List<Point3d> ComputeIntPerimeterPoints(Point3d testPoint, List<Point3d> intersectionPoints, List<Point3d> endPoints) {
            List<Point3d> intPerimeterPoints = new List<Point3d>();
            foreach (Point3d pt in intersectionPoints) {
                if (testPoint.DistanceToSquared(pt) < testPoint.DistanceToSquared(endPoints[0])) {
                    intPerimeterPoints.Add(pt);
                }
            }
            intPerimeterPoints.Add(intPerimeterPoints[0]);  /// close perimeter
            return intPerimeterPoints;
        }


        public List<Point3d> ComputeExtPerimeterPoints(Point3d testPoint, List<Point3d> intersectionPoints, List<Point3d> endPoints)
        {
            List<Point3d> extPerimeterPoints = new List<Point3d>();
            foreach (Point3d pt in intersectionPoints)
            {
                extPerimeterPoints.Add(pt);
            }
            extPerimeterPoints.Add(extPerimeterPoints[0]);  /// close perimeter
            return extPerimeterPoints;
        }


        public Curve CreatePerimeterCurve(List<Point3d> perimeterPoints) {
            Curve perimeterCurve = Curve.CreateInterpolatedCurve(perimeterPoints, 1, CurveKnotStyle.Uniform);
            perimeterCurve.MakeClosed(10.0);
            return perimeterCurve;
        }

        public List<Point3d> GetVertices(Curve perimeterCurve) {
            List<Point3d> verts = new List<Point3d>();
            perimeterCurve.GetNextDiscontinuity(Continuity.C1_continuous, 0.0, 1.0, out double t);
            verts.Add(perimeterCurve.PointAt(t));
            return verts;
        }

        

        public Brep[] CreateIsoVist(Curve perimeterCurve) {
            Brep[] area = Brep.CreatePlanarBreps(perimeterCurve, 0.01);
            return area;
        }



        ///.........................COMPUTE ISOVIST AREA.....................................

        public Double ComputeIsoVistArea(Brep[] area)
        {
            AreaMassProperties mp = AreaMassProperties.Compute(area, true, false, false, false);
            return mp.Area;
        }

        ///.........................COMPUTE COMPACTNESS.....................................

        public Double ComputeCopmactness(double perimeter, double area) {
            double compactness = (area * Math.PI * 4) / Math.Pow(perimeter, 2) ;
            return compactness;
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

                if (propString.Contains("Isovist") || propString.Contains("SUID"))
                {

                    result.Add(propString);
                }
            }

            return result;
        }




        public List<Point3d> ComputeVertices (List<Point3d>intPerimeterPoints) {
            List<Point3d> vertices = new List<Point3d>();
            intPerimeterPoints.Add(intPerimeterPoints[1]);
            for (int i = 1; i < (intPerimeterPoints.Count - 2); i++ ) {
                Point3d Pt0 = intPerimeterPoints[i - 1];
                Point3d Pt1 = intPerimeterPoints[i];
                Point3d Pt2 = intPerimeterPoints[i + 1];

                LineCurve lineCurve = new LineCurve(Pt0, Pt2);
                lineCurve.ClosestPoint(Pt1, out double t, 0.01);
                Point3d closestPt = lineCurve.PointAt(t);

                if (Pt1.DistanceToSquared(closestPt) > 0.0000001) {
                    vertices.Add(Pt1);
                } 

                /*double tol = 0.001;

                if (Math.Pow(((Pt2.X - Pt0.X) / (Pt1.X - Pt0.X)) - ((Pt2.Y - Pt0.Y) / (Pt1.Y - Pt0.Y)), 2) > tol) {
                    vertices.Add(Pt1);
                }*/
            }
            return vertices;
        }

        public Point3d ComputeCentreOfGravity(List <Point3d> vertices) {
            List<double> allX = new List<double>();
            List<double> allY = new List<double>();
            List<double> allZ = new List<double>();

            foreach (Point3d vertice in vertices) {
                allX.Add(vertice.X);
                allY.Add(vertice.Y);
                allZ.Add(vertice.Z);
            }

            Point3d gravityPt = new Point3d(allX.Average(), allY.Average(), allZ.Average());
            return gravityPt;
        }

        ///.........................COMPUTE DRIFT.....................................

        public Vector3d IsovistDrift(SpatialUnit testSU, Point3d gravityCntr, out double driftMagnitude, out int driftAngle) {
            Vector3d drift = gravityCntr - testSU.Point3d;
            driftMagnitude = drift.Length;
            double driftAngleRadians = Vector3d.VectorAngle(Vector3d.XAxis, drift, Plane.WorldXY);
            driftAngle = (int)((180.0 / Math.PI) * driftAngleRadians);
            return drift;
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
        protected override System.Drawing.Bitmap Icon => Properties.Resources.isovisticon;
      


        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8984EDFF-6F6C-4384-A818-D5316B589D88");
    }
}