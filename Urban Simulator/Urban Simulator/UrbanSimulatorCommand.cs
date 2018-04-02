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

           if (!subdivideBlocks(TheUrbanModel, 30, 20));                    // subdivide block into plots
            return Result.Failure;


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
            int noIterations = 6;

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
            Random blockType = Random();
        Brep precinctPolySrf =    model.PrecinctSrf.ToBrep().Faces[0].Split(model.roadNetworks,RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            List<block> blocks = new List<block>();

            foreach(BrepFace itBF in precinctPolySrf.Faces)
            { Brep ItBlock = itBF.DuplicateFace(false);
                ItBlock.Faces.ShrinkFaces();
                int theBlockType = blockType.Next(5);

                    blocks.Add(new block (ItBlock, theBlockType))   ;
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
            
            foreach (block itBlock in model.blocks)
            {
                
                Brep itSrf = itBlock.blockSrf;
                itBlock.plot = new List<Brep>();
                Curve[] borderCrvs = itSrf.DuplicateNakedEdgeCurves(true, false);

                List<Curve> splitLines = new List<Curve>();

                itSrf.Faces[0].SetDomain(0, new Interval(0, 1));
                iitSrf.Faces[0].SetDomain(1, new Interval(0, 1));

                Point3d pt1 = itSrf.Faces[0].PointAt(0, 0);
                Point3d pt2 = itSrf.Faces[0].PointAt(0, 1);
                Point3d pt3 = itSrf.Faces[0].PointAt(1, 1);
                Point3d pt4 = itSrf.Faces[0].PointAt(1, 0);

                

                double length = pt1.DistanceTo(pt2);
                double width = pt1.DistanceTo(pt4);

                Point3d sdPt1 = new Point3d();
                Point3d sdPt2 = new Point3d();

                if (length > width)
                {
                    if (width > (minPlotDepth * 2))
                    {
                       sdPt1 = itSrf.Surfaces[0].PointAt(0.5, 0);
                        sdPt2 = itSrf.Surfaces[0].PointAt(0.5, 1);



                    }
                    else
                    {
                        if (length > (minPlotDepth * 2))
                        {
                           sdPt1 = itSrf.Surfaces[0].PointAt(0, 0.5);
                            sdPt2 = itSrf.Surfaces[0].PointAt(1, 0.5);



                        }
                    }
                    Line subDline = new Line(sdPt1, sdPt2);
                    Curve subDCrv = subDline.ToNurbsCurve();

                    splitLines.Add(subDCrv);

                    double crvLength = subDCrv.GetLength();
                    double noPlots = Math.Floor(crvLength / maxPlotWidth);

                    for(int t= 0; t < noPlots; t ++)
                    {
                       double    tVal = t* 1 / noPlots;
                        Plane PerpFrm;

                        Point3d evalPt = subDCrv.PointAtNormalizedLength(tVal);

                        subDCrv.PerpendicularFrameAt(tVal, out PerpFrm);

                        Point3d ptPer2Up = Point3d.Add(evalPt, PerpFrm.XAxis);
                        Point3d ptPer2Down = Point3d.Add(evalPt, PerpFrm.XAxis);

                        // draw a line perpindicular
                        Line ln1 = new Line(evalPt, ptPer2Up);
                        Line ln2 = new Line(evalPt, ptPer2Down);


                        Curve lnExt1 = ln1.ToNurbsCurve().ExtendByLine(CurveEnd.End, borderCrvs);

                        Curve lnExt2 = ln2.ToNurbsCurve().ExtendByLine(CurveEnd.End, borderCrvs);

                        splitLines.Add(lnExt1);
                        splitLines.Add(lnExt2);





                    }

                    Brep plotPolySrf = itSrf.Faces[0].Split(splitLines, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                    

                    foreach (BrepFace itBF in plotPolySrf.Faces)
                    {
                        Brep itPlot = itBF.DuplicateFace(false);
                        itPlot.Faces.ShrinkFaces();
                        itBlock.plot.Add(itPlot );
                        RhinoDoc.ActiveDoc.Objects.AddBrep(itPlot);
                    }
                  
                    
                    RhinoDoc.ActiveDoc.Views.Redraw();
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
