// ----------------------------------------------------------------------------
// <copyright file="DataTypeProcessor.cs" company="ABC Software Ltd">
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
    using System.ComponentModel;
    using System.Linq;
    using Abc.Xacml.DataTypes;
    using Abc.Xacml.Interfaces;
    using Abc.Xacml.Policy;

    public class DataTypeProcessor {
        private static DataTypeProcessor processor = null;
        private static readonly object locker = new object();

        private static SortedDictionary<string, TypeConverterWrapper> typeConverters = new SortedDictionary<string, TypeConverterWrapper>()
        {
            { "http://www.w3.org/2001/XMLSchema#string", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(string)), typeof(string)) },
            { "http://www.w3.org/2001/XMLSchema#boolean", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(bool)), typeof(bool)) },
            { "http://www.w3.org/2001/XMLSchema#integer", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(int)), typeof(int)) },
            { "http://www.w3.org/2001/XMLSchema#double", new TypeConverterWrapper(new DoubleTypeConverter(), typeof(double)) },
            { "http://www.w3.org/2001/XMLSchema#time", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(Time)), typeof(Time)) },
            { "http://www.w3.org/2001/XMLSchema#date", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(Date)), typeof(Date)) },
            { "http://www.w3.org/2001/XMLSchema#dateTime", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(DateTime)), typeof(DateTime)) },
            { "http://www.w3.org/TR/2002/WD-xquery-operators-20020816#dayTimeDuration", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(DayTimeDuration)), typeof(DayTimeDuration)) },
            { "http://www.w3.org/TR/2002/WD-xquery-operators-20020816#yearMonthDuration", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(YearMonthDuration)), typeof(YearMonthDuration)) },
            { "http://www.w3.org/2001/XMLSchema#anyURI", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(Uri)), typeof(Uri)) },
            { "http://www.w3.org/2001/XMLSchema#hexBinary", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(HexBinary)), typeof(HexBinary)) },
            { "http://www.w3.org/2001/XMLSchema#base64Binary", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(Base64Binary)), typeof(Base64Binary)) },
            { "urn:oasis:names:tc:xacml:1.0:data-type:rfc822Name", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(Rfc822Name)), typeof(Rfc822Name)) },
            { "urn:oasis:names:tc:xacml:1.0:data-type:x500Name", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(x500Name)), typeof(x500Name)) },
            { "urn:oasis:names:tc:xacml:2.0:data-type:ipAddress", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(IPAddress)), typeof(IPAddress)) },
            { "urn:oasis:names:tc:xacml:2.0:data-type:dnsName", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(DnsName)), typeof(DnsName)) },
        
            // 3.0
            { "http://www.w3.org/2001/XMLSchema#dayTimeDuration", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(DayTimeDuration)), typeof(DayTimeDuration)) },
            { "http://www.w3.org/2001/XMLSchema#yearMonthDuration", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(YearMonthDuration)), typeof(YearMonthDuration)) },
            { "urn:oasis:names:tc:xacml:3.0:data-type:xpathExpression", new TypeConverterWrapper(TypeDescriptor.GetConverter(typeof(XPathExpressionType)), typeof(XPathExpressionType),
                (source, dest) =>
                {
                    XacmlAttributeValue convSource = source as XacmlAttributeValue;
                    XPathExpressionType exprType = dest as XPathExpressionType;

                    if (convSource != null && exprType != null)
                    {
                        string xPathCategory;
                        xPathCategory = convSource.Attributes.FirstOrDefault(a => a.Name == "XPathCategory").Value;
                        exprType.XPathCategory = xPathCategory;
                    }
                }) },
        };

        private DataTypeProcessor() {
        }

        internal static DataTypeProcessor Instance {
            get {
                if (processor == null) {
                    lock (locker) {
                        if (processor == null) {
                            processor = new DataTypeProcessor();
                            foreach (var t in ExtensibilityManager.GetExportedTypes<ITypesExtender>()) {
                                IDictionary<string, TypeConverterWrapper> extensionTypes = t.GetExtensionTypes();
                                foreach (var extensionType in extensionTypes) {
                                    DataTypeProcessor.typeConverters.Add(extensionType.Key, extensionType.Value);
                                }
                            }
                        }
                    }
                }

                return processor;
            }
        }

        public TypeConverterWrapper this[string value] {
            get {
                TypeConverterWrapper converter;
                if (typeConverters.TryGetValue(value, out converter)) {
                    return converter;
                }
                else {
                    throw new ArgumentException("Unknown data type name in match expression", nameof(value));
                }
            }
        }

        public string this[TypeConverter value] {
            get {
                var pair = DataTypeProcessor.typeConverters.Where(o => o.Value.IsForType(value));
                if (!pair.Any()) {
                    throw new ArgumentException("Unknown TypeConverter in match expression", nameof(value));
                }

                return pair.First().Key;
            }
        }
    }
}
