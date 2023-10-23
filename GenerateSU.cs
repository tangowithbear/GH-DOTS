using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Components;
using Rhino.Collections;
using Rhino.Geometry;
using Rhino.UI.ObjectProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;

namespace IsovistTest {
    public class GenerateSUcomponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GenerateSUcomponent()
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
            pManager.AddPointParameter("All Points", "PTs", "A list of all Points", GH_ParamAccess.list);


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
            pManager.AddTextParameter("SpatialUnit ID", "SUID", "Spatial Unit Identifire", GH_ParamAccess.item);
            pManager.AddGenericParameter("SpatialUnit", "SU", "Generated Spatial Unit", GH_ParamAccess.item);
            pManager.AddTextParameter("Properties data", "D", "Show all properties with their values", GH_ParamAccess.list);

            

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
            List<Point3d> allPts = new List<Point3d>();
            List<Point3d> allTestPoints = new List<Point3d>();


            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.

            //if (!DA.GetData(0, ref plane)) return;
            //if (!DA.GetData(1, ref radius0)) return;
            //if (!DA.GetData(2, ref radius1)) return;
            //if (!DA.GetData(3, ref turns)) return;


            if (!DA.GetData(0, ref testPoint)) return; 
            if (!DA.GetDataList<Point3d>(1, allPts)) return;

            // We should now validate the data and warn the user if invalid data is supplied.

            if (testPoint == Point3d.Unset) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Test point is provided");
                return;
            }
            if (allPts.Count <= 1) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No points to check");
                return;
            }



            //object tiutout = null;
            //if (tiutout is Point3d point3d) {
            //}

            //else {
            //    List<object> tiutoutList = null;
            //    List<Point3d> myPoint3dList = tiutoutList.OfType<Point3d>().ToList();
            //}



            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:

            SpatialUnit spatialUnit = new SpatialUnit(testPoint);
            double area = Area(allPts);
            spatialUnit.Area = area;
            List<string> data = AggregateProperties(spatialUnit);


            DA.SetData(0, spatialUnit.Point3d);
            DA.SetData(1, spatialUnit.SUID);
            DA.SetData(2, spatialUnit);
            DA.SetDataList(3, data);

        }


        /// ...............................GET A DISTANCE BETTWEEN UNITS.................................


        public double Area(List<Point3d> allPts) {

            Double minDistanceSquared = allPts[0].DistanceToSquared(allPts[1]);

            for (int i = 2; i < allPts.Count; i++) {
                if (allPts[0].DistanceToSquared(allPts[i]) < minDistanceSquared)  {
                    minDistanceSquared = allPts[0].DistanceToSquared(allPts[i]);

                }
            }
            return minDistanceSquared;
        }



        /// ...............................MAKE A PROPERTY/VALUE LIST.................................

        public List<string> AggregateProperties (SpatialUnit testSU) {

            List<string> result = new List<string>();

            Type t = testSU.GetType();
            PropertyInfo[] props = t.GetProperties(); 
            foreach (var property in props) {

                //string propString = string.Format("{0} : {1}", property.Name, property.GetValue(testSU));
                string propString = $"{property.Name} : {property.GetValue(testSU)}";

                result.Add(propString);
            }

            return result;
        } 
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
        public override Guid ComponentGuid => new Guid("0B9CC89F-4381-447E-B76D-3DF183C7718E");
    }
}
