using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;


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

                // Znajdź wszystkie polilinie 2-punktowe -> FacadeElements
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

                // Rozdziel elementy na Mullions i Transoms
                SeparateFacadeElements();

                _ed.WriteMessage($"\nMullionów: {Mullions.Count}, Transomów: {Transoms.Count}");

                SeparateAreaLoads();
                _ed.WriteMessage($"\nRozdzielono: {PressureLoads.Count} Pressure, {SuctionLoads.Count} Suction.");

                foreach (var mullion in Mullions)
                {
                    mullion.AssignWindLoads(PressureLoads, SuctionLoads);
                }

                foreach (var transom in Transoms)
                {
                    transom.AssignWindLoads(PressureLoads, SuctionLoads);
                }

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
                // Prosty przykład: jeśli start.X == end.X => Mullion, jeśli start.Y == end.Y => Transom
                if (elem.Start.X == elem.End.X)
                {
                    Mullion mullion = new Mullion
                    {
                        Layer = elem.Layer,
                        Start = elem.Start,
                        End = elem.End
                        // Pozostałe właściwości przypisz w przyszłości
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
    }
}
