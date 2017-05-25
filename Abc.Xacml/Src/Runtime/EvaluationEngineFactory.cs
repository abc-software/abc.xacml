// ----------------------------------------------------------------------------
// <copyright file="EvaluationEngineFactory.cs" company="ABC Software Ltd">
//    Copyright © 2015 ABC Software Ltd. All rights reserved.
//
//    This library is free software; you can redistribute it and/or
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
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
#if NET40
    using Diagnostic;
#else
    using Abc.Diagnostics;
#endif

    /// <summary>
    /// XACML evaluation engine factory.
    /// </summary>
    public static class EvaluationEngineFactory {
        public static EvaluationEngine Create(XmlReader reader, IXacmlPolicyRepository ch) {
            Contract.Requires<ArgumentNullException>(reader != null);

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
                throw DiagnosticTools.ExceptionUtil.ThrowHelperError(new XmlException("Unknown XML"));
            }

            engine.ch = ch;
            return engine;
        }

        public static EvaluationEngine Create(XmlDocument policyDoc, IXacmlPolicyRepository ch) {
            Contract.Requires<ArgumentNullException>(policyDoc != null);

            using (XmlReader reader = new XmlNodeReader(policyDoc.DocumentElement)) {
                return EvaluationEngineFactory.Create(reader, ch);
            }
        }

        public static EvaluationEngine Create(XDocument policyDoc, IXacmlPolicyRepository ch) {
            Contract.Requires<ArgumentNullException>(policyDoc != null);

            using (XmlReader reader = policyDoc.CreateReader()) {
                return EvaluationEngineFactory.Create(reader, ch);
            }
        }

    }
}
