using System.Xml;

namespace Abc.Xacml.Geo.UnitTests {
    public static class XmlDocumentExtnesions {
#if NETSTANDARD1_6
        public static void Load(this XmlDocument doc, string filename) {
            using (var reader = XmlReader.Create(filename)) {
                doc.Load(reader);
            }
        }
#endif
    }
}
