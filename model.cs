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

namespace IsovistTest {
    public class ModelComponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ModelComponent()
          : base("Model structure", "Model",
            "Struction related metrics",
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

            pManager.AddGenericParameter("Spatial Unit", "SU", "Spatial unit to test", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Interior Structure", "IS", "Building geometry excluding the envelope", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Envelope", "E", "Building envelope", GH_ParamAccess.list);



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

            pManager.AddPointParameter("TestPoint", "TP", "SU location", GH_ParamAccess.item);
            pManager.AddNumberParameter("Floor Height", "FH", "Floor height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance to structure", "DS", "Distance to closest structural element", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance to Envelope", "DE", "Distence to closest envelope element", GH_ParamAccess.item);
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
            List<GeometryBase> structureObstacles = new List<GeometryBase>();    // HOW TO ASSIGN NULL TO POINTS / GEOMETRY
            List<GeometryBase> envelopeObstacles = new List<GeometryBase>();





            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.

            //if (!DA.GetData(0, ref plane)) return;
            //if (!DA.GetData(1, ref radius0)) return;
            //if (!DA.GetData(2, ref radius1)) return;
            //if (!DA.GetData(3, ref turns)) return;

            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();

            if (!DA.GetData(0, ref obj)) return;
            if (!DA.GetDataList<GeometryBase>(1, structureObstacles)) return;
            if (!DA.GetDataList<GeometryBase>(2, envelopeObstacles)) return;


            // We should now validate the data and warn the user if invalid data is supplied.

            if (obj == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Spatial Unit  is provided");
                return;
            }

            if (structureObstacles == null || structureObstacles == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No structuralobstacles is provided");
                return;
            }

            if (envelopeObstacles == null || envelopeObstacles == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No envelopeobstacles is provided");
                return;
            }


            SpatialUnit testSU = obj.Value as SpatialUnit;

            if (testSU == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Test point is not valid");
                return;
            }

            testPoint = testSU.Gen_Point3d;






            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:



            List<Point3d> intersectionPoints = new List<Point3d>();

            List<GeometryBase> obstacles = new List<GeometryBase>(structureObstacles);
            obstacles.AddRange(envelopeObstacles);

            double floorHeight = ComputeFloorToCeilingDistance(testSU, obstacles);
            double disToStructure = FindDistanceToClosestGeometry(testSU.Gen_Point3d, structureObstacles);
            double disToEnvelope = FindDistanceToClosestGeometry(testSU.Gen_Point3d, envelopeObstacles);

            testSU.Model_FloorHeight = floorHeight;
            testSU.Model_DistanceToStructure = disToStructure;
            testSU.Model_DistancetToEnvelope = disToEnvelope;

            List<string> data = AggregateProperties(testSU);

            DA.SetData(0, testPoint);
            DA.SetData(1, floorHeight);
            DA.SetData(2, disToStructure);
            DA.SetData(3, disToEnvelope);
            DA.SetDataList(4, data);
        }



        /// .........................COMPUTE FLOOR TO FLOOR HEIGHT................................
        public double ComputeFloorToCeilingDistance (SpatialUnit SU, List<GeometryBase> obstacles) {
            double floorHeight = 0.00;
            Vector3d downVector = new Vector3d(0, 0, -1);
            Vector3d upVector   = new Vector3d(0, 0,  1);

            Ray3d rayDown = new Ray3d(SU.Gen_Point3d, downVector);
            Point3d[] downIntersectionPoints = Intersection.RayShoot(rayDown, obstacles, 1);

            double minDistance = double.MaxValue;
            Point3d floorPoint = Point3d.Unset;

            foreach (Point3d Pt in downIntersectionPoints) {
                double distance = SU.Gen_Point3d.DistanceTo(Pt);
                if (distance < minDistance) {
                    minDistance = distance;
                    floorPoint = Pt;
                }
            }


            Ray3d rayUp = new Ray3d(floorPoint, upVector);
            Point3d[] upIntersectionPoints = Intersection.RayShoot(rayUp, obstacles, 1);

            double minDistance2 = double.MaxValue;
            Point3d ceilingPoint = Point3d.Unset;

            foreach (Point3d Pt in upIntersectionPoints) {
                double distance = floorPoint.DistanceTo(Pt);
                if (distance < minDistance2) {
                    minDistance2 = distance;
                    ceilingPoint = Pt;
                }
            }

            floorHeight = minDistance2;

            return floorHeight;
        }

        public double FindDistanceToClosestGeometry(Point3d testPoint, List<GeometryBase> obstacles) {
           
            if (obstacles == null || obstacles.Count == 0) {
                // Handle the case when the list of geometries is empty
                return double.NaN;
            }
            double minDistance = double.MaxValue;

            foreach (Brep geometry in obstacles) {
                Point3d closestPoint = geometry.ClosestPoint(testPoint);
                if (closestPoint != Point3d.Unset) { 
                    double distance = testPoint.DistanceTo(closestPoint);
                    if (distance < minDistance) {
                        minDistance = distance;
                    }
                }
            }

            return minDistance;
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

                else propertyValue = $"{property.GetValue(testSU)}";

                string propString = $"{property.Name} : {propertyValue} ";

                if (propString.Contains("Model") || propString.Contains("SUID")) {

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
        public override Guid ComponentGuid => new Guid("137E17C9-46D4-4D05-8C45-47DE8B81319C");

    }
}
