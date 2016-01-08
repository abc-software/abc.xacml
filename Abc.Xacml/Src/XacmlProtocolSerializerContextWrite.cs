// ----------------------------------------------------------------------------
// <copyright file="XacmlProtocolSerializerContextWrite.cs" company="ABC Software Ltd">
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
    using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(data != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Request, this.version.NamespaceContext);

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
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(xacmlContextEnvironment != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Environment, this.version.NamespaceContext);

            foreach (var attr in xacmlContextEnvironment.Attributes) {
                this.WriteContextAttribute(writer, attr);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextAction(XmlWriter writer, XacmlContextAction xacmlContextAction) {
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(xacmlContextAction != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Action, this.version.NamespaceContext);

            foreach (var attr in xacmlContextAction.Attributes) {
                this.WriteContextAttribute(writer, attr);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextResource(XmlWriter writer, XacmlContextResource resource) {
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(resource != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Resource, this.version.NamespaceContext);

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
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(xacmlContextResourceContent != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.ResourceContent, this.version.NamespaceContext);

            // UNDONE AnyAttribute

            writer.WriteRaw(xacmlContextResourceContent.Value);

            writer.WriteEndElement();
        }

        protected virtual void WriteContextSubject(XmlWriter writer, XacmlContextSubject subject) {
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(subject != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Subject, this.version.NamespaceContext);

            if (subject.SubjectCategory != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.SubjectCategory, subject.SubjectCategory.ToString());
            }

            foreach (var attr in subject.Attributes) {
                this.WriteContextAttribute(writer, attr);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextAttribute(XmlWriter writer, XacmlContextAttribute attr) {
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(attr != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Attribute, this.version.NamespaceContext);

            writer.WriteAttributeString(XacmlConstants.AttributeNames.AttributeId, attr.AttributeId.ToString());
            writer.WriteAttributeString(XacmlConstants.AttributeNames.DataType, attr.DataType.ToString());

            if (!string.IsNullOrEmpty(attr.Issuer)) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.Issuer, attr.Issuer);
            }

            if (attr.IssueInstant != null) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.IssueInstant, attr.IssueInstant.ToString());
            }

            if (attr.AttributeValues.Count > 1) {
                throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlSerializationException("AttributeValues shoul be 1 in version 1.0"));
            }

            this.WriteContextAttributeValue(writer, attr.AttributeValues.First());

            writer.WriteEndElement();
        }

        protected virtual void WriteContextAttributeValue(XmlWriter writer, XacmlContextAttributeValue xacmlContextAttributeValue) {
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(xacmlContextAttributeValue != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.AttributeValue, this.version.NamespaceContext);

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
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(data != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Response, this.version.NamespaceContext);

            // Results

            if (data.Results.Count == 0) {
                throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlSerializationException("Results is empty"));
            }

            foreach (var result in data.Results) {
                this.WriteContextResult(writer, result);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextResult(XmlWriter writer, XacmlContextResult result) {
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(result != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Result, this.version.NamespaceContext);

            if (!string.IsNullOrEmpty(result.ResourceId)) {
                writer.WriteAttributeString(XacmlConstants.AttributeNames.ResourceId, result.ResourceId);
            }

            this.WriteContextDecision(writer, result.Decision);

            if (result.Status == null) {
                throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlSerializationException("status must be set for XACML 1.0/1.1"));
            }

            this.WriteContextStatus(writer, result.Status);

            if (result.Obligations.Count > 1) {
                throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlSerializationException("Obligations should be < 2 until version 2.0"));
            }

            if (result.Obligations.Count > 0) {
                this.WriteObligation(writer, result.Obligations.First());
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextStatus(XmlWriter writer, XacmlContextStatus xacmlContextStatus) {
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(xacmlContextStatus != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Status, this.version.NamespaceContext);

            this.WriteContextStatusCode(writer, xacmlContextStatus.StatusCode);

            if (!string.IsNullOrEmpty(xacmlContextStatus.StatusMessage)) {
                writer.WriteElementString(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.StatusMessage, this.version.NamespaceContext, xacmlContextStatus.StatusMessage);
            }

            if (xacmlContextStatus.StatusDetail.Count > 0) {
                writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.StatusDetail, this.version.NamespaceContext);

                foreach (XmlElement element in xacmlContextStatus.StatusDetail) {
                    element.WriteTo(writer);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextStatusCode(XmlWriter writer, XacmlContextStatusCode xacmlContextStatusCode) {
            Contract.Requires<ArgumentNullException>(writer != null);
            Contract.Requires<ArgumentNullException>(xacmlContextStatusCode != null);

            writer.WriteStartElement(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.StatusCode, this.version.NamespaceContext);

            writer.WriteAttributeString(XacmlConstants.AttributeNames.Value, xacmlContextStatusCode.Value.ToString());

            if (xacmlContextStatusCode.StatusCode != null) {
                this.WriteContextStatusCode(writer, xacmlContextStatusCode.StatusCode);
            }

            writer.WriteEndElement();
        }

        protected virtual void WriteContextDecision(XmlWriter writer, XacmlContextDecision xacmlContextDecision) {
            Contract.Requires<ArgumentNullException>(writer != null);

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
                    throw Diagnostic.DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlSerializationException("Wrong Decision value"));
            }

            writer.WriteElementString(XacmlConstants.Prefixes.Context, XacmlConstants.ElementNames.Decision, this.version.NamespaceContext, value);
        }

        #endregion Response
    }
}