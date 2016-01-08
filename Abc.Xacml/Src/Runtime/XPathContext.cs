// ----------------------------------------------------------------------------
// <copyright file="XPathContext.cs" company="ABC Software Ltd">
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
    using System.Xml;

    public class XPathContext {
        public XPathContext(Uri version, XmlDocument document, IDictionary<string, string> ns) {
            Contract.Requires<ArgumentNullException>(document != null);

            this.Version = version;
            this.Document = document;
            this.Namespaces = ns;
        }

        public Uri Version { get; private set; }

        public XmlDocument Document { get; private set; }

        public IDictionary<string, string> Namespaces { get; private set; }
    }
}
