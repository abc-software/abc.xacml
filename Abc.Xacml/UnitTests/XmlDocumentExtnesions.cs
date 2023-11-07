using System.Xml;

namespace Abc.Xacml.UnitTests {
    public static class XmlDocumentExtnesions {
#if NETCOREAPP1_1
        public static void Load(this XmlDocument doc, string filename) {
            using (var reader = XmlReader.Create(filename)) {
                doc.Load(reader);
            }
        }
#endif
    }
}
