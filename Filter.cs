﻿using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Components;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Collections;
using Rhino.Commands;
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
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        public FilterComponent()
          : base("Filter", "Filter",
            "Filter Spatial Units",
            "IndoorSpaceManager", "Query") {
        }
        /// Registers all the input parameters for this component.
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.


            pManager.AddGenericParameter("Spatial Units",  "SUs",  "A list of Spatial Units to test ",          GH_ParamAccess.list);
            pManager.AddIntegerParameter("Name Predicate", "NP",   "Property name condition, default 'is' ",    GH_ParamAccess.item);
            pManager.AddTextParameter   ("Name Subject",   "N",    "Property Name subject as text",             GH_ParamAccess.item);
            pManager.AddIntegerParameter("Value Predicate","VP",   "Propery value contidion, default 'equals'", GH_ParamAccess.item);
            pManager.AddTextParameter   ("Value Subject",  "V",    "Value as number or text, default 0",        GH_ParamAccess.item); 


            //Param_Integer param = pManager[3] as Param_Integer;

            var namePredicate = (Param_Integer)pManager[1];

            namePredicate.AddNamedValue("Name is", 0);
            namePredicate.AddNamedValue("Name is not", 1);
            namePredicate.AddNamedValue("Name containts", 2);
            namePredicate.AddNamedValue("Name does not contain", 3);

            var valuePredicate = (Param_Integer)pManager [3];

            valuePredicate.AddNamedValue("equals", 0);
            valuePredicate.AddNamedValue("not equals", 1);
            valuePredicate.AddNamedValue("greater than or equal", 2);
            valuePredicate.AddNamedValue("less than", 3);

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
            pManager.AddTextParameter("test", "T", "test", GH_ParamAccess.item);
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


            List<SpatialUnit> allSUs = new List<SpatialUnit>();
            int namePredicate   = 0;
            string nameSubject  = null;
            int valuePredicate  = 0;
            var valueSubject    = (dynamic)null;
      

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.

            //Grasshopper.Kernel.Types.GH_ObjectWrapper obj = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            //List<GH_ObjectWrapper> objs = new List<GH_ObjectWrapper>();

            if (!DA.GetDataList<SpatialUnit>(0, allSUs)) return;
            if (!DA.GetData(1, ref namePredicate)) return;
            if (!DA.GetData(2, ref nameSubject)) return;
            if (!DA.GetData(3, ref valuePredicate)) return;
            if (!DA.GetData(4, ref valueSubject)) return;


            // We should now validate the data and warn the user if invalid data is supplied.

            if (allSUs.Count <= 1) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No points to check");
                return;
            }

            if (namePredicate == -1) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No Name predicate is providate");
                return;
            }

            /*SpatialUnit testSU = obj.Value as SpatialUnit;

            if (testSU == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Test point is not valid");
                return;
            }*/


            List <Point3d> testPoints = new List<Point3d> ();
            foreach ( SpatialUnit SU in allSUs) {
                testPoints.Add(SU.Point3d);
            }

           

            string testOutput = null;


          


            //object ob = null;
            //DA.GetData(1, ref ob);

            //var nameSubject = Params.Input[1].Sources[0];
            // string nick = nameSubject.NickName;

            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:


            double percentage = 0;
            int number = 0;

            List<string> targetProperties = DefineTargetPropertyName(allSUs, nameSubject, namePredicate);


            List<string> data = new List<string>();
            foreach ( SpatialUnit SU in allSUs) {
                AggregateProperties(SU);
            }


            DA.SetDataList(0, allSUs);
            DA.SetDataList(1, testPoints);
            DA.SetData(2, percentage);
            DA.SetData(3, number);
            DA.SetData(4, testOutput);
            DA.SetDataList (5, data);
        }


        public List<string> DefineTargetPropertyName (List<SpatialUnit> allSUs, string nameSubject, int namePredicate) {
            List<string> targetProperties = new List<string>();
            SpatialUnit testSU = allSUs[0];

            Type t = testSU.GetType();
            PropertyInfo[] props = t.GetProperties();

            if (namePredicate == 0) { 
                foreach (var property in props) {
                    if (property.Name == nameSubject) {
                        targetProperties.Add(property.Name);
                    }
                }
            }

            else if (namePredicate == 1) {
                foreach (var property in props) {
                    if (!property.Name.Contains(nameSubject)) {
                        targetProperties.Add(property.Name);
                    }
                }
            } 
            
            else if (namePredicate == 2) {
                foreach (var property in props) {
                    if (property.Name.Contains(nameSubject)) {
                        targetProperties.Add(property.Name);
                    }
                }
            } 
            
            else if (namePredicate == 3) {
                foreach (var property in props) {
                    if (!property.Name.Contains(nameSubject)) {
                        targetProperties.Add(property.Name);
                    }
                }
            }

            return targetProperties;
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
        protected override System.Drawing.Bitmap Icon => null;



        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("124F8D8D-10CF-436D-AAB4-9262ED44D584");

    }
}