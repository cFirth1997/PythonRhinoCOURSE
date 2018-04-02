using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Urban_Simulator
{
   public class UrbanModel
    {
         public string name = "Urban Model";
         public Surface PrecinctSrf;
        public List<Curve> roadNetworks;
        public List<Brep> blocks;
        public List<Brep> plot;

        public UrbanModel()

        {
        }


    }
}
