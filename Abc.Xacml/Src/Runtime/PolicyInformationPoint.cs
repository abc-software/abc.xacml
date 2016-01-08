// ----------------------------------------------------------------------------
// <copyright file="PolicyInformationPoint.cs" company="ABC Software Ltd">
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
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml;
    using Abc.Xacml.Context;
    using Abc.Xacml.Policy;

    public class PolicyInformationPoint {
        XacmlContextRequest request;
        AttributesProcessor attributesProcessor;
        XPathProcessor xpathProcessor;
        XmlDocument requestDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyInformationPoint"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="requestDoc">The request document.</param>
        public PolicyInformationPoint(XacmlContextRequest request, XmlDocument requestDoc) {
            Contract.Requires<ArgumentNullException>(request != null);
            Contract.Requires<ArgumentNullException>(requestDoc != null);

            this.request = request;
            this.attributesProcessor = AttributesProcessor.Instance;
            this.xpathProcessor = XPathProcessor.Instance;
            this.requestDocument = requestDoc;
        }

        /// <remarks>
        /// used only for XACML 3.0
        /// </remarks>
        public IEnumerable<string> GetAttributeDesignatorValues(Uri attributeId, Uri dataType, string issuer, Uri category) {
            Contract.Requires<ArgumentNullException>(attributeId != null);
            Contract.Requires<ArgumentNullException>(dataType != null);
            Contract.Requires<ArgumentNullException>(category != null);

            IEnumerable<XacmlContextAttributes> categoryMatch = this.request.Attributes.Where(o => Uri.Compare(o.Category, category, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0);
            IEnumerable<XacmlAttribute> attributeIdMatch = categoryMatch.SelectMany(o => o.Attributes).Where(o => Uri.Compare(o.AttributeId, attributeId, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0);

            if (!string.IsNullOrEmpty(issuer)) {
                attributeIdMatch = attributeIdMatch.Where(o => string.Equals(o.Issuer, issuer, StringComparison.InvariantCultureIgnoreCase));
            }

            var result = attributeIdMatch.SelectMany(o => o.AttributeValues).Where(o => Uri.Compare(o.DataType, dataType, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0).Select(o => o.Value);

            if (result.Count() == 0) {
                string attribute = attributesProcessor[attributeId.ToString()];
                if (attribute != null) {
                    result = new List<string>() { attribute };
                }
            }

            return result;
        }

        public IEnumerable<string> GetSubjectAttributeDesignatorValues(Uri attributeId, Uri dataType, string issuer, Uri subjectCategory) {
            Contract.Requires<ArgumentNullException>(attributeId != null);
            Contract.Requires<ArgumentNullException>(dataType != null);

            Func<Uri, Uri, bool> compareSubjectCategory = (c1, c2) => {
                if (c1 == null && c2 == null) {
                    return true;
                }

                Uri DefaultSubjectCategory = new Uri("urn:oasis:names:tc:xacml:1.0:subject-category:access-subject");
                if (c1 == null) {
                    c1 = DefaultSubjectCategory;
                }

                if (c2 == null) {
                    c2 = DefaultSubjectCategory;
                }

                return Uri.Compare(c1, c2, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0;
            };

            return this.GetAttributeDesignatorValues(this.request.Subjects.Where(o => compareSubjectCategory(o.SubjectCategory, subjectCategory)).SelectMany(o => o.Attributes), attributeId, dataType, issuer);
        }

        public IEnumerable<string> GetResourceAttributeDesignatorValues(Uri attributeId, Uri dataType, string issuer) {
            Contract.Requires<ArgumentNullException>(attributeId != null);
            Contract.Requires<ArgumentNullException>(dataType != null);

            return this.GetAttributeDesignatorValues(this.request.Resources.SelectMany(o => o.Attributes), attributeId, dataType, issuer);
        }

        public IEnumerable<string> GetActionAttributeDesignatorValues(Uri attributeId, Uri dataType, string issuer) {
            Contract.Requires<ArgumentNullException>(attributeId != null);
            Contract.Requires<ArgumentNullException>(dataType != null);

            return this.GetAttributeDesignatorValues(this.request.Action.Attributes, attributeId, dataType, issuer);
        }

        public IEnumerable<string> GetEnvironmentAttributeDesignatorValues(Uri attributeId, Uri dataType, string issuer) {
            Contract.Requires<ArgumentNullException>(attributeId != null);
            Contract.Requires<ArgumentNullException>(dataType != null);

            ICollection<XacmlContextAttribute> attributes = null;
            
            // XACML 1.0/1.1
            if (this.request.Environment != null) { 
                attributes = this.request.Environment.Attributes;
            }
            
            if (attributes == null) {
                attributes = new List<XacmlContextAttribute>();
            }

            return this.GetAttributeDesignatorValues(attributes, attributeId, dataType, issuer);
        }

        public IEnumerable<XmlNode> GetAttributeByXPath(Uri xpathVersion, string xpathExpression, IDictionary<string, string> namespaces = null) {
            Contract.Requires<ArgumentNullException>(xpathVersion != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(xpathExpression));

            return this.xpathProcessor[xpathVersion.ToString()].Invoke(this.requestDocument, @"/*[local-name()='Request']", xpathExpression, namespaces);
        }

        /// <remarks>
        /// used only for XACML 3.0
        /// </remarks>
        public IEnumerable<XmlNode> GetAttributeByXPath(Uri xpathVersion, string xpathExpression, Uri category, Uri contextSelectorId = null, IDictionary<string, string> namespaces = null) {
            Contract.Requires<ArgumentNullException>(xpathVersion != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(xpathExpression));
            Contract.Requires<ArgumentNullException>(category != null);

            var xpath = this.xpathProcessor[xpathVersion.ToString()];
            if (contextSelectorId != null) {
                //PROFILE - Multiple Decision Profile - #POL01 - #SPEC2744
                XacmlAttribute attribute = this.request.Attributes.Where(o => string.Equals(o.Category.OriginalString, category.OriginalString))
                    .First()
                    .Attributes.Where(o => string.Equals(o.AttributeId.OriginalString, contextSelectorId.OriginalString))
                    .FirstOrDefault();

                if (attribute == null) {
                    throw new XacmlIndeterminateException("Cannot find attribute with name: " + contextSelectorId);
                }

                XacmlAttributeValue xPathExpressionDataTypeAttribute = attribute.AttributeValues.Where(o => string.Equals(o.DataType.OriginalString, "urn:oasis:names:tc:xacml:3.0:data-type:xpathExpression")).FirstOrDefault();

                if (xPathExpressionDataTypeAttribute == null) {
                    throw new XacmlIndeterminateException("Cannot find attribute with name: " + contextSelectorId);
                }

                //IEnumerable<XmlNode> nodes = XPathProcessor.Get().GetValue(this.requestDocument, string.Format(@"//*[local-name()='Attributes'][@Category='{0}']/*[local-name()='Content']/*", category), xPathExpressionDataTypeAttribute.Value, namespaces);
                IEnumerable<XmlNode> nodes = xpath.Invoke(this.requestDocument, string.Format(@"//*[local-name()='Attributes'][@Category='{0}']/*[local-name()='Content']/*", category), xPathExpressionDataTypeAttribute.Value, namespaces);

                List<XmlNode> result = new List<XmlNode>();
                foreach (XmlNode node in nodes) {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(node.OuterXml);
                    result.AddRange(xpath.Invoke(doc, @"/*", xpathExpression, namespaces));
                }

                return result;
            }
            else {
                return xpath.Invoke(this.requestDocument, string.Format(@"//*[local-name()='Attributes'][@Category='{0}']/*[local-name()='Content']/*", category), xpathExpression, namespaces);
            }
        }

        public ICollection<XacmlContextAttributes> GetAttributesWithIncludeInResult() {
            List<XacmlContextAttributes> result = new List<XacmlContextAttributes>();

            foreach (XacmlContextAttributes attributes in this.request.Attributes) {
                if (attributes.Attributes.Any(o => o.IncludeInResult)) {
                    result.Add(new XacmlContextAttributes(attributes.Category, attributes.Attributes.Where(o => o.IncludeInResult)) {
                        Id = attributes.Id,
                        Content = attributes.Content,
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Pazīme, ka ir jaatgriež sarakstu ar piemerotiem Policy/PolicySet
        /// </summary>
        /// <returns></returns>
        public bool ReturnPolicyIdList() {
            return this.request.ReturnPolicyIdList;
        }

        private IEnumerable<string> GetAttributeDesignatorValues(IEnumerable<XacmlContextAttribute> attributes, Uri attributeId, Uri dataType, string issuer) {
            Contract.Requires(attributes != null);
            Contract.Requires(attributeId != null);
            Contract.Requires(dataType != null);

            IEnumerable<XacmlContextAttribute> attributeIdMatch = attributes.Where(o => Uri.Compare(o.AttributeId, attributeId, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0 && Uri.Compare(o.DataType, dataType, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (!string.IsNullOrEmpty(issuer)) {
                attributeIdMatch = attributeIdMatch.Where(o => string.Equals(o.Issuer, issuer, StringComparison.InvariantCultureIgnoreCase));
            }

            var result = attributeIdMatch.SelectMany(o => o.AttributeValues).Select(o => o.Value);

            if (result.Count() == 0) {
                string attribute = attributesProcessor[attributeId.ToString()];
                if (attribute != null) {
                    result = new List<string>() { attribute };
                }
            }

            return result;
        }
    }
}
