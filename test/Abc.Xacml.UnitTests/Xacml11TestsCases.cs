﻿using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Abc.Xacml.UnitTests {
    public class Xacml11TestsCases {
        internal static readonly string TestCasePath = GetTestCasePath();
        static string[] TestCaseToIgnore = { "IIE003", "IID029", "IID030" };
        static string[] NotRealisedProfilesTest = { 
                                                      "IIA002", // Attribute Repository
                                                      "IIIC001", // Hierarchical
                                                      "IIIC002", // Hierarchical
                                                      "IIIC003", // Hierarchical
                                                  };
        static IDictionary<string, Type> TestCaseWithError = new Dictionary<string, Type>()
            {
                { "IIA004", typeof(XmlException) },
                { "IIA005", typeof(XmlException) },
            };

        private static string GetTestCasePath() {
            var dir = SetUpClass.BaseDirectory;
			return Path.Combine(dir, "..", "..", "..", "_Data", "XACML_Samples", "1.1", "OfficialTestCases");
		}

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
                        yield return new TestCaseData(null, null, null).SetCategory("Errors_11").SetName(error);
                    }
                    else {
                        Type expectedException = null;
                        if (TestCaseWithError.ContainsKey(key)) {
                            expectedException = TestCaseWithError[key];
                        }

                        var testCaseData = new TestCaseData(policy, request, response, expectedException)
                           .SetCategory("Official_10")
                           .SetName(key + "_10");

                        if (NotRealisedProfilesTest.Contains(key)) {
                            testCaseData.Ignore("Not implemeneted");
                        }

                        yield return testCaseData;
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
                        yield return new TestCaseData(null, null, null, null, null).SetCategory("Errors_11").SetName(error + "_11");
                    else
                        yield return new TestCaseData(policy, request, response, additionalPolicy, additionalPolicySet).SetCategory("Official_11").SetName(key + "_11");
                }
            }
        }
    }
}
