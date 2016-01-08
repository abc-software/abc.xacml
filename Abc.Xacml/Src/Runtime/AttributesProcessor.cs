// ----------------------------------------------------------------------------
// <copyright file="AttributesProcessor.cs" company="ABC Software Ltd">
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
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics.Contracts;
    using Abc.Xacml.Interfaces;

    internal class AttributesProcessor {
        private static AttributesProcessor processor = null;
        private static object locker = new object();
        private static SortedDictionary<string, Func<string>> attributes = new SortedDictionary<string, Func<string>>()
        {
            { "urn:oasis:names:tc:xacml:1.0:environment:current-time", () => DateTime.Now.ToUniversalTime().ToString("o") },
            { "urn:oasis:names:tc:xacml:1.0:environment:current-date", () => DateTime.Now.ToUniversalTime().Date.ToString("yyyy-MM-dd") },
            { "urn:oasis:names:tc:xacml:1.0:environment:current-dateTime", () => DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") },
        };

        private AttributesProcessor() {
        }

        internal static AttributesProcessor Instance {
            get {
                if (processor == null) {
                    lock (locker) {
                        if (processor == null) {
                            processor = new AttributesProcessor();

                            var catalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "Abc.Xacml.*.dll");
                            var container = new CompositionContainer(catalog);
                            var exportedTypes = container.GetExportedValues<IAttributesExtender>();
                            foreach (var t in exportedTypes) {
                                IDictionary<string, Func<string>> extensionTypes = t.GetExtensionAttributes();
                                foreach (var extensionType in extensionTypes) {
                                    AttributesProcessor.attributes.Add(extensionType.Key, extensionType.Value);
                                }
                            }
                        }
                    }
                }

                return processor;
            }
        }


        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified value.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/> function name.
        /// </value>
        /// <param name="value">The function name.</param>
        /// <returns>The function result.</returns>
        public string this[string value] {
            get {
                Contract.Requires<ArgumentNullException>(value != null);

                Func<string> func;
                if (attributes.TryGetValue(value, out func)) {
                    return func();
                }

                return null;
            }
        }
    }
}
