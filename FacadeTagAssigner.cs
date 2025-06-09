using System.Collections.Generic;

namespace StickFacadeDataExtraction
{
    public static class FacadeTagAssigner
    {
        public static void AssignTags(List<Mullion> mullions, List<Transom> transoms)
        {
            int mullionIndex = 1;
            foreach (var m in mullions)
            {
                m.Tag = $"M_{mullionIndex:D3}";
                mullionIndex++;
            }

            int transomIndex = 1;
            foreach (var t in transoms)
            {
                t.Tag = $"T_{transomIndex:D3}";
                transomIndex++;
            }
        }
    }
}
