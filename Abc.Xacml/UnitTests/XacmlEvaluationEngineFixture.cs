namespace Abc.IdentityModel.Xacml.UnitTests {
    using System;
    using System.Xml;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    using Abc.Xacml;
    using Abc.Xacml.Context;
    using Abc.Xacml.Runtime;
    using Abc.Xacml.Policy;
    using Moq;

    [TestFixture]
    public class XacmlEvaluationEngineFixture {
        public static void XacmlResponseAssert(XacmlContextResponse responseData, XacmlContextResponse evaluatedResponse) {
            var responseResults = responseData.Results.ToList();
            var evaluationResults = evaluatedResponse.Results.ToList();

            Assert.AreEqual(responseResults.Count, evaluationResults.Count, "Results count mismatch");

            for (int i = 0; i < responseResults.Count; i++) {
                Assert.AreEqual(responseResults[i].ResourceId, evaluationResults[i].ResourceId);
                Assert.AreEqual(responseResults[i].Decision, evaluationResults[i].Decision, evaluationResults[i].Status.StatusMessage);
                // UNDONE: Assert.AreEqual(responseResults[i].Status.StatusCode.Value, evaluationResults[i].Status.StatusCode.Value);

                Assert.AreEqual(responseResults[i].Attributes.Count, evaluationResults[i].Attributes.Count, "Attributes count mismatch");
                foreach (var responseAttribute in responseResults[i].Attributes) {
                    var evaluateAttribute = evaluationResults[i].Attributes.First(a => a.Category == responseAttribute.Category);
                    Assert.AreEqual(responseAttribute.Attributes.Count, evaluateAttribute.Attributes.Count);
                }

                Assert.AreEqual(responseResults[i].Obligations.Count, evaluationResults[i].Obligations.Count, "Obligations count mismatch");
                Assert.AreEqual(responseResults[i].Advices.Count, evaluationResults[i].Advices.Count, "Advices count mismatch");

                Assert.AreEqual(responseResults[i].PolicyIdReferences.Count, evaluationResults[i].PolicyIdReferences.Count, "PolicyIdentifierList - Policy count mismatch");
                Assert.AreEqual(responseResults[i].PolicySetIdReferences.Count, evaluationResults[i].PolicySetIdReferences.Count, "PolicyIdentifierList - PolicySet count mismatch");
            }
        }

        #region XACML 1.0/1.1

        [Test]
        [TestCaseSource(typeof(Xacml11TestsCases), "TestCases")]
        public void ConformanceTests_11(XmlDocument policy, XmlDocument request, XmlDocument response) {
            var serialize = new Xacml11ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            EvaluationEngine engine = EvaluationEngineFactory.Create(policy, null);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [TestCaseSource(typeof(Xacml11TestsCases), "TestCasesIIE")]
        public void ConformanceTestsIIE_11(XmlDocument policy, XmlDocument request, XmlDocument response, XmlDocument aPolicy, XmlDocument aPolicySet) {
            var serialize = new Xacml11ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            XacmlPolicy aPolicyData;
            XacmlPolicySet aPolicySetData;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(aPolicy.OuterXml))) {
                aPolicyData = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(aPolicySet.OuterXml))) {
                aPolicySetData = serialize.ReadPolicySet(reader);
            }

            Mock<IXacmlPolicyRepository> policyRepositoryMock = new Mock<IXacmlPolicyRepository>();
            policyRepositoryMock.Setup(x => x.RequestPolicy(aPolicyData.PolicyId)).Returns(aPolicyData);
            policyRepositoryMock.Setup(x => x.RequestPolicySet(aPolicySetData.PolicySetId)).Returns(aPolicySetData);

            EvaluationEngine engine = EvaluationEngineFactory.Create(policy, policyRepositoryMock.Object);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [Category("Official_11")]
        public void IIE003_11() {
            XmlDocument policy = new XmlDocument();
            XmlDocument request = new XmlDocument();
            XmlDocument response = new XmlDocument();

            XmlDocument policy1 = new XmlDocument();
            XmlDocument policy2 = new XmlDocument();

            policy.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IIE003Policy.xml"));
            request.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IIE003Request.xml"));
            response.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IIE003Response.xml"));
            policy1.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IIE003PolicyId1.xml"));
            policy2.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IIE003PolicyId2.xml"));

            var serialize = new Xacml11ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            XacmlPolicy policy1Data;
            XacmlPolicy policy2Data;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy1.OuterXml))) {
                policy1Data = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy2.OuterXml))) {
                policy2Data = serialize.ReadPolicy(reader);
            }

            Mock<IXacmlPolicyRepository> policyRepositoryMock = new Mock<IXacmlPolicyRepository>();
            policyRepositoryMock.Setup(x => x.RequestPolicy(policy1Data.PolicyId)).Returns(policy1Data);
            policyRepositoryMock.Setup(x => x.RequestPolicy(policy2Data.PolicyId)).Returns(policy2Data);
            policyRepositoryMock.Setup(x => x.RequestPolicySet(It.IsAny<Uri>())).Returns<XacmlPolicySet>(null);

            EvaluationEngine engine = EvaluationEngineFactory.Create(policy, policyRepositoryMock.Object);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [Category("Official_11")]
        public void IID029_11() {
            XmlDocument request = new XmlDocument();
            XmlDocument response = new XmlDocument();

            XmlDocument policy1 = new XmlDocument();
            XmlDocument policy2 = new XmlDocument();

            request.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IID029Request.xml"));
            response.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IID029Response.xml"));
            policy1.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IID029Policy1.xml"));
            policy2.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IID029Policy2.xml"));

            var serialize = new Xacml11ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            XacmlPolicy policy1Data;
            XacmlPolicy policy2Data;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy1.OuterXml))) {
                policy1Data = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy2.OuterXml))) {
                policy2Data = serialize.ReadPolicy(reader);
            }

            var policySet = new XacmlPolicySet(Xacml10Constants.PolicyCombiningAlgorithms.FirstApplicable, new XacmlTarget()); // TODO: PolicyCombiningAlgorithms
            policySet.Policies.Add(policy1Data);
            policySet.Policies.Add(policy2Data);

            EvaluationEngine engine = new EvaluationEngine(policySet);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [Category("Official_11")]
        public void IID030_11() {
            XmlDocument request = new XmlDocument();
            XmlDocument response = new XmlDocument();

            XmlDocument policy1 = new XmlDocument();
            XmlDocument policy2 = new XmlDocument();

            request.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IID030Request.xml"));
            response.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IID030Response.xml"));
            policy1.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IID030Policy1.xml"));
            policy2.Load(Path.Combine(Xacml11TestsCases.TestCasePath, "IID030Policy2.xml"));

            var serialize = new Xacml11ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            XacmlPolicy policy1Data;
            XacmlPolicy policy2Data;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy1.OuterXml))) {
                policy1Data = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy2.OuterXml))) {
                policy2Data = serialize.ReadPolicy(reader);
            }

            var policySet = new XacmlPolicySet(Xacml10Constants.PolicyCombiningAlgorithms.OnlyOneApplicable, new XacmlTarget()); // TODO: PolicyCombiningAlgorithms
            policySet.Policies.Add(policy1Data);
            policySet.Policies.Add(policy2Data);

            EvaluationEngine engine = new EvaluationEngine(policySet);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        #endregion

        #region 2.0

        [Test]
        public void EvaluateRun() {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@"..\..\_Data\XACML_Samples\2.0\EvaluationSampleRequest.xml"); //c:\aa.xml

            var serialize = new Xacml20ProtocolSerializer();

            using (XmlReader reader = XmlReader.Create(new StringReader(xmlDoc.OuterXml))) {  // string data = reader.ReadOuterXml();
                var data = serialize.ReadContextRequest(reader);


                XmlDocument xmlDoc2 = new XmlDocument();
                xmlDoc2.Load(@"..\..\_Data\XACML_Samples\2.0\EvaluationSamplePolicy.xml"); //c:\aa.xml

                var serialize2 = new Xacml20ProtocolSerializer();

                using (XmlReader reader2 = XmlReader.Create(new StringReader(xmlDoc2.OuterXml))) {
                    var data2 = serialize2.ReadPolicy(reader2);

                    //EvaluationEngine engine = new EvaluationEngine(data2);
                    //engine.Evaluate(data);

                    Assert.IsNotNull(data);

                }

                Assert.IsNotNull(data);
            }
        }

        [Test]
        [TestCaseSource(typeof(Xacml20TestsCases), "TestCases")]
        public void ConfirmanceTests_20(XmlDocument policy, XmlDocument request, XmlDocument response) {
            var serialize = new Xacml20ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }
            EvaluationEngine engine = EvaluationEngineFactory.Create(policy, null);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [TestCaseSource(typeof(Xacml20TestsCases), "TestCasesIIE")]
        public void ConfirmanceTestsIIE_20(XmlDocument policy, XmlDocument request, XmlDocument response, XmlDocument aPolicy, XmlDocument aPolicySet) {
            var serialize = new Xacml20ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            XacmlPolicy aPolicyData;
            XacmlPolicySet aPolicySetData;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(aPolicy.OuterXml))) {
                aPolicyData = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(aPolicySet.OuterXml))) {
                aPolicySetData = serialize.ReadPolicySet(reader);
            }

            Mock<IXacmlPolicyRepository> policyRepositoryMock = new Mock<IXacmlPolicyRepository>();
            policyRepositoryMock.Setup(x => x.RequestPolicy(aPolicyData.PolicyId)).Returns(aPolicyData);
            policyRepositoryMock.Setup(x => x.RequestPolicySet(aPolicySetData.PolicySetId)).Returns(aPolicySetData);

            EvaluationEngine engine = EvaluationEngineFactory.Create(policy, policyRepositoryMock.Object);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [Category("Official_20")]
        public void IIE003_20() {
            XmlDocument policy = new XmlDocument();
            XmlDocument request = new XmlDocument();
            XmlDocument response = new XmlDocument();

            XmlDocument policy1 = new XmlDocument();
            XmlDocument policy2 = new XmlDocument();

            policy.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IIE003Policy.xml"));
            request.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IIE003Request.xml"));
            response.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IIE003Response.xml"));
            policy1.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IIE003PolicyId1.xml"));
            policy2.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IIE003PolicyId2.xml"));

            var serialize = new Xacml20ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            XacmlPolicy policy1Data;
            XacmlPolicy policy2Data;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy1.OuterXml))) {
                    policy1Data = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy2.OuterXml))) {
                    policy2Data = serialize.ReadPolicy(reader);
            }

            Mock<IXacmlPolicyRepository> policyRepositoryMock = new Mock<IXacmlPolicyRepository>();
            policyRepositoryMock.Setup(x => x.RequestPolicy(policy1Data.PolicyId)).Returns(policy1Data);
            policyRepositoryMock.Setup(x => x.RequestPolicy(policy2Data.PolicyId)).Returns(policy2Data);
            policyRepositoryMock.Setup(x => x.RequestPolicySet(It.IsAny<Uri>())).Returns<XacmlPolicySet>(null);

            EvaluationEngine engine = EvaluationEngineFactory.Create(policy, policyRepositoryMock.Object);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [Category("Official_20")]
        public void IID029_20() {
            XmlDocument request = new XmlDocument();
            XmlDocument response = new XmlDocument();

            XmlDocument policy1 = new XmlDocument();
            XmlDocument policy2 = new XmlDocument();

            request.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IID029Request.xml"));
            response.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IID029Response.xml"));
            policy1.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IID029Policy1.xml"));
            policy2.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IID029Policy2.xml"));

            var serialize = new Xacml20ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            XacmlPolicy policy1Data;
            XacmlPolicy policy2Data;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy1.OuterXml))) {
                    policy1Data = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy2.OuterXml))) {
                    policy2Data = serialize.ReadPolicy(reader);
            }

            var policySet = new XacmlPolicySet(Xacml10Constants.PolicyCombiningAlgorithms.OnlyOneApplicable, new XacmlTarget()); // TODO: PolicyCombiningAlgorithms
            policySet.Policies.Add(policy1Data);
            policySet.Policies.Add(policy2Data);

            EvaluationEngine engine = new EvaluationEngine(policySet);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [Category("Official_20")]
        public void IID030_20() {
            XmlDocument request = new XmlDocument();
            XmlDocument response = new XmlDocument();

            XmlDocument policy1 = new XmlDocument();
            XmlDocument policy2 = new XmlDocument();

            request.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IID030Request.xml"));
            response.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IID030Response.xml"));
            policy1.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IID030Policy1.xml"));
            policy2.Load(Path.Combine(Xacml20TestsCases.TestCasePath, "IID030Policy2.xml"));

            var serialize = new Xacml20ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            XacmlPolicy policy1Data;
            XacmlPolicy policy2Data;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy1.OuterXml))) {
                    policy1Data = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy2.OuterXml))) {
                    policy2Data = serialize.ReadPolicy(reader);
            }

            var policySet = new XacmlPolicySet(Xacml10Constants.PolicyCombiningAlgorithms.OnlyOneApplicable, new XacmlTarget()); // TODO: PolicyCombiningAlgorithms
            policySet.Policies.Add(policy1Data);
            policySet.Policies.Add(policy2Data);

            EvaluationEngine engine = new EvaluationEngine(policySet);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        #endregion

        #region 3.0

        [Test]
        [TestCaseSource(typeof(Xacml30TestsCases), "TestCases")]
        public void ConformanceTest_30(XmlDocument policy, XmlDocument request, XmlDocument response) {
            var serialize = new Xacml30ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            using (XmlReader reader = new XmlNodeReader(request.DocumentElement)) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = new XmlNodeReader(response.DocumentElement)) {
                responseData = serialize.ReadContextResponse(reader);
            }

            EvaluationEngine engine = EvaluationEngineFactory.Create(policy, null);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [TestCaseSource(typeof(Xacml30TestsCases), "TestCasesIIE")]
        public void ConformanceTestIIE_30(XmlDocument policy, XmlDocument request, XmlDocument response, XmlDocument aPolicy, XmlDocument aPolicySet) {
            var serialize = new Xacml30ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;
            XacmlPolicy aPolicyData;
            XacmlPolicySet aPolicySetData;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(aPolicy.OuterXml))) {
                aPolicyData = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(aPolicySet.OuterXml))) {
                aPolicySetData = serialize.ReadPolicySet(reader);
            }

            Mock<IXacmlPolicyRepository> policyRepositoryMock = new Mock<IXacmlPolicyRepository>();
            policyRepositoryMock.Setup(x => x.RequestPolicy(aPolicyData.PolicyId)).Returns(aPolicyData);
            policyRepositoryMock.Setup(x => x.RequestPolicySet(aPolicySetData.PolicySetId)).Returns(aPolicySetData);

            EvaluationEngine engine = EvaluationEngineFactory.Create(policy, policyRepositoryMock.Object);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [Category("Official_30")]
        public void IIE003_30() {
            XmlDocument policy = new XmlDocument();
            XmlDocument request = new XmlDocument();
            XmlDocument response = new XmlDocument();

            XmlDocument policy1 = new XmlDocument();
            XmlDocument policy2 = new XmlDocument();

            policy.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IIE003Policy.xml"));
            request.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IIE003Request.xml"));
            response.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IIE003Response.xml"));
            policy1.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IIE003PolicyId1.xml"));
            policy2.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IIE003PolicyId2.xml"));

            var serialize = new Xacml30ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;
            XacmlPolicy policy1Data;
            XacmlPolicy policy2Data;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy1.OuterXml))) {
                    policy1Data = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy2.OuterXml))) {
                    policy2Data = serialize.ReadPolicy(reader);
            }

            Mock<IXacmlPolicyRepository> policyRepositoryMock = new Mock<IXacmlPolicyRepository>();
            policyRepositoryMock.Setup(x => x.RequestPolicy(policy1Data.PolicyId)).Returns(policy1Data);
            policyRepositoryMock.Setup(x => x.RequestPolicy(policy2Data.PolicyId)).Returns(policy2Data);
            policyRepositoryMock.Setup(x => x.RequestPolicySet(It.IsAny<Uri>())).Returns<XacmlPolicySet>(null);

            EvaluationEngine engine = EvaluationEngineFactory.Create(policy, policyRepositoryMock.Object);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [Category("Official_30")]
        public void IID029_30() {
            XmlDocument request = new XmlDocument();
            XmlDocument response = new XmlDocument();

            XmlDocument policy1 = new XmlDocument();
            XmlDocument policy2 = new XmlDocument();

            request.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IID029Request.xml"));
            response.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IID029Response.xml"));
            policy1.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IID029Policy1.xml"));
            policy2.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IID029Policy2.xml"));

            var serialize = new Xacml30ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            XacmlPolicy policy1Data;
            XacmlPolicy policy2Data;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy1.OuterXml))) {
                policy1Data = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy2.OuterXml))) {
                policy2Data = serialize.ReadPolicy(reader);
            }

            var policySet = new XacmlPolicySet(Xacml10Constants.PolicyCombiningAlgorithms.OnlyOneApplicable, new XacmlTarget()); // TODO: PolicyCombiningAlgorithms
            policySet.Policies.Add(policy1Data);
            policySet.Policies.Add(policy2Data);

            EvaluationEngine engine = new EvaluationEngine(policySet);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        [Test]
        [Category("Official_30")]
        public void IID030_30() {
            XmlDocument request = new XmlDocument();
            XmlDocument response = new XmlDocument();

            XmlDocument policy1 = new XmlDocument();
            XmlDocument policy2 = new XmlDocument();

            request.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IID030Request.xml"));
            response.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IID030Response.xml"));
            policy1.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IID030Policy1.xml"));
            policy2.Load(Path.Combine(Xacml30TestsCases.TestCasePath, "IID030Policy2.xml"));

            var serialize = new Xacml30ProtocolSerializer();
            XacmlContextRequest requestData;
            XacmlContextResponse responseData;

            XacmlPolicy policy1Data;
            XacmlPolicy policy2Data;

            using (XmlReader reader = XmlReader.Create(new StringReader(request.OuterXml))) {
                requestData = serialize.ReadContextRequest(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(response.OuterXml))) {
                responseData = serialize.ReadContextResponse(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy1.OuterXml))) {
                policy1Data = serialize.ReadPolicy(reader);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(policy2.OuterXml))) {
                policy2Data = serialize.ReadPolicy(reader);
            }

            var policySet = new XacmlPolicySet(Xacml10Constants.PolicyCombiningAlgorithms.OnlyOneApplicable, new XacmlTarget()); // TODO: PolicyCombiningAlgorithms
            policySet.Policies.Add(policy1Data);
            policySet.Policies.Add(policy2Data);

            EvaluationEngine engine = new EvaluationEngine(policySet);

            XacmlContextResponse evaluatedResponse = engine.Evaluate(requestData, request);

            XacmlResponseAssert(responseData, evaluatedResponse);
        }

        #endregion

    }
}
