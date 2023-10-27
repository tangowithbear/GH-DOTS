using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Collections;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IsovistTest {
    public class SpatialUnit {

        public string SUID      { get; set; }
        public Point3d Point3d  { get; set; }
        public double Area      { get; set; }

        /// public double Height { get; set; }
        ///public int Level { get; set; }

        ///...........................VIEW CONTENT.......................
        
        public double ViewContent_SkyPercentage      { get; set; }
        public double ViewContent_GroundPercentage   { get; set; }
        public double ViewContent_BuiltPercentage    { get; set; }

        /// ...........................ISOVIST...........................



       // public Brep[]  Isovist_Ext_Isovist             { get; set; }
        public int     Isovist_Radius                    { get; set; }
        public Curve   Isovist_Ext_PerimeterCurve        { get; set; }
        public double  Isovist_Ext_Area                  { get; set; }
        public double  Isovist_Ext_Perimeter             { get; set; }
        public double  Isovist_Ext_Compactness           { get; set; }
        public double  Isovist_Ext_Occlusivity           { get; set; }

       // public Brep[]  Isovist_Int_Isovist             { get; set; }
        public Curve   Isovist_Int_PerimeterCurve        { get; set; }
        public Point3d Isovist_Int_CentreOfGravity       { get; set; }
        public double  Isovist_Int_Area                  { get; set; }
        public double  Isovist_Int_Perimeter             { get; set; }
        public int     Isovist_Int_NumberOfVertices      { get; set; }
        public int     Isovist_Int_DriftDirection        { get; set; }
        public double  Isovist_Int_DriftMagnitude        { get; set; }
        public double  Isovist_Int_Compactness           { get; set; }
        public double  Isovist_Int_Occlusivity           { get; set; }

        public double Isovist_Int_MajorAxisLength { get; set; }
        public double Isovist_Int_MajorAxisOrientation { get; set; }
        public double Isovist_Int_MinorAxisLength { get; set; }
        public double Isovist_Int_MinorAxisOrientation { get; set; }

        public int Orientation_Ext_EastScore             { get; set; }
        public int Orientation_Ext_NordEastScore         { get; set; }
        public int Orientation_Ext_NordScore             { get; set; }
        public int Orientation_Ext_NordWestScore         { get; set; }
        public int Orientation_Ext_WestScore             { get; set; }
        public int Orientation_Ext_SouthWestScore        { get; set; }
        public int Orientation_Ext_SouthScore            { get; set; }
        public int Orientation_Ext_SouthEastScore        { get; set; }


        ///public double Skewnes;

        /// ...........................VISIBILITY...........................

        public double Visibility_Percentage         { get; set; }

        //public int    Visibility_Threshold      { get; set; }
        //public bool   Visibility_Visibility     { get; set; }

        ///...........................CONNECTIVITY......................... 

        public double               Connectivity_Percentage         { get; set; }
        public int                  Connectivity_NumberOfVisibleSUs { get; set; }
        public HashSet<SpatialUnit> Connectivity_VisibleUnits       { get; set; }
        public List<SpatialUnit>    Connectivity_FarthestVisibleSUs { get; set; }
        public int                  Connectivity_ThroughVision      { get; set; }

        /// public List<Tuple<SpatialUnit,Point3d,double, bool>>[]  { get; set; }
        /// public List<double> Distance                 { get; set; }
        /// public List<bool>   SolidTransparentObstacle { get; set; }


        public  SpatialUnit(Point3d point3d) {

            this.SUID = point3d.X.ToString() + "_" + point3d.Y.ToString() + "_" + point3d.Z.ToString();
            this.Point3d = point3d;
        }
    }
}

