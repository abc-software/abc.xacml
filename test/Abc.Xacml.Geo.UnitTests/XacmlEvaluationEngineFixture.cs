namespace Abc.Xacml.Geo.UnitTests {
    using System;
    using System.Xml;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    using Abc.Xacml;
    using Abc.Xacml.Context;
    using Abc.Xacml.Runtime;

    [TestFixture]
    public class XacmlEvaluationEngineFixture {

        #region GeoXacml

        [Test]
        [TestCaseSource(typeof(GeoXacmlTestsCases), "TestCases")]
        public void RunOfficialTestsCheckResult_Geo(XmlDocument policy, XmlDocument request, XmlDocument response) {
            var serialize = new Xacml30ProtocolSerializer();
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

            Assert.AreEqual(responseData.Results.First().Decision, evaluatedResponse.Results.First().Decision, evaluatedResponse.Results.First().Status.StatusMessage);
            Assert.True(evaluatedResponse.Results.First().Obligations.Count ==
                    responseData.Results.First().Obligations.Count);
        }

        #endregion
    }
}
