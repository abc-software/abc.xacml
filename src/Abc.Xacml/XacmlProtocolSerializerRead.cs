// ----------------------------------------------------------------------------
// <copyright file="XacmlProtocolSerializerRead.cs" company="ABC Software Ltd">
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
    using System.Xml;
    using Abc.Xacml.Policy;

    public partial class XacmlProtocolSerializer {
        /// <summary>
        /// Reads the policy set.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public virtual XacmlPolicySet ReadPolicySet(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!this.CanRead(reader, XacmlConstants.ElementNames.PolicySet)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            // Collect namespaces
            IDictionary<string, string> nsMgr = new Dictionary<string, string>();
            for (int i = 0; i < reader.AttributeCount; i++) {
                reader.MoveToAttribute(i);
                if (reader.Prefix == "xmlns") {
                    nsMgr.Add(reader.LocalName, reader.Value);
                }
            }

            Uri gaPolicySetId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.PolicySetId);

            Uri gaPolicyCombiningAlgId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.PolicyCombiningAlgId);

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
            };

            policySet.Namespaces = nsMgr;

            IDictionary<Tuple<string, string>, Action> dicts = new Dictionary<Tuple<string, string>, Action>()
            {
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicySet, this.Version.NamespacePolicy), () => policySet.PolicySets.Add(this.ReadPolicySet(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.Policy, this.Version.NamespacePolicy), () => policySet.Policies.Add(this.ReadPolicy(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicySetIdReference, this.Version.NamespacePolicy), () => policySet.PolicySetIdReferences.Add(this.ReadPolicySetIdReference(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicyIdReference, this.Version.NamespacePolicy), () => policySet.PolicyIdReferences.Add(this.ReadPolicyIdReference(reader)) },
            };

            this.ReadChoiceMultiply(reader, dicts);

            if (reader.IsStartElement(XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy);

                this.ReadList(policySet.Obligations, XacmlConstants.ElementNames.Obligation, this.Version.NamespacePolicy, ReadObligation, reader);

                // end obligations
                reader.ReadEndElement();
            }

            return policySet;
        }

        /// <summary>
        /// Determines whether this instance can read the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="localName">Name of the local.</param>
        /// <returns>
        ///   <c>true</c> if this instance can read the specified reader; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool CanRead(XmlReader reader, string localName) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (localName == "*") {
                return CanReadFirst(reader, reader.LocalName);
            }
            else {
                return CanReadFirst(reader, localName);
            }
        }

        /// <summary>
        /// Determines whether this instance [can read first] the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="localName">Name of the local.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can read first] the specified reader; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool CanReadFirst(XmlReader reader, string localName) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.IsStartElement(localName, this.Version.NamespacePolicy)) {
                // reader.ReadStartElement();
                return true;
            }

            return false;
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
        /// </exception>
        public virtual XacmlPolicy ReadPolicy(XmlReader reader) {
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

            if (!this.CanRead(reader, XacmlConstants.ElementNames.Policy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            var gaPolicyId = reader.GetAttribute("PolicyId");

            if (string.IsNullOrEmpty(gaPolicyId)) {
                throw ThrowHelperXml(reader, "PolicyId IsNullOrEmpty");
            }

            var gaRuleCombiningAlgId = reader.GetAttribute("RuleCombiningAlgId");

            if (string.IsNullOrEmpty(gaRuleCombiningAlgId)) {
                throw ThrowHelperXml(reader, "RuleCombiningAlgId IsNullOrEmpty");
            }

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

            XacmlTarget target = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy)) {
                target = ReadTarget(reader);
            }

            XacmlPolicy policy = new XacmlPolicy(new Uri(gaPolicyId, UriKind.RelativeOrAbsolute), new Uri(gaRuleCombiningAlgId, UriKind.RelativeOrAbsolute), target) {
                Description = description,
                XPathVersion = xpathVersion != null ? new Uri(xpathVersion) : null,
            };

            policy.Namespaces = nsMgr;

            XacmlRule rule = null;
            while (reader.IsStartElement(XacmlConstants.ElementNames.Rule, this.Version.NamespacePolicy)) {
                rule = ReadRule(reader);
                policy.Rules.Add(rule);
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
        /// Reads the rule.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">
        /// Rule NotStartElement
        /// or
        /// RuleId IsNullOrEmpty
        /// or
        /// Effect IsNullEmpty
        /// </exception>
        /// <exception cref="XacmlSerializationException">Wrong XacmlEffectType value</exception>
        protected virtual XacmlRule ReadRule(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Rule, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "Rule NotStartElement");
            }

            var gaRuleId = reader.GetAttribute("RuleId");

            if (string.IsNullOrEmpty(gaRuleId)) {
                throw ThrowHelperXml(reader, "RuleId IsNullOrEmpty");
            }

            var gaEffect = reader.GetAttribute("Effect");

            if (string.IsNullOrEmpty(gaEffect)) {
                throw ThrowHelperXml(reader, "Effect IsNullEmpty");
            }

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
        /// Reads the target.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">
        /// Target NotStartElement
        /// or
        /// Subjects NotStartElement
        /// or
        /// Resources NotStartElement
        /// or
        /// Actions NotStartElement
        /// </exception>
        protected virtual XacmlTarget ReadTarget(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "Target NotStartElement");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy);

            var subjects = new List<XacmlSubject>();
            var resources = new List<XacmlResource>();
            var actions = new List<XacmlAction>();

            Dictionary<string, Action> dict = new Dictionary<string, Action>
            {
                { XacmlConstants.ElementNames.Subject, () => { this.ReadList(subjects, XacmlConstants.ElementNames.Subject, this.Version.NamespacePolicy, this.ReadSubject, reader, true); } },
                { XacmlConstants.ElementNames.Resource, () => { this.ReadList(resources, XacmlConstants.ElementNames.Resource, this.Version.NamespacePolicy, this.ReadResource, reader, true); } },
                { XacmlConstants.ElementNames.Action, () => { this.ReadList(actions, XacmlConstants.ElementNames.Action, this.Version.NamespacePolicy, this.ReadAction, reader, true); } },
            };

            Action<string, string, string> read = (type, baseType, anyType) => {
                if (reader.IsStartElement(baseType, this.Version.NamespacePolicy)) {
                    reader.Read();
                    if (!reader.IsStartElement(type, this.Version.NamespacePolicy)) {
                        // Ignore (AnySubject|AnyResource|AnyAction) element
                        reader.ReadStartElement(anyType, this.Version.NamespacePolicy);
                    }
                    else {
                        dict[type].Invoke();
                    }

                    reader.ReadEndElement();
                }
                else {
                    throw ThrowHelperXml(reader, string.Format("{0} NotStartElement", baseType));
                }
            };

            // Subjects
            read(XacmlConstants.ElementNames.Subject, XacmlConstants.ElementNames.Subjects, XacmlConstants.ElementNames.AnySubject);

            // Resources
            read(XacmlConstants.ElementNames.Resource, XacmlConstants.ElementNames.Resources, XacmlConstants.ElementNames.AnyResource);

            // Actions
            read(XacmlConstants.ElementNames.Action, XacmlConstants.ElementNames.Actions, XacmlConstants.ElementNames.AnyAction);

            reader.ReadEndElement();
            return new XacmlTarget(subjects, resources, actions);
        }

        /// <summary>
        /// Reads the condition.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">Condition NotStartElement</exception>
        protected virtual XacmlExpression ReadCondition(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Condition, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "Condition NotStartElement");
            }

            Uri functionId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.FunctionId);

            reader.ReadStartElement(XacmlConstants.ElementNames.Condition, this.Version.NamespacePolicy);

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
            };

            this.ReadChoiceMultiply(reader, dicts);

            reader.ReadEndElement();

            return new XacmlExpression() { Property = apply };
        }

        /// <summary>
        /// Reads the apply.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">Apply NotStartElement</exception>
        protected virtual XacmlApply ReadApply(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Apply, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "Apply NotStartElement");
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
            };

            this.ReadChoiceMultiply(reader, dicts);

            reader.ReadEndElement();

            return apply;
        }

        /// <summary>
        /// Reads the function.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">Function NotStartElement</exception>
        protected virtual XacmlFunction ReadFunction(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Function, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "Function NotStartElement");
            }

            Uri functionId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.FunctionId);

            XacmlFunction func = new XacmlFunction(functionId);
            reader.Read();
            return func;
        }

        /// <summary>
        /// Reads the subject.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">Subject NotStartElement</exception>
        protected virtual XacmlSubject ReadSubject(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Subject, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "Subject NotStartElement");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Subject, this.Version.NamespacePolicy);
            List<XacmlSubjectMatch> matches = new List<XacmlSubjectMatch>();
            this.ReadListAbstract(matches, XacmlConstants.ElementNames.SubjectMatch, this.Version.NamespacePolicy, this.ReadMatch, reader, true);
            XacmlSubject subj = new XacmlSubject(matches);

            reader.ReadEndElement();
            return subj;
        }

        /// <summary>
        /// Reads the resource.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">Resource NotStartElement</exception>
        protected virtual XacmlResource ReadResource(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Resource, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "Resource NotStartElement");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Resource, this.Version.NamespacePolicy);
            List<XacmlResourceMatch> matches = new List<XacmlResourceMatch>();
            this.ReadListAbstract(matches, XacmlConstants.ElementNames.ResourceMatch, this.Version.NamespacePolicy, this.ReadMatch, reader, true);
            XacmlResource res = new XacmlResource(matches);

            reader.ReadEndElement();
            return res;
        }

        /// <summary>
        /// Reads the action.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">Action NotStartElement</exception>
        protected virtual XacmlAction ReadAction(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Action, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "Action NotStartElement");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Action, this.Version.NamespacePolicy);

            List<XacmlActionMatch> matches = new List<XacmlActionMatch>();

            this.ReadListAbstract(matches, XacmlConstants.ElementNames.ActionMatch, this.Version.NamespacePolicy, ReadMatch, reader, true);

            XacmlAction act = new XacmlAction(matches);

            reader.ReadEndElement();
            return act;
        }

        /// <summary>
        /// Reads the match.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlMatch ReadMatch(XmlReader reader) {
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

                switch (matchType) {
                    case XacmlConstants.ElementNames.ActionMatch:
                        ret = des != null
                               ? new XacmlActionMatch(new Uri(gaMatchId, UriKind.RelativeOrAbsolute), attributeValue, (XacmlActionAttributeDesignator)des)
                               : new XacmlActionMatch(new Uri(gaMatchId, UriKind.RelativeOrAbsolute), attributeValue, sel);
                        break;
                    case XacmlConstants.ElementNames.SubjectMatch:
                        ret = des != null
                               ? new XacmlSubjectMatch(new Uri(gaMatchId, UriKind.RelativeOrAbsolute), attributeValue, (XacmlSubjectAttributeDesignator)des)
                               : new XacmlSubjectMatch(new Uri(gaMatchId, UriKind.RelativeOrAbsolute), attributeValue, sel);
                        break;
                    case XacmlConstants.ElementNames.ResourceMatch:
                        ret = des != null
                               ? new XacmlResourceMatch(new Uri(gaMatchId, UriKind.RelativeOrAbsolute), attributeValue, (XacmlResourceAttributeDesignator)des)
                               : new XacmlResourceMatch(new Uri(gaMatchId, UriKind.RelativeOrAbsolute), attributeValue, sel);
                        break;
                    default:
                        break;
                }

                return ret;
            };

            if (reader.LocalName == XacmlConstants.ElementNames.ActionMatch) {
                return read(reader.LocalName, XacmlConstants.ElementNames.ActionAttributeDesignator, ReadAttributeDesignator);
            }
            else if (reader.LocalName == XacmlConstants.ElementNames.SubjectMatch) {
                return read(reader.LocalName, XacmlConstants.ElementNames.SubjectAttributeDesignator, ReadAttributeDesignator);
            }
            else if (reader.LocalName == XacmlConstants.ElementNames.ResourceMatch) {
                return read(reader.LocalName, XacmlConstants.ElementNames.ResourceAttributeDesignator, ReadAttributeDesignator);
            }
            else {
                throw ThrowHelperXml(reader, string.Format("Incorrect start element. redaer.LocalName={0} ReadMatch", reader.LocalName));
            }
        }

        /// <summary>
        /// Reads the attribute value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">
        /// AttributeValue NotStartElement
        /// or
        /// DataType IsNullOrEmpty
        /// </exception>
        protected virtual XacmlAttributeValue ReadAttributeValue(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.AttributeValue, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "AttributeValue NotStartElement");
            }

            IDictionary<string, string> attributes = new Dictionary<string, string>();
            var dataType = reader.GetAttribute(XacmlConstants.AttributeNames.DataType);

            if (string.IsNullOrEmpty(dataType)) {
                throw ThrowHelperXml(reader, "DataType IsNullOrEmpty");
            }

            while (reader.MoveToNextAttribute()) {
                attributes.Add(reader.Name, reader.Value);
            }

            // Move the reader back to the element node.
            reader.MoveToElement();

            string value = null; //Any

            if (dataType == "http://www.w3.org/2001/XMLSchema#string") {
                // Do not need to use Trim because it blank characters are important for Regular Expression
                value = reader.ReadElementContentAsString();
            }
            else {
                value = reader.ReadInnerXml();
            }

            var attribute = new XacmlAttributeValue(new Uri(dataType, UriKind.RelativeOrAbsolute), value);

            foreach (var item in attributes) {
                attribute.Attributes.Add(new System.Xml.Linq.XAttribute(item.Key, item.Value)); // UNDONE: namespace
            }

            return attribute;
        }

        /// <summary>
        /// Reads the attribute designator.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlAttributeDesignator ReadAttributeDesignator(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            Func<string, XacmlAttributeDesignator> readDesignator = (elementName) => {
                if (!reader.IsStartElement(elementName, this.Version.NamespacePolicy)) {
                    throw ThrowHelperXml(reader, string.Format("{0} NotStartElement", elementName));
                }

                // @AttributeId
                Uri attributeId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.AttributeId);

                // @DataType
                Uri dataType = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.DataType);

                // @Issuer
                string issuer = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Issuer, isRequered: false);

                // @MustBePresent
                bool? mustbepresent = this.ReadAttribute<bool?>(reader, XacmlConstants.AttributeNames.MustBePresent, isRequered: false);

                // @SubjectCategory only for SubjectAttributeDesignator
                Uri subjectcategory = null;
                if (elementName == XacmlConstants.ElementNames.SubjectAttributeDesignator) {
                    subjectcategory = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.SubjectCategory, isRequered: false);
                }

                reader.ReadStartElement(elementName, this.Version.NamespacePolicy);

                switch (elementName) {
                    case XacmlConstants.ElementNames.SubjectAttributeDesignator:
                        return new XacmlSubjectAttributeDesignator(attributeId, dataType) {
                            Issuer = issuer,
                            MustBePresent = mustbepresent,
                            Category = subjectcategory,
                        };
                    case XacmlConstants.ElementNames.ResourceAttributeDesignator:
                        return new XacmlResourceAttributeDesignator(attributeId, dataType) {
                            Issuer = issuer,
                            MustBePresent = mustbepresent,
                        };
                    case XacmlConstants.ElementNames.ActionAttributeDesignator:
                        return new XacmlActionAttributeDesignator(attributeId, dataType) {
                            Issuer = issuer,
                            MustBePresent = mustbepresent,
                        };
                    case XacmlConstants.ElementNames.EnvironmentAttributeDesignator:
                        return new XacmlEnvironmentAttributeDesignator(attributeId, dataType) {
                            Issuer = issuer,
                            MustBePresent = mustbepresent,
                        };
                    default:
                        return null;
                }
            };

            if (reader.LocalName == XacmlConstants.ElementNames.SubjectAttributeDesignator
             || reader.LocalName == XacmlConstants.ElementNames.ResourceAttributeDesignator
             || reader.LocalName == XacmlConstants.ElementNames.ActionAttributeDesignator
             || reader.LocalName == XacmlConstants.ElementNames.EnvironmentAttributeDesignator) {
                return readDesignator(reader.LocalName);
            }
            else {
                throw ThrowHelperXml(reader, string.Format("Incorrect start element. redaer.LocalName={0} ReadMatch", reader.LocalName));
            }
        }

        /// <summary>
        /// Reads the attribute selector.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">
        /// AttributeSelector NotStartElement
        /// or
        /// RequestContextPath IsNullOrEmpty
        /// </exception>
        protected virtual XacmlAttributeSelector ReadAttributeSelector(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "AttributeSelector NotStartElement");
            }

            // @RequestContextPath
            var path = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.RequestContextPath);

            // @DataType
            var dataType = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.DataType);

            // @MustBePresent
            bool? mustBePresent = this.ReadAttribute<bool?>(reader, XacmlConstants.AttributeNames.MustBePresent, isRequered: false);

            reader.Read();

            return new XacmlAttributeSelector(path, dataType) {
                MustBePresent = mustBePresent,
            };
        }

        /// <summary>
        /// Reads the obligation.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">Obligation NotStartElement</exception>
        /// <exception cref="XacmlSerializationException">Wrong XacmlEffectType value</exception>
        protected virtual XacmlObligation ReadObligation(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Obligation, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "Obligation NotStartElement");
            }

            Uri gaObligationId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.ObligationId);

            string gaFulFillOn = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.FulfillOn);
            XacmlEffectType effectType = XacmlEffectType.Deny;
            if (string.Equals(gaFulFillOn, "Deny", StringComparison.OrdinalIgnoreCase)) {
                effectType = XacmlEffectType.Deny;
            }
            else if (string.Equals(gaFulFillOn, "Permit", StringComparison.OrdinalIgnoreCase)) {
                effectType = XacmlEffectType.Permit;
            }
            else {
                throw ThrowHelperXml(reader, "Wrong XacmlEffectType value");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Obligation, this.Version.NamespacePolicy);

            List<XacmlAttributeAssignment> assigments = new List<XacmlAttributeAssignment>();
            this.ReadList(assigments, XacmlConstants.ElementNames.AttributeAssignment, this.Version.NamespacePolicy, this.ReadAttributeAssigment, reader, isRequired: true);

            reader.ReadEndElement();

            return new XacmlObligation(gaObligationId, effectType, assigments);
        }

        /// <summary>
        /// Reads the attribute assigment.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">AttributeAssignment NotStartElement</exception>
        protected virtual XacmlAttributeAssignment ReadAttributeAssigment(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.AttributeAssignment, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "AttributeAssignment NotStartElement");
            }

            Uri gaDataType = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.DataType);

            Uri gaAttributeId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.AttributeId);

            reader.ReadStartElement(XacmlConstants.ElementNames.AttributeAssignment, this.Version.NamespacePolicy);
            string content = reader.ReadContentAsString();
            reader.ReadEndElement();

            return new XacmlAttributeAssignment(gaAttributeId, gaDataType, content);
        }

        /// <summary>
        /// Reads the policy set identifier reference.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">PolicySetIdReference NotStartElement</exception>
        protected virtual Uri ReadPolicySetIdReference(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.PolicySetIdReference, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "PolicySetIdReference NotStartElement");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.PolicySetIdReference, this.Version.NamespacePolicy);
            string result = reader.ReadContentAsString();
            reader.ReadEndElement();

            return new Uri(result, UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Reads the policy identifier reference.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">PolicyIdReference NotStartElement</exception>
        protected virtual Uri ReadPolicyIdReference(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.PolicyIdReference, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "PolicyIdReference NotStartElement");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.PolicyIdReference, this.Version.NamespacePolicy);
            string result = reader.ReadContentAsString();
            reader.ReadEndElement();

            return new Uri(result);
        }
    }
}
