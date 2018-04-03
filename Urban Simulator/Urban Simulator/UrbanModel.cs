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

            if (this.plotSrf.GetArea() < 50)
             return false;
            

            if (inpPlotType > 0)
            {
                int minBldHeight = 0;
                int maxBldHeight = 3;


                if (inpPlotType == 1)
                {
                    minBldHeight = 3;
                    maxBldHeight = 9;
                }

                if (inpPlotType == 2)
                {
                    minBldHeight = 18;
                    maxBldHeight = 30;
                }

                if (inpPlotType == 3)
                {
                     minBldHeight = 60;
                     maxBldHeight = 150;
                }

                double actualBulidingHeight = this.bldHeights.Next(minBldHeight, maxBldHeight);

                System.Drawing.Color bldCol = System.Drawing.Color.White;   

                if (actualBulidingHeight < 6);
                bldCol = System.Drawing.Color.FromArgb(168, 126, 198);
                else if (actualBulidingHeight <12)
                    bldCol = System.Drawing.Color.FromArgb(255, 173, 194);
                else if (actualBulidingHeight < 36)
                    bldCol = System.Drawing.Color.FromArgb(243, 104, 75);
                else if (actualBulidingHeight <92)
                    bldCol = System.Drawing.Color.FromArgb(225, 164, 24);
                else if (actualBulidingHeight <120)
                    bldCol = System.Drawing.Color.FromArgb(254, 255, 51);


                ObjectAttributes  oa = new ObjectAttributes
                    oa.ColorSource = ObjectColorSource.ColorFromObjects;
                    oa.ObjectColor = System.Drawing.Color.FromArgb() 


                Curve border = Curve.JoinCurves(this.plotSrf.DuplicateNakedEdgeCurves(true, false))[0];
                this.bulidingOutlines = Curve.JoinCurves(border.Offset(Plane.WorldXY, -5, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.None))[0];



                this.buliding = Extrusion.Create(this.bulidingOutlines, actualBulidingHeight, true);
                RhinoDoc.ActiveDoc.Objects.AddExtrusion(this.buliding, oa);
                RhinoDoc.ActiveDoc.Objects.AddCurve(bulidingOutlines);
            }

            return true;
    }
                

        }

            
        
    }

  
