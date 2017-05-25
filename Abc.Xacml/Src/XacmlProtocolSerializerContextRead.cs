// ----------------------------------------------------------------------------
// <copyright file="XacmlProtocolSerializerContextRead.cs" company="ABC Software Ltd">
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
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Xml;
    using Abc.Xacml.Context;
    using Abc.Xacml.Policy;
#if NET40
    using Diagnostic;
#else
    using Abc.Diagnostics;
#endif

    public partial class XacmlProtocolSerializer {
        /// <summary>
        /// Determines whether this instance can read the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="localName">Name of the local.</param>
        /// <returns>
        ///   <c>true</c> if this instance can read the specified reader; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanReadContext(XmlReader reader, string localName, string version) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");

            return reader.IsStartElement(localName, version);
        }

        #region Request

        public virtual XacmlContextRequest ReadContextRequest(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");

            if (!XacmlProtocolSerializer.CanReadContext(reader, XacmlConstants.ElementNames.Request, this.version.NamespaceContext)) {
                throw DiagnosticTools.ExceptionUtil.ThrowHelperError(new InvalidOperationException());
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Request, this.version.NamespaceContext);

            List<XacmlContextSubject> subjects = new List<XacmlContextSubject>();
            this.ReadList(subjects, XacmlConstants.ElementNames.Subject, this.version.NamespaceContext, ReadContextSubject, reader, isRequired: true);

            XacmlContextRequest result = new XacmlContextRequest(
                this.ReadRequired(XacmlConstants.ElementNames.Resource, this.version.NamespaceContext, this.ReadContextResource, reader),
                this.ReadRequired(XacmlConstants.ElementNames.Action, this.version.NamespaceContext, this.ReadContextAction, reader),
                subjects
                );

            result.Environment = this.ReadOptional(XacmlConstants.ElementNames.Environment, this.version.NamespaceContext, this.ReadContextEnvironment, reader);

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextSubject ReadContextSubject(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.Subject, this.version.NamespaceContext));

            XacmlContextSubject result = new XacmlContextSubject();
            result.SubjectCategory = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.SubjectCategory, isRequered: false);

            // Read attributes

            if (reader.IsEmptyElement) {
                reader.Read();
                return result;
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Subject, this.version.NamespaceContext);
            // Read elements

            this.ReadList(result.Attributes, XacmlConstants.ElementNames.Attribute, this.version.NamespaceContext, this.ReadContextAttribute, reader);

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextAttribute ReadContextAttribute(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.Attribute, this.version.NamespaceContext));

            // Read attributes
            Uri attributeId = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.AttributeId, isRequered: true);
            Uri dataType = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.DataType, isRequered: true);
            string issuer = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.Issuer, isRequered: false);
            DateTime? issueInstant = null;
            string issueInstantString = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.IssueInstant, isRequered: false);
            if (!string.IsNullOrWhiteSpace(issueInstantString)) {
                issueInstant = XmlConvert.ToDateTime(issueInstantString, XmlDateTimeSerializationMode.Local);
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Attribute, this.version.NamespaceContext);
            // Read elements

            XacmlContextAttributeValue attrVal = this.ReadRequired(XacmlConstants.ElementNames.AttributeValue, this.version.NamespaceContext, this.ReadContextAttributeValue, reader);

            XacmlContextAttribute result = new XacmlContextAttribute(attributeId, dataType, attrVal) {
                Issuer = issuer,
                IssueInstant = issueInstant
            };

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextAttributeValue ReadContextAttributeValue(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.AttributeValue, this.version.NamespaceContext));

            XacmlContextAttributeValue result = new XacmlContextAttributeValue();
            if (reader.IsEmptyElement) {
                reader.Read();
                return result;
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.AttributeValue, this.version.NamespaceContext);

            // Read elements
            result.Value = reader.ReadContentAsString(); // JG:
            //result.Value = reader.ReadInnerXml(); // TODO: 

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextAction ReadContextAction(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.Action, this.version.NamespaceContext));

            XacmlContextAction result = new XacmlContextAction();
            // Read attributes

            if (reader.IsEmptyElement) {
                reader.Read();
                return result;
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Action, this.version.NamespaceContext);
            // Read elements

            this.ReadList(result.Attributes, XacmlConstants.ElementNames.Attribute, this.version.NamespaceContext, this.ReadContextAttribute, reader, isRequired: false);

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextEnvironment ReadContextEnvironment(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.Environment, this.version.NamespaceContext));

            XacmlContextEnvironment result = new XacmlContextEnvironment();
            // Read attributes

            if (reader.IsEmptyElement) {
                reader.Read();
                return result;
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Environment, this.version.NamespaceContext);
            // Read elements

            this.ReadList(result.Attributes, XacmlConstants.ElementNames.Attribute, this.version.NamespaceContext, this.ReadContextAttribute, reader, isRequired: false);

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextResource ReadContextResource(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.Resource, this.version.NamespaceContext));

            XacmlContextResource result = new XacmlContextResource();
            // Read attributes

            if (reader.IsEmptyElement) {
                reader.Read();
                return result;
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Resource, this.version.NamespaceContext);
            // Read elements

            result.ResourceContent = this.ReadOptional(XacmlConstants.ElementNames.ResourceContent, this.version.NamespaceContext, this.ReadContextResourceContent, reader);

            this.ReadList(result.Attributes, XacmlConstants.ElementNames.Attribute, this.version.NamespaceContext, this.ReadContextAttribute, reader, isRequired: false);

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextResourceContent ReadContextResourceContent(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.ResourceContent, this.version.NamespaceContext));

            // UNDONE: Read attributes

            XacmlContextResourceContent result = new XacmlContextResourceContent();
            if (reader.IsEmptyElement) {
                reader.Read();
                return result;
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.ResourceContent, this.version.NamespaceContext);
            // Read elements
            result.Value = reader.ReadOuterXml();

            reader.ReadEndElement();

            return result;
        }

        #endregion Request

        #region Response

        public virtual XacmlContextResponse ReadContextResponse(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");

            if (!XacmlProtocolSerializer.CanReadContext(reader, XacmlConstants.ElementNames.Response, this.version.NamespaceContext)) {
                throw DiagnosticTools.ExceptionUtil.ThrowHelperError(new InvalidOperationException());
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.Response, this.version.NamespaceContext);

            List<XacmlContextResult> results = new List<XacmlContextResult>();
            this.ReadList(results, XacmlConstants.ElementNames.Result, this.version.NamespaceContext, ReadContextResult, reader, isRequired: true);

            XacmlContextResponse result = new XacmlContextResponse(results);

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextResult ReadContextResult(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.Result, this.version.NamespaceContext));

            // Read attributes
            string resourceId = this.ReadAttribute<string>(reader, XacmlConstants.AttributeNames.ResourceId, isRequered: false);

            reader.ReadStartElement(XacmlConstants.ElementNames.Result, this.version.NamespaceContext);
            // Read elements

            XacmlContextResult result = new XacmlContextResult(
                this.ReadRequired(XacmlConstants.ElementNames.Decision, this.version.NamespaceContext, ReadContextDecision, reader),
                this.ReadRequired(XacmlConstants.ElementNames.Status, this.version.NamespaceContext, ReadContextStatus, reader)
                ) {
                    ResourceId = resourceId,
                };

            if (reader.IsStartElement(XacmlConstants.ElementNames.Obligations, this.version.NamespacePolicy)) {
                reader.ReadStartElement(XacmlConstants.ElementNames.Obligations, this.version.NamespacePolicy);

                this.ReadList<XacmlObligation>(result.Obligations, XacmlConstants.ElementNames.Obligation, this.version.NamespacePolicy, ReadObligation, reader, isRequired: false);

                // end obligations
                reader.ReadEndElement();
            }

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextStatus ReadContextStatus(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.Status, this.version.NamespaceContext));

            reader.ReadStartElement(XacmlConstants.ElementNames.Status, this.version.NamespaceContext);

            // Read elements
            XacmlContextStatus result = new XacmlContextStatus(this.ReadRequired(XacmlConstants.ElementNames.StatusCode, this.version.NamespaceContext, ReadContextStatusCode, reader));

            result.StatusMessage = this.ReadOptional(XacmlConstants.ElementNames.StatusMessage, this.version.NamespaceContext, ReadContextStatusMessage, reader);

            if (reader.IsStartElement(XacmlConstants.ElementNames.StatusDetail, this.version.NamespaceContext)) {
                bool isEmptyElement = reader.IsEmptyElement;

                // XmlUtil.ValidateXsiType(reader, XacmlConstants.XmlTypes.StatusDetailType, this.version.NamespaceContext);

                if (isEmptyElement) {
                    reader.Read();
                }
                else {
                    XmlDocument document = new XmlDocument();
                    document.PreserveWhitespace = true;
                    document.Load(reader.ReadSubtree());
                    foreach (XmlElement element in document.DocumentElement.ChildNodes) {
                        result.StatusDetail.Add(element);
                    }

                    reader.ReadEndElement();
                }
            }

            reader.ReadEndElement();

            return result;
        }

        protected virtual string ReadContextStatusMessage(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.StatusMessage, this.version.NamespaceContext));

            reader.ReadStartElement(XacmlConstants.ElementNames.StatusMessage, this.version.NamespaceContext);
            // Read elements

            if (reader.IsEmptyElement) {
                reader.Read();
                return string.Empty;
            }

            string result = reader.ReadContentAsString();

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextStatusCode ReadContextStatusCode(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.StatusCode, this.version.NamespaceContext));

            // Read attributes
            Uri statusCode = this.ReadAttribute<Uri>(reader, XacmlConstants.AttributeNames.Value, isRequered: true);

            XacmlContextStatusCode result = new XacmlContextStatusCode(statusCode);
            if (reader.IsEmptyElement) {
                reader.Read();
                return result;
            }

            reader.ReadStartElement(XacmlConstants.ElementNames.StatusCode, this.version.NamespaceContext);
            // Read elements
            result.StatusCode = this.ReadOptional(XacmlConstants.ElementNames.StatusCode, this.version.NamespaceContext, ReadContextStatusCode, reader);

            reader.ReadEndElement();

            return result;
        }

        protected virtual XacmlContextDecision ReadContextDecision(XmlReader reader) {
            Contract.Requires<ArgumentNullException>(reader != null, "reader");
            Contract.Requires<XmlException>(reader.IsStartElement(XacmlConstants.ElementNames.Decision, this.version.NamespaceContext));

            reader.ReadStartElement(XacmlConstants.ElementNames.Decision, this.version.NamespaceContext);
            // Read elements

            string decisionText = reader.ReadContentAsString();
            XacmlContextDecision result;

            if (string.Equals(decisionText, "Deny", StringComparison.OrdinalIgnoreCase)) {
                result = XacmlContextDecision.Deny;
            }
            else if (string.Equals(decisionText, "Permit", StringComparison.OrdinalIgnoreCase)) {
                result = XacmlContextDecision.Permit;
            }
            else if (string.Equals(decisionText, "Indeterminate", StringComparison.OrdinalIgnoreCase)) {
                result = XacmlContextDecision.Indeterminate;
            }
            else if (string.Equals(decisionText, "NotApplicable", StringComparison.OrdinalIgnoreCase)) {
                result = XacmlContextDecision.NotApplicable;
            }
            else {
                throw DiagnosticTools.ExceptionUtil.ThrowHelperError(new XacmlSerializationException("Wrong XacmlContextDecision value"));
            }

            reader.ReadEndElement();

            return result;
        }

        #endregion Response
    }
}