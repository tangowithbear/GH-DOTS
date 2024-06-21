using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Collections;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DOTS {
    public class SpatialUnit {

        /// ...........................CAST...........................

        public string   SUID         { get; set; }
        public Point3d  Gen_Point3d      { get; set; }
        public double   Gen_X            { get; set; }
        public double   Gen_Y            { get; set; }
        public double   Gen_Z            { get; set; }
        public double   Gen_Area         { get; set; }


        /// ...........................MODEL...........................

        public double?  Model_Height                { get; set; }
        public double?  Model_FloorHeight           { get; set; }
        public double?  Model_DistanceToStructure   { get; set; }
        public double?  Model_DistancetToEnvelope   { get; set; }


        /// ...........................ISOVIST...........................


        // public Brep[]  Isovist_Ext_Isovist                { get; set; }
        public int?     Isovist_Radius                      { get; set; }
        public Curve    Isovist_Ext_PerimeterCurve          { get; set; }
        public double?  Isovist_Ext_MinVista                { get; set; }
        public double?  Isovist_Ext_MaxVista                { get; set; }
        public double?  Isovist_Ext_Area                    { get; set; }
        public double?  Isovist_Ext_Perimeter               { get; set; }
        public double?  Isovist_Ext_Compactness             { get; set; }
        public double?  Isovist_Ext_Occlusivity             { get; set; }

       // public Brep[]  Isovist_Int_Isovist                { get; set; }
        public Curve    Isovist_Int_PerimeterCurve          { get; set; }
        public double?  Isovist_Int_MinVista                { get; set; }
        public double?  Isovist_Int_MaxVista                { get; set; }
        public Point3d  Isovist_Int_CentreOfGravity         { get; set; }
        public double?  Isovist_Int_Area                    { get; set; }
        public double?  Isovist_Int_Perimeter               { get; set; }
        public int?     Isovist_Int_NumberOfVertices        { get; set; }
        public int?     Isovist_Int_DriftDirection          { get; set; }
        public double?  Isovist_Int_DriftMagnitude          { get; set; }
        public double?  Isovist_Int_Compactness             { get; set; }
        public int?     Isovist_Int_Occlusivity             { get; set; }

        public double? Isovist_Int_MajorAxisLength           { get; set; }
        public double? Isovist_Int_MajorAxisOrientation      { get; set; }
        public double? Isovist_Int_MinorAxisLength           { get; set; }
        public double? Isovist_Int_MinorAxisOrientation      { get; set; }

        public int? ViewAccess_Ext_EastArea      { get; set; }
        public int? ViewAccess_Ext_NordEastArea  { get; set; }
        public int? ViewAccess_Ext_NordArea      { get; set; }
        public int? ViewAccess_Ext_NordWestArea  { get; set; }
        public int? ViewAccess_Ext_WestArea      { get; set; }
        public int? ViewAccess_Ext_SouthWestArea { get; set; }
        public int? ViewAccess_Ext_SouthArea     { get; set; }
        public int? ViewAccess_Ext_SouthEastArea { get; set; }



        ///public double Skewnes;

        /// ...........................VISIBILITY...........................

        public double? Visibility_Percentage         { get; set; }

        //public int    Visibility_Threshold      { get; set; }
        //public bool   Visibility_Visibility     { get; set; }

        ///...........................CONNECTIVITY......................... 

        public double? Connectivity_Percentage                      { get; set; }
        public int?  Connectivity_NumberOfVisibleSUs              { get; set; }
        public HashSet<SpatialUnit> Connectivity_VisibleUnits       { get; set; }
        public List<SpatialUnit> Connectivity_FarthestVisibleSUs    { get; set; }
        public int? Connectivity_ThroughVision                    { get; set; }

        /// public List<Tuple<SpatialUnit,Point3d,double, bool>>[]  { get; set; }
        /// public List<double> Distance                 { get; set; }
        /// public List<bool>   SolidTransparentObstacle { get; set; }


        ///...........................VIEW CONTENT.......................

        public int ViewContent_SkyPercentage                     { get; set; }
        public int ViewContent_GroundPercentage                  { get; set; }
        public int ViewContent_BuiltPercentage                   { get; set; }




        public  SpatialUnit(Point3d point3d) {

            //this.SUID = point3d.Gen_X.ToString() + "_" + point3d.Gen_Y.ToString() + "_" + point3d.Gen_Z.ToString();
            this.SUID = null;
            this.Gen_Point3d = point3d;
            this.Gen_X = point3d.X;
            this.Gen_Y = point3d.Y;
            this.Gen_Z = point3d.Z;

            this.Model_Height = null;
            this.Model_FloorHeight = null;
            this.Model_DistanceToStructure = null;
            this.Model_DistancetToEnvelope = null;

            this.Isovist_Int_MajorAxisOrientation = null;
            this.Isovist_Int_MinorAxisOrientation = null;
            this.Isovist_Int_MajorAxisLength = null;
            this.Isovist_Int_MinorAxisLength = null;

            this.ViewContent_BuiltPercentage = 0;
            this.ViewContent_GroundPercentage = 0;
            this.ViewContent_SkyPercentage = 0;

            this.Connectivity_Percentage = null;
            this.Connectivity_NumberOfVisibleSUs = null;
            this.Connectivity_ThroughVision = null;

            this.ViewAccess_Ext_EastArea = null;
            this.ViewAccess_Ext_NordEastArea = null;
            this.ViewAccess_Ext_NordArea = null;
            this.ViewAccess_Ext_NordWestArea = null;
            this.ViewAccess_Ext_WestArea = null;
            this.ViewAccess_Ext_SouthWestArea = null;
            this.ViewAccess_Ext_SouthArea = null;
            this.ViewAccess_Ext_SouthEastArea = null;


            this.Isovist_Radius = 1000;
            this.Isovist_Ext_Area = null;
            this.Isovist_Ext_Compactness = null;
            this.Isovist_Ext_Occlusivity = null;
            this.Isovist_Ext_Perimeter = null;
            this.Isovist_Ext_PerimeterCurve = null;
            this.Isovist_Ext_MaxVista = null;
            this.Isovist_Ext_MinVista = null;
            this.Isovist_Int_Area = null;
            this.Isovist_Int_CentreOfGravity = Point3d.Unset;
            this.Isovist_Int_Compactness = null;
            this.Isovist_Int_DriftDirection = null;
            this.Isovist_Int_DriftMagnitude = null;
            this.Isovist_Int_NumberOfVertices = null;
            this.Isovist_Int_Occlusivity = null;
            this.Isovist_Int_Perimeter = null;
            this.Isovist_Int_PerimeterCurve = null;
            this.Isovist_Int_MaxVista = null;
            this.Isovist_Int_MinVista = null;

            this.Visibility_Percentage = null;


        }
    }
}

