using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Components;
using Rhino.Collections;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.UI.ObjectProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace ISM {
    public class ViewAccessComponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ViewAccessComponent()
          : base("ViewAccess", "ViewAccess",
            "Analyse the orientation of the isovist",
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


            pManager.AddGenericParameter("Spatial Unit", "SU", "Spatial unit to test", GH_ParamAccess.item);
            //pManager.AddGeometryParameter("Interior isovist", "IFOV", "Interior field of view", GH_ParamAccess.list);
            //pManager.AddGeometryParameter("Exterior isovist", "EFOW", "Exterior field of view", GH_ParamAccess.list);


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

            pManager.AddPointParameter("Test point","P",  "Spatial unit test field of view area",    GH_ParamAccess.item);
            pManager.AddGenericParameter("SpatialUnit", "SU", "Generated Spatial Unit", GH_ParamAccess.item);
            pManager.AddCurveParameter("Perimeter", "C", "Isovist perimeter Curve", GH_ParamAccess.item);
            pManager.AddPointParameter("Perimeter Points", "P", "Isovist perimeter Points", GH_ParamAccess.list);
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


            Point3d testPoint = Point3d.Unset;

            //Curve interiorPerimeter = null;
            //Curve exteriorPerimeter = null;


            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.

            //if (!DA.GetData(0, ref plane)) return;


            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();

            if (!DA.GetData(0, ref obj)) return;
            //if (!DA.GetData(1, ref interiorPerimeter)) return;
            //if (!DA.GetData(2, ref exteriorPerimeter)) return;




            // We should now validate the data and warn the user if invalid data is supplied.

            if (obj == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Spatial Unit  is provided");
                return;
            }


            SpatialUnit testSU = obj.Value as SpatialUnit;

            if (testSU == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Test point is not valid");
                return;
            }

            testPoint = testSU.Gen_Point3d;
            Curve perimeterCurve = testSU.Isovist_Ext_PerimeterCurve;



            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:

            List<Point3d> endPoints = ComputeEndPoints(testSU);
            bool orient = ComputeOrientation(testPoint, endPoints, perimeterCurve, out List<Point3d> perimeterPoints,
                                            out int east, out int nordEast, out int nord, out int nordWest,
                                            out int west, out int southWest, out int south, out int southEast);


            testSU.ViewAccess_Ext_EastScore =      east;
            testSU.ViewAccess_Ext_NordEastScore =  nordEast;
            testSU.ViewAccess_Ext_NordScore =      nord;
            testSU.ViewAccess_Ext_NordWestScore =  nordWest;
            testSU.ViewAccess_Ext_WestScore =      west;
            testSU.ViewAccess_Ext_SouthWestScore = southWest;
            testSU.ViewAccess_Ext_SouthScore =     south;
            testSU.ViewAccess_Ext_SouthEastScore = southEast;


            List<string> data = AggregateProperties(testSU);

            DA.SetData(0, testPoint);
            DA.SetData(1, testSU);
            DA.SetData(2, perimeterCurve);
            DA.SetDataList(3, perimeterPoints); 
            DA.SetDataList(4, data);
        }


        /// .........................COMPUTE ENDPOINTS.......................................
        public List<Point3d> ComputeEndPoints(SpatialUnit testSU) {
            Plane plane1 = new Plane(testSU.Gen_Point3d, Vector3d.XAxis, Vector3d.YAxis);
            Circle c = new Circle(plane1, testSU.Gen_Point3d,(double) testSU.Isovist_Radius);

            int N = 360;
            List<Point3d> endPoints = new List<Point3d>();
            double angle = (2 * Math.PI) / N;

            for (int i = 0; i < N; i++) {
                double t = i * angle;
                endPoints.Add(c.PointAt(t));
            }

            return endPoints;
        }

        ////////////////////////////////// COMPUTE ORIENTATION SCORES ///////////////////////
        public bool ComputeOrientation (Point3d testPoint, List<Point3d> endPoints, Curve perimeterCurve, out List<Point3d> perimeterPoints,  
                                        out int east, out int nordEast, out int nord, out int nordWest, 
                                        out int west, out int southWest, out int south, out int southEast) {
            perimeterPoints = new List<Point3d>();
            east      = Convert.ToInt32(ComputeScore(testPoint, endPoints, perimeterCurve, 338, 383,    out List<Point3d> eastPerimeterPoints));      perimeterPoints.AddRange(eastPerimeterPoints);  
            nordEast  = Convert.ToInt32(ComputeScore(testPoint, endPoints, perimeterCurve, 24, 68,      out List<Point3d> nordEastPerimeterPoints));  perimeterPoints.AddRange(nordEastPerimeterPoints);
            nord      = Convert.ToInt32(ComputeScore(testPoint, endPoints, perimeterCurve, 69, 113,     out List<Point3d> nordPerimeterPoints));      perimeterPoints.AddRange(nordPerimeterPoints);
            nordWest  = Convert.ToInt32(ComputeScore(testPoint, endPoints, perimeterCurve, 114, 158,    out List<Point3d> nordWestPerimeterPoints));  perimeterPoints.AddRange(nordWestPerimeterPoints);
            west      = Convert.ToInt32(ComputeScore(testPoint, endPoints, perimeterCurve, 159, 203,    out List<Point3d> westPerimeterPoints));      perimeterPoints.AddRange(westPerimeterPoints);
            southWest = Convert.ToInt32(ComputeScore(testPoint, endPoints, perimeterCurve, 204, 248,    out List<Point3d> southWestPerimeterPoints)); perimeterPoints.AddRange(southWestPerimeterPoints);
            south     = Convert.ToInt32(ComputeScore(testPoint, endPoints, perimeterCurve, 249, 294,    out List<Point3d> southPerimeterPoints));     perimeterPoints.AddRange(southPerimeterPoints);
            southEast = Convert.ToInt32(ComputeScore(testPoint, endPoints, perimeterCurve, 295, 338,    out List<Point3d> southEastPerimeterPoints)); perimeterPoints.AddRange(southEastPerimeterPoints);
            return true;
        }

        ///////////////////////////////// COMPUTE ONE ORIENTATION ///////////////////////////

        public double ComputeScore(Point3d testPoint, List<Point3d> endPoints, Curve perimeterCurve, int startAngle, int endAngle, out List<Point3d> localPerimeterPoints) {
            double score = 0;
            localPerimeterPoints = new List<Point3d>();
            for (int i = startAngle; i <= endAngle; i++) {
                LineCurve ray = new LineCurve (testPoint, endPoints[i%360]);
                ray.ToNurbsCurve();
                ray.ClosestPoints(perimeterCurve, out Point3d pointOnRay, out Point3d poinOnCurve);
                score += Math.Round(testPoint.DistanceTo(pointOnRay), 1);
                localPerimeterPoints.Add(pointOnRay);

            }
            return score;
        }



        public List<string> AggregateProperties(SpatialUnit testSU) {

            List<string> result = new List<string>();

            Type t = testSU.GetType();
            PropertyInfo[] props = t.GetProperties();
            foreach (var property in props) {

                //string propString = string.Format("{0} : {1}", property.Name, property.GetValue(testSU));
                string propString = $"{property.Name} : {property.GetValue(testSU)}";

                if (propString.Contains("ViewAccess") || propString.Contains("SUID")) {

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
        protected override System.Drawing.Bitmap Icon => null;



        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("145C11C4-F9AD-4A7D-A912-776EF1D1934C");

    }
}
