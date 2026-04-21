using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using StickFacadeDataExtraction.Reporting;
using System.Collections.Generic;

namespace StickFacadeDataExtraction
{
    public class CoreProgram
    {
        [CommandMethod("GETFACADEDATA")]
        public void Run()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Editor ed = doc.Editor;

            ed.WriteMessage("\nStarting collecting facade data...\n");

            // Create and run the processor
            FacadeDataProcessor processor = new FacadeDataProcessor(doc);
            processor.ProcessFacadeData();

            

            ed.WriteMessage("\nFinished collecting facade data.\n");

            
            // Use the reporter and pass the data from processor
            var reporter = new FacadeDataReporter();
            reporter.ReportMullions(processor.Mullions);
            reporter.ReportTransoms(processor.Transoms);
        }

    }


}
