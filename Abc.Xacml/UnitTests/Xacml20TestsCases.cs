using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using NUnit.Framework;

namespace Abc.IdentityModel.Xacml.UnitTests
{
    public class Xacml20TestsCases {
        internal static string TestCasePath = @"..\..\_Data\XACML_Samples\2.0\OfficialTestCases";//@"OfficialTestCases\2.0";
        static string[] TestCaseToIgnore = { "IIE003", "IID029", "IID030" };
        static string[] NotRealisedPrifilesTest = { 
                                                      "IIA002", // Attribute Repository
                                                      "IIIC001", // Hierarhy
                                                      "IIIC002", // Hierarhy
                                                      "IIIC003", // Hierarhy
                                                  };
        static IDictionary<string, Type> TestCaseWithError = new Dictionary<string, Type>()
            {
                { "IIA004", typeof(XmlException) },
                { "IIA005", typeof(XmlException) },
            };

        public static IEnumerable TestCases {
            get {
                Regex regex = new Regex(@"I{2,3}[A-Z]\d{3}");
                var secRes = Directory.GetFiles(TestCasePath, "*.xml")
                    .Select(o => regex.Match(Path.GetFileNameWithoutExtension(o)).Value)
                    .Where(o => !o.StartsWith("IIE"))
                    .Distinct();

                foreach (var key in secRes) {
                    if (TestCaseToIgnore.Contains(key))
                        continue;

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
                        yield return new TestCaseData(null, null, null).SetCategory("Errors_20").SetName(error);
                    }
                    else {
                        if (NotRealisedPrifilesTest.Contains(key)) {
                            yield return new TestCaseData(policy, request, response).SetCategory("Official_20_NotImplemented").SetName(key + "_20");
                        }
                        else {
                            var testCaseData = new TestCaseData(policy, request, response).SetCategory("Official_20").SetName(key + "_20");
                            if (TestCaseWithError.ContainsKey(key)) {
                                yield return testCaseData.Throws(TestCaseWithError[key]);
                            }

                            yield return testCaseData;
                        }
                    }
                }
            }
        }

        public static IEnumerable TestCasesIIE {
            get {
                var secRes = Directory.GetFiles(TestCasePath, "*.xml")
                    .Select(o => Path.GetFileNameWithoutExtension(o).Substring(0, 6))
                    .Where(o => o.StartsWith("IIE"))
                    .Distinct();

                foreach (var key in secRes) {
                    if (TestCaseToIgnore.Contains(key))
                        continue;

                    // Get policy
                    XmlDocument policy = new XmlDocument();
                    XmlDocument request = new XmlDocument();
                    XmlDocument response = new XmlDocument();

                    XmlDocument additionalPolicy = new XmlDocument();
                    XmlDocument additionalPolicySet = new XmlDocument();

                    string error = string.Empty;
                    try {
                        policy.Load(Path.Combine(TestCasePath, key + "Policy.xml"));
                        request.Load(Path.Combine(TestCasePath, key + "Request.xml"));
                        response.Load(Path.Combine(TestCasePath, key + "Response.xml"));
                        additionalPolicy.Load(Path.Combine(TestCasePath, key + "PolicyId1.xml"));
                        additionalPolicySet.Load(Path.Combine(TestCasePath, key + "PolicySetId1.xml"));
                    }
                    catch (Exception ex) {
                        error = ex.Message;
                    }

                    if (!string.IsNullOrEmpty(error))
                        yield return new TestCaseData(null, null, null, null, null).SetCategory("Errors_20").SetName(error + "_20");
                    else
                        yield return new TestCaseData(policy, request, response, additionalPolicy, additionalPolicySet).SetCategory("Official_20").SetName(key + "_20");
                }
            }
        }
    }
}
