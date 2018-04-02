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
        public List<block> blocks;
    
        public UrbanModel()

        {
        }


    }
    public class block
    {
        public int type;  //1= park, 2 = low rise , 3 mide rise, 4 = high rise
        public Brep blockSrf;

        public List<plot> plot;
        public block(Brep inpSrf, int inpType)
        {
            this.blockSrf = inpSrf;
            this.type = inpType;
        }
    }

    public class plot
    {
        public Brep plotSrf;
        public Curve bulidingOutlines;
        public Extrusion buliding;

        public plot(Brep inpSrf, int inpPlotType)
        {
            this.plotSrf = inpSrf;
            this.createBuliding(inpPlotType);
        }

        public void createBuliding(int inpPlotType)
        {
            Random bldHeights = new Random();

            if (inpPlotType > 0)
            {
                int minBldHeight = 0;
                int maxBldHeight = 3;


                if (inpPlotType == 1)
                {
                    int minBldHeight = 3;
                    int maxBldHeight = 6;
                }

                if (inpPlotType == 2)
                {
                    int minBldHeight = 9;
                    int maxBldHeight = 18;
                }

                if (inpPlotType == 3)
                {
                    int minBldHeight = 24;
                    int maxBldHeight = 60;
                }
                Curve border = Curve.JoinCurves(this.plotSrf.DuplicateNakedEdgeCurves(true, false))[0];
                this.bulidingOutlines = Curve.JoinCurves(border.Offset(Plane.WorldXY, -5, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.None))[0];



                this.buliding = Extrusion.Create(this.bulidingOutlines, bldHeights.Next(minBldHeight, maxBldHeight), true);
                RhinoDoc.ActiveDoc.Objects.AddExtrusion(this.buliding);
                RhinoDoc.ActiveDoc.Objects.AddCurve(bulidingOutlines);
            }
        

    }
                

        }

            
        
    }
    }
}  
