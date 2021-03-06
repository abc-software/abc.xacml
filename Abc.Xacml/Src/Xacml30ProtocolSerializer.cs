﻿// ----------------------------------------------------------------------------
// <copyright file="Xacml30ProtocolSerializer.cs" company="ABC Software Ltd">
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
    using Abc.Xacml.Context;
    using Abc.Xacml.Policy;

    /// <summary>
    /// The XACML3.0 protocol serializer.
    /// </summary>
    /// <seealso cref="Abc.Xacml.Xacml20ProtocolSerializer" />
    public class Xacml30ProtocolSerializer : Xacml20ProtocolSerializer {
        /// <summary>
        /// Initializes a new instance of the <see cref="Xacml30ProtocolSerializer"/> class.
        /// </summary>
        public Xacml30ProtocolSerializer() 
            : base(XacmlVersion.Xacml30) {
        }

        /// <summary>
        /// Reads the match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">MatchId IsNullOrEmpty</exception>
        protected override XacmlMatch ReadMatch(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Match, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            var gaMatchId = reader.GetAttribute("MatchId");
            if (string.IsNullOrEmpty(gaMatchId)) {
                throw ThrowHelperXml(reader, "MatchId IsNullOrEmpty");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Match, this.Version.NamespacePolicy);

            var attributeValue = ReadAttributeValue(reader);

            XacmlMatch result;
            if (reader.IsStartElement(XacmlConstants.ElementNames.AttributeDesignator, this.Version.NamespacePolicy)) {
                var attributeDesignator = this.ReadAttributeDesignator(reader);
                result = new XacmlMatch(new Uri(gaMatchId, UriKind.RelativeOrAbsolute), attributeValue, attributeDesignator);
            }
            else {
                XacmlAttributeSelector attributeSelector = ReadAttributeSelector(reader);
                result = new XacmlMatch(new Uri(gaMatchId, UriKind.RelativeOrAbsolute), attributeValue, attributeSelector);
            }

            reader.ReadEndElement();
            return result;
        }

        /// <summary>
        /// Reads the attribute designator.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>        
        protected override XacmlAttributeDesignator ReadAttributeDesignator(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.AttributeDesignator, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            Uri gaAttributeId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.AttributeId);

            Uri gaDataType = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.DataType);
            bool mustbepresent = this.ReadAttribute<bool>(reader, XacmlConstants.AttributeNames.MustBePresent);
            Uri category = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.Category);

            // optional fields
            string issuer = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Issuer, isRequered: false);

            if (reader.IsEmptyElement) {
                reader.Read();
            }
            else {
                reader.ReadStartElement(XacmlConstants.ElementNames.AttributeDesignator, this.Version.NamespacePolicy);
                reader.ReadEndElement();
            }

            return new XacmlAttributeDesignator(category, gaAttributeId, gaDataType, mustbepresent) {
                Issuer = issuer,
            };
        }

        /// <summary>
        /// Reads the attribute selector.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected override XacmlAttributeSelector ReadAttributeSelector(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            var result = new XacmlAttributeSelector(
                this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.Category),
                this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Path),
                this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.DataType),
                this.ReadAttribute<bool>(reader, XacmlConstants.AttributeNames.MustBePresent)) {
                ContextSelectorId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.ContextSelectorId, isRequered: false),
            };

            reader.ReadInnerXml();

            return result;
        }

        /// <summary>
        /// Reads the target.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException">Target should contain AnyOf or be empty</exception>
        protected override XacmlTarget ReadTarget(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            if (reader.IsEmptyElement) {
                reader.Read();
                return new XacmlTarget(null);
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy);

            XacmlTarget target = new XacmlTarget(null);

            // AnyOf
            if (!reader.IsStartElement(XacmlConstants.ElementNames.AnyOf, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "Target should contain AnyOf or be empty");
            }

            ////this.ReadList<ICollection<ICollection<XacmlMatch>>>(target.AnyOf, XacmlConstants.ElementNames.AnyOf, this.Version.NamespacePolicy,
            ////    a => {
            ////        a.ReadStartElement(XacmlConstants.ElementNames.AnyOf, this.Version.NamespacePolicy);
            ////        ICollection<ICollection<XacmlMatch>> am = new List<ICollection<XacmlMatch>>();
            ////        this.ReadList<ICollection<XacmlMatch>>(am, XacmlConstants.ElementNames.AllOf, this.Version.NamespacePolicy,
            ////            o => {
            ////                o.ReadStartElement(XacmlConstants.ElementNames.AllOf, this.Version.NamespacePolicy);
            ////                ICollection<XacmlMatch> m = new List<XacmlMatch>();
            ////                this.ReadList<XacmlMatch>(m, XacmlConstants.ElementNames.Match, this.Version.NamespacePolicy, this.ReadMatch, o, true);
            ////                o.ReadEndElement();
            ////                return m;
            ////            }, a, true);
            ////        a.ReadEndElement();
            ////        return am;
            ////    }, reader, false);

            this.ReadList<XacmlAnyOf>(target.AnyOf, XacmlConstants.ElementNames.AnyOf, this.Version.NamespacePolicy,
                a => {
                    a.ReadStartElement(XacmlConstants.ElementNames.AnyOf, this.Version.NamespacePolicy);
                    ICollection<XacmlAllOf> am = new List<XacmlAllOf>();
                    this.ReadList<XacmlAllOf>(am, XacmlConstants.ElementNames.AllOf, this.Version.NamespacePolicy,
                        o => {
                            o.ReadStartElement(XacmlConstants.ElementNames.AllOf, this.Version.NamespacePolicy);
                            ICollection<XacmlMatch> m = new List<XacmlMatch>();
                            this.ReadList<XacmlMatch>(m, XacmlConstants.ElementNames.Match, this.Version.NamespacePolicy, this.ReadMatch, o, true);
                            o.ReadEndElement();
                            return new XacmlAllOf(m);
                        }, a, true);
                    a.ReadEndElement();
                    return new XacmlAnyOf(am);
                }, reader, false);

            reader.ReadEndElement();
            return target;
        }

        /// <summary>
        /// Reads the type of the expression.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="XacmlSerializationException">Wrong VariableDefinition element content</exception>
        protected virtual IXacmlApply ReadExpressionType(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            // move to first element
            reader.IsStartElement("*");

            IXacmlApply result;

            switch (reader.Name) {
                case XacmlConstants.ElementNames.VariableReference:
                        result = this.ReadOptional(XacmlConstants.ElementNames.VariableReference, this.Version.NamespacePolicy,
                            new ReadElement<XacmlVariableReference>(
                                o => {
                                    string res = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.VariableId);
                                    reader.Read();
                                    return new XacmlVariableReference(res);
                                }
                            ), reader);
                        break;
                case XacmlConstants.ElementNames.AttributeSelector:
                        result = this.ReadOptional(XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy, this.ReadAttributeSelector, reader);
                        break;
                case XacmlConstants.ElementNames.AttributeDesignator:
                        result = this.ReadOptional(XacmlConstants.ElementNames.AttributeDesignator, this.Version.NamespacePolicy, this.ReadAttributeDesignator, reader);
                        break;
                case XacmlConstants.ElementNames.AttributeValue:
                        result = this.ReadOptional(XacmlConstants.ElementNames.AttributeValue, this.Version.NamespacePolicy, this.ReadAttributeValue, reader);
                        break;
                case XacmlConstants.ElementNames.Function: 
                        result = this.ReadOptional(XacmlConstants.ElementNames.Function, this.Version.NamespacePolicy, this.ReadFunction, reader);
                        break;
                case XacmlConstants.ElementNames.Apply: 
                        result = this.ReadOptional(XacmlConstants.ElementNames.Apply, this.Version.NamespacePolicy, this.ReadApply, reader);
                        break;
                default: 
                        throw ThrowHelperXml(reader, "Wrong VariableDefinition element content");
            }

            return result;
        }

        /// <summary>
        /// Reads the condition
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected override XacmlExpression ReadCondition(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Condition, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Condition, this.Version.NamespacePolicy);

            XacmlExpression condition = new XacmlExpression() {
                Property = this.ReadExpressionType(reader)
            };

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

            if (reader.IsStartElement(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy)) {
                apply.Description = reader.ReadElementContentAsString(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy);
            }

            IDictionary<Tuple<string, string>, Action> dicts = new Dictionary<Tuple<string, string>, Action>()
            {
                { new Tuple<string, string>(XacmlConstants.ElementNames.Apply, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadApply(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.Function, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadFunction(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.AttributeValue, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadAttributeValue(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.AttributeDesignator, this.Version.NamespacePolicy), () => apply.Parameters.Add(this.ReadAttributeDesignator(reader)) },
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
        /// Reads the attribute.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlAttribute ReadAttribute(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Attribute, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            XacmlAttribute result = new XacmlAttribute(
                this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.AttributeId),
                this.ReadAttribute<bool>(reader, XacmlConstants.AttributeNames.IncludeInResult)
                );

            result.Issuer = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Issuer, isRequered: false);

            reader.ReadStartElement(XacmlConstants.ElementNames.Attribute, this.Version.NamespacePolicy);

            this.ReadList<XacmlAttributeValue>(result.AttributeValues, XacmlConstants.ElementNames.AttributeValue, this.Version.NamespacePolicy, this.ReadAttributeValue, reader, isRequired: true);

            reader.ReadEndElement();

            return result;
        }

        /// <summary>
        /// Reads the policy issuer.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="XacmlInvalidDataTypeException">XACMLAdmin Profile not implemented</exception>
        protected virtual XacmlPolicyIssuer ReadPolicyIssuer(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.PolicyIssuer, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            // PROFILE - XACMLAdmin - #POL02 - #SPEC1833
            throw new Abc.Xacml.Runtime.XacmlInvalidDataTypeException("XACMLAdmin Profile not implemented");

            if (reader.IsEmptyElement) {
                reader.Read();
                return new XacmlPolicyIssuer();
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.PolicyIssuer, this.Version.NamespacePolicy);

            XacmlPolicyIssuer result = new XacmlPolicyIssuer();

            if (reader.IsStartElement(XacmlConstants.ElementNames.Content, this.Version.NamespacePolicy)) {
                result.Content = reader.ReadInnerXml();
            }

            this.ReadList<XacmlAttribute>(result.Attributes, XacmlConstants.ElementNames.Attribute, this.Version.NamespacePolicy, ReadAttribute, reader, false);

            reader.ReadEndElement();

            return result;
        }

        /// <summary>
        /// Reads the attribute assignment expression.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlAttributeAssignmentExpression ReadAttributeAssignmentExpression(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.AttributeAssignmentExpression, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            Uri attributeId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.AttributeId);
            Uri category = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.Category, isRequered: false);
            string issuer = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Issuer, isRequered: false);

            reader.ReadStartElement(XacmlConstants.ElementNames.AttributeAssignmentExpression, this.Version.NamespacePolicy);

            var result = new XacmlAttributeAssignmentExpression(attributeId, this.ReadExpressionType(reader));

            reader.ReadEndElement();

            return result;
        }

        /// <summary>
        /// Reads the obligation expression.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="XacmlSerializationException">Wrong XacmlEffectType value</exception>
        protected virtual XacmlObligationExpression ReadObligationExpression(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.ObligationExpression, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

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

            var result = new XacmlObligationExpression(this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.ObligationId), effectType);

            reader.ReadStartElement(XacmlConstants.ElementNames.ObligationExpression, this.Version.NamespacePolicy);

            this.ReadList<XacmlAttributeAssignmentExpression>(result.AttributeAssignmentExpressions, XacmlConstants.ElementNames.AttributeAssignmentExpression, this.Version.NamespacePolicy, this.ReadAttributeAssignmentExpression, reader, isRequired: false);

            reader.ReadEndElement();

            return result;
        }

        /// <summary>
        /// Reads the advice expression.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <exception cref="XacmlSerializationException">Wrong XacmlEffectType value</exception>
        protected virtual XacmlAdviceExpression ReadAdviceExpression(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.AdviceExpression, this.Version.NamespacePolicy)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            string appliesTo = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.AppliesTo);
            XacmlEffectType effectType = XacmlEffectType.Deny;
            if (string.Equals(appliesTo, "Deny", StringComparison.OrdinalIgnoreCase)) {
                effectType = XacmlEffectType.Deny;
            }
            else if (string.Equals(appliesTo, "Permit", StringComparison.OrdinalIgnoreCase)) {
                effectType = XacmlEffectType.Permit;
            }
            else {
                throw ThrowHelperXml(reader, "Wrong XacmlEffectType value");
            }

            XacmlAdviceExpression result = new XacmlAdviceExpression(
                this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.AdviceId),
                effectType
                );

            reader.ReadStartElement(XacmlConstants.ElementNames.AdviceExpression, this.Version.NamespacePolicy);

            this.ReadList<XacmlAttributeAssignmentExpression>(result.AttributeAssignmentExpressions, XacmlConstants.ElementNames.AttributeAssignmentExpression, this.Version.NamespacePolicy, this.ReadAttributeAssignmentExpression, reader, isRequired: false);

            reader.ReadEndElement();

            return result;
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

            string version = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Version, isRequered: false);

            int? maxDelegationDepth = this.ReadAttribute<int?>(reader, XacmlConstants.AttributeNames.MaxDelegationDepth, this.Version.NamespacePolicy, isRequered: false);

            reader.ReadStartElement(XacmlConstants.ElementNames.Policy, this.Version.NamespacePolicy);

            string description = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy)) {
                description = reader.ReadElementContentAsString(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy);
            }

            XacmlPolicyIssuer issuer = this.ReadOptional<XacmlPolicyIssuer>(XacmlConstants.ElementNames.PolicyIssuer, this.Version.NamespacePolicy, this.ReadPolicyIssuer, reader);

            // PolicyDefault
            string xpathVersion = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.PolicyDefaults, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.PolicyDefaults, this.Version.NamespacePolicy);

                if (!reader.IsStartElement(XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy)) {
                    throw ThrowHelperXml(reader, "XPathVerison NotStartElement");
                }

                xpathVersion = reader.ReadElementContentAsString(XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy);

                reader.ReadEndElement();
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Target, this.Version.NamespacePolicy)) {
                throw new XmlException("Target IsNullOrEmpty");
            }

            XacmlTarget target = ReadTarget(reader);

            XacmlPolicy policy = new XacmlPolicy(new Uri(gaPolicyId, UriKind.RelativeOrAbsolute), new Uri(gaRuleCombiningAlgId, UriKind.RelativeOrAbsolute), target) {
                Description = description,
                XPathVersion = xpathVersion != null ? new Uri(xpathVersion) : null,
                PolicyIssuer = issuer,
                MaxDelegationDepth = maxDelegationDepth,
                Namespaces = nsMgr,
                Version = version,
            };

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

            if (reader.IsStartElement(XacmlConstants.ElementNames.ObligationExpressions, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.ObligationExpressions, this.Version.NamespacePolicy);
                this.ReadList(policy.ObligationExpressions, XacmlConstants.ElementNames.ObligationExpression, this.Version.NamespacePolicy, this.ReadObligationExpression, reader, isRequired: true);
                reader.ReadEndElement();
            }

            if (reader.IsStartElement(XacmlConstants.ElementNames.AdviceExpressions, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.AdviceExpressions, this.Version.NamespacePolicy);
                this.ReadList(policy.AdviceExpressions, XacmlConstants.ElementNames.AdviceExpression, this.Version.NamespacePolicy, this.ReadAdviceExpression, reader, isRequired: true);
                reader.ReadEndElement();
            }

            // end policy
            reader.ReadEndElement();

            return policy;
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
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            Uri gaPolicySetId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.PolicySetId);
            Uri gaPolicyCombiningAlgId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.PolicyCombiningAlgId);
            string version = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Version, isRequered: false);

            int? maxDelegationDepth = this.ReadAttribute<int?>(reader, XacmlConstants.AttributeNames.MaxDelegationDepth, this.Version.NamespacePolicy, isRequered: false);

            reader.ReadStartElement(XacmlConstants.ElementNames.PolicySet, this.Version.NamespacePolicy);

            string description = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy)) {
                description = reader.ReadElementContentAsString(XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy);
            }

            XacmlPolicyIssuer issuer = this.ReadOptional<XacmlPolicyIssuer>(XacmlConstants.ElementNames.PolicyIssuer, this.Version.NamespacePolicy, this.ReadPolicyIssuer, reader);

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
                MaxDelegationDepth = maxDelegationDepth,
                PolicyIssuer = issuer,
                Namespaces = nsMgr,
                Version = version,
            };

            IDictionary<Tuple<string, string>, Action> dicts = new Dictionary<Tuple<string, string>, Action>()
            {
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicySet, this.Version.NamespacePolicy), () => policySet.PolicySets.Add(this.ReadPolicySet(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.Policy, this.Version.NamespacePolicy), () => policySet.Policies.Add(this.ReadPolicy(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicySetIdReference, this.Version.NamespacePolicy), () => policySet.PolicySetIdReferences_3_0.Add(this.ReadPolicySetIdReference_3_0(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicyIdReference, this.Version.NamespacePolicy), () => policySet.PolicyIdReferences_3_0.Add(this.ReadPolicyIdReference_3_0(reader)) },

                { new Tuple<string, string>(XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy), () => {
                    reader.ReadStartElement(XacmlConstants.ElementNames.CombinerParameters, this.Version.NamespacePolicy);
                    this.ReadList(policySet.CombinerParameters, XacmlConstants.ElementNames.CombinerParameter, this.Version.NamespacePolicy, this.ReadCombinerParameter, reader, isRequired: false);
                    reader.ReadEndElement(); }
                },
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicyCombinerParameters, this.Version.NamespacePolicy), () => policySet.PolicyCombinerParameters.Add(this.ReadPolicyCombinerParameters(reader)) },
                { new Tuple<string, string>(XacmlConstants.ElementNames.PolicySetCombinerParameters, this.Version.NamespacePolicy), () => policySet.PolicySetCombinerParameters.Add(this.ReadPolicySetCombinerParameters(reader)) },
            };

            this.ReadChoiceMultiply(reader, dicts);

            if (reader.IsStartElement(XacmlConstants.ElementNames.ObligationExpressions, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.ObligationExpressions, this.Version.NamespacePolicy);
                this.ReadList(policySet.Obligations_3_0, XacmlConstants.ElementNames.ObligationExpression, this.Version.NamespacePolicy, this.ReadObligationExpression, reader, isRequired: true);
                reader.ReadEndElement();
            }

            if (reader.IsStartElement(XacmlConstants.ElementNames.AdviceExpressions, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.AdviceExpressions, this.Version.NamespacePolicy);
                this.ReadList(policySet.Advices, XacmlConstants.ElementNames.AdviceExpression, this.Version.NamespacePolicy, this.ReadAdviceExpression, reader, isRequired: true);
                reader.ReadEndElement();
            }

            return policySet;
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
                throw new XmlException("RuleId IsNullOrEmpty");
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

            XacmlRule result = new XacmlRule(gaRuleId, effectType) { Description = description, Target = target, Condition = condition };
            if (reader.IsStartElement(XacmlConstants.ElementNames.ObligationExpressions, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.ObligationExpressions, this.Version.NamespacePolicy);
                this.ReadList(result.Obligations, XacmlConstants.ElementNames.ObligationExpression, this.Version.NamespacePolicy, this.ReadObligationExpression, reader, isRequired: true);
                reader.ReadEndElement();
            }

            if (reader.IsStartElement(XacmlConstants.ElementNames.AdviceExpressions, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.AdviceExpressions, this.Version.NamespacePolicy);
                this.ReadList(result.Advices, XacmlConstants.ElementNames.AdviceExpression, this.Version.NamespacePolicy, this.ReadAdviceExpression, reader, isRequired: true);
                reader.ReadEndElement();
            }

            reader.ReadEndElement();
            return result;
        }

        /// <summary>
        /// Writes the policy issuer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        public virtual void WritePolicyIssuer(XmlWriter writer, XacmlPolicyIssuer data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicyIssuer, this.Version.NamespacePolicy);

            if (!string.IsNullOrEmpty(data.Content)) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Content, this.Version.NamespacePolicy, data.Content);
            }

            foreach (XacmlAttribute attr in data.Attributes) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Attribute, this.Version.NamespacePolicy);

                writer.WriteAttributeString(XacmlConstants.AttributeNames.AttributeId, attr.AttributeId.OriginalString);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.IncludeInResult, attr.IncludeInResult.ToString());

                if (!string.IsNullOrEmpty(attr.Issuer)) {
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.Issuer, attr.Issuer);
                }

                foreach (XacmlAttributeValue attrVal in attr.AttributeValues) {
                    this.WriteAttributeValue(writer, attrVal);
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

            if (data.MaxDelegationDepth.HasValue) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.MaxDelegationDepth, data.MaxDelegationDepth.Value.ToString());
            }

            // ?Description
            if (data.Description != null) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy, data.Description);
            }

            if (data.PolicyIssuer != null) {
                this.WritePolicyIssuer(writer, data.PolicyIssuer);
            }

            // PolicyDefaults
            if (data.XPathVersion != null) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicyDefaults, this.Version.NamespacePolicy);
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy, data.XPathVersion.ToString());
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

            if (data.ObligationExpressions.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.ObligationExpressions, this.Version.NamespacePolicy);

                foreach (XacmlObligationExpression ex in data.ObligationExpressions) {
                    this.WriteObligationExpression(writer, ex);
                }

                writer.WriteEndElement();
            }

            if (data.AdviceExpressions.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AdviceExpressions, this.Version.NamespacePolicy);

                foreach (XacmlAdviceExpression ex in data.AdviceExpressions) {
                    this.WriteAdviceExpression(writer, ex);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
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

            if (data.MaxDelegationDepth.HasValue) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.MaxDelegationDepth, data.MaxDelegationDepth.Value.ToString());
            }

            if (data.Description != null) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy, data.Description);
            }

            if (data.PolicyIssuer != null) {
                this.WritePolicyIssuer(writer, data.PolicyIssuer);
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
            foreach (var policySetIdReference in data.PolicySetIdReferences_3_0) {
                this.WritePolicySetIdReference(writer, policySetIdReference);
            }

            // PolicyIdReference
            foreach (var policyIdReference in data.PolicyIdReferences_3_0) {
                this.WritePolicyIdReference(writer, policyIdReference);
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

            if (data.Obligations_3_0.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.ObligationExpressions, this.Version.NamespacePolicy);

                foreach (XacmlObligationExpression ex in data.Obligations_3_0) {
                    this.WriteObligationExpression(writer, ex);
                }

                writer.WriteEndElement();
            }

            if (data.Advices.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AdviceExpressions, this.Version.NamespacePolicy);

                foreach (XacmlAdviceExpression ex in data.Advices) {
                    this.WriteAdviceExpression(writer, ex);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the obligation expression.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected virtual void WriteObligationExpression(XmlWriter writer, XacmlObligationExpression data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.ObligationExpression, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.ObligationId, data.ObligationId.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.FulfillOn, data.FulfillOn.ToString());

            foreach (XacmlAttributeAssignmentExpression expr in data.AttributeAssignmentExpressions) {
                this.WriteAttributeAssignmentExpression(writer, expr);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the advice expression.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected virtual void WriteAdviceExpression(XmlWriter writer, XacmlAdviceExpression data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AdviceExpression, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.AdviceId, data.AdviceId.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.AppliesTo, data.AppliesTo.ToString());

            foreach (XacmlAttributeAssignmentExpression expr in data.AttributeAssignmentExpressions) {
                this.WriteAttributeAssignmentExpression(writer, expr);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the attribute assignment expression.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected virtual void WriteAttributeAssignmentExpression(XmlWriter writer, XacmlAttributeAssignmentExpression data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeAssignmentExpression, this.Version.NamespacePolicy);

            writer.WriteAttributeString(XacmlConstants.AttributeNames.AttributeId, data.AttributeId.OriginalString);

            if (data.Category != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Category, data.Category.OriginalString);
            }

            if (!string.IsNullOrEmpty(data.Issuer)) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Issuer, data.Issuer);
            }

            this.WriteExpressionType(writer, data);

            writer.WriteEndElement();
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

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeDesignator, this.Version.NamespacePolicy);

            writer.WriteAttributeString(XacmlConstants.AttributeNames.Category, data.Category.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.AttributeId, data.AttributeId.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, data.DataType.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.MustBePresent, XmlConvert.ToString(data.MustBePresent.HasValue && data.MustBePresent.Value));

            if (!string.IsNullOrEmpty(data.Issuer)) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Issuer, data.Issuer);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Write expression type(not element)
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="data"></param>
        protected override void WriteExpressionType(XmlWriter writer, XacmlExpression data) {
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
                writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, XmlConvert.ToString(prop.MustBePresent.HasValue && prop.MustBePresent.Value));

                writer.WriteEndElement();
            }
            else if (applyElemType == typeof(XacmlAttributeDesignator)) {
                this.WriteAttributeDesignator(writer, data.Property as XacmlAttributeDesignator);
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
        /// Writes the attribute selector.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected override void WriteAttributeSelector(XmlWriter writer, XacmlAttributeSelector data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeSelector, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.Category, data.Category.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.Path, data.Path);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, data.DataType.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.MustBePresent, data.MustBePresent.ToString());

            if (data.ContextSelectorId != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.ContextSelectorId, data.ContextSelectorId.OriginalString);
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

            if (data.Obligations.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.ObligationExpressions, this.Version.NamespacePolicy);

                foreach (XacmlObligationExpression ex in data.Obligations) {
                    this.WriteObligationExpression(writer, ex);
                }

                writer.WriteEndElement();
            }

            if (data.Advices.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AdviceExpressions, this.Version.NamespacePolicy);

                foreach (XacmlAdviceExpression ex in data.Advices) {
                    this.WriteAdviceExpression(writer, ex);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the match.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        protected override void WriteMatch(XmlWriter writer, XacmlMatch data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.SubjectMatch, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.MatchId, data.MatchId.OriginalString);

            this.WriteAttributeValue(writer, data.AttributeValue);

            if (data.AttributeSelector == null && data.AttributeDesignator == null) {
                throw new InvalidOperationException("AttributeSelector and AttributeDesignator is null.");
            }

            if (data.AttributeDesignator != null) {
                this.WriteAttributeDesignator(writer, data.AttributeDesignator);
            }

            if (data.AttributeSelector != null) {
                this.WriteAttributeSelector(writer, data.AttributeSelector);
            }

            writer.WriteEndElement();
        }

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

            foreach (ICollection<ICollection<XacmlMatch>> anyOf in data.AnyOf) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AnyOf, this.Version.NamespacePolicy);

                foreach (ICollection<XacmlMatch> allOf in anyOf) {
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AllOf, this.Version.NamespacePolicy);

                    foreach (XacmlMatch match in allOf) {
                        this.WriteMatch(writer, match);
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            //// End Target
            writer.WriteEndElement();
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

            if (data.Description != null) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Description, this.Version.NamespacePolicy, data.Description);
            }

            foreach (IXacmlApply applyElem in data.Parameters) {
                this.WriteExpressionType(writer, new XacmlExpression() { Property = applyElem });
            }
        }

        /// <summary>
        /// Reads the context attributes.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlContextAttributes ReadContextAttributes(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Attributes, this.Version.NamespaceContext)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            Uri category = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.Category);
            string id = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Id, namespaceURI: XacmlConstants.Namespaces.XmlNamespace, isRequered: false);

            var result = new XacmlContextAttributes(category) { Id = id };

            if (reader.IsEmptyElement) {
                reader.Read();
                return result;
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Attributes, this.Version.NamespacePolicy);

            if (reader.IsStartElement(XacmlConstants.ElementNames.Content, this.Version.NamespacePolicy)) {
                result.Content = reader.ReadInnerXml();
            }

            this.ReadList<XacmlAttribute>(result.Attributes, XacmlConstants.ElementNames.Attribute, this.Version.NamespacePolicy, ReadAttribute, reader, false);

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

            bool returnPolicyIdList = this.ReadAttribute<bool>(reader, XacmlConstants.AttributeNames.ReturnPolicyIdList);

            //PROFILE - Multiple Decision Profile - #POL01 - #SPEC2760
            bool combinedDecision = this.ReadAttribute<bool>(reader, XacmlConstants.AttributeNames.CombinedDecision);
            if (combinedDecision) {
                throw new Abc.Xacml.Runtime.XacmlInvalidDataTypeException("Multiple Decision Profile not implemented");
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Request, this.Version.NamespaceContext);

            Uri pathVersion = null;
            if (reader.IsStartElement(XacmlConstants.ElementNames.RequestDefaults, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.RequestDefaults, this.Version.NamespacePolicy);
                if (!reader.IsStartElement(XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy)) {
                    throw new XmlException("XPathVerison NotStartElement");
                }

                pathVersion = new Uri(reader.ReadElementContentAsString(XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy), UriKind.RelativeOrAbsolute);
                reader.ReadEndElement();
            }

            List<XacmlContextAttributes> attributes = new List<XacmlContextAttributes>();
            this.ReadList<XacmlContextAttributes>(attributes, XacmlConstants.ElementNames.Attributes, this.Version.NamespaceContext, this.ReadContextAttributes, reader, isRequired: true);

            XacmlContextRequest result = new XacmlContextRequest(returnPolicyIdList, combinedDecision, attributes) {
                XPathVersion = pathVersion,
            };

            if (reader.IsStartElement(XacmlConstants.ElementNames.MultiRequests, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.MultiRequests, this.Version.NamespaceContext);

                this.ReadList<XacmlContextRequestReference>(result.RequestReferences, XacmlConstants.ElementNames.RequestReference, this.Version.NamespaceContext,
                    o => {
                        reader.ReadStartElement(XacmlConstants.ElementNames.RequestReference, this.Version.NamespaceContext);
                        ICollection<string> refer = new List<string>();
                        this.ReadList<string>(refer, XacmlConstants.ElementNames.AttributesReference, this.Version.NamespaceContext,
                            b => {
                                var referenceId = this.ReadAttribute<string>(b, XacmlConstants.AttributeNames.ReferenceId);
                                b.Read();
                                return referenceId;
                            },
                            o, isRequired: true);
                        reader.ReadEndElement();
                        return new XacmlContextRequestReference(refer);
                    }, reader, isRequired: true);

                reader.ReadEndElement();
            }

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

                this.ReadList<XacmlObligation>(result.Obligations, XacmlConstants.ElementNames.Obligation, this.Version.NamespacePolicy, ReadObligation, reader, isRequired: true);

                // end obligations
                reader.ReadEndElement();
            }

            if (reader.IsStartElement(XacmlConstants.ElementNames.AssociatedAdvice, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.AssociatedAdvice, this.Version.NamespacePolicy);

                this.ReadList<XacmlAdvice>(result.Advices, XacmlConstants.ElementNames.Advice, this.Version.NamespacePolicy, this.ReadAdvice, reader, isRequired: true);

                // end advice
                reader.ReadEndElement();
            }

            this.ReadList<XacmlContextAttributes>(result.Attributes, XacmlConstants.ElementNames.Attributes, this.Version.NamespaceContext, this.ReadContextAttributes, reader, isRequired: false);

            if (reader.IsStartElement(XacmlConstants.ElementNames.PolicyIdentifierList, this.Version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.PolicyIdentifierList, this.Version.NamespacePolicy);

                IDictionary<Tuple<string, string>, Action> dicts = new Dictionary<Tuple<string, string>, Action>()
                {
                    { new Tuple<string, string>(XacmlConstants.ElementNames.PolicyIdReference, this.Version.NamespacePolicy), () => result.PolicyIdReferences.Add(this.ReadPolicyIdReference_3_0(reader)) },
                    { new Tuple<string, string>(XacmlConstants.ElementNames.PolicySetIdReference, this.Version.NamespacePolicy), () => result.PolicySetIdReferences.Add(this.ReadPolicySetIdReference_3_0(reader)) },
                };

                this.ReadChoiceMultiply(reader, dicts);

                reader.ReadEndElement();
            }

            reader.ReadEndElement();

            return result;
        }

        /// <summary>
        /// Reads the policy identifier reference_3_0.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlContextPolicyIdReference ReadPolicyIdReference_3_0(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.PolicyIdReference, this.Version.NamespaceContext)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            string version = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Version, isRequered: false);
            string earliestVersion = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.EarliestVersion, isRequered: false);
            string latestVersion = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.LatestVersion, isRequered: false);

            XacmlContextPolicyIdReference result = new XacmlContextPolicyIdReference() {
                Version = string.IsNullOrEmpty(version) ? null : new XacmlVersionMatchType(version),
                EarliestVersion = string.IsNullOrEmpty(earliestVersion) ? null : new XacmlVersionMatchType(earliestVersion),
                LatestVersion = string.IsNullOrEmpty(latestVersion) ? null : new XacmlVersionMatchType(latestVersion),
            };

            result.Value = reader.ReadInnerXml();
            return result;
        }

        /// <summary>
        /// Reads the policy set identifier reference_3_0.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlContextPolicySetIdReference ReadPolicySetIdReference_3_0(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.PolicySetIdReference, this.Version.NamespaceContext)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            string version = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Version, isRequered: false);
            string earliestVersion = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.EarliestVersion, isRequered: false);
            string latestVersion = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.LatestVersion, isRequered: false);

            XacmlContextPolicySetIdReference result = new XacmlContextPolicySetIdReference() {
                Version = string.IsNullOrEmpty(version) ? null : new XacmlVersionMatchType(version),
                EarliestVersion = string.IsNullOrEmpty(earliestVersion) ? null : new XacmlVersionMatchType(earliestVersion),
                LatestVersion = string.IsNullOrEmpty(latestVersion) ? null : new XacmlVersionMatchType(latestVersion),
            };

            result.Value = reader.ReadInnerXml();
            return result;
        }

        /// <summary>
        /// Reads the obligation.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected override XacmlObligation ReadObligation(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Obligation, this.Version.NamespaceContext)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            Uri gaObligationId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.ObligationId);

            reader.ReadStartElement(XacmlConstants.ElementNames.Obligation, this.Version.NamespacePolicy);

            List<XacmlAttributeAssignment> assigments = new List<XacmlAttributeAssignment>();
            this.ReadList(assigments, XacmlConstants.ElementNames.AttributeAssignment, this.Version.NamespacePolicy, this.ReadAttributeAssigment, reader, isRequired: true);

            reader.ReadEndElement();

            return new XacmlObligation(gaObligationId, assigments);
        }

        /// <summary>
        /// Reads the advice.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected virtual XacmlAdvice ReadAdvice(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.Advice, this.Version.NamespaceContext)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            Uri gaAdviceId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.AdviceId);

            reader.ReadStartElement(XacmlConstants.ElementNames.Advice, this.Version.NamespacePolicy);

            List<XacmlAttributeAssignment> assigments = new List<XacmlAttributeAssignment>();
            this.ReadList(assigments, XacmlConstants.ElementNames.AttributeAssignment, this.Version.NamespacePolicy, this.ReadAttributeAssigment, reader, isRequired: true);

            reader.ReadEndElement();

            return new XacmlAdvice(gaAdviceId, assigments);
        }

        /// <summary>
        /// Reads the attribute assigment.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected override XacmlAttributeAssignment ReadAttributeAssigment(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.AttributeAssignment, this.Version.NamespaceContext)) {
                throw ThrowHelperXml(reader, "XML message is not valid.");
            }

            Uri gaDataType = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.DataType);
            Uri gaAttributeId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.AttributeId);

            Uri category = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.Category, isRequered: false);
            string issuer = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Issuer, isRequered: false);

            reader.ReadStartElement(XacmlConstants.ElementNames.AttributeAssignment, this.Version.NamespacePolicy);
            string content = reader.ReadContentAsString();
            reader.ReadEndElement();

            return new XacmlAttributeAssignment(gaAttributeId, gaDataType, content) {
                Category = category,
                Issuer = issuer,
            };
        }

        /// <summary>
        /// Writes the context attributes.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected virtual void WriteContextAttributes(XmlWriter writer, XacmlContextAttributes data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Attributes, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.Category, data.Category.OriginalString);

            if (data.Id != null) {
                writer.WriteAttributeString(XacmlConstants.Prefixes.Xml, XacmlConstants.AttributeNames.Id, XacmlConstants.Namespaces.XmlNamespace, data.Id.ToString());
            }

            if (!string.IsNullOrEmpty(data.Content)) {
                writer.WriteElementString(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Content, this.Version.NamespacePolicy, data.Content);
            }

            foreach (var attr in data.Attributes) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Attribute, this.Version.NamespacePolicy);

                writer.WriteAttributeString(XacmlConstants.AttributeNames.AttributeId, attr.AttributeId.OriginalString);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.IncludeInResult, attr.IncludeInResult.ToString());

                if (!string.IsNullOrEmpty(attr.Issuer)) {
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.Issuer, attr.Issuer);
                }

                foreach (XacmlAttributeValue attrVal in attr.AttributeValues) {
                    this.WriteAttributeValue(writer, attrVal);
                }

                writer.WriteEndElement();
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
            writer.WriteAttributeString(XacmlConstants.AttributeNames.ReturnPolicyIdList, data.ReturnPolicyIdList.ToString());
            writer.WriteAttributeString(XacmlConstants.AttributeNames.CombinedDecision, data.CombinedDecision.ToString());

            if (data.XPathVersion != null) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.RequestDefaults, this.Version.NamespacePolicy);
                writer.WriteElementString(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.XPathVersion, this.Version.NamespacePolicy, data.XPathVersion.OriginalString);
                writer.WriteEndElement();
            }

            foreach (XacmlContextAttributes attr in data.Attributes) {
                this.WriteContextAttributes(writer, attr);
            }

            if (data.RequestReferences.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.MultiRequests, this.Version.NamespaceContext);

                foreach (var referCol in data.RequestReferences) {
                    writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.RequestReference, this.Version.NamespaceContext);

                    foreach (string refer in referCol.AttributeReferences) {
                        writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.AttributesReference, this.Version.NamespaceContext);
                        writer.WriteAttributeString(XacmlConstants.AttributeNames.ReferenceId, refer);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the attribute assignment.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected override void WriteAttributeAssignment(XmlWriter writer, XacmlAttributeAssignment data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeAssignment, this.Version.NamespacePolicy);

            writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, data.DataType.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.AttributeId, data.AttributeId.OriginalString);

            if (data.Category != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Category, data.Category.OriginalString);
            }

            if (!string.IsNullOrEmpty(data.Issuer)) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Issuer, data.Issuer);
            }

            if (data.Value != null) {
                writer.WriteString(data.Value);
            }
            else {
                this.WriteOpenElement(writer, (XacmlOpenElement)data);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the obligation.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected override void WriteObligation(XmlWriter writer, XacmlObligation data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Obligation, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.ObligationId, data.ObligationId.OriginalString);

            foreach (XacmlAttributeAssignment attributeAssigment in data.AttributeAssignment) {
                this.WriteAttributeAssignment(writer, attributeAssigment);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the advice.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected virtual void WriteAdvice(XmlWriter writer, XacmlAdvice data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Advice, this.Version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.AdviceId, data.AdviceId.OriginalString);

            foreach (var attributeAssigment in data.AttributeAssignment) {
                this.WriteAttributeAssignment(writer, attributeAssigment);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the context result.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The result.</param>
        protected override void WriteContextResult(XmlWriter writer, XacmlContextResult data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Result, this.Version.NamespaceContext);

            this.WriteContextDecision(writer, data.Decision);

            if (data.Status != null) {
                this.WriteContextStatus(writer, data.Status);
            }

            if (data.Obligations.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Obligations, this.Version.NamespacePolicy);

                foreach (XacmlObligation val in data.Obligations) {
                    this.WriteObligation(writer, val);
                }

                writer.WriteEndElement();
            }

            if (data.Advices.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AssociatedAdvice, this.Version.NamespacePolicy);

                foreach (XacmlAdvice val in data.Advices) {
                    this.WriteAdvice(writer, val);
                }

                writer.WriteEndElement();
            }

            if (data.Attributes.Count > 0) {
                foreach (XacmlContextAttributes attr in data.Attributes) {
                    this.WriteContextAttributes(writer, attr);
                }
            }

            if (data.PolicyIdReferences.Count > 0 || data.PolicySetIdReferences.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicyIdentifierList, this.Version.NamespacePolicy);

                foreach (XacmlContextPolicyIdReference pref in data.PolicyIdReferences) {
                    this.WritePolicyIdReference(writer, pref);
                }

                foreach (XacmlContextPolicySetIdReference psref in data.PolicySetIdReferences) {
                    this.WritePolicySetIdReference(writer, psref);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the policy identifier reference.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected virtual void WritePolicyIdReference(XmlWriter writer, XacmlContextPolicyIdReference data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicyIdReference, this.Version.NamespacePolicy);

            if (data.Version != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Version, data.Version.ToString());
            }

            if (data.EarliestVersion != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.EarliestVersion, data.EarliestVersion.ToString());
            }

            if (data.LatestVersion != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.LatestVersion, data.LatestVersion.ToString());
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the policy set identifier reference.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected virtual void WritePolicySetIdReference(XmlWriter writer, XacmlContextPolicySetIdReference data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicySetIdReference, this.Version.NamespacePolicy);

            if (data.Version != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Version, data.Version.ToString());
            }

            if (data.EarliestVersion != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.EarliestVersion, data.EarliestVersion.ToString());
            }

            if (data.LatestVersion != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.LatestVersion, data.LatestVersion.ToString());
            }

            writer.WriteEndElement();
        }

        #region Not supported 
        /// <inheritdoc/>
        protected override XacmlSubject ReadSubject(XmlReader reader) {
            throw ThrowHelperXml(reader, "Not supported function in 3.0.");
        }

        /// <inheritdoc/>
        protected override XacmlAction ReadAction(XmlReader reader) {
            throw ThrowHelperXml(reader, "Not supported function in 3.0.");
        }

        /// <inheritdoc/>
        protected override XacmlResource ReadResource(XmlReader reader) {
            throw ThrowHelperXml(reader, "Not supported function in 3.0.");
        }

        /// <inheritdoc/>
        protected override XacmlEnvironment ReadEnvironment(XmlReader reader) {
            throw ThrowHelperXml(reader, "Not supported function in 3.0.");
        }

        /// <inheritdoc/>
        protected override Uri ReadPolicyIdReference(XmlReader reader) {
            throw ThrowHelperXml(reader, "Not supported function in 3.0. Use ReadPolicyIdReference_3_0 instead");
        }

        /// <inheritdoc/>
        protected override Uri ReadPolicySetIdReference(XmlReader reader) {
            throw ThrowHelperXml(reader, "Not supported function in 3.0. Use ReadPolicySetIdReference_3_0 instead");
        }

        /// <inheritdoc/>
        protected override XacmlContextAttribute ReadContextAttribute(XmlReader reader) {
            throw ThrowHelperXml(reader, "Not supported function in 3.0. Use ŖeadAttribute instead");
        }

        #endregion
    }
}
