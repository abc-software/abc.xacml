// ----------------------------------------------------------------------------
// <copyright file="XacmlAllOf.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.Policy {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
#if NET40
    using Diagnostic;
#else
    using Abc.Diagnostics;
#endif

    /// <summary>
    /// The conjunctive sequence of &lt;Match&gt; elements
    /// </summary>
    public class XacmlAllOf {
        private readonly ICollection<XacmlMatch> matches = new Collection<XacmlMatch>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlAllOf"/> class.
        /// </summary>
        /// <param name="matches">The matches.</param>
        public XacmlAllOf(IEnumerable<XacmlMatch> matches) {
            Contract.Requires<ArgumentNullException>(matches != null);

            foreach (var item in matches) {
                if (item == null) {
                    throw DiagnosticTools.ExceptionUtil.ThrowHelperArgument("Item is null!");
                }

                this.matches.Add(item);
            }

            if (this.matches.Count == 0) {
                throw DiagnosticTools.ExceptionUtil.ThrowHelperArgument("SubjectMatch count is 0!");
            }
        }

        /// <summary>
        /// Gets the matches.
        /// </summary>
        public ICollection<XacmlMatch> Matches {
            get {
                return this.matches;
            }
        }
    }
}
