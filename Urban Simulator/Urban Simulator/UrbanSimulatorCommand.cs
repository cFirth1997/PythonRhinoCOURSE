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




            getPrecinct(TheUrbanModel);                 //ask user to select surface representing precinct
            generateRoadNetwork(TheUrbanModel);           // Using precinct generate road network
            //createBlocks()                   // Using road network generates blocks
            //subdivideBlocks()                 // subdivide block into plots
            //instantiateBulidings               // place bulidings on plots

            RhinoApp.WriteLine("The Urban Simulator is complete.");
            return Result.Success;
        }

        public bool getPrecinct(UrbanModel model)
        {

            GetObject obj = new GetObject();
            obj.GeometryFilter = Rhino.DocObjects.ObjectType.Surface;
            obj.SetCommandPrompt("Please set surface for precinct");

            GetResult res = obj.Get();

            if (res != GetResult.Object)
                return false;

            if (obj.ObjectCount == 1)

                model.PrecinctSrf = obj.Object(0).Surface();




            return true;

        }
    
    
        public bool generateRoadNetwork(UrbanModel model)

        {
            int noIterations = 1;

            List<Curve> obstCrvs = new List<CurveEnd>(); 

            //extract edges from the precinct- temp geometry
           Curve[] borderCrvs = model.PrecinctSrf.ToBrep().DuplicateNakedEdgeCurves(true, false);

            foreach (Curve itCrv in borderCrvs)
                obstCrvs.Add(itCrv);



            if(borderCrvs.Length > 0)
            { int noBorders = borderCrvs.Length;
                Random rnd = new Random();
               Curve theCrv = borderCrvs [rnd.Next(noBorders)];


                //select a random point from one of the edges
                double t = new Random().NextDouble();
                Plane PerpFrm;



              Point3d pt =   theCrv.PointAtNormalizedLength(t);
                theCrv.PerpendicularFrameAt(t, out PerpFrm);

                Point3d pt2 = Point3d.Add(pt, PerpFrm.XAxis);

                // draw a line perpindicular
                Line ln = new Line(pt, pt2);
                Curve lnExt = ln.ToNurbsCurve().ExtendByLine(CurveEnd.End, borderCrvs);

                obstCrvs.Add(lnExt);

                recursivePerpLine(theCrv,ref     obstCrvs,1,  2);


                // draw a line perpindicular

            }



            // draw a line perpindicular
            // collect and repeat the process (select random point and draw perp line) 
            //
            return true;


        }
        public bool recursivePerpLine(Curve inpCrv,  ref    List<Curve> inpObst, int dir, int cnt)


        {

            if (cnt < 1)
                return false;

            //select a random point from one of the edges
            double t = new Random().NextDouble();
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

            recursivePerpLine(lnExt, ref inpObst, 1, cnt - 1);
            recursivePerpLine(lnExt, ref inpObst, -1, cnt - 1);

            return true;

        }

    }
}
