using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using StickFacadeDataExtraction;

namespace StickFacadeDataExtraction.Reporting
{
    public class FacadeDataExporter
    {
        public void ExportToCsvInDrawingFolder(List<Mullion> mullions, List<Transom> transoms)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null || string.IsNullOrEmpty(doc.Name))
            {
                doc?.Editor.WriteMessage("\nNie można ustalić ścieżki aktywnego rysunku.");
                return;
            }

            var folder = Path.GetDirectoryName(doc.Name);
            var filePath = Path.Combine(folder, "FacadeReport.csv");

            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Nagłówek
                    writer.WriteLine("Type;Tag;Layer;StartX;StartY;EndX;EndY;Position;WindPressure;WindSuction;Dist1;Dist2");

                    // Mullions
                    foreach (var m in mullions)
                    {
                        writer.WriteLine($"Mullion;{m.Tag};{m.Layer};{m.Start.X};{m.Start.Y};{m.End.X};{m.End.Y};{m.Position};{Format(m.WindPressureValue)};{Format(m.WindSuctionValue)};{Format(m.DistanceToNeighbour1)};{Format(m.DistanceToNeighbour2)}");
                    }

                    // Transoms
                    foreach (var t in transoms)
                    {
                        writer.WriteLine($"Transom;{t.Tag};{t.Layer};{t.Start.X};{t.Start.Y};{t.End.X};{t.End.Y};{t.Position};{Format(t.WindPressureValue)};{Format(t.WindSuctionValue)};{Format(t.DistanceToNeighbour1)};{Format(t.DistanceToNeighbour2)}");
                    }
                }

                doc.Editor.WriteMessage($"\nZapisano raport do: {filePath}");
            }
            catch (IOException ex)
            {
                doc.Editor.WriteMessage($"\nBłąd zapisu pliku CSV: {ex.Message}");
            }
        }

        private string Format(double? value)
        {
            return value.HasValue ? value.Value.ToString("F2", new CultureInfo("pl-PL")) : "";
        }
    }
}
