using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Components;
using Grasshopper.Kernel.Data;
using Rhino.Collections;
using Rhino.Geometry;
using Rhino.UI.ObjectProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;

namespace DOTS {
    public class GenerateSUtestcomponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GenerateSUtestcomponent()
          : base("GenSU", "GenerateSU",
            "Cast to SU",
            "DOTS", "Map") {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.


            //pManager.AddPointParameter("Test Point", "P", "Test point for a spatial unit", GH_ParamAccess.item);
            pManager.AddPointParameter("All Points", "PTs", "A list of all Points", GH_ParamAccess.list);
            pManager.AddPointParameter("Origin", "O", "Project Origin set by user", GH_ParamAccess.item);

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
            pManager.AddTextParameter("SpatialUnit ID", "SUID", "Spatial Unit Identifyer", GH_ParamAccess.item);
            pManager.AddGenericParameter("SpatialUnit", "SU", "Generated Spatial Units", GH_ParamAccess.item);
            pManager.AddPointParameter("Origin", "O", "Project Origin location", GH_ParamAccess.item);
            pManager.AddTextParameter("Properties data", "D", "Show all properties with their values", GH_ParamAccess.item);



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


            List<Point3d> allPts = new List<Point3d>();
            Point3d origin = Point3d.Unset;

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            // We should now validate the data and warn the user if invalid data is supplied.

            if (!DA.GetDataList<Point3d>(0, allPts)) { 
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No point is provided");
                return;
            }

            if (!DA.GetData<Point3d>(1, ref origin)) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Origin is provided");
                return;
            }

            //object tiutout = null;
            //if (tiutout is Gen_Point3d point3d) {
            //}

            //else {
            //    List<object> tiutoutList = null;
            //    List<Gen_Point3d> myPoint3dList = tiutoutList.OfType<Gen_Point3d>().ToList();
            //}

            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:

            double area = Area(allPts);

            List<SpatialUnit> allSUs = new List<SpatialUnit>();
            List<Point3d> allTestPts = new List<Point3d>();
            List<List<string>> dataList = new List<List<string>>();  ///
            List<string> listSUID = new List<string> ();

            foreach (Point3d Pt in allPts) {
                SpatialUnit spatialUnitTmp = new SpatialUnit(Pt);
                spatialUnitTmp.Gen_Area = area;
                allSUs.Add(spatialUnitTmp);
                allTestPts.Add(Pt);
            }

            for (int i = 0; i < allSUs.Count; i++) { 
                allSUs[i].SUID = "SU" + i;
                listSUID.Add(allSUs[i].SUID);
                List<string> data = AggregateProperties(allSUs[i]);
                dataList.Add(data);
            }

            Grasshopper.DataTree<object> tree = new Grasshopper.DataTree<object>();

            for (int i = 0; i < dataList.Count; i++) {
                for (int j = 0; j < dataList[i].Count; j++) {
                    tree.Add(dataList[i][j], new GH_Path(i));
                }   
            }


            DA.SetDataList  (0, allTestPts);
            DA.SetDataList  (1, listSUID);
            DA.SetDataList  (2, allSUs);
            DA.SetData      (3, origin);
            DA.SetDataTree  (4, tree);
        }


        /// ...............................GET A DISTANCE BETTWEEN UNITS.................................


        public double Area(List<Point3d> allPts) {

            Double minDistanceSquared = allPts[0].DistanceToSquared(allPts[1]);

            for (int i = 2; i < allPts.Count; i++) {
                if (allPts[0].DistanceToSquared(allPts[i]) < minDistanceSquared) {
                    minDistanceSquared = allPts[0].DistanceToSquared(allPts[i]);

                }
            }
            return minDistanceSquared;
        }

        /// ............................... FIND ORIGIN..............................................

        /*public Point3d FindOrigin(List<Point3d> allPts) {

            Point3d localOrigin = allPts[0];
            Point3d worldOrigin = new Point3d(0, 0, 0);
            double smallestDistance = (allPts[0].DistanceToSquared(worldOrigin));

            foreach (Point3d Pt in allPts) {
                if (Pt.DistanceToSquared(worldOrigin) < smallestDistance) localOrigin = Pt;
            }
            return localOrigin;
        }*/


        /// ...............................MAKE A PROPERTY/VALUE LIST.................................

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

                else if (property.GetValue(testSU) == null)
                    continue;

                else if ((property.Name == "Isovist_Int_CentreOfGravity") || (property.Name == "Isovist_Radius")) 
                    continue;

                else propertyValue = $"{property.GetValue(testSU)}";

                string propString = $"{property.Name} : {propertyValue} ";



                if (propString.Contains("Gen") || propString.Contains("SUID")) {

                    result.Add(propString);
                }
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
        public override Guid ComponentGuid => new Guid("EFA927CA-D30A-4411-B3FC-34B84046A363");
    }
}
