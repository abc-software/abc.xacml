﻿// ----------------------------------------------------------------------------
// <copyright file="IAlgorithmsExtender.cs" company="ABC Software Ltd">
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
 
namespace Abc.Xacml.Interfaces {
    using System.Collections.Generic;
    using Abc.Xacml.Runtime;

    /// <summary>
    /// The XACML algorithm extender.
    /// </summary>
    public interface IAlgorithmsExtender {
        /// <summary>
        /// Gets the extension algorithms.
        /// </summary>
        /// <returns>The algorithms.</returns>
        IDictionary<string, AlgorithmsProcessor.AlgorithmsRunner> GetExtensionAlgorithms();
    }
}
