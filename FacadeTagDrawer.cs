using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;

namespace StickFacadeDataExtraction
{
    public static class FacadeTagDrawer
    {
        public static void DrawTags(List<Mullion> mullions, List<Transom> transoms)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (var m in mullions)
                {
                    var center = MidPoint(m.Start, m.End);
                    var mtext = CreateTagMText(m.Tag, center);
                    mtext.Layer = m.Layer; // <- ustawienie warstwy
                    btr.AppendEntity(mtext);
                    tr.AddNewlyCreatedDBObject(mtext, true);
                }

                foreach (var t in transoms)
                {
                    var center = MidPoint(t.Start, t.End);
                    var mtext = CreateTagMText(t.Tag, center);
                    mtext.Layer = t.Layer; // <- ustawienie warstwy
                    btr.AppendEntity(mtext);
                    tr.AddNewlyCreatedDBObject(mtext, true);
                }

                tr.Commit();
            }
        }


        private static Point3d MidPoint(Point2d p1, Point2d p2)
        {
            return new Point3d((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, 0);
        }

        private static MText CreateTagMText(string tag, Point3d location)
        {
            return new MText
            {
                Contents = tag,
                TextHeight = 200,
                Location = location,
                Attachment = AttachmentPoint.MiddleCenter
            };
        }
    }
}
