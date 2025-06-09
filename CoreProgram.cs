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

            ed.WriteMessage("\nRozpoczynam zbieranie danych fasady...\n");

            // Utwórz i uruchom procesor
            FacadeDataProcessor processor = new FacadeDataProcessor(doc);
            processor.ProcessFacadeData();

            

            ed.WriteMessage("\nZakończono zbieranie danych fasady.\n");

            // Użyj reporterów i przekaż im dane z processor
            var reporter = new FacadeDataReporter();
            reporter.ReportMullions(processor.Mullions);
            reporter.ReportTransoms(processor.Transoms);
        }

    }


}
