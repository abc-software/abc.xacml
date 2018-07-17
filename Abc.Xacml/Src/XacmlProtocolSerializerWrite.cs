// ----------------------------------------------------------------------------
// <copyright file="XacmlProtocolSerializerWrite.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml {
    using System;
    using System.Xml;
    using System.Xml.Linq;
    using Abc.Xacml.Policy;

    /// <summary>
    /// class XacmlSerializer
    /// </summary>
    public partial class XacmlProtocolSerializer {
        /// <summary>
        /// public void WritePolicySet
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlPolicySet data</param>
        public virtual void WritePolicySet(XmlWriter writer, XacmlPolicySet data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicySet, this.version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.PolicySetId, data.PolicySetId.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.PolicyCombiningAlgId, data.PolicyCombiningAlgId.OriginalString);

            if (data.Description != null) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Description, this.version.NamespacePolicy, data.Description);
            }

            // PolicySetDefaults
            if (data.XPathVersion != null) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicySetDefaults, this.version.NamespacePolicy);
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.XPathVersion, this.version.NamespacePolicy, data.XPathVersion.ToString());
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
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicySetIdReference, this.version.NamespacePolicy, policySetIdReference.ToString());
            }

            // PolicyIdReference
            foreach (var policyIdReference in data.PolicyIdReferences) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicyIdReference, this.version.NamespacePolicy, policyIdReference.ToString());
            }

            // Obligations
            if (data.Obligations.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Obligations, this.version.NamespacePolicy);
                foreach (var obligation in data.Obligations) {
                    this.WriteObligation(writer, obligation);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// public void WritePolicy
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlPolicy data</param>
        public virtual void WritePolicy(XmlWriter writer, XacmlPolicy data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Policy, this.version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.PolicyId, data.PolicyId.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.RuleCombiningAlgId, data.RuleCombiningAlgId.OriginalString);

            // ?Description
            if (data.Description != null) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Description, this.version.NamespacePolicy, data.Description);
            }

            // PolicyDefaults
            if (data.XPathVersion != null) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.PolicyDefaults, this.version.NamespacePolicy);
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.XPathVersion, this.version.NamespacePolicy, data.XPathVersion.ToString());
                writer.WriteEndElement();
            }

            // Target
            this.WriteTarget(writer, data.Target);

            // Rule
            foreach (var rule in data.Rules) {
                this.WriteRule(writer, rule);
            }

            // Obligatoins
            if (data.Obligations.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Obligations, this.version.NamespacePolicy);
                foreach (var obligation in data.Obligations) {
                    this.WriteObligation(writer, obligation);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// protected virtual void WriteRule
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlRule data</param>
        protected virtual void WriteRule(XmlWriter writer, XacmlRule data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Rule, this.version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.RuleId, data.RuleId);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.Effect, data.Effect.ToString());

            if (data.Description != null) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Description, this.version.NamespacePolicy, data.Description);
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
        /// Writes the condition.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="apply">The apply.</param>
        protected virtual void WriteCondition(XmlWriter writer, XacmlExpression data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Condition, this.version.NamespacePolicy);
            this.WriteApplyType(writer, data.Property as XacmlApply); // TODO:
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the apply.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="apply">The apply.</param>
        protected virtual void WriteApply(XmlWriter writer, XacmlApply apply) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (apply == null) {
                throw new ArgumentNullException(nameof(apply));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Apply, this.version.NamespacePolicy);
            this.WriteApplyType(writer, apply);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Write ApplyType (Not element!)
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="apply"></param>
        private void WriteApplyType(XmlWriter writer, XacmlApply apply) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (apply == null) {
                throw new ArgumentNullException(nameof(apply));
            }

            writer.WriteAttributeString(XacmlConstants.AttributeNames.FunctionId, apply.FunctionId.OriginalString);

            foreach (IXacmlApply applyElem in apply.Parameters) {
                Type applyElemType = applyElem.GetType();
                if (applyElemType == typeof(XacmlAttributeSelector)) {
                    XacmlAttributeSelector elem = applyElem as XacmlAttributeSelector;
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeSelector, this.version.NamespacePolicy);
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
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Function, this.version.NamespacePolicy);
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.FunctionId, (applyElem as XacmlFunction).FunctionId.OriginalString);
                    writer.WriteEndElement();
                }
                else if (applyElemType == typeof(XacmlApply)) {
                    this.WriteApply(writer, applyElem as XacmlApply);
                }
            }
        }

        /// <summary>
        /// protected virtual void WriteTarget
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlTarget data</param>
        protected virtual void WriteTarget(XmlWriter writer, XacmlTarget data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            //// Start Target
            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Target, this.version.NamespacePolicy);
            //// Start Subjects
            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Subjects, this.version.NamespacePolicy);
            if (data.Subjects.Count == 0) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AnySubject, this.version.NamespacePolicy, string.Empty);
            }
            else {
                foreach (var subject in data.Subjects) {
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Subject, this.version.NamespacePolicy);

                    foreach (var subjectMatch in subject.Matches) {
                        this.WriteMatch(writer, subjectMatch);
                    }

                    writer.WriteEndElement();
                }
            }

            //// End Subjects
            writer.WriteEndElement();

            //// Start Resources
            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Resources, this.version.NamespacePolicy);
            if (data.Resources.Count == 0) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AnyResource, this.version.NamespacePolicy, string.Empty);
            }
            else {
                foreach (var resource in data.Resources) {
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Resource, this.version.NamespacePolicy);

                    foreach (var resourceMatch in resource.Matches) {
                        this.WriteMatch(writer, resourceMatch);
                    }

                    writer.WriteEndElement();
                }
            }

            //// End Resources
            writer.WriteEndElement();

            //// Start Actions
            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Actions, this.version.NamespacePolicy);
            if (data.Actions.Count == 0) {
                writer.WriteElementString(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AnyAction, this.version.NamespacePolicy, string.Empty);
            }
            else {
                foreach (var action in data.Actions) {
                    writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Action, this.version.NamespacePolicy);

                    foreach (var actionMatch in action.Matches) {
                        this.WriteMatch(writer, actionMatch);
                    }

                    writer.WriteEndElement();
                }
            }

            //// End Actions
            writer.WriteEndElement();

            if (data.Environments != null && data.Environments.Count > 0) {
                throw new InvalidOperationException("Environments property just for version 2.0 or higher");
            }

            //// End Target
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the match.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected virtual void WriteMatch(XmlWriter writer, XacmlMatch data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            Action<string, dynamic> action = (matchType, match) => {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, matchType, this.version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.MatchId, match.MatchId.OriginalString);

                this.WriteAttributeValue(writer, match.AttributeValue);

                if (match.AttributeSelector == null && match.AttributeDesignator == null) {
                    throw new InvalidOperationException("AttributeSelector and AttributeDesignator is null.");
                }

                if (match.AttributeDesignator != null) {
                    this.WriteAttributeDesignator(writer, match.AttributeDesignator);
                }

                if (match.AttributeSelector != null) {
                    this.WriteAttributeSelector(writer, match.AttributeSelector);
                }

                writer.WriteEndElement();
            };

            var actionMatch = data as XacmlActionMatch;
            if (actionMatch != null) {
                action(XacmlConstants.ElementNames.ActionMatch, actionMatch);
            }

            var resourceMatch = data as XacmlResourceMatch;
            if (resourceMatch != null) {
                action(XacmlConstants.ElementNames.ResourceMatch, resourceMatch);
            }

            var subjectMatch = data as XacmlSubjectMatch;
            if (subjectMatch != null) {
                action(XacmlConstants.ElementNames.SubjectMatch, subjectMatch);
            }
        }

        /// <summary>
        /// protected virtual void WriteAttributeValue
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlAttributeValue data</param>
        protected virtual void WriteAttributeValue(XmlWriter writer, XacmlAttributeValue data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeValue, this.version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, data.DataType.OriginalString);

            if (data.Value != null) {
                writer.WriteString(data.Value);
            }
            else {
                this.WriteOpenElement(writer, (XacmlOpenElement)data);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the open element.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected virtual void WriteOpenElement(XmlWriter writer, XacmlOpenElement data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            foreach (XAttribute attr in data.Attributes) {
                //writer.WriteAttributeString(attr.Prefix, attr.LocalName, attr.NamespaceURI, attr.Value);

                // XAttribute
                writer.WriteAttributeString(attr.Name.LocalName, attr.Name.NamespaceName, attr.Value);
            }

            foreach (var elem in data.Elements) {
                elem.WriteTo(writer);
            }
        }

        /// <summary>
        /// Writes the attribute designator.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The data.</param>
        protected virtual void WriteAttributeDesignator(XmlWriter writer, XacmlAttributeDesignator data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            Action<string, dynamic, bool> action = (designatorType, attributeDesignator, writeSubjectCategory) => {
                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, designatorType, this.version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.AttributeId, attributeDesignator.AttributeId.OriginalString);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, attributeDesignator.DataType.OriginalString);

                if (attributeDesignator.Issuer != null) {
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.Issuer, attributeDesignator.Issuer);
                }

                if (attributeDesignator.MustBePresent != null) {
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.MustBePresent, XmlConvert.ToString(attributeDesignator.MustBePresent));
                }

                if (writeSubjectCategory && attributeDesignator.Category != null) {
                    writer.WriteAttributeString(XacmlConstants.AttributeNames.SubjectCategory, attributeDesignator.Category.OriginalString);
                }

                writer.WriteEndElement();
            };

            var subjectAttributeDesignator = data as XacmlSubjectAttributeDesignator;
            if (subjectAttributeDesignator != null) {
                action(XacmlConstants.ElementNames.SubjectAttributeDesignator, subjectAttributeDesignator, true);
            }

            var resourceAttributeDesignator = data as XacmlResourceAttributeDesignator;
            if (resourceAttributeDesignator != null) {
                action(XacmlConstants.ElementNames.ResourceAttributeDesignator, resourceAttributeDesignator, false);
            }

            var actionAttributeDesignator = data as XacmlActionAttributeDesignator;
            if (actionAttributeDesignator != null) {
                action(XacmlConstants.ElementNames.ActionAttributeDesignator, actionAttributeDesignator, false);
            }
        }

        /// <summary>
        ///  protected virtual void WriteAttributeSelector
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlAttributeSelector data</param>
        protected virtual void WriteAttributeSelector(XmlWriter writer, XacmlAttributeSelector data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeSelector, this.version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.RequestContextPath, data.Path.ToString());
            writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, data.DataType.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.MustBePresent, data.MustBePresent.ToString());
            writer.WriteEndElement();
        }

        /// <summary>
        /// protected virtual void WriteResource
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlResource data</param>
        protected virtual void WriteResource(XmlWriter writer, XacmlResource data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Resource, this.version.NamespacePolicy);

            foreach (var resourceMatch in data.Matches) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.MatchId, resourceMatch.MatchId.OriginalString);

                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeValue, this.version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, resourceMatch.AttributeValue.DataType.OriginalString);
                //// writer.WriteString("Text");
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// protected virtual void WriteAction
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlAction data</param>
        protected virtual void WriteAction(XmlWriter writer, XacmlAction data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Action, this.version.NamespacePolicy);

            foreach (var actionMatch in data.Matches) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.MatchId, actionMatch.MatchId.OriginalString);

                writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeValue, this.version.NamespacePolicy);
                writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, actionMatch.AttributeValue.DataType.OriginalString);
                //// writer.WriteString("Text");
                writer.WriteEndElement();
            }
        }

        protected virtual void WriteAttributeAssignment(XmlWriter writer, XacmlAttributeAssignment data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.AttributeAssignment, this.version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, data.DataType.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.AttributeId, data.AttributeId.OriginalString);

            if (data.Value != null) {
                writer.WriteString(data.Value);
            }
            else {
                this.WriteOpenElement(writer, (XacmlOpenElement)data);
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// protected virtual void WriteObligation
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlObligation data</param>
        protected virtual void WriteObligation(XmlWriter writer, XacmlObligation data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Policy, XacmlConstants.ElementNames.Obligation, this.version.NamespacePolicy);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.ObligationId, data.ObligationId.OriginalString);
            writer.WriteAttributeString(XacmlConstants.AttributeNames.FulfillOn, data.FulfillOn.ToString());

            foreach (XacmlAttributeAssignment attributeAssigment in data.AttributeAssignment) {
                this.WriteAttributeAssignment(writer, attributeAssigment);
            }

            writer.WriteEndElement();
        }
    }
}
