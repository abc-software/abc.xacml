using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Abc.Xacml.Geo.UnitTests {
    public class GeoXacmlTestsCases {
        internal static string TestCasePath = GetTestCasePath();
        public static string[] TestCaseToIgnore = { };

        public static string[] NotRealisedProfilesTest = {
#if NETCOREAPP1_0_OR_GREATER || NET6_0_OR_GREATER
            "GX003", // convert-to-metre
            "GX004" // convert-to-square-metre
#endif
        };

        private static string GetTestCasePath() {
#if NETCOREAPP1_1
            var dir = AppContext.BaseDirectory;
#else
            var dir = Path.GetDirectoryName(new Uri(typeof(GeoXacmlTestsCases).Assembly.CodeBase).LocalPath);
#endif
            return Path.Combine(dir, "..", "..", "..", "_Data", "GeoXacml");
        }

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
                        var testCaseData = new TestCaseData(policy, request, response)
                           .SetCategory("Official_Geo")
                           .SetName(key + "_Geo");

                        if (NotRealisedProfilesTest.Contains(key)) {
                            testCaseData.Ignore("Not implemeneted");
                        }

                        yield return testCaseData;
                    }
                }
            }
        }
    }
}
