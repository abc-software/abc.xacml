// ----------------------------------------------------------------------------
// <copyright file="EvaluationEngineFactory.cs" company="ABC Software Ltd">
//    Copyright © 2018 ABC Software Ltd. All rights reserved.
//
//    This library is free software; you can redistribute it and/or.
//    modify it under the terms of the GNU Lesser General Public
//    License  as published by the Free Software Foundation, either
//    version 3 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with the library. If not, see http://www.gnu.org/licenses/.
// </copyright>
// ----------------------------------------------------------------------------

namespace Abc.Xacml.Runtime {
    using System;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// XACML evaluation engine factory.
    /// </summary>
    public static class EvaluationEngineFactory {
        public static EvaluationEngine Create(XmlReader reader, IXacmlPolicyRepository ch) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            EvaluationEngine engine;

            if (reader.IsStartElement(XacmlConstants.ElementNames.Policy, Xacml10Constants.Namespaces.Policy)) {
                Xacml10ProtocolSerializer serializer = new Xacml10ProtocolSerializer();
                engine = new EvaluationEngine(serializer.ReadPolicy(reader));
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.PolicySet, Xacml10Constants.Namespaces.Policy)) {
                Xacml10ProtocolSerializer serializer = new Xacml10ProtocolSerializer();
                engine = new EvaluationEngine(serializer.ReadPolicySet(reader));
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.Policy, Xacml20Constants.Namespaces.Policy)) {
                Xacml20ProtocolSerializer serializer = new Xacml20ProtocolSerializer();
                engine = new EvaluationEngine(serializer.ReadPolicy(reader));
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.PolicySet, Xacml20Constants.Namespaces.Policy)) {
                Xacml20ProtocolSerializer serializer = new Xacml20ProtocolSerializer();
                engine = new EvaluationEngine(serializer.ReadPolicySet(reader));
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.Policy, Xacml30Constants.Namespaces.Policy)) {
                Xacml30ProtocolSerializer serializer = new Xacml30ProtocolSerializer();
                engine = new EvaluationEngine30(serializer.ReadPolicy(reader));
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.PolicySet, Xacml30Constants.Namespaces.Policy)) {
                Xacml30ProtocolSerializer serializer = new Xacml30ProtocolSerializer();
                engine = new EvaluationEngine30(serializer.ReadPolicySet(reader));
            }
            else {
                throw new XmlException("Unknown XML");
            }

            engine.ch = ch;
            return engine;
        }

        public static EvaluationEngine Create(XmlDocument policyDoc, IXacmlPolicyRepository ch) {
            if (policyDoc == null) {
                throw new ArgumentNullException(nameof(policyDoc));
            }

#if NETSTANDARD1_6
            var stringReader = new System.IO.StringReader(policyDoc.DocumentElement.OuterXml);
            using (XmlReader reader = XmlReader.Create(stringReader)) {
#else
            using (XmlReader reader = new XmlNodeReader(policyDoc.DocumentElement)) {
#endif
                return EvaluationEngineFactory.Create(reader, ch);
            }
        }

        public static EvaluationEngine Create(XDocument policyDoc, IXacmlPolicyRepository ch) {
            if (policyDoc == null) {
                throw new ArgumentNullException(nameof(policyDoc));
            }

            using (XmlReader reader = policyDoc.CreateReader()) {
                return EvaluationEngineFactory.Create(reader, ch);
            }
        }
    }
}
