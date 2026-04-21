using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;


namespace StickFacadeDataExtraction.Reporting
{
    public class FacadeDataReporter
    {
        private readonly Editor _editor;

        public FacadeDataReporter()
        {
            _editor = Application.DocumentManager.MdiActiveDocument.Editor;
        }

        public void ReportMullions(List<Mullion> mullions)
        {
            _editor.WriteMessage($"\n--- MULLIONS ({mullions.Count}) ---\n");
            int index = 1;
            foreach (var mullion in mullions)
            {
                string line = $"Mullion #{index++}: ";
                line += ReportCommonFacadeElementData(mullion) + ", ";
                line += $"Position: {mullion.Position ?? "brak"}, ";
                line += $"TAG: {mullion.Tag ?? "brak"}, ";
                line += $"Wind Suction: {FormatNullable(mullion.WindSuctionValue)}, ";
                line += $"Wind Pressure: {FormatNullable(mullion.WindPressureValue)}, ";
                line += $"Distance to Neighbour 1: {FormatNullable(mullion.DistanceToNeighbour1)}, ";
                line += $"Distance to Neighbour 2: {FormatNullable(mullion.DistanceToNeighbour2)}";
                _editor.WriteMessage(line + "\n");
            }
        }

        public void ReportTransoms(List<Transom> transoms)
        {
            _editor.WriteMessage($"\n--- TRANSOMS ({transoms.Count}) ---\n");
            int index = 1;
            foreach (var transom in transoms)
            {
                string line = $"Transom #{index++}: ";
                line += ReportCommonFacadeElementData(transom) + ", ";
                line += $"TAG: {transom.Tag ?? "brak"}, ";
                line += $"Position: {transom.Position ?? "brak"}, ";
                line += $"Wind Suction: {FormatNullable(transom.WindSuctionValue)}, ";
                line += $"Wind Pressure: {FormatNullable(transom.WindPressureValue)}, ";
                line += $"Distance to Neighbour 1: {FormatNullable(transom.DistanceToNeighbour1)}, ";
                line += $"Distance to Neighbour 2: {FormatNullable(transom.DistanceToNeighbour2)}";
                _editor.WriteMessage(line + "\n");
            }
        }

        private string ReportCommonFacadeElementData(FacadeElement element)
        {
            return $"Layer: {element.Layer ?? "brak"}, Start: {element.Start}, End: {element.End}";
        }

        private string FormatNullable(double? value)
        {
            return value.HasValue ? value.Value.ToString("F2") : "brak";
        }
    }
}
