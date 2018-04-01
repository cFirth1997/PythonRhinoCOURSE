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
            
            RhinoApp.WriteLine("The {0} command will add a line right now.", EnglishName);

           
            RhinoApp.WriteLine("The {0} command added one line to the document.", EnglishName);

            // ---

            return Result.Success;
        }
    }
}
