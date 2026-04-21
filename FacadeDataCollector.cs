using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;

using System;
using System.Linq;
using System.Diagnostics;
using StickFacadeDataExtraction.Reporting;


namespace StickFacadeDataExtraction
{
    public class FacadeDataProcessor
    {
        private Document _doc;
        private Editor _ed;

        public List<FacadeElement> AllFacadeElements { get; private set; } = new List<FacadeElement>();
        public List<Mullion> Mullions { get; private set; } = new List<Mullion>();
        public List<Transom> Transoms { get; private set; } = new List<Transom>();
        public List<FacadeAreaLoad> AreaLoads { get; private set; } = new List<FacadeAreaLoad>();
        public List<PressureAreaLoad> PressureLoads { get; private set; } = new List<PressureAreaLoad>();
        public List<SuctionAreaLoad> SuctionLoads { get; private set; } = new List<SuctionAreaLoad>();


        public FacadeDataProcessor(Document doc)
        {
            _doc = doc;
            _ed = doc.Editor;
        }

        public void ProcessFacadeData()
        {
            using (Transaction tr = _doc.Database.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(_doc.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord modelSpace = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                // Find all 2-vertex polylines -> FacadeElements
                foreach (ObjectId entId in modelSpace)
                {
                    Entity ent = (Entity)tr.GetObject(entId, OpenMode.ForRead);
                    if (ent is Polyline pl)
                    {
                        if (pl.NumberOfVertices == 2)
                        {
                            FacadeElement fe = CreateFacadeElementFromPolyline(pl);
                            if (fe != null)
                            {
                                AllFacadeElements.Add(fe);
                            }
                        }
                        else if (pl.NumberOfVertices == 4)
                        {
                            FacadeAreaLoad areaLoad = CreateFacadeAreaLoadFromPolyline(pl);
                            if (areaLoad != null)
                            {
                                AreaLoads.Add(areaLoad);
                            }
                        }
                    }
                }

                _ed.WriteMessage($"\nZnaleziono {AllFacadeElements.Count} elementów fasady (2-punktowych).");
                _ed.WriteMessage($"\nZnaleziono {AreaLoads.Count} obciążeń powierzchniowych (4-punktowych).");

                // Separate elements into Mullions and Transoms
                SeparateFacadeElements();

                _ed.WriteMessage($"\nMullions: {Mullions.Count}, Transoms: {Transoms.Count}");

                // (optionally) assign positions if not already assigned
                AssignPositions(Mullions, Transoms);

                AssociateTransomsToMullions(Mullions, Transoms);

                foreach (var m in Mullions)
                {
                    Log($"Mullion {m.Start}-{m.End} -> Transoms: {m.ConnectedTransoms.Count}");
                }

                AssignDistancesToMullions();

                foreach (var m in Mullions)
                {
                    Log($"Mullion {m.Start}-{m.End} Distances: D1={m.DistanceToNeighbour1}, D2={m.DistanceToNeighbour2}");
                }

                AssignTransomNeighbours(Transoms);

                AssignDistanceToTransoms(Transoms);

                SeparateAreaLoads();
                _ed.WriteMessage($"\nRozdzielono: {PressureLoads.Count} Pressure, {SuctionLoads.Count} Suction.");
                foreach (var transom in Transoms)
                {
                    transom.AssignWindLoads(PressureLoads, SuctionLoads);
                }

                foreach (var mullion in Mullions)
                {
                    mullion.AssignWindLoadsFromConnectedTransoms();
                }

                FacadeTagAssigner.AssignTags(Mullions, Transoms);
                FacadeTagDrawer.DrawTags(Mullions, Transoms);

                var exporter = new FacadeDataExporter();
                exporter.ExportToCsvInDrawingFolder(Mullions, Transoms);

                tr.Commit();
            }
        }

        private FacadeElement CreateFacadeElementFromPolyline(Polyline pl)
        {
            // Pobierz start i koniec linii (2 wierzchołki)
            Point2d start = pl.GetPoint2dAt(0);
            Point2d end = pl.GetPoint2dAt(1);

            string layer = pl.Layer;

            var element = new FacadeElement
            {
                Layer = layer,
                Start = start,
                End = end
            };

            element.AnalyzeMetadata();

            return element;
        }

        private FacadeAreaLoad CreateFacadeAreaLoadFromPolyline(Polyline pl)
        {
            string layer = pl.Layer;
            var areaLoad = new FacadeAreaLoad
            {
                Layer = layer
            };

            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                areaLoad.Vertices.Add(pl.GetPoint2dAt(i));
            }

            return areaLoad;
        }

        private void SeparateAreaLoads()
        {
            foreach (var load in AreaLoads)
            {
                if (load.Layer.StartsWith("WP"))
                {
                    var parts = load.Layer.Split('_');
                    if (parts.Length >= 4 && double.TryParse(parts[3], out double val))
                    {
                        var pressure = new PressureAreaLoad
                        {
                            Layer = load.Layer,
                            Vertices = load.Vertices,
                            Direction = parts[1],
                            Zone = parts[2],
                            Value = val
                        };
                        PressureLoads.Add(pressure);
                    }
                }
                else if (load.Layer.StartsWith("WS"))
                {
                    var parts = load.Layer.Split('_');
                    if (parts.Length >= 4 && double.TryParse(parts[3], out double val))
                    {
                        var suction = new SuctionAreaLoad
                        {
                            Layer = load.Layer,
                            Vertices = load.Vertices,
                            Direction = parts[1],
                            Zone = parts[2],
                            Value = val
                        };
                        SuctionLoads.Add(suction);
                    }
                }
            }
        }


        private void SeparateFacadeElements()
        {
            foreach (var elem in AllFacadeElements)
            {
                // Logic: if start.X == end.X => Mullion, if start.Y == end.Y => Transom
                if (elem.Start.X == elem.End.X)
                {
                    Mullion mullion = new Mullion
                    {
                        Layer = elem.Layer,
                        Start = elem.Start,
                        End = elem.End
                       
                    };
                    Mullions.Add(mullion);
                }
                else if (elem.Start.Y == elem.End.Y)
                {
                    Transom transom = new Transom
                    {
                        Layer = elem.Layer,
                        Start = elem.Start,
                        End = elem.End
                    };
                    Transoms.Add(transom);
                }
            }
        }
        private void AssociateTransomsToMullions(List<Mullion> mullions, List<Transom> transoms)
        {
            foreach (var mullion in mullions)
            {
                foreach (var transom in transoms)
                {
                    if (Math.Abs(mullion.Start.X - transom.Start.X) < 1e-3 || Math.Abs(mullion.End.X - transom.End.X) < 1e-3)
                    {
                        if (FacadeGeometryHelper.IsPointOnLine(transom.Start, mullion.Start, mullion.End) ||
                        FacadeGeometryHelper.IsPointOnLine(transom.End, mullion.Start, mullion.End))
                        {
                            mullion.ConnectedTransoms.Add(transom);
                        }
                    }
                }

            }
        }
        private void AssignTransomNeighbours(List<Transom> transoms)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage($"\n[DEBUG] AssignTransomNeighbours start. Transoms count: {transoms.Count}");

            foreach (var transom in transoms)
            {
                var center = transom.GetCenter();
                var candidates = transoms.Where(t => t != transom && Math.Abs(t.Start.X - transom.Start.X) < 1e-3).ToList();

                ed.WriteMessage($"\n[DEBUG] Transom Start: ({transom.Start.X}, {transom.Start.Y}), Position: {transom.Position}, Center: ({center.X}, {center.Y})");
                ed.WriteMessage($" Candidates on same X: {candidates.Count}");

                if (transom.Position == "BOT" || transom.Position == "MID")
                {
                    var upperNeighbour = candidates
                        .Where(t => t.GetCenter().Y > center.Y)
                        .OrderBy(t => t.GetCenter().Y)
                        .FirstOrDefault();

                    transom.UpperNeighbour = upperNeighbour;

                    ed.WriteMessage(" UpperNeighbour: " +
                        (upperNeighbour != null ? $"Start: ({upperNeighbour.Start.X}, {upperNeighbour.Start.Y}), Center: ({upperNeighbour.GetCenter().X}, {upperNeighbour.GetCenter().Y})" : "null"));
                }

                if (transom.Position == "TOP" || transom.Position == "MID")
                {
                    var lowerNeighbour = candidates
                        .Where(t => t.GetCenter().Y < center.Y)
                        .OrderByDescending(t => t.GetCenter().Y)
                        .FirstOrDefault();

                    transom.LowerNeighbour = lowerNeighbour;

                    ed.WriteMessage(" LowerNeighbour: " +
                        (lowerNeighbour != null ? $"Start: ({lowerNeighbour.Start.X}, {lowerNeighbour.Start.Y}), Center: ({lowerNeighbour.GetCenter().X}, {lowerNeighbour.GetCenter().Y})" : "null"));
                }
            }

            ed.WriteMessage("\n[DEBUG] AssignTransomNeighbours end.");
        }

        private void AssignDistancesToMullions()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;

            foreach (var mullion in Mullions)
            {
                ed.WriteMessage($"\nMullion Start.X: {mullion.Start.X}, Position: {mullion.Position}, Connected transoms count: {mullion.ConnectedTransoms.Count}");

                foreach (var t in mullion.ConnectedTransoms)
                {
                    double length = Math.Abs(t.End.X - t.Start.X);
                    ed.WriteMessage($"\n  Transom Start.X: {t.Start.X}, End.X: {t.End.X}, Length: {length}");
                }

                if (mullion.Position == "MID")
                {
                    var leftTransomCandidates = mullion.ConnectedTransoms
                    .Where(t => t.End.X <= mullion.Start.X && t.Start.X <= mullion.Start.X)
                    .OrderByDescending(t => t.End.X)
                    .ToList();

                    ed.WriteMessage($"\n  Left transom candidates: {leftTransomCandidates.Count}");
                    foreach (var lt in leftTransomCandidates)
                        ed.WriteMessage($"\n    Left candidate Start.X: {lt.Start.X}, End.X: {lt.End.X}");

                    var rightTransomCandidates = mullion.ConnectedTransoms
                    .Where(t => t.Start.X >= mullion.Start.X && t.End.X >= mullion.Start.X)
                    .OrderBy(t => t.Start.X)
                    .ToList();

                    ed.WriteMessage($"\n  Right transom candidates: {rightTransomCandidates.Count}");
                    foreach (var rt in rightTransomCandidates)
                        ed.WriteMessage($"\n    Right candidate Start.X: {rt.Start.X}, End.X: {rt.End.X}");

                    var leftTransom = leftTransomCandidates.FirstOrDefault();
                    var rightTransom = rightTransomCandidates.FirstOrDefault();

                    mullion.DistanceToNeighbour1 = leftTransom != null ? (double?)(Math.Abs(leftTransom.End.X - leftTransom.Start.X)) : null;
                    mullion.DistanceToNeighbour2 = rightTransom != null ? (double?)(Math.Abs(rightTransom.End.X - rightTransom.Start.X)) : null;
                }
                else if (mullion.Position == "SID")
                {
                    var sideTransom = mullion.ConnectedTransoms.FirstOrDefault();
                    mullion.DistanceToNeighbour1 = sideTransom != null ? (double?)(Math.Abs(sideTransom.End.X - sideTransom.Start.X)) : null;
                }
            }
        }

        private void AssignDistanceToTransoms(List<Transom> transoms)
        {
            foreach (var transom in transoms)
            {
                var center = transom.GetCenter();

                if (transom.UpperNeighbour != null)
                {
                    var upperCenter = transom.UpperNeighbour.GetCenter();
                    transom.DistanceToNeighbour1 = Math.Abs(upperCenter.Y - center.Y);
                }
                else
                {
                    transom.DistanceToNeighbour1 = null;
                }

                if (transom.LowerNeighbour != null)
                {
                    var lowerCenter = transom.LowerNeighbour.GetCenter();
                    transom.DistanceToNeighbour2 = Math.Abs(center.Y - lowerCenter.Y);
                }
                else
                {
                    transom.DistanceToNeighbour2 = null;
                }
            }
        }


        private void AssignPositions(List<Mullion> mullions, List<Transom> transoms)
        {
            foreach (var mullion in mullions)
            {
                if (mullion.Layer.Contains("MID"))
                    mullion.Position = "MID";
                else if (mullion.Layer.Contains("SID"))
                    mullion.Position = "SID";
            }

            foreach (var transom in transoms)
            {
                if (transom.Layer.Contains("TOP"))
                    transom.Position = "TOP";
                else if (transom.Layer.Contains("BOT"))
                    transom.Position = "BOT";
                else if (transom.Layer.Contains("MID"))
                    transom.Position = "MID";
            }
        }

        private void Log(string msg)
        {
            _ed.WriteMessage($"\n[DEBUG] {msg}");
        }

    }
}
