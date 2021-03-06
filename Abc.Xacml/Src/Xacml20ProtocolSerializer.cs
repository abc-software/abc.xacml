﻿// ----------------------------------------------------------------------------
// <copyright file="Xacml20ProtocolSerializer.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Abc.Xacml.Context;
    using Abc.Xacml.Policy;

    /// <summary>
    /// The XACML 2.0 Protocol serializer
    /// </summary>
    public class Xacml20ProtocolSerializer : XacmlProtocolSerializer {
        /// <summary>
        /// Initializes a new instance of the <see cref="Xacml20ProtocolSerializer"/> class.
        /// </summary>
        public Xacml20ProtocolSerializer()
            : base(XacmlVersion.Xacml20) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Xacml20ProtocolSerializer"/> class.
        /// </summary>
        /// <param name="version">The XACML version.</param>
        protected Xacml20ProtocolSerializer(XacmlVersion version)
            : base(version) {
        }

        #region Policy

        /// <summary>
        /// Writes the target.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected override void WriteTarget(XmlWriter writer, XacmlTarget data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            //// Start Target
            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy);

            if (data.Subjects.Any()) {
                //// Start Subjects
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Subjects, this.Version.NamespacePolicy);
                foreach (var subject in data.Subjects) {
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Subject, this.Version.NamespacePolicy);

                    foreach (XacmlSubjectMatch subjectMatch in subject.Matches.OfType<XacmlSubjectMatch>()) {
                        this.WriteMatch(writer, subjectMatch);
                    }

                    writer.WriteEndElement();
                }

                //// End Subjects
                writer.WriteEndElement();
            }

            if (data.Resources.Any()) {
                //// Start Resources
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Resources, this.Version.NamespacePolicy);

                foreach (var resource in data.Resources) {
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Resource, this.Version.NamespacePolicy);

                    foreach (var resourceMatch in resource.Matches) {
                        this.WriteMatch(writer, resourceMatch);
                    }

                    writer.WriteEndElement();
                }

                //// End Resources
                writer.WriteEndElement();
            }

            if (data.Actions.Any()) {
                //// Start Actions
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Actions, this.Version.NamespacePolicy);
                foreach (var action in data.Actions) {
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Action, this.Version.NamespacePolicy);

                    foreach (var actionMatch in action.Matches) {
                        this.WriteMatch(writer, actionMatch);
                    }

                    writer.WriteEndElement();
                }

                //// End Actions
                writer.WriteEndElement();
            }

            if (data.Environments.Any()) {
                //// Start Environment
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Environments, this.Version.NamespacePolicy);
                foreach (var environment in data.Environments) {
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Environment, this.Version.NamespacePolicy);

                    foreach (var environmentMatch in environment.Matches) {
                        this.WriteMatch(writer, environmentMatch);
                    }

                    writer.WriteEndElement();
                }

                //// End Actions
                writer.WriteEndElement();
            }

            //// End Target
            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the target.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">
        /// Subject IsNullOrEmpty
        /// or
        /// Resource IsNullOrEmpty
        /// or
        /// Action IsNullOrEmpty
        /// or
        /// Environment IsNullOrEmpty
        /// </exception>
        protected override XacmlTarget ReadTarget(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            if (reader.IsEmptyElement) {
                reader.Read();
                return new XacmlTarget();
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy);

            var subjects = new List<XacmlSubject>();
            var resources = new List<XacmlResource>();
            var actions = new List<XacmlAction>();
            var environment = new List<XacmlEnvironment>();

            Dictionary<string, Action> dict = new Dictionary<string, Action>
            {
                { XacmlConstants.ElementNames.Subject, () => { this.ReadList(subjects, XacmlConstants.ElementNames.Subject, this.Version.NamespacePolicy, this.ReadSubject, reader, true); } },
                { XacmlConstants.ElementNames.Resource, () => { this.ReadList(resources, XacmlConstants.ElementNames.Resource, this.Version.NamespacePolicy, this.ReadResource, reader, true); } },
                { XacmlConstants.ElementNames.Action, () => { this.ReadList(actions, XacmlConstants.ElementNames.Action, this.Version.NamespacePolicy, this.ReadAction, reader, true); } },
                { XacmlConstants.ElementNames.Environment, () => { this.ReadList(environment, XacmlConstants.ElementNames.Environment, this.Version.NamespacePolicy, this.ReadEnvironment, reader, true); } }
            };

            Action<string, string> read = (type, baseType) => {
                if (reader.IsStartElement(baseType, this.Version.NamespacePolicy)) {
                    reader.Read();

                    if (!reader.IsStartElement(type, this.Version.NamespacePolicy)) {
                        throw ThrowHelperXml(reader, string.Format("{0} IsNullOrEmpty", type));
                    }
                    else {
                        dict[type].Invoke();
                    }

                    reader.ReadEndElement();
                }
            };

            // Subjects
            read(XacmlConstants.ElementNames.Subject, XacmlConstants.ElementNames.Subjects);

            // Resources
            read(XacmlConstants.ElementNames.Resource, XacmlConstants.ElementNames.Resources);

            // Actions
            read(XacmlConstants.ElementNames.Action, XacmlConstants.ElementNames.Actions);

            // Environments
            read(XacmlConstants.ElementNames.Environment, XacmlConstants.ElementNames.Environments);

            reader.ReadEndElement();
            return new XacmlTarget(subjects, resources, actions, environment);
        }

        /// <summary>
        /// Reads the environment.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlEnvironment ReadEnvironment(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Environment, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Environment, this.Version.NamespacePolicy);
            List<XacmlEnvironmentMatch> matches = new List<XacmlEnvironmentMatch>();
            this.ReadListAbstract(matches, XacmlConstants.ElementNames.EnvironmentMatch, this.Version.NamespacePolicy, this.ReadMatch, reader, true);
            XacmlEnvironment env = new XacmlEnvironment(matches);

            reader.ReadEndElement();
            return env;
        }

        /// <summary>
        /// Reads the match.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected override XacmlMatch ReadMatch(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            var gaMatchId = reader.GetAttribute("MatchId");

            if (string.IsNullOrEmpty(gaMatchId)) {
                throw ThrowHelperXml(reader, "MatchId IsNullOrEmpty");
            }

            Func<string, string, ReadElement<XacmlAttributeDesignator>, XacmlMatch> read = (matchType, designatorType, readFuncAttributeDesignator) => {
                XacmlMatch ret = null;
                if (!reader.IsStartElement(matchType, this.Version.NamespacePolicy)) {
                    throw ThrowHelperXml(reader, string.Format("{0} NotStartElement", matchType));
                }

                reader.ReadStartElement(matchType, this.Version.NamespacePolicy);

                var attributeValue = ReadAttributeValue(reader);

                IDictionary<Tuple<string, string>, Action> dicts = null;
                XacmlAttributeSelector sel = null;
                XacmlAttributeDesignator des = null;

                dicts = new Dictionary<Tuple<string, string>, Action>()
                    {
                        { new Tuple<string, string>(designatorType, this.Version.NamespacePolicy), () => des = readFuncAttributeDesignator(reader) },
                        { new Tuple<string, string>(XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy), () => sel = ReadAttributeSelector(reader) },
                    };

                this.ReadChoice(reader, dicts, isRequired: true);
                reader.ReadEndElement();

                ret = (des != null
                        ? new XacmlEnvironmentMatch(new Uri(gaMatchId, UriKind.RelativeOrAbsolute), attributeValue, (XacmlEnvironmentAttributeDesignator)des)
                        : new XacmlEnvironmentMatch(new Uri(gaMatchId, UriKind.RelativeOrAbsolute), attributeValue, sel));

                return ret;
            };

            if (reader.LocalName == XacmlConstants.ElementNames.EnvironmentMatch) {
                return read(XacmlConstants.ElementNames.EnvironmentMatch, XacmlConstants.ElementNames.EnvironmentAttributeDesignator, this.ReadAttributeDesignator);
            }
            else {
                return base.ReadMatch(reader);
            }
        }

        /// <summary>
        /// Reads the policy set.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public override XacmlPolicySet ReadPolicySet(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            IDictionary<string, string> nsMgr = new Dictionary<string, string>();

            for (int i = 0; i < reader.AttributeCount; i++) {
                reader.MoveToAttribute(i);
                if (reader.Prefix == "xmlns") {
                    nsMgr.Add(reader.LocalName, reader.Value);
                }
            }

            if (!this.CanRead(reader, XacmlConstants.ElementNames.PolicySet)) {
                throw ThrowHelperXml(reader, "Cannot read PolicySet.");
            }

            Uri gaPolicySetId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.PolicySetId);
            Uri gaPolicyCombiningAlgId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.PolicyCombiningAlgId);
            string version = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Version, isRequered: false);

            reader.ReadStartElement(XacmlConstants.ElementNames.PolicySet, this.Version.NamespacePolicy);

            string description = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy)) {
                description = reader.ReadElementContentAsString(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy);
            }

            // PolicySetDefault
            string xpathVersion = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.PolicySetDefaults, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.PolicySetDefaults, this.Version.NamespacePolicy);
                if (!reader.IsStartElement(XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy)) {
                    throw ThrowHelperXml(reader, "XPathVerison NotStartElement");
                }

                xpathVersion = reader.ReadElementContentAsString(XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy);

                reader.ReadEndElement();
            }

            XacmlTarget target = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy)) {
                target = ReadTarget(reader);
            }

            XacmlPolicySet policySet = new XacmlPolicySet(gaPolicySetId, gaPolicyCombiningAlgId, target) {
                Description = description,
                XPathVersion = xpathVersion != null ? new Uri(xpathVersion) : null,
                Version = version
            };

            policySet.Namespaces = nsMgr;

            IDictionary<Tuple<string, string>, Action> dicts = new Dictionary<Tuple<string, string>, Action>()
            {
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicySet, this.Version.NamespacePolicy), () => policySet.PolicySets.Add(this.ReadPolicySet(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.Policy, this.Version.NamespacePolicy), () => policySet.Policies.Add(this.ReadPolicy(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicySetIdReference, this.Version.NamespacePolicy), () => policySet.PolicySetIdReferences.Add(this.ReadPolicySetIdReference(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicyIdReference, this.Version.NamespacePolicy), () => policySet.PolicyIdReferences.Add(this.ReadPolicyIdReference(reader)) },

                { new Tuple<string, string>(XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy), () => {
                    reader.ReadStartElement(XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy);
                    this.ReadList(policySet.CombinerParameters, XacmlConstants.ElementNames.CombinerParameter, this.Version.NamespacePolicy, this.ReadCombinerParameter, reader, isRequired: false);
                    reader.ReadEndElement(); }
                },
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicyCombinerParameters, this.Version.NamespacePolicy), () => policySet.PolicyCombinerParameters.Add(this.ReadPolicyCombinerParameters(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicySetCombinerParameters, this.Version.NamespacePolicy), () => policySet.PolicySetCombinerParameters.Add(this.ReadPolicySetCombinerParameters(reader)) },
            };

            this.ReadChoiceMultiply(reader, dicts);

            if (reader.IsStartElement(XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy);

                this.ReadList(policySet.Obligations, XacmlConstants.ElementNames.Obligation, this.Version.NamespacePolicy, ReadObligation, reader);

                // end obligations
                reader.ReadEndElement();
            }

            reader.ReadEndElement();

            return policySet;
        }

        /// <summary>
        /// Reads the policy.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.Xml.XmlException">
        /// PolicyId IsNullOrEmpty
        /// or
        /// RuleCombiningAlgId IsNullOrEmpty
        /// or
        /// Target IsNullOrEmpty
        /// </exception>
        /// <exception cref="XacmlSerializationException">
        /// VariableDefinition or Rule are required
        /// or
        /// CombinerParameterc count > 1
        /// or
        /// RuleCombinerParameters count > 1
        /// </exception>
        public override XacmlPolicy ReadPolicy(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!this.CanRead(reader, XacmlConstants.ElementNames.Policy)) {
                throw ThrowHelperXml(reader, "Cannot read Policy.");
            }

            IDictionary<string, string> nsMgr = new Dictionary<string, string>();

            for (int i = 0; i < reader.AttributeCount; i++) {
                reader.MoveToAttribute(i);
                if (reader.Prefix == "xmlns") {
                    nsMgr.Add(reader.LocalName, reader.Value);
                }
            }

            var gaPolicyId = reader.GetAttribute("PolicyId");

            if (string.IsNullOrEmpty(gaPolicyId)) {
                throw ThrowHelperXml(reader, "PolicyId IsNullOrEmpty");
            }

            var gaRuleCombiningAlgId = reader.GetAttribute("RuleCombiningAlgId");

            if (string.IsNullOrEmpty(gaRuleCombiningAlgId)) {
                throw ThrowHelperXml(reader, "RuleCombiningAlgId IsNullOrEmpty");
            }

            string version = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Version, isRequered: false);

            reader.ReadStartElement(XacmlConstants.ElementNames.Policy, this.Version.NamespacePolicy);

            string description = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy)) {
                description = reader.ReadElementContentAsString(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy);
            }

            // PolicySetDefault
            string xpathVersion = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.PolicyDefaults, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.PolicyDefaults, this.Version.NamespacePolicy);

                if (!reader.IsStartElement(XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy)) {
                    throw ThrowHelperXml(reader, "XPathVerison NotStartElement");
                }

                xpathVersion = reader.ReadElementContentAsString(XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy);

                reader.ReadEndElement();
            }

            ICollection<XacmlCombinerParameter> combParams = new List<XacmlCombinerParameter>();
            if (reader.IsStartElement(XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy);
                this.ReadList(combParams, XacmlConstants.ElementNames.CombinerParameter, this.Version.NamespacePolicy, this.ReadCombinerParameter, reader, isRequired: false);
                reader.ReadEndElement();
            }

            XacmlTarget target = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy)) {
                target = ReadTarget(reader);
            }
            else {
                throw ThrowHelperXml(reader, "Target IsNullOrEmpty");
            }

            XacmlPolicy policy = new XacmlPolicy(new Uri(gaPolicyId, UriKind.RelativeOrAbsolute), new Uri(gaRuleCombiningAlgId), target) {
                Description = description,
                XPathVersion = xpathVersion != null ? new Uri(xpathVersion) : null,
                Version = version
            };

            policy.Namespaces = nsMgr;

            foreach (var combPar in combParams) {
                policy.CombinerParameters.Add(combPar);
            }

            IDictionary<Tuple<string, string>, Action> dicts = new Dictionary<Tuple<string, string>, Action>()
            {
                { new Tuple<string, string>(XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy), () => {
                    reader.ReadStartElement(XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy);
                    this.ReadList(policy.ChoiceCombinerParameters, XacmlConstants.ElementNames.CombinerParameter, this.Version.NamespacePolicy, this.ReadCombinerParameter, reader, isRequired: false);
                    reader.ReadEndElement(); }
                },
                { new Tuple<string, string>(XacmlConstants.ElementNames.RuleCombinerParameters, this.Version.NamespacePolicy), () => policy.RuleCombinerParameters.Add(this.ReadRuleCombinerParameters(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.VariableDefinition, this.Version.NamespacePolicy), () => policy.VariableDefinitions.Add(this.ReadVariableDefinition(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.Rule, this.Version.NamespacePolicy), () => policy.Rules.Add(this.ReadRule(reader)) },
            };

            this.ReadChoiceMultiply(reader, dicts);

            // choice [1 .. *]
            if (policy.VariableDefinitions.Count == 0 && policy.Rules.Count == 0) {
                throw ThrowHelperXml(reader, "VariableDefinition or Rule are required");
            }

            if (policy.CombinerParameters.Count > 1) {
                throw ThrowHelperXml(reader, "CombinerParameterc count > 1");
            }

            if (policy.RuleCombinerParameters.Count > 1) {
                throw ThrowHelperXml(reader, "RuleCombinerParameters count > 1");
            }

            if (reader.IsStartElement(XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy);

                this.ReadList(policy.Obligations, XacmlConstants.ElementNames.Obligation, this.Version.NamespacePolicy, ReadObligation, reader, isRequired: true);

                // end obligations
                reader.ReadEndElement();
            }

            // end policy
            reader.ReadEndElement();

            return policy;
        }

        /// <summary>
        /// Reads the combiner parameter.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlCombinerParameter ReadCombinerParameter(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.CombinerParameter, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            string parameterName = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.ParameterName);

            reader.ReadStartElement();

            XacmlAttributeValue attr = this.ReadRequired(XacmlConstants.ElementNames.AttributeValue, this.Version.NamespacePolicy, this.ReadAttributeValue, reader);

            reader.ReadEndElement();

            return new XacmlCombinerParameter(parameterName, attr);
        }

        /// <summary>
        /// Reads the rule.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">
        /// RuleId IsNullOrEmpty
        /// or
        /// Effect IsNullEmpty
        /// </exception>
        /// <exception cref="XacmlSerializationException">Wrong XacmlEffectType value</exception>
        protected override XacmlRule ReadRule(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Rule, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            var gaRuleId = reader.GetAttribute("RuleId");

            if (string.IsNullOrEmpty(gaRuleId)) {
                throw ThrowHelperXml(reader, "RuleId IsNullOrEmpty");
            }

            var gaEffect = reader.GetAttribute("Effect");
            XacmlEffectType effectType = XacmlEffectType.Deny;
            if (string.Equals(gaEffect, "Deny", StringComparison.OrdinalIgnoreCase)) {
                effectType = XacmlEffectType.Deny;
            }
            else if (string.Equals(gaEffect, "Permit", StringComparison.OrdinalIgnoreCase)) {
                effectType = XacmlEffectType.Permit;
            }
            else {
                throw ThrowHelperXml(reader, "Wrong XacmlEffectType value");
            }

            if (string.IsNullOrEmpty(gaEffect)) {
                throw ThrowHelperXml(reader, "Effect IsNullEmpty");
            }

            if (reader.IsEmptyElement) {
                reader.Read();
                return new XacmlRule(gaRuleId, effectType);
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Rule, this.Version.NamespacePolicy);

            string description = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy)) {
                description = reader.ReadElementContentAsString(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy);
            }

            XacmlTarget target = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy)) {
                target = this.ReadTarget(reader);
            }

            XacmlExpression condition = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.Condition, this.Version.NamespacePolicy)) {
                condition = this.ReadCondition(reader);
            }

            reader.ReadEndElement();
            return new XacmlRule(gaRuleId, effectType) { Description = description, Target = target, Condition = condition };
        }

        /// <summary>
        /// Reads the rule combiner parameters.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlRuleCombinerParameters ReadRuleCombinerParameters(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.RuleCombinerParameters, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            string ruleIdRef = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.RuleIdRef);

            reader.ReadStartElement(XacmlConstants.ElementNames.RuleCombinerParameters, this.Version.NamespacePolicy);

            XacmlRuleCombinerParameters par = new XacmlRuleCombinerParameters(ruleIdRef);

            this.ReadList(par.CombinerParameters, XacmlConstants.ElementNames.CombinerParameter, this.Version.NamespacePolicy, this.ReadCombinerParameter, reader);

            reader.ReadEndElement();

            return par;
        }

        /// <summary>
        /// Reads the policy combiner parameters.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlPolicyCombinerParameters ReadPolicyCombinerParameters(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.PolicyCombinerParameters, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            Uri policyIdRef = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.PolicyIdRef);

            reader.ReadStartElement(XacmlConstants.ElementNames.PolicyCombinerParameters, this.Version.NamespacePolicy);

            XacmlPolicyCombinerParameters par = new XacmlPolicyCombinerParameters(policyIdRef);

            this.ReadList(par.CombinerParameters, XacmlConstants.ElementNames.CombinerParameter, this.Version.NamespacePolicy, this.ReadCombinerParameter, reader);

            reader.ReadEndElement();

            return par;
        }

        /// <summary>
        /// Reads the policy set combiner parameters.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlPolicySetCombinerParameters ReadPolicySetCombinerParameters(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.PolicySetCombinerParameters, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            Uri policySetIdRef = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.PolicySetIdRef);

            reader.ReadStartElement(XacmlConstants.ElementNames.PolicySetCombinerParameters, this.Version.NamespacePolicy);

            XacmlPolicySetCombinerParameters par = new XacmlPolicySetCombinerParameters(policySetIdRef);

            this.ReadList(par.CombinerParameters, XacmlConstants.ElementNames.CombinerParameter, this.Version.NamespacePolicy, this.ReadCombinerParameter, reader);

            reader.ReadEndElement();

            return par;
        }

        /// <summary>
        /// Reads the variable definition.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="XacmlSerializationException">Wrong VariableDefinition element content</exception>
        protected virtual XacmlVariableDefinition ReadVariableDefinition(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.VariableDefinition, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            string variableId = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.VariableId);

            XacmlVariableDefinition result = new XacmlVariableDefinition(variableId);
            if (reader.IsEmptyElement) {
                return result;
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.VariableDefinition, this.Version.NamespacePolicy);

            switch (reader.Name) {
                case XacmlConstants.ElementNames.VariableReference:
                        result.Property = this.ReadOptional(XacmlConstants.ElementNames.VariableReference, this.Version.NamespacePolicy,
                            new ReadElement<XacmlVariableReference>(
                                o => new XacmlVariableReference(this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.VariableId))
                            ), reader);
                        break;
                case XacmlConstants.ElementNames.AttributeSelector:
                        result.Property = this.ReadOptional(XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy, this.ReadAttributeSelector, reader);
                        break;
                case XacmlConstants.ElementNames.ResourceAttributeDesignator:
                        result.Property = this.ReadOptional(XacmlConstants.ElementNames.ResourceAttributeDesignator, this.Version.NamespacePolicy, this.ReadAttributeDesignator, reader);
                        break;
                case XacmlConstants.ElementNames.ActionAttributeDesignator:
                        result.Property = this.ReadOptional(XacmlConstants.ElementNames.ActionAttributeDesignator, this.Version.NamespacePolicy, this.ReadAttributeDesignator, reader);
                        break;
                case XacmlConstants.ElementNames.EnvironmentAttributeDesignator:
                        result.Property = this.ReadOptional(XacmlConstants.ElementNames.EnvironmentAttributeDesignator, this.Version.NamespacePolicy, this.ReadAttributeDesignator, reader);
                        break;
                case XacmlConstants.ElementNames.SubjectAttributeDesignator:
                        result.Property = this.ReadOptional(XacmlConstants.ElementNames.SubjectAttributeDesignator, this.Version.NamespacePolicy, this.ReadAttributeDesignator, reader);
                        break;
                case XacmlConstants.ElementNames.AttributeValue:
                        result.Property = this.ReadOptional(XacmlConstants.ElementNames.AttributeValue, this.Version.NamespacePolicy, this.ReadAttributeValue, reader);
                        break;
                case XacmlConstants.ElementNames.Function:
                        result.Property = this.ReadOptional(XacmlConstants.ElementNames.Function, this.Version.NamespacePolicy, this.ReadFunction, reader);
                        break;
                case XacmlConstants.ElementNames.Apply:
                        result.Property = this.ReadOptional(XacmlConstants.ElementNames.Apply, this.Version.NamespacePolicy, this.ReadApply, reader);
                        break;
                default:
                        throw ThrowHelperXml(reader, "Wrong VariableDefinition element content");
            }

            reader.ReadEndElement();

            return result;
        }

        /// <summary>
        /// Reads the condition_2_0.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="XacmlSerializationException">Wrong VariableDefinition element content</exception>
        protected override XacmlExpression ReadCondition(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Condition, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Condition, this.Version.NamespacePolicy);

            XacmlExpression condition = new XacmlExpression();

            if (reader.IsStartElement(XacmlConstants.ElementNames.VariableReference, this.Version.NamespacePolicy)) {
                condition.Property = this.ReadOptional(XacmlConstants.ElementNames.VariableReference, this.Version.NamespacePolicy,
                            new ReadElement<XacmlVariableReference>(
                                o => {
                                    string res = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.VariableId);
                                    reader.Read();
                                    return new XacmlVariableReference(res);
                                }
                            ), reader);
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy)) {
                condition.Property = this.ReadOptional(XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy, this.ReadAttributeSelector, reader);
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.ResourceAttributeDesignator, this.Version.NamespacePolicy)) {
                condition.Property = this.ReadOptional(XacmlConstants.ElementNames.ResourceAttributeDesignator, this.Version.NamespacePolicy, this.ReadAttributeDesignator, reader);
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.ActionAttributeDesignator, this.Version.NamespacePolicy)) {
                condition.Property = this.ReadOptional(XacmlConstants.ElementNames.ActionAttributeDesignator, this.Version.NamespacePolicy, this.ReadAttributeDesignator, reader);
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.EnvironmentAttributeDesignator, this.Version.NamespacePolicy)) {
                condition.Property = this.ReadOptional(XacmlConstants.ElementNames.EnvironmentAttributeDesignator, this.Version.NamespacePolicy, this.ReadAttributeDesignator, reader);
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.SubjectAttributeDesignator, this.Version.NamespacePolicy)) {
                condition.Property = this.ReadOptional(XacmlConstants.ElementNames.SubjectAttributeDesignator, this.Version.NamespacePolicy, this.ReadAttributeDesignator, reader);
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.AttributeValue, this.Version.NamespacePolicy)) {
                condition.Property = this.ReadOptional(XacmlConstants.ElementNames.AttributeValue, this.Version.NamespacePolicy, this.ReadAttributeValue, reader);
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.Function, this.Version.NamespacePolicy)) {
                condition.Property = this.ReadOptional(XacmlConstants.ElementNames.Function, this.Version.NamespacePolicy, this.ReadFunction, reader);
            }
            else if (reader.IsStartElement(XacmlConstants.ElementNames.Apply, this.Version.NamespacePolicy)) {
                condition.Property = this.ReadOptional(XacmlConstants.ElementNames.Apply, this.Version.NamespacePolicy, this.ReadApply, reader);
            }
            else {
                throw ThrowHelperXml(reader, "Wrong VariableDefinition element content");
            }

            reader.ReadEndElement();
            return condition;
        }

        /// <summary>
        /// Reads the apply.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected override XacmlApply ReadApply(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Apply, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            Uri functionId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.FunctionId);

            reader.ReadStartElement(XacmlConstants.ElementNames.Apply, this.Version.NamespacePolicy);

            XacmlApply apply = new XacmlApply(functionId);

            IDictionary<Tuple<string, string>, Action> dicts = new Dictionary<Tuple<string, string>, Action>()
            {
                { new Tuple<string, string>(XacmlConstants.ElementNames.Apply, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadApply(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.Function, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadFunction(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.AttributeValue, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadAttributeValue(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.SubjectAttributeDesignator, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadAttributeDesignator(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.ResourceAttributeDesignator, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadAttributeDesignator(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.ActionAttributeDesignator, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadAttributeDesignator(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.EnvironmentAttributeDesignator, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadAttributeDesignator(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadAttributeSelector(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.VariableReference, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadOptional(XacmlConstants.ElementNames.VariableReference, this.Version.NamespacePolicy,
                            new ReadElement<XacmlVariableReference>(
                                o =>
                                    {
                                        string res = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.VariableId);
                                        reader.Read();
                                        return new XacmlVariableReference(res);
                                    }
                            ), reader)) },
            };

            this.ReadChoiceMultiply(reader, dicts);

            reader.ReadEndElement();

            return apply;
        }

        /// <summary>
        /// Writes the policy set.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        public override void WritePolicySet(XmlWriter writer, XacmlPolicySet data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicySet, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.PolicySetId, data.PolicySetId.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.PolicyCombiningAlgId, data.PolicyCombiningAlgId.OriginalString);

            if (!string.IsNullOrEmpty(data.Version)) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Version, data.Version);
            }

            if (data.Description != null) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy, data.Description);
            }

            // PolicySetDefaults
            if (data.XPathVersion != null) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicySetDefaults, this.Version.NamespacePolicy);
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy, data.XPathVersion.ToString());
                writer.WriteEndElement();
            }

            // Target
            this.WriteTarget(writer, data.Target);

            // PolicySet
            foreach (var policySet in data.PolicySets) {
                this.WritePolicySet(writer, policySet);
            }

            // Policy
            foreach (var policy in data.Policies) {
                this.WritePolicy(writer, policy);
            }

            // PolicySetIdReference
            foreach (var policySetIdReference in data.PolicySetIdReferences) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicySetIdReference, this.Version.NamespacePolicy, policySetIdReference.ToString());
            }

            // PolicyIdReference
            foreach (var policyIdReference in data.PolicyIdReferences) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicyIdReference, this.Version.NamespacePolicy, policyIdReference.ToString());
            }

            if (data.CombinerParameters.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy);

                foreach (var combinerParameter in data.CombinerParameters) {
                    this.WriteCombinerParameter(writer, combinerParameter);
                }

                writer.WriteEndElement();
            }

            foreach (var policyCombinedParameters in data.PolicyCombinerParameters) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicyCombinerParameters, this.Version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.PolicyIdRef, policyCombinedParameters.PolicyIdRef.OriginalString);

                foreach (var combinedParameter in policyCombinedParameters.CombinerParameters) {
                    this.WriteCombinerParameter(writer, combinedParameter);
                }

                writer.WriteEndElement();
            }

            foreach (var policySetCombinedParameters in data.PolicySetCombinerParameters) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicySetCombinerParameters, this.Version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.PolicySetIdRef, policySetCombinedParameters.PolicySetIdRef.OriginalString);

                foreach (var combinedParameter in policySetCombinedParameters.CombinerParameters) {
                    this.WriteCombinerParameter(writer, combinedParameter);
                }

                writer.WriteEndElement();
            }

            // Obligations
            if (data.Obligations.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy);
                foreach (var obligation in data.Obligations) {
                    this.WriteObligation(writer, obligation);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the policy.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        public override void WritePolicy(XmlWriter writer, XacmlPolicy data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Policy, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.PolicyId, data.PolicyId.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.RuleCombiningAlgId, data.RuleCombiningAlgId.OriginalString);

            if (!string.IsNullOrEmpty(data.Version)) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Version, data.Version);
            }

            // ?Description
            if (data.Description != null) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy, data.Description);
            }

            // PolicyDefaults
            if (data.XPathVersion != null) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicyDefaults, this.Version.NamespacePolicy);
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy, data.XPathVersion.ToString());
                writer.WriteEndElement();
            }

            // Combined parameters
            if (data.CombinerParameters != null) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy);

                foreach (var combinerParameter in data.CombinerParameters) {
                    this.WriteCombinerParameter(writer, combinerParameter);
                }

                writer.WriteEndElement();
            }

            // Target
            this.WriteTarget(writer, data.Target);

            if (data.CombinerParameters.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy);

                foreach (var combinerParameter in data.ChoiceCombinerParameters) {
                    this.WriteCombinerParameter(writer, combinerParameter);
                }

                writer.WriteEndElement();
            }

            foreach (var ruleCombinedParameters in data.RuleCombinerParameters) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.RuleCombinerParameters, this.Version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.RuleIdRef, ruleCombinedParameters.RuleIdRef);

                foreach (var combinedParameter in ruleCombinedParameters.CombinerParameters) {
                    this.WriteCombinerParameter(writer, combinedParameter);
                }

                writer.WriteEndElement();
            }

            foreach (var variableDefinition in data.VariableDefinitions) {
                this.WriteVariableDefinition(writer, variableDefinition);
            }

            // Rule
            foreach (var rule in data.Rules) {
                this.WriteRule(writer, rule);
            }

            // Obligatoins
            if (data.Obligations.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy);
                foreach (var obligation in data.Obligations) {
                    this.WriteObligation(writer, obligation);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the rule.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected override void WriteRule(XmlWriter writer, XacmlRule data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Rule, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.RuleId, data.RuleId);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.Effect, data.Effect.ToString());

            if (data.Description != null) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy, data.Description);
            }

            if (data.Target != null) {
                this.WriteTarget(writer, data.Target);
            }

            if (data.Condition != null) {
                this.WriteCondition(writer, data.Condition);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the variable definition.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The variable definition.</param>
        protected virtual void WriteVariableDefinition(XmlWriter writer, XacmlVariableDefinition data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.VariableDefinition, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.VariableId, data.VariableId);

            this.WriteExpressionType(writer, data);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the condition.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The expression.</param>
        protected override void WriteCondition(XmlWriter writer, XacmlExpression data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Condition, this.Version.NamespacePolicy);
            this.WriteExpressionType(writer, data);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Write expression type(not element)
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="data"></param>
        protected virtual void WriteExpressionType(XmlWriter writer, XacmlExpression data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            Type applyElemType = data.Property.GetType();
            if (applyElemType == typeof(XacmlVariableReference)) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.VariableReference, this.Version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.VariableId, (data.Property as XacmlVariableReference).VariableReference);
                writer.WriteEndElement();
            }
            else if (applyElemType == typeof(XacmlAttributeSelector)) {
                XacmlAttributeSelector prop = data.Property as XacmlAttributeSelector;
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.RequestContextPath, prop.Path);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, prop.DataType.OriginalString);

                if (prop.MustBePresent.HasValue) {
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, XmlConvert.ToString(prop.MustBePresent.Value));
                }

                writer.WriteEndElement();
            }
            else if (applyElemType == typeof(XacmlResourceAttributeDesignator)) {
                this.WriteAttributeDesignator(writer, data.Property as XacmlResourceAttributeDesignator);
            }
            else if (applyElemType == typeof(XacmlActionAttributeDesignator)) {
                this.WriteAttributeDesignator(writer, data.Property as XacmlActionAttributeDesignator);
            }
            else if (applyElemType == typeof(XacmlEnvironmentAttributeDesignator)) {
                this.WriteAttributeDesignator(writer, data.Property as XacmlEnvironmentAttributeDesignator);
            }
            else if (applyElemType == typeof(XacmlSubjectAttributeDesignator)) {
                this.WriteAttributeDesignator(writer, data.Property as XacmlSubjectAttributeDesignator);
            }
            else if (applyElemType == typeof(XacmlAttributeValue)) {
                this.WriteAttributeValue(writer, data.Property as XacmlAttributeValue);
            }
            else if (applyElemType == typeof(XacmlFunction)) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Function, this.Version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.FunctionId, (data.Property as XacmlFunction).FunctionId.OriginalString);
                writer.WriteEndElement();
            }
            else if (applyElemType == typeof(XacmlApply)) {
                this.WriteApply(writer, data.Property as XacmlApply);
            }
        }

        /// <summary>
        /// Writes the attribute designator.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected override void WriteAttributeDesignator(XmlWriter writer, XacmlAttributeDesignator data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            Action<string, dynamic, bool> action = (designatorType, attributeDesignator, writeSubjectCategory) => {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, designatorType, this.Version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.AttributeId, attributeDesignator.AttributeId.OriginalString);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, attributeDesignator.DataType.OriginalString);

                if (attributeDesignator.Issuer != null) {
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.Issuer, attributeDesignator.Issuer);
                }

                if (attributeDesignator.MustBePresent != null) {
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.MustBePresent, XmlConvert.ToString(attributeDesignator.MustBePresent));
                }

                writer.WriteEndElement();
            };

            var environmentAttributeDesignator = data as XacmlEnvironmentAttributeDesignator;
            if (environmentAttributeDesignator != null) {
                action(XacmlConstants.ElementNames.EnvironmentAttributeDesignator, environmentAttributeDesignator, false);
            }
            else {
                base.WriteAttributeDesignator(writer, data);
            }
        }

        /// <summary>
        /// Writes the match.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected override void WriteMatch(XmlWriter writer, XacmlMatch data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            Action<string, dynamic> action = (matchType, match) => {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, matchType, this.Version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.MatchId, match.MatchId.OriginalString);

                this.WriteAttributeValue(writer, match.AttributeValue);

                if (match.AttributeSelector == null && match.AttributeDesignator == null) {
                    throw new InvalidOperationException("AttributeSelectot and AttributeDescnator is null.");
                }

                if (match.AttributeDesignator != null) {
                    this.WriteAttributeDesignator(writer, match.AttributeDesignator);
                }

                if (match.AttributeSelector != null) {
                    this.WriteAttributeSelector(writer, match.AttributeSelector);
                }

                writer.WriteEndElement();
            };

            var environmentMatch = data as XacmlEnvironmentMatch;
            if (environmentMatch != null) {
                action(XacmlConstants.ElementNames.EnvironmentMatch, environmentMatch);
            }
            else {
                base.WriteMatch(writer, data);
            }
        }

        /// <summary>
        /// Writes the apply.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The xacml apply.</param>
        protected override void WriteApply(XmlWriter writer, XacmlApply data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Apply, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.FunctionId, data.FunctionId.OriginalString);

            foreach (IXacmlApply applyElem in data.Parameters) {
                Type applyElemType = applyElem.GetType();
                if (applyElemType == typeof(XacmlVariableReference)) {
                    XacmlVariableReference elem = applyElem as XacmlVariableReference;
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.VariableReference, this.Version.NamespacePolicy);
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.VariableId, elem.VariableReference);
                    writer.WriteEndElement();
                }
                else if (applyElemType == typeof(XacmlAttributeSelector)) {
                    XacmlAttributeSelector elem = applyElem as XacmlAttributeSelector;
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy);
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.RequestContextPath, elem.Path);
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, elem.DataType.OriginalString);

                    if (elem.MustBePresent.HasValue) {
                        writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, XmlConvert.ToString(elem.MustBePresent.Value));
                    }

                    writer.WriteEndElement();
                }
                else if (applyElemType == typeof(XacmlResourceAttributeDesignator)) {
                    this.WriteAttributeDesignator(writer, applyElem as XacmlResourceAttributeDesignator);
                }
                else if (applyElemType == typeof(XacmlActionAttributeDesignator)) {
                    this.WriteAttributeDesignator(writer, applyElem as XacmlActionAttributeDesignator);
                }
                else if (applyElemType == typeof(XacmlEnvironmentAttributeDesignator)) {
                    this.WriteAttributeDesignator(writer, applyElem as XacmlEnvironmentAttributeDesignator);
                }
                else if (applyElemType == typeof(XacmlSubjectAttributeDesignator)) {
                    this.WriteAttributeDesignator(writer, applyElem as XacmlSubjectAttributeDesignator);
                }
                else if (applyElemType == typeof(XacmlAttributeValue)) {
                    this.WriteAttributeValue(writer, applyElem as XacmlAttributeValue);
                }
                else if (applyElemType == typeof(XacmlFunction)) {
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Function, this.Version.NamespacePolicy);
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.FunctionId, (applyElem as XacmlFunction).FunctionId.OriginalString);
                    writer.WriteEndElement();
                }
                else if (applyElemType == typeof(XacmlApply)) {
                    this.WriteApply(writer, applyElem as XacmlApply);
                }
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the combiner parameter.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="combinerParameter">The combiner parameter.</param>
        protected void WriteCombinerParameter(XmlWriter writer, XacmlCombinerParameter combinerParameter) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (combinerParameter == null) {
                throw new ArgumentNullException(nameof(combinerParameter));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.CombinerParameter, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.ParameterName, combinerParameter.ParameterName);

            this.WriteAttributeValue(writer, combinerParameter.AttributeValue);

            writer.WriteEndElement();
        }

        #endregion

        #region Context

        /// <summary>
        /// Reads the context attribute.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected override XacmlContextAttribute ReadContextAttribute(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Attribute, this.Version.NamespaceContext)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            // Read attributes
            Uri attributeId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.AttributeId, isRequered: true);
            Uri dataType = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.DataType, isRequered: true);
            string issuer = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Issuer, isRequered: false);

            reader.ReadStartElement(XacmlConstants.ElementNames.Attribute, this.Version.NamespaceContext);

            // Read elements
            ICollection<XacmlContextAttributeValue> attrValues = new List<XacmlContextAttributeValue>();
            this.ReadList(attrValues, XacmlConstants.ElementNames.AttributeValue, this.Version.NamespaceContext, ReadContextAttributeValue, reader, isRequired: true);

            XacmlContextAttribute result = new XacmlContextAttribute(attributeId, dataType, attrValues) {
                Issuer = issuer,
            };

            reader.ReadEndElement();

            return result;
        }

        /// <summary>
        /// Reads the context request.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public override XacmlContextRequest ReadContextRequest(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!XacmlProtocolSerializer.CanReadContext(reader, XacmlConstants.ElementNames.Request, this.Version.NamespaceContext)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Request, this.Version.NamespaceContext);

            List<XacmlContextSubject> subjects = new List<XacmlContextSubject>();
            this.ReadList(subjects, XacmlConstants.ElementNames.Subject, this.Version.NamespaceContext, ReadContextSubject, reader, isRequired: true);

            List<XacmlContextResource> resources = new List<XacmlContextResource>();
            this.ReadList(resources, XacmlConstants.ElementNames.Resource, this.Version.NamespaceContext, ReadContextResource, reader, isRequired: true);

            XacmlContextRequest result = new XacmlContextRequest(
                resources,
                this.ReadRequired(XacmlConstants.ElementNames.Action, this.Version.NamespaceContext, this.ReadContextAction, reader),
                subjects,
                this.ReadRequired(XacmlConstants.ElementNames.Environment, this.Version.NamespaceContext, this.ReadContextEnvironment, reader)
                );

            reader.ReadEndElement();

            return result;
        }

        /// <summary>
        /// Reads the context result.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected override XacmlContextResult ReadContextResult(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Result, this.Version.NamespaceContext)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            // Read attributes
            string resourceId = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.ResourceId, isRequered: false);

            reader.ReadStartElement(XacmlConstants.ElementNames.Result, this.Version.NamespaceContext);

            // Read elements
            XacmlContextResult result = new XacmlContextResult(
                this.ReadRequired(XacmlConstants.ElementNames.Decision, this.Version.NamespaceContext, ReadContextDecision, reader)
                ) {
                Status = this.ReadOptional(XacmlConstants.ElementNames.Status, this.Version.NamespaceContext, ReadContextStatus, reader),
                ResourceId = resourceId,
            };

            if (reader.IsStartElement(XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy);

                this.ReadList<XacmlObligation>(result.Obligations, XacmlConstants.ElementNames.Obligation, this.Version.NamespacePolicy, ReadObligation, reader, isRequired: false);

                // end obligations
                reader.ReadEndElement();
            }

            reader.ReadEndElement();

            return result;
        }

        /// <summary>
        /// Writes the context attribute.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="attr">The attribute.</param>
        /// <exception cref="XacmlSerializationException">IssueInstant obsolleten from version 2.0</exception>
        protected override void WriteContextAttribute(XmlWriter writer, XacmlContextAttribute attr) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (attr == null) {
                throw new ArgumentNullException(nameof(attr));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Attribute, this.Version.NamespaceContext);

            writer.WriteAttributeString(XacmlConstants.AttributeNames.AttributeId, attr.AttributeId.ToString());
            writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, attr.DataType.ToString());

            if (!string.IsNullOrEmpty(attr.Issuer)) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Issuer, attr.Issuer);
            }

            if (attr.IssueInstant != null) {
                throw new InvalidOperationException("IssueInstant obsolleten from version 2.0");
            }

            foreach (var value in attr.AttributeValues) {
                this.WriteContextAttributeValue(writer, value);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the context request.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        public override void WriteContextRequest(XmlWriter writer, XacmlContextRequest data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Request, this.Version.NamespaceContext);

            // Subject
            foreach (var subject in data.Subjects) {
                this.WriteContextSubject(writer, subject);
            }

            foreach (var resource in data.Resources) {
                this.WriteContextResource(writer, resource);
            }

            this.WriteContextAction(writer, data.Action);

            this.WriteContextEnvironment(writer, data.Environment);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the context result.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="result">The result.</param>
        /// <exception cref="XacmlSerializationException">Obligations should be &lt; 2 until version 2.0</exception>
        protected override void WriteContextResult(XmlWriter writer, XacmlContextResult result) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (result == null) {
                throw new ArgumentNullException(nameof(result));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Result, this.Version.NamespaceContext);

            if (!string.IsNullOrEmpty(result.ResourceId)) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.ResourceId, result.ResourceId);
            }

            this.WriteContextDecision(writer, result.Decision);

            if (result.Status != null) {
                this.WriteContextStatus(writer, result.Status);
            }

            if (result.Obligations.Count > 1) {
                throw new InvalidOperationException("Obligations should be < 2 until version 2.0");
            }

            if (result.Obligations.Count > 0) {
                this.WriteObligation(writer, result.Obligations.First<XacmlObligation>());
            }

            writer.WriteEndElement();
        }

        #endregion
    }
}