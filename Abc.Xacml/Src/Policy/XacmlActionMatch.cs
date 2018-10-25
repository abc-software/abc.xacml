// ----------------------------------------------------------------------------
// <copyright file="XacmlActionMatch.cs" company="ABC Software Ltd">
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

namespace Abc.Xacml.Policy {
    using System;

    /// <summary>
    /// class XacmlActionMatch
    /// </summary>
    public class XacmlActionMatch : XacmlMatch {
        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlActionMatch"/> class.
        /// </summary>
        /// <param name="matchId">The match identifier.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="attributeDesignator">The attribute designator.</param>
        public XacmlActionMatch(Uri matchId, XacmlAttributeValue attributeValue, XacmlActionAttributeDesignator attributeDesignator)
            : base(matchId, attributeValue, attributeDesignator) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XacmlActionMatch"/> class.
        /// </summary>
        /// <param name="matchId">The match identifier.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="attributeSelector">The attribute selector.</param>
        public XacmlActionMatch(Uri matchId, XacmlAttributeValue attributeValue, XacmlAttributeSelector attributeSelector)
            : base(matchId, attributeValue, attributeSelector) {
        }
    }
}