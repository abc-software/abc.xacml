// ----------------------------------------------------------------------------
// <copyright file="XacmlProtocolSerializerCommon.cs" company="ABC Software Ltd">
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
    using System.Xml;

    public partial class XacmlProtocolSerializer {
        /// <summary>
        /// The version
        /// </summary>
        protected XacmlVersion version;

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlProtocolSerializer"/> class.
        /// </summary>
        /// <param name="version">The XACML version.</param>
        protected XacmlProtocolSerializer(XacmlVersion version) {
            this.version = version;
        }

        /// <summary>
        /// Gets the XACML version.
        /// </summary>
        public XacmlVersion Version {
            get {
                return this.version;
            }
        }

        protected static Exception ThrowHelperXml(XmlReader reader, string message) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            IXmlLineInfo info = reader as IXmlLineInfo;
            return new XmlException(message, null, info != null ? info.LineNumber : 0, info != null ? info.LinePosition : 0);
        }

        protected bool ReadChoice(XmlReader reader, IDictionary<Tuple<string, string>, Action> actions, bool isRequired = false) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (actions == null) {
                throw new ArgumentNullException(nameof(actions));
            }

            bool founded = false;
            foreach (KeyValuePair<Tuple<string, string>, Action> elementType in actions) {
                if (reader.IsStartElement(elementType.Key.Item1, elementType.Key.Item2)) {
                    founded = true;
                    elementType.Value();
                    return true;
                }
            }

            if (isRequired && !founded) {
                throw ThrowHelperXml(reader, "Unknown element " + reader.LocalName);
            }
            else {
                return false;
            }
        }

        protected void ReadChoiceMultiply(XmlReader reader, IDictionary<Tuple<string, string>, Action> actions, bool isRequired = false) {
            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (actions == null) {
                throw new ArgumentNullException(nameof(actions));
            }

            if (isRequired && (reader.NodeType == XmlNodeType.EndElement)) {
                throw new XmlException(reader.Name + " is empty");
            }

            while (reader.NodeType != XmlNodeType.EndElement) {
                if (!this.ReadChoice(reader, actions, isRequired))
                    break;

                isRequired = false;
            }
        }

        protected T ReadRequired<T>(string elementName, string elementNamespace, ReadElement<T> readFunction, XmlReader reader) {
            if (elementName == null) {
                throw new ArgumentNullException(nameof(elementName));
            }

            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (readFunction == null) {
                throw new ArgumentNullException(nameof(readFunction));
            }

            if (!reader.IsStartElement(XacmlConstants.ElementNames.ActionAttributeDesignator, this.version.NamespacePolicy)) {
                T result = readFunction.Invoke(reader);
                return result;
            }
            else {
                throw new XmlException(elementName + " is required");
            }
        }

        protected T ReadOptional<T>(string elementName, string elementNamespace, ReadElement<T> readFunction, XmlReader reader) {
            if (elementName == null) {
                throw new ArgumentNullException(nameof(elementName));
            }

            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (readFunction == null) {
                throw new ArgumentNullException(nameof(readFunction));
            }

            if (!reader.IsStartElement(elementName, elementNamespace)) {
                return default(T);
            }
            else {
                T result = readFunction.Invoke(reader);
                return result;
            }
        }

        protected T1 ReadOptionalAbstract<T1, T2>(string elementName, string elementNamespace, ReadElement<T2> readFunction, XmlReader reader)
            where T1 : T2 {
            if (elementName == null) {
                throw new ArgumentNullException(nameof(elementName));
            }

            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (readFunction == null) {
                throw new ArgumentNullException(nameof(readFunction));
            }

            if (!reader.IsStartElement(elementName, elementNamespace)) {
                return (T1)default(T2);
            }
            else {
                T1 result = (T1)readFunction.Invoke(reader);
                return result;
            }
        }

        protected void ReadList<T>(ICollection<T> list, string elementName, string elementNamespace, ReadElement<T> readFunction, XmlReader reader, bool isRequired = false) {
            if (elementName == null) {
                throw new ArgumentNullException(nameof(elementName));
            }

            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (readFunction == null) {
                throw new ArgumentNullException(nameof(readFunction));
            }

            if (list == null) {
                throw new ArgumentNullException(nameof(list));
            }

            if (isRequired && !reader.IsStartElement(elementName, elementNamespace)) {
                throw new XmlException("A least 1 " + elementName + " is required");
            }

            while (reader.IsStartElement(elementName, elementNamespace)) {
                T elem = this.ReadOptional(elementName, elementNamespace, readFunction, reader);
                list.Add(elem);
            }
        }

        protected void ReadListAbstract<T1, T2>(ICollection<T1> list, string elementName, string elementNamespace, ReadElement<T2> readFunction, XmlReader reader, bool isRequired = false)
            where T1 : T2 {
            if (elementName == null) {
                throw new ArgumentNullException(nameof(elementName));
            }

            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            if (readFunction == null) {
                throw new ArgumentNullException(nameof(readFunction));
            }

            if (list == null) {
                throw new ArgumentNullException(nameof(list));
            }

            if (isRequired && !reader.IsStartElement(elementName, elementNamespace)) {
                throw new XmlException("A least 1 " + elementName + " is required");
            }

            while (reader.IsStartElement(elementName, elementNamespace)) {
                T1 elem = this.ReadOptionalAbstract<T1, T2>(elementName, elementNamespace, readFunction, reader);
                list.Add(elem);
            }
        }

        protected T ReadAttribute<T>(XmlReader reader, string attribute, string namespaceURI = null, bool isRequered = true) {
            if (attribute == null) {
                throw new ArgumentNullException(nameof(attribute));
            }

            if (reader == null) {
                throw new ArgumentNullException(nameof(reader));
            }

            string attributeResult = namespaceURI != null ? reader.GetAttribute(attribute, namespaceURI) : reader.GetAttribute(attribute);
            if (isRequered && string.IsNullOrEmpty(attributeResult)) {
                throw new XmlException(attribute + " IsNullOrEmpty");
            }

            if (string.IsNullOrEmpty(attributeResult)) {
                return default(T);
            }

            object val;
            if (typeof(T) == typeof(bool?) || typeof(T) == typeof(bool)) {
                val = XmlConvert.ToBoolean(attributeResult);
            }
            else if (typeof(T) == typeof(Uri)) {
                val = new Uri(attributeResult, UriKind.RelativeOrAbsolute);
            }
            else if (typeof(T) == typeof(string)) {
                val = attributeResult;
            }
            else if (typeof(T) == typeof(int?)) {
                val = XmlConvert.ToInt32(attributeResult);
            }
            else {
                throw new NotSupportedException();
            }

            return (T)val;
        }

        protected delegate T ReadElement<out T>(XmlReader reader);
    }
}
