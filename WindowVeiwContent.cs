using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Components;
using Grasshopper.Kernel.Geometry;
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
    public class WindowViewContentComponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public WindowViewContentComponent()
          : base("Window View Content", "WindowContent",
            "Evaliate window content",
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
            pManager.AddNumberParameter("Sky", "S", "Percentage of the visible sky", GH_ParamAccess.item);
            pManager.AddNumberParameter("Ground", "G", "Percentage of visible ground", GH_ParamAccess.item);
            pManager.AddNumberParameter("Built", "B", "Percentage of visible built environment", GH_ParamAccess.item);


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

            pManager.AddPointParameter("Test Point", "TP", "Test Point", GH_ParamAccess.item);
            //pManager.AddNumberParameter("values", "V", "Calculated numerical values", GH_ParamAccess.list);
            pManager.AddBrepParameter("SkyPercentage", "Sky", "Visible Sky Proportional Chart", GH_ParamAccess.list);
            pManager.AddBrepParameter("GroundPercentage", "Ground", "Visible Ground Proportional Chart", GH_ParamAccess.list);
            pManager.AddBrepParameter("BuildPercentage", "Built", "Visible Buildings Proportional Chart", GH_ParamAccess.list);
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


            Point3d testPoint = Point3d.Unset;   
        
            double skyPercentage = 0.00;
            double groundPercentage = 0.00;
            double builtPercentage = 0.00;
            //List<double> numValues = new List<double> { skyPercentage, groundPercentage, builtPercentage};

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.

            //if (!DA.GetData(0, ref plane)) return;
            //if (!DA.GetData(1, ref radius0)) return;
            //if (!DA.GetData(2, ref radius1)) return;
            //if (!DA.GetData(3, ref turns)) return;

            Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();

            if (!DA.GetData(0, ref obj)) return;
            if (!DA.GetData(1, ref skyPercentage)) return;
            if (!DA.GetData(2, ref groundPercentage)) return;
            if (!DA.GetData(3, ref builtPercentage)) return;





            // We should now validate the data and warn the user if invalid data is supplied.

            if (obj == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Spatial Unit  is provided");
                return;
            }
            if (skyPercentage < 0 || groundPercentage < 0 || builtPercentage < 0) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The value should be positive or zero");
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


            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:



            testSU.ViewContent_SkyPercentage = skyPercentage;
            testSU.ViewContent_GroundPercentage = groundPercentage;
            testSU.ViewContent_BuiltPercentage = builtPercentage;



            Brep[] skyViz    = CreateChartViz(testSU.Gen_Point3d, testSU.Gen_Area * 100, (testSU.Gen_Area * 100) - skyPercentage);
            Brep[] builtViz  = CreateChartViz(testSU.Gen_Point3d, groundPercentage + builtPercentage, groundPercentage);
            Brep[] groundViz = CreateChartViz(testSU.Gen_Point3d, groundPercentage, 0.5);

            List<string> data = AggregateProperties(testSU);


            DA.SetData(0, testPoint);
           // DA.SetDataList(1, numValues);
            DA.SetDataList(1, skyViz);
            DA.SetDataList(2, builtViz);
            DA.SetDataList(3, groundViz);
            DA.SetDataList(4, data);
        }

        public Brep[] CreateChartViz(Point3d centerPt, double upperValue, double downValue) {
            Circle innerCircle = new Circle(centerPt, downValue / 100);
            Circle outerCircle = new Circle(centerPt, upperValue / 100);
            Brep[] chartViz = Brep.CreateFromLoft(new List<Curve> { innerCircle.ToNurbsCurve(), outerCircle.ToNurbsCurve() }, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);
            return chartViz;
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
        public override Guid ComponentGuid => new Guid("952E8448-E6F8-47B8-BCDF-FC4F5F698F3B");

    }
}