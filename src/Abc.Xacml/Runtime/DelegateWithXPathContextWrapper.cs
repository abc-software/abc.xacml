// ----------------------------------------------------------------------------
// <copyright file="DelegateWithXPathContextWrapper.cs" company="ABC Software Ltd">
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
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The wrapper class for delegate.
    /// </summary>
    public class DelegateWithXPathContextWrapper : DelegateWrapper {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateWithXPathContextWrapper"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        public DelegateWithXPathContextWrapper(Type type, MethodInfo method)
            : base(type, method) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateWithXPathContextWrapper"/> class.
        /// </summary>
        /// <param name="del">The delegate.</param>
        public DelegateWithXPathContextWrapper(Delegate del)
            : base(del) {
        }

        /// <inheritdoc/>
        public override object DynamicInvoke(XPathContext xpathContext, params object[] param) {
            if (xpathContext == null) {
                throw new ArgumentNullException(nameof(xpathContext));
            }

            try {
                // fix params keyword
                // Delegate.DynamicInvoke ignores ParamArrayAttribute, should to convert to IEnumerable<T>
                // Plus, add Request document param and Namespecas from policy, required for functions like XPath
#if NETSTANDARD1_6
                MethodInfo method = this.del.GetMethodInfo();
#else
                MethodInfo method = this.del.Method;
#endif
                return method.Invoke(null, this.prepareParams(new object[] { xpathContext }.Concat(param).ToArray()).ToArray());
            }
            catch (TargetInvocationException ex) {
                throw new XacmlIndeterminateException("Error in function evaluation: ", ex);
            }
            catch (ArgumentException ex) {
                throw new XacmlInvalidDataTypeException("Wrong argument", ex);
            }
            catch (InvalidCastException ex) {
                throw new XacmlInvalidDataTypeException("Wrong data type", ex);
            }
        }
    }
}
