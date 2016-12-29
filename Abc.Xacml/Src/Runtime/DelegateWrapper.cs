﻿// ----------------------------------------------------------------------------
// <copyright file="DelegateWrapper.cs" company="ABC Software Ltd">
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
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    /// <summary>
    /// Wrapper class for delegate.
    /// </summary>
    public class DelegateWrapper {
        protected Delegate del;
        protected Func<object[], object[]> prepareParams = o => o;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateWrapper"/> class.
        /// </summary>
        /// <param name="del">The delegate.</param>
        public DelegateWrapper(Delegate del) {
            Contract.Requires<ArgumentNullException>(del != null);

            this.del = del;

            // Parveido params vertības, par sarakstu, jo dinamicinvoke neatbalsta params
            ParameterInfo[] allParams = del.Method.GetParameters();
            ParameterInfo lastParam = allParams.LastOrDefault();
            if (lastParam != null) {
                bool hasParamsArg = lastParam.GetCustomAttributes(typeof(ParamArrayAttribute), false).Any();
                if (hasParamsArg) {
                    int normalParamsCount = del.Method.GetParameters().Count() - 1;
                    this.prepareParams = o => {
                        List<object> paramArray = o.Skip(normalParamsCount).ToList();
                        Type paramsType = paramArray.First().GetType();

                        // ja parametru datu tips atšķiras, tad tiek konvertēts uz Object massivu
                        if (paramArray.Any(a => !a.GetType().Equals(paramsType))) {
                            paramsType = typeof(object);
                        }

                        int elemCount = paramArray.Count;
                        Array newParamArray = Array.CreateInstance(paramsType, elemCount);

                        for (int i = 0; i < elemCount; i++) {
                            newParamArray.SetValue(paramArray[i], i);
                        }

                        List<object> generatedParams = o.Take(normalParamsCount).ToList();
                        generatedParams.Add(newParamArray);

                        return generatedParams.ToArray();
                    };
                }
            }
        }

        /// <summary>
        /// Dynamics the invoke.
        /// </summary>
        /// <param name="xpathContext">The xpath context.</param>
        /// <param name="param">An array of objects that are the arguments to pass to the method represented by the current delegate.</param>
        /// <returns>The object returned by the method represented by the delegate.</returns>
        /// <exception cref="XacmlIndeterminateException">Error in function evaluation: </exception>
        /// <exception cref="XacmlInvalidDataTypeException">
        /// Wrong argument
        /// or
        /// Wrong data type
        /// </exception>
        public virtual object DynamicInvoke(XPathContext xpathContext, params object[] param) {
            try {
                // fix params keyword
                // Delegate.DynamicInvoke ignores ParamArrayAttribute, should to convert to IEnumerable<T>
                return this.del.Method.Invoke(null, this.prepareParams(param));
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
