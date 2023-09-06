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

        public string SUID     { get; set; }
        public Point3d Point3d { get; set; }

        /// public double Height { get; set; }
        ///public int Level { get; set; }

        ///...........................VIEW CONTENT.......................
        
        public double ViewContent_SkyPercentage      { get; set; }
        public double ViewContent_GroundPercentage   { get; set; }
        public double ViewContent_BuiltPercentage    { get; set; }

        /// ...........................ISOVIST...........................

        public Brep[]  Isovist_InteriorIsovist   { get; set; }
        public double  Isovist_IntIsovistArea    { get; set; }

        public Brep[]  Isovist_ExteriorIsovist   { get; set; }
        public double  Isovist_ExtIsovistArea    { get; set; }
        public Point3d Isovist_CentreOfGravity   { get; set; }
        public double  Isovist_DriftMagnitude    { get; set; }

        ///public string MainViewDirection;
        ///public double DriftMagnetude;
        ///public double Max Radius;
        ///public double Compactness;
        ///public double Skewnes;

        /// ...........................VISIBILITY...........................

        public double Visibility_Percentage  { get; set; }
        public int    Visibility_Threshold   { get; set; }
        public bool   Visibility_Visibility  { get; set; }

        ///...........................CONNECTIVITY......................... 

        public double               Connectivity_Percentage         { get; set; }
        public int                  Connectivity_NumberOfVisibleSUs { get; set; }
        public List<Point3d>        Connectivity_VisibleTestPoints  { get; set; }
        public HashSet<SpatialUnit> Connectivity_VisibleUnits       { get; set; }

       /// public List<Tuple<SpatialUnit,Point3d,double, bool>>[]  { get; set; }
       /// public List<double> Distance                 { get; set; }
       /// public List<bool>   SolidTransparentObstacle { get; set; }


    public  SpatialUnit(Point3d point3d) {

            this.SUID = point3d.X.ToString() + "_" + point3d.Y.ToString() + "_" + point3d.Z.ToString();
            this.Point3d = point3d;



            this.Visibility_Percentage = 0;
            this.Visibility_Threshold  = 0;
            this.Visibility_Visibility = false;

        }
    }
}
