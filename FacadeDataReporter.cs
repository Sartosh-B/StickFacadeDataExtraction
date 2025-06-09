using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using StickFacadeDataExtraction; // Dodaj, jeśli klasy Mullion/Transom są w tym namespace

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
                _editor.WriteMessage($"\nMullion #{index++}\n");
                ReportCommonFacadeElementData(mullion);
                _editor.WriteMessage($"  Position: {mullion.Position ?? "brak"}\n");
                _editor.WriteMessage($"  Wind Suction: {FormatNullable(mullion.WindSuctionValue)}\n");
                _editor.WriteMessage($"  Wind Pressure: {FormatNullable(mullion.WindPressureValue)}\n");
                _editor.WriteMessage($"  Distance to Neighbour 1: {FormatNullable(mullion.DistanceToNeighbour1)}\n");
                _editor.WriteMessage($"  Distance to Neighbour 2: {FormatNullable(mullion.DistanceToNeighbour2)}\n");
            }
        }

        public void ReportTransoms(List<Transom> transoms)
        {
            _editor.WriteMessage($"\n--- TRANSOMS ({transoms.Count}) ---\n");
            int index = 1;
            foreach (var transom in transoms)
            {
                _editor.WriteMessage($"\nTransom #{index++}\n");
                ReportCommonFacadeElementData(transom);
                _editor.WriteMessage($"  Position: {transom.Position ?? "brak"}\n");
                _editor.WriteMessage($"  Wind Suction: {FormatNullable(transom.WindSuctionValue)}\n");
                _editor.WriteMessage($"  Wind Pressure: {FormatNullable(transom.WindPressureValue)}\n");
                _editor.WriteMessage($"  Distance to Neighbour 1: {FormatNullable(transom.DistanceToNeighbour1)}\n");
                _editor.WriteMessage($"  Distance to Neighbour 2: {FormatNullable(transom.DistanceToNeighbour2)}\n");
            }
        }

        private void ReportCommonFacadeElementData(FacadeElement element)
        {
            _editor.WriteMessage($"  Layer: {element.Layer ?? "brak"}\n");
            _editor.WriteMessage($"  Start: {element.Start}\n");
            _editor.WriteMessage($"  End: {element.End}\n");
        }

        private string FormatNullable(double? value)
        {
            return value.HasValue ? value.Value.ToString("F2") : "brak";
        }
    }
}
