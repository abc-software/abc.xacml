using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Abc.Xacml.Geo.UnitTests {
    public class GeoXacmlTestsCases {
        internal static string TestCasePath = @"..\..\_Data\GeoXacml";
        public static string[] TestCaseToIgnore = { };
        public static string[] NotRealisedPrifilesTest = { };

        public static IEnumerable TestCases {
            get {
                Regex regex = new Regex(@"GX\d{3}");
                var secRes = Directory.GetFiles(TestCasePath, "*.xml")
                    .Select(o => regex.Match(Path.GetFileNameWithoutExtension(o)).Value)
                    .Distinct();

                foreach (var key in secRes) {
                    if (TestCaseToIgnore.Contains(key)) {
                        continue;
                    }

                    // Get policy
                    XmlDocument policy = new XmlDocument();
                    XmlDocument request = new XmlDocument();
                    XmlDocument response = new XmlDocument();

                    string error = string.Empty;
                    try {
                        policy.Load(Path.Combine(TestCasePath, key + "Policy.xml"));
                        request.Load(Path.Combine(TestCasePath, key + "Request.xml"));
                        response.Load(Path.Combine(TestCasePath, key + "Response.xml"));
                    }
                    catch (Exception ex) {
                        error = ex.Message;
                    }

                    if (!string.IsNullOrEmpty(error)) {
                        yield return new TestCaseData(null, null, null).SetCategory("Errors_Geo").SetName(error);
                    }
                    else {
                        if (NotRealisedPrifilesTest.Contains(key)) {
                            yield return new TestCaseData(policy, request, response).SetCategory("NotRealisedProfiles_Geo").SetName(key + "_Geo");
                        }
                        else {
                            yield return new TestCaseData(policy, request, response).SetCategory("Official_Geo").SetName(key + "_Geo");
                        }
                    }
                }
            }
        }
    }
}
