// ----------------------------------------------------------------------------
// <copyright file="XacmlProtocolSerializerContextWrite.cs" company="ABC Software Ltd">
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
    using System.Linq;
    using System.Xml;
    using Abc.Xacml.Context;

    public partial class XacmlProtocolSerializer {
        #region Request

        /// <summary>
        /// public void WriteRequest
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlContextRequest data</param>
        public virtual void WriteContextRequest(XmlWriter writer, XacmlContextRequest data) {
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

            this.WriteContextResource(writer, data.Resources.First());

            this.WriteContextAction(writer, data.Action);

            if (data.Environment != null) {
                this.WriteContextEnvironment(writer, data.Environment);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextEnvironment(XmlWriter writer, XacmlContextEnvironment xacmlContextEnvironment) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (xacmlContextEnvironment == null) {
                throw new ArgumentNullException(nameof(xacmlContextEnvironment));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Environment, this.Version.NamespaceContext);

            foreach (var attr in xacmlContextEnvironment.Attributes) {
                this.WriteContextAttribute(writer, attr);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextAction(XmlWriter writer, XacmlContextAction xacmlContextAction) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (xacmlContextAction == null) {
                throw new ArgumentNullException(nameof(xacmlContextAction));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Action, this.Version.NamespaceContext);

            foreach (var attr in xacmlContextAction.Attributes) {
                this.WriteContextAttribute(writer, attr);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextResource(XmlWriter writer, XacmlContextResource resource) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (resource == null) {
                throw new ArgumentNullException(nameof(resource));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Resource, this.Version.NamespaceContext);

            // ResourceContent
            if (resource.ResourceContent != null) {
                this.WriteContextResourceContent(writer, resource.ResourceContent);
            }

            foreach (var attr in resource.Attributes) {
                this.WriteContextAttribute(writer, attr);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextResourceContent(XmlWriter writer, XacmlContextResourceContent xacmlContextResourceContent) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (xacmlContextResourceContent == null) {
                throw new ArgumentNullException(nameof(xacmlContextResourceContent));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.ResourceContent, this.Version.NamespaceContext);

            // UNDONE AnyAttribute

            writer.WriteRaw(xacmlContextResourceContent.Value);

            writer.WriteEndElement();
        }

        protected virtual void WriteContextSubject(XmlWriter writer, XacmlContextSubject subject) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (subject == null) {
                throw new ArgumentNullException(nameof(subject));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Subject, this.Version.NamespaceContext);

            if (subject.SubjectCategory != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.SubjectCategory, subject.SubjectCategory.ToString());
            }

            foreach (var attr in subject.Attributes) {
                this.WriteContextAttribute(writer, attr);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextAttribute(XmlWriter writer, XacmlContextAttribute attr) {
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
                writer.WriteAttributeString(XacmlConstants.AttributeNames.IssueInstant, attr.IssueInstant.ToString());
            }

            if (attr.AttributeValues.Count > 1) {
                throw new InvalidOperationException("AttributeValues shoul be 1 in version 1.0");
            }

            this.WriteContextAttributeValue(writer, attr.AttributeValues.First());

            writer.WriteEndElement();
        }

        protected virtual void WriteContextAttributeValue(XmlWriter writer, XacmlContextAttributeValue xacmlContextAttributeValue) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (xacmlContextAttributeValue == null) {
                throw new ArgumentNullException(nameof(xacmlContextAttributeValue));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.AttributeValue, this.Version.NamespaceContext);

            // UNDONE AnyAttribute

            writer.WriteRaw(xacmlContextAttributeValue.Value);

            writer.WriteEndElement();
        }

        #endregion Request

        #region Response

        /// <summary>
        /// public void WriteResponse
        /// </summary>
        /// <param name="writer">XmlWriter writer</param>
        /// <param name="data">XacmlContextResponse data</param>
        public virtual void WriteContextResponse(XmlWriter writer, XacmlContextResponse data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Response, this.Version.NamespaceContext);

            // Results
            if (data.Results.Count == 0) {
                throw new InvalidOperationException("Results is empty");
            }

            foreach (var result in data.Results) {
                this.WriteContextResult(writer, result);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextResult(XmlWriter writer, XacmlContextResult data) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Result, this.Version.NamespaceContext);

            if (!string.IsNullOrEmpty(data.ResourceId)) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.ResourceId, data.ResourceId);
            }

            this.WriteContextDecision(writer, data.Decision);

            if (data.Status == null) {
                throw new InvalidOperationException("status must be set for XACML 1.0/1.1");
            }

            this.WriteContextStatus(writer, data.Status);

            if (data.Obligations.Count > 1) {
                throw new InvalidOperationException("Obligations should be < 2 until version 2.0");
            }

            if (data.Obligations.Count > 0) {
                this.WriteObligation(writer, data.Obligations.First());
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextStatus(XmlWriter writer, XacmlContextStatus xacmlContextStatus) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (xacmlContextStatus == null) {
                throw new ArgumentNullException(nameof(xacmlContextStatus));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Status, this.Version.NamespaceContext);

            this.WriteContextStatusCode(writer, xacmlContextStatus.StatusCode);

            if (!string.IsNullOrEmpty(xacmlContextStatus.StatusMessage)) {
                writer.WriteElementString(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.StatusMessage, this.Version.NamespaceContext, xacmlContextStatus.StatusMessage);
            }

            if (xacmlContextStatus.StatusDetail.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.StatusDetail, this.Version.NamespaceContext);

                foreach (XmlElement element in xacmlContextStatus.StatusDetail) {
                    element.WriteTo(writer);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextStatusCode(XmlWriter writer, XacmlContextStatusCode xacmlContextStatusCode) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            if (xacmlContextStatusCode == null) {
                throw new ArgumentNullException(nameof(xacmlContextStatusCode));
            }

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.StatusCode, this.Version.NamespaceContext);

            writer.WriteAttributeString(XacmlConstants.AttributeNames.Value, xacmlContextStatusCode.Value.ToString());

            if (xacmlContextStatusCode.StatusCode != null) {
                this.WriteContextStatusCode(writer, xacmlContextStatusCode.StatusCode);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextDecision(XmlWriter writer, XacmlContextDecision xacmlContextDecision) {
            if (writer == null) {
                throw new ArgumentNullException(nameof(writer));
            }

            string value;
            switch (xacmlContextDecision) {
                case XacmlContextDecision.Deny:
                    value = "Deny";
                    break;

                case XacmlContextDecision.Indeterminate:
                    value = "Indeterminate";
                    break;

                case XacmlContextDecision.NotApplicable:
                    value = "NotApplicable";
                    break;

                case XacmlContextDecision.Permit:
                    value = "Permit";
                    break;

                default:
                    throw new InvalidOperationException("Wrong Decision value");
            }

            writer.WriteElementString(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Decision, this.Version.NamespaceContext, value);
        }

        #endregion Response
    }
}