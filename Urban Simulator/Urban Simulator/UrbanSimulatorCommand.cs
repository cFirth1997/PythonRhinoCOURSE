using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Urban_Simulator
{
    [System.Runtime.InteropServices.Guid("b528ce08-f9ec-4f22-8d4b-d789a994b3e4")]
    public class UrbanSimulatorCommand : Command
    {
        public UrbanSimulatorCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static UrbanSimulatorCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "UrbanSimulator"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            RhinoApp.WriteLine("The Urban Simulator has begun.");

            UrbanModel TheUrbanModel = new UrbanModel();




           if (!getPrecinct(TheUrbanModel));                          //ask user to select surface representing precinct
            return Result.Failure;

           if (! generateRoadNetwork(TheUrbanModel));                   // Using precinct generate road network
            return Result.Failure;
            createBlocks(TheUrbanModel);                                // Using road network generates blocks

           if (!subdivideBlocks(TheUrbanModel, 30, 15));                    // subdivide block into plots
            return Result.Failure; 
            //instantiateBulidings                                   // place bulidings on plots

            RhinoApp.WriteLine("The Urban Simulator is complete.");

            RhinoDoc.ActiveDoc.Views.Redraw();

            return Result.Success;
        }

          public bool getPrecinct(UrbanModel model)
        {

            GetObject obj = new GetObject();
            obj.GeometryFilter = Rhino.DocObjects.ObjectType.Surface;
            obj.SetCommandPrompt("Please set surface for precinct");

            GetResult res = obj.Get();
            
                if (res != GetResult.Object)
            { RhinoApp.WriteLine("User failed to select surface");
                return false;
            }
            
                if (obj.ObjectCount == 1)
                    model.PrecinctSrf = obj.Object(0).Surface();
                return true;
            

        }
          public bool generateRoadNetwork(UrbanModel model)

        {
            int noIterations = 4;

            Random RndRoadT = new Random();
            List<Curve> obstCrvs = new List<Curve>();

            //extract edges from the precinct- temp geometry
            Curve[] borderCrvs = model.PrecinctSrf.ToBrep().DuplicateNakedEdgeCurves(true, false);

            foreach (Curve itCrv in borderCrvs)
                obstCrvs.Add(itCrv);



            if(borderCrvs.Length > 0)
            { int noBorders = borderCrvs.Length;
                Random rnd = new Random();
               Curve theCrv = borderCrvs [rnd.Next(noBorders)];
                

                recursivePerpLine(theCrv,ref  obstCrvs, RndRoadT, 1, noIterations);



            }


            model.roadNetworks = obstCrvs;
            if (obstCrvs.Count > borderCrvs.Length)
                return true;

            else
                return false;




        }
          public bool recursivePerpLine(Curve inpCrv,  ref    List<Curve> inpObst, Random inpRnd, int dir, int cnt)

 { if (cnt < 1)
                return false;

            //select a random point from one of the edges
            double t = inpRnd.Next(30,70) / 100.0;
            Plane PerpFrm;

            Point3d pt = inpCrv.PointAtNormalizedLength(t);
            inpCrv.PerpendicularFrameAt(t, out PerpFrm);

            Point3d pt2 = Point3d.Add(pt, PerpFrm.XAxis * dir);

            // draw a line perpindicular
            Line ln = new Line(pt, pt2);
            Curve lnExt = ln.ToNurbsCurve().ExtendByLine(CurveEnd.End, inpObst);

            if (lnExt == null)
                return false;

            inpObst.Add(lnExt);

            RhinoDoc.ActiveDoc.Objects.AddLine(lnExt.PointAtStart, lnExt.PointAtEnd);

            RhinoDoc.ActiveDoc.Objects.AddPoint(pt);
            RhinoDoc.ActiveDoc.Views.Redraw();

            recursivePerpLine(lnExt, ref inpObst,inpRnd, 1, cnt - 1);
            recursivePerpLine(lnExt, ref inpObst, inpRnd, -1, cnt - 1);

            return true;

        }



        public bool createBlocks(UrbanModel model)
        { 

        Brep precinctPolySrf =    model.PrecinctSrf.ToBrep().Faces[0].Split(model.roadNetworks,RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            List<Brep> blocks = new List<Brep>();

            foreach(BrepFace itBF in precinctPolySrf.Faces)
            { Brep ItBlock = itBF.DuplicateFace(false);
                ItBlock.Faces.ShrinkFaces();
                    blocks.Add(ItBlock);
                RhinoDoc.ActiveDoc.Objects.AddBrep(ItBlock);
                    }
            if (blocks.Count > 0)
            {
                model.blocks = blocks;
                return true;
            }

           else {
                return false;
            }

        }


        public bool subdivideBlocks(UrbanModel model, int minPlotDepth, int maxPlotWidth)
        {
            foreach (Brep itBlock in model.blocks)
            {

                List<Curve> splitLines = new List<Curve>();

                itBlock.Faces[0].SetDomain(0, new Interval(0, 1));
                itBlock.Faces[0].SetDomain(1, new Interval(0, 1));

                Point3d pt1 = itBlock.Faces[0].PointAt(0, 0);
                Point3d pt2 = itBlock.Faces[0].PointAt(0, 1);
                Point3d pt3 = itBlock.Faces[0].PointAt(1, 1);
                Point3d pt4 = itBlock.Faces[0].PointAt(1, 0);

                

                double length = pt1.DistanceTo(pt2);
                double width = pt1.DistanceTo(pt4);

                if (length > width)
                { if (width > (minPlotDepth * 2))
                    {
                        Point3d sdPt1 = itBlock.Surfaces[0].PointAt(0.5, 0);
                        Point3d sdPt2 = itBlock.Surfaces[0].PointAt(0.5, 1);

                        Line subDline = new Line(sdPt1, sdPt2);
                        Curve subDCrv = subDline.ToNurbsCurve();

                        splitLines.Add(subDCrv);

                        RhinoDoc.ActiveDoc.Objects.AddLine(sdPt1, sdPt2);
                        RhinoDoc.ActiveDoc.Views.Redraw();
                    }

                    }
                else
                    {
                        if (length > (minPlotDepth * 2))
                        {
                            Point3d sdPt1 = itBlock.Surfaces[0].PointAt(0, 0.5);
                            Point3d sdPt2 = itBlock.Surfaces[0].PointAt(1, 0.5);

                            Line subDline = new Line(sdPt1, sdPt2);
                        Curve subDCrv = subDline.ToNurbsCurve();

                        splitLines.Add(subDCrv);

                            RhinoDoc.ActiveDoc.Objects.AddLine(sdPt1, sdPt2);
                            RhinoDoc.ActiveDoc.Views.Redraw();

                        }
                  }

                //check the dimensions 
                //find the shorter dimensions
                //validate if should be subdivide
                //if so subdidivde half way
                //hten si=ubdivide into smaller plots based on minim plot width


            }
        
        
        
        
        
        return true;
        
       }

        
    }
}
