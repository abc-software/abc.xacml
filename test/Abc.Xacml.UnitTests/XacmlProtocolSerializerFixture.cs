namespace Abc.Xacml.UnitTests {
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Abc.Xacml;
    using Abc.Xacml.Context;
    using Abc.Xacml.Policy;
    using NUnit.Framework;

    /// <summary>
    ///This is a test class for Serializ1Test and is intended
    ///to contain all Serializ1Test Unit Tests
    ///</summary>
    [TestFixture()]
    public class XacmlSerializerFixture {
        [SetUp]
        public void SetUp()  {
            this.readersettings = null; //HACK:
        }

        private static string TestCasePath {
            get {
                var dir = SetUpClass.BaseDirectory;
                return Path.Combine(dir, "..", "..", "..", "_Data");
            }
        }

        #region XACML 1.0/1.1

        [Test()]
        public void WritePolicy_11() {
            var subject = new XacmlSubject(
                new XacmlSubjectMatch[] 
                {  
                    new XacmlSubjectMatch(
                        new Uri("http://www.MatchId.www"),
                        new XacmlAttributeValue(new Uri("http://www.DataType.www")), 
                        new XacmlSubjectAttributeDesignator(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www")) { Issuer = "String", MustBePresent = false, Category = new Uri("http://www.subjectCategory.www") })
                });

            var target = new XacmlTarget(subject, null, null);
            XacmlPolicySet xacmlPolicySet = new XacmlPolicySet(new Uri("http://www.PolicySetId.www"), new Uri("http://www.PolicyCombiningAlgId.www"), target);
            xacmlPolicySet.Description = "description string";
            xacmlPolicySet.XPathVersion = Xacml10Constants.XPathVersions.Xpath10;
            
            XacmlPolicy xacmlPolicy = new XacmlPolicy(new Uri("http://www.PolicyId.www"), new Uri("http://www.RuleCombiningAlgId.www"), new XacmlTarget()) {
                Description = "description string",
                XPathVersion = Xacml10Constants.XPathVersions.Xpath10,
            };

            XacmlRule xacmlRule = new XacmlRule("http://www.RuleId.www", XacmlEffectType.Permit) {
                Description = "xacmlRule description"
            };

            xacmlPolicy.Rules.Add(xacmlRule);
            XacmlAttributeAssignment xacmlAttributeAssignment = new XacmlAttributeAssignment(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www"));
            XacmlObligation xacmlObligation = new XacmlObligation(new Uri("http://www.ObligationId.www"), XacmlEffectType.Permit, new XacmlAttributeAssignment[] { xacmlAttributeAssignment });
            xacmlPolicy.Obligations.Add(xacmlObligation);

            xacmlPolicySet.Policies.Add(xacmlPolicy);

            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder)) {
                var serializer = new Xacml10ProtocolSerializer();
                serializer.WritePolicySet(writer, xacmlPolicySet);
            }

            string xml = builder.ToString();
            ValidateMessage(xml, Path.Combine(TestCasePath, "cs-xacml-schema-context-01.xsd"));
        }

        [Test]
        public void WriteRequest_11()
        {
            var s = new XacmlContextSubject(new XacmlContextAttribute(new Uri("uri:action"), new Uri("uri:type"), new XacmlContextAttributeValue()));
            var r = new XacmlContextResource(new XacmlContextAttribute(new Uri("uri:action"), new Uri("uri:type"), new XacmlContextAttributeValue()));
            var a = new XacmlContextAction(new XacmlContextAttribute(new Uri("uri:action"), new Uri("uri:type"), new XacmlContextAttributeValue()));
            var request = new XacmlContextRequest(r, a, s);

            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder))
            {
                var serializer = new Xacml10ProtocolSerializer();
                serializer.WriteContextRequest(writer, request);
            }

            string xml = builder.ToString();
            ValidateMessage(xml, Path.Combine(TestCasePath, "cs-xacml-schema-context-01.xsd"));
        }

        [Test]
        public void WriteResponse_11()
        {
            var response = new XacmlContextResponse(new XacmlContextResult(XacmlContextDecision.NotApplicable, new XacmlContextStatus(XacmlContextStatusCode.Success)));

            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder))
            {
                var serializer = new Xacml11ProtocolSerializer();
                serializer.WriteContextResponse(writer, response);
            }

            string xml = builder.ToString();
            ValidateMessage(xml, Path.Combine(TestCasePath, "cs-xacml-schema-context-01.xsd"));
        }

        [Test]
        public void ReadRequest_11() {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(TestCasePath, "XACML_Samples", "1.1", "Example_1", "Request.xml"));

            var serialize = new Xacml10ProtocolSerializer();

            using (XmlReader reader = XmlReader.Create(new StringReader(xmlDoc.OuterXml))) {
                var data = serialize.ReadContextRequest(reader);
                Assert.IsNotNull(data);

                // Subject
                Assert.AreEqual(1, data.Subjects.Count);
                {
                    var subject = data.Subjects.First();
                    Assert.AreEqual(new Uri("urn:oasis:names:tc:xacml:1.0:subject-category:access-subject"), subject.SubjectCategory);
                    Assert.AreEqual(1, subject.Attributes.Count);
                    {
                        var att = subject.Attributes.First();
                        Assert.AreEqual(new Uri("urn:oasis:names:tc:xacml:1.0:subject:subject-id"), att.AttributeId);
                        Assert.AreEqual(new Uri("urn:oasis:names:tc:xacml:1.0:data-type:rfc822Name"), att.DataType);
                        Assert.AreEqual(1, att.AttributeValues.Count);
                        Assert.AreEqual("bs@simpsons.com", att.AttributeValues.First().Value);
                    }
                }

                // Resource
                Assert.AreEqual(1, data.Resources.Count);
                {
                    var resource = data.Resources.First();
                    Assert.AreEqual(1, resource.Attributes.Count);
                    {
                        var att = resource.Attributes.First();
                        Assert.AreEqual(new Uri("urn:oasis:names:tc:xacml:1.0:resource:ufspath"), att.AttributeId);
                        Assert.AreEqual(new Uri("http://www.w3.org/2001/XMLSchema#anyURI"), att.DataType);
                        Assert.AreEqual(1, att.AttributeValues.Count);
                        Assert.AreEqual("/medico/record/patient/BartSimpson", att.AttributeValues.First().Value);
                    }
                }

                // Action
                var action = data.Action;
                Assert.AreEqual(1, action.Attributes.Count);
                {
                    var att = action.Attributes.First();
                    Assert.AreEqual(new Uri("urn:oasis:names:tc:xacml:1.0:action:action-id"), att.AttributeId);
                    Assert.AreEqual(new Uri("http://www.w3.org/2001/XMLSchema#string"), att.DataType);
                    Assert.AreEqual(1, att.AttributeValues.Count);
                    Assert.AreEqual("read", att.AttributeValues.First().Value);
                }

            }
        }

        [Test]
        public void ReadPolicy_11() {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(TestCasePath, "XACML_Samples", "1.1", "Example_1", "Rule_1.xml"));

            var serialize = new Xacml10ProtocolSerializer();

            using (XmlReader reader = XmlReader.Create(new StringReader(xmlDoc.OuterXml))) {  
                var data = serialize.ReadPolicy(reader);

                Assert.IsNotNull(data);
            }
        }

        [Test]
        public void ReadResponse_11()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(TestCasePath, @"XACML_Samples\1.1\Example_1\Response.xml"));

            var serialize = new Xacml10ProtocolSerializer();

            using (XmlReader reader = XmlReader.Create(new StringReader(xmlDoc.OuterXml))) {
                var data = serialize.ReadContextResponse(reader);

                Assert.IsNotNull(data);

                Assert.AreEqual(1, data.Results.Count);
                Assert.AreEqual(XacmlContextDecision.NotApplicable, data.Results.First().Decision);
            }
        }

#endregion

        #region XACML 2.0

        /// <summary>
        ///A test for WriteAction
        ///</summary>
        [Test]
        public void WritePolicy_20() {
            var subject = new XacmlSubject(
                new XacmlSubjectMatch[] 
                {  
                    new XacmlSubjectMatch(
                        new Uri("http://www.MatchId.www"),
                        new XacmlAttributeValue(new Uri("http://www.DataType.www")), 
                        new XacmlSubjectAttributeDesignator(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www")) { Issuer = "String", MustBePresent = false, Category = new Uri("http://www.subjectCategory.www")}
                        )
                });

            var resource = new XacmlResource(
            new XacmlResourceMatch[] 
                { 
                    new XacmlResourceMatch(
                        new Uri("http://www.MatchId.www"), 
                        new XacmlAttributeValue(new Uri("http://www.DataType.www") /*, "xxxx" */), 
                        new XacmlResourceAttributeDesignator(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www")) { Issuer = "String", MustBePresent = false} 
                        )
                });

            var action = new XacmlAction(
                new XacmlActionMatch[] 
                { 
                    new XacmlActionMatch(
                        new Uri("http://www.MatchId.www"), 
                        new XacmlAttributeValue(new Uri("http://www.DataType.www")), 
                        new XacmlActionAttributeDesignator(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www")){ Issuer = "String", MustBePresent = false} 
                        )
                });

            var target = new XacmlTarget(subject, resource, action, null);

            // new Uri("http://www.PolicySetId.www")
            XacmlPolicySet xacmlPolicySet = new XacmlPolicySet(new Uri("http://www.PolicyCombiningAlgId.www"), target) {
                Description = "description string",
                XPathVersion = Xacml10Constants.XPathVersions.Xpath10,
            };
            
            ////#region Policy
            XacmlEnvironment env = new XacmlEnvironment(
                new XacmlEnvironmentMatch[]
                {
                    new XacmlEnvironmentMatch(
                        new Uri("http://www.EnvironmentMatchIdId.www"), 
                        new XacmlAttributeValue(new Uri("http://www.AttributValue.www")), 
                        new XacmlEnvironmentAttributeDesignator(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www")){ Issuer = "String", MustBePresent = false} 
                        )
                });

            XacmlTarget targetWithEnvironment = new XacmlTarget(null, null, null, new XacmlEnvironment[] { env });

            XacmlPolicy xacmlPolicy = new XacmlPolicy(new Uri("http://www.PolicyId.www"), new Uri("http://www.RuleCombiningAlgId.www"), targetWithEnvironment) {
                Description = "description string",
                XPathVersion = Xacml10Constants.XPathVersions.Xpath10,
            };

            XacmlRule xacmlRule = new XacmlRule("http://www.RuleId.www", XacmlEffectType.Permit) {
                Description = "xacmlRule description"
            };

            xacmlPolicy.Rules.Add(xacmlRule);

            XacmlAttributeAssignment xacmlAttributeAssignment = new XacmlAttributeAssignment(new Uri("http://www.AttributeId.www"), new Uri("http://www.DataType.www"));
            XacmlObligation xacmlObligation = new XacmlObligation(new Uri("http://www.ObligationId.www"), XacmlEffectType.Permit, new XacmlAttributeAssignment[] { xacmlAttributeAssignment });
            xacmlPolicy.Obligations.Add(xacmlObligation);

            xacmlPolicySet.Policies.Add(xacmlPolicy);

            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder)) {
                var serializer = new Xacml20ProtocolSerializer();
                serializer.WritePolicySet(writer, xacmlPolicySet);
            }

            string xml = builder.ToString();
            ValidateMessage(xml, Path.Combine(TestCasePath, "access_control-xacml-2.0-policy-schema-os.xsd"));
        }


        #endregion

        #region XACML 3.0


        #endregion

        private XmlReaderSettings readersettings;
        private void ValidateMessage(string xml, string schema)
        {
            if (readersettings == null) {
                AppContext.SetSwitch("Switch.System.Xml.AllowDefaultResolver", true);

                readersettings = new XmlReaderSettings();
                readersettings.IgnoreWhitespace = true;
                readersettings.IgnoreComments = true;
                readersettings.ValidationType = ValidationType.Schema;
                readersettings.ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings
                                                 | System.Xml.Schema.XmlSchemaValidationFlags.AllowXmlAttributes
                                                 | System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints;
                readersettings.ValidationEventHandler +=
                    delegate(object sender, System.Xml.Schema.ValidationEventArgs vargs) {
                        Assert.Fail(
                            "Schema problem: Line {2}: {0}: {1}",
                            vargs.Severity,
                            vargs.Message,
                            vargs.Exception.LineNumber);
                    };

                //  XmlReaderSettings dtdSettings = new XmlReaderSettings();
                //  dtdSettings.ProhibitDtd = false;             
                // "http://www.w3.org/TR/xmldsig-core/xmldsig-core-schema.xsd"

                //readersettings.Schemas.Add(null, @"..\..\..\..\_Data\details.xsd");
                //readersettings.Schemas.Add(null, XmlReader.Create(@"..\..\..\..\_Data\xmldsig-core-schema.xsd", dtdSettings));

                readersettings.Schemas.Add(null, schema);
            }

            using (XmlReader vr = XmlReader.Create(new System.IO.StringReader(xml), readersettings)) {
                while (vr.Read()) ;
            }
        }
    }
}
