// ----------------------------------------------------------------------------
// <copyright file="XacmlContextRequestReference.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.Context {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    public class XacmlContextRequestReference {
        private readonly ICollection<string> attributeRefereneces = new Collection<string>();

        public XacmlContextRequestReference(IEnumerable<string> references) {
            foreach (var item in references) {
                this.attributeRefereneces.Add(item);
            }
        }

        public ICollection<string> AttributeReferences {
            get {
                return this.attributeRefereneces;
            }
        }
    }
}
