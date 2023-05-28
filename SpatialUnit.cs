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

        public string SUID { get; set; }
        public Point3d Point3d { get; set; }

        /// public double Height { get; set; }
        ///public int Level { get; set; }


        ///public List <Polyline> SkyPolygone  { get; set; }
        ///public List <Polyline> GroundPolygone {  }

        /// ...........................ISOVIST...........................

        ///public brep InteriorIsovist;
        ///public brep ExteriorIsovist;
        ///public double IntIsovistArea;
        ///public double ExtIsovistArea;
        ///public string MainViewDirection;
        ///public double DriftMagnetude;

        /// ...........................VISIBILITY...........................

        ///public int Percentage;
        ///public int Threshold;
        ///public int visibility;

        ///...........................CONNCETIVITY.........................


        public  SpatialUnit(Point3d point3d) {

            this.SUID = point3d.X.ToString() + "_" + point3d.Y.ToString() + "_" + point3d.Z.ToString();
            this.Point3d = point3d;

        }
    }
}
