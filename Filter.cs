using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Components;
using Grasshopper.Kernel.Parameters;
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

namespace IsovistTest {
    public class FilterComponent : GH_Component {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FilterComponent()
          : base("Filter", "Filter",
            "Filter Spatial Units",
            "IndoorSpaceManager", "Query") {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.


            pManager.AddGenericParameter("Spatial Units", "SUs", "A list of Spatial Units to test ", GH_ParamAccess.list);
            pManager.AddTextParameter("Name",  "N", "Property Name Option",  GH_ParamAccess.item);
            pManager.AddTextParameter("Value", "V", "Property Value Option", GH_ParamAccess.item); 
            pManager.AddIntegerParameter("Integer", "I", "input int", GH_ParamAccess.item);

            Param_Integer param = pManager[2] as Param_Integer;

            param.AddNamedValue("option_1", 0);
            param.AddNamedValue("option_2", 1);
            param.AddNamedValue("option_3", 2);


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

            pManager.AddGenericParameter("SpatialUnit", "SU", "Generated Spatial Unit", GH_ParamAccess.item);
            pManager.AddPointParameter("Test point", "P", "Spatial unit test field of view area", GH_ParamAccess.item);
            pManager.AddNumberParameter("%", "%", "Percentage of tested spatial units that meets the filtering condition", GH_ParamAccess.item);
            pManager.AddNumberParameter("Number", "N", "Number of tested spatial units that meets the filtering condition", GH_ParamAccess.item);
            pManager.AddTextParameter("Property Values", "V", "A list of values for the selected property for the tested spatial units that meets the filtering condition", GH_ParamAccess.list);

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

            testPoint = testSU.Point3d;

            object ob = null;
            DA.GetData(1, ref ob);

            var v = Params.Input[1].Sources[0];
            string s = v.NickName;


            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:

            double percentage = 0;
            int number = 0;

            List<string> data = AggregateProperties(testSU);

            DA.SetData(0, testSU);
            DA.SetData(1, testPoint);
            DA.SetData(2, percentage);
            DA.SetData(3, number);
            DA.SetDataList(4, data);
        }




        public List<string> AggregateProperties(SpatialUnit testSU) {

            List<string> result = new List<string>();

            Type t = testSU.GetType();
            PropertyInfo[] props = t.GetProperties();
            foreach (var property in props) {

                //string propString = string.Format("{0} : {1}", property.Name, property.GetValue(testSU));
                string propString = $"{property.Name} : {property.GetValue(testSU)}";

                if (propString.Contains("h") || propString.Contains("SUID")) {

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
       // protected override System.Drawing.Bitmap Icon => Properties.Resources.visibility;



        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("145C11C4-F9AD-4A7D-A912-776EF1D1934C");

    }
}
