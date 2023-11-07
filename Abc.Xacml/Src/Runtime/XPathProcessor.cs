// ----------------------------------------------------------------------------
// <copyright file="XPathProcessor.cs" company="ABC Software Ltd">
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
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.XPath;
    using Abc.Xacml.DataTypes;
    using Abc.Xacml.Interfaces;

    public class XPathProcessor {
        public delegate IEnumerable<XmlNode> XPathRunner(XmlDocument xml, string xpathContextSelector, string xPathExpression, IDictionary<string, string> namespaces = null, XPathExpressionType changeContextExpression = null);

        private static readonly object Locker = new object();
        private static XPathProcessor processor = null;

        private static SortedDictionary<string, XPathRunner> versions = new SortedDictionary<string, XPathRunner>()
        {
            { Xacml10Constants.XPathVersions.Xpath10String, DotNetXPath },
            { Xacml30Constants.XPathVersions.Xpath20String, DotNetXPath },
        };

        /// <summary>
        /// Prevents a default instance of the <see cref="XPathProcessor"/> class from being created.
        /// </summary>
        private XPathProcessor() {
        }

        internal static XPathProcessor Instance {
            get {
                if (processor == null) {
                    lock (Locker) {
                        if (processor == null) {
                            processor = new XPathProcessor();
                            foreach (var t in ExtensibilityManager.GetExportedTypes<IXPathExtender>()) {
                                IDictionary<string, XPathRunner> extensionTypes = t.GetExtensionXPathVersions();
                                foreach (var extensionType in extensionTypes) {
                                    XPathProcessor.versions.Add(extensionType.Key, extensionType.Value);
                                }
                            }
                        }
                    }
                }

                return processor;
            }
        }

        public XPathRunner this[string value] {
            get {
                XPathRunner action;
                if (versions.TryGetValue(value, out action)) {
                    return action;
                }

                throw new ArgumentException("Unknown combining algorithm name", nameof(value));
            }
        }

        public static IEnumerable<XmlNode> DotNetXPath(XmlDocument xml, string xpathContextSelector, string xPathExpression, IDictionary<string, string> namespaces, XPathExpressionType changeContextExpression) {
            if (xml == null) {
                throw new ArgumentNullException(nameof(xml));
            }

            if (xpathContextSelector == null) {
                throw new ArgumentNullException(nameof(xpathContextSelector));
            }

            if (xPathExpression == null) {
                throw new ArgumentNullException(nameof(xPathExpression));
            }

            if (xpathContextSelector.Length == 0) {
                throw new ArgumentException("Value cannot be empty.", nameof(xpathContextSelector));
            }

            if (xPathExpression.Length == 0) {
                throw new ArgumentException("Value cannot be empty.", nameof(xPathExpression));
            }

            XmlNode context = xml.SelectSingleNode(xpathContextSelector);

            if (changeContextExpression != null) {
                context = context.SelectSingleNode(changeContextExpression.ToString());
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(context.OuterXml);

            XPathNavigator navigator = context.CreateNavigator();

            navigator = doc.DocumentElement.CreateNavigator();
            List<XmlNode> result = new List<XmlNode>();

            XPathNodeIterator iterator;
            try {
                if (namespaces == null) {
                    iterator = navigator.Select(xPathExpression);
                }
                else {
                    XmlNamespaceManager manager = new XmlNamespaceManager(xml.NameTable);
                    foreach (var elem in namespaces) {
                        manager.AddNamespace(elem.Key, elem.Value);
                    }

                    iterator = navigator.Select(xPathExpression, manager);
                }
            }
            catch (XPathException ex) {
                throw new XacmlInvalidDataTypeException("Invalid XPath query", ex);
            }

            if (iterator.Count > 0) {
                while (iterator.MoveNext()) {
                    result.Add(iterator.Current.UnderlyingObject as XmlNode);
                }
            }

            return result;
        }
    }
}
