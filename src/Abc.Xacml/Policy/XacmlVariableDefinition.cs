// ----------------------------------------------------------------------------
// <copyright file="XacmlVariableDefinition.cs" company="ABC Software Ltd">
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

    public class XacmlVariableDefinition : XacmlExpression {
        private object calculatedValue = null;
        private string variableId;

        public XacmlVariableDefinition(string variableId)
            : base() {
            this.VariableId = variableId;
        }

        public string VariableId {
            get {
                return this.variableId;
            }

            set {
                this.variableId = value;
            }
        }

        /// <summary>
        /// Mainīga vertība
        /// Kamēr nav apreķināta ir tukša
        /// Apreķinot tiek iestatīta vertība
        /// Izmantojama, lai vairakas reizes nereķinātu vienu un to pašu vertību
        /// WARNING: Nav XML sastavdaļa
        /// </summary>
        public object CalculatedValue {
            get {
                return this.calculatedValue;
            }

            set {
                this.calculatedValue = value;
            }
        }
    }
}
