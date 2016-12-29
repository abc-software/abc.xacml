// ----------------------------------------------------------------------------
// <copyright file="XacmlContextStatus.cs" company="ABC Software Ltd">
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
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Xml;

    /// <summary>
    /// The <c>XacmlContextStatus</c> class represents the status of the authorization decision result.
    /// </summary>
    /// <remarks>See the xacml-context:Status element defined in [XacmlCore, 6.12] for more details.</remarks>
    public class XacmlContextStatus {
        private XacmlContextStatusCode statusCode;
        private string statusMessage;
        private readonly Collection<XmlElement> statusDetail = new Collection<XmlElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlContextStatus"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public XacmlContextStatus(XacmlContextStatusCode statusCode) {
            Contract.Requires<ArgumentNullException>(statusCode != null);
            this.statusCode = statusCode;
        }

        /// <summary>
        /// Gets or sets the message associated with the status.
        /// </summary>
        /// <value>The message associated with the status.</value>
        /// <remarks>See [XacmlCore, 6.14] for more details.</remarks>
        public string StatusMessage {
            get {
                return this.statusMessage;
            }

            set {
                this.statusMessage = !string.IsNullOrEmpty(value) ? value : null;
            }
        }

        /// <summary>
        /// Gets custom XML elements associated with the status.
        /// </summary>
        /// <value>The custom XML elements associated with the status.</value>
        /// <remarks>See [XacmlCore, 6.15] for more details.</remarks>
        public Collection<XmlElement> StatusDetail {
            get {
                return this.statusDetail;
            }
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <remarks>See [XacmlCore, 6.13] for more details.</remarks>
        public XacmlContextStatusCode StatusCode {
            get {
                return this.statusCode;
            }

            set {
                Contract.Requires<ArgumentNullException>(value != null);
                this.statusCode = value;
            }
        }
    }
}