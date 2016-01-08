﻿// ----------------------------------------------------------------------------
// <copyright file="TypeConverterWrapper.cs" company="ABC Software Ltd">
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
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Converter result</typeparam>
    /// <typeparam name="R">Converter source</typeparam>
    public class TypeConverterWrapper {
        public Func<IEnumerable<object>, object> ConvertEnumerable { get; set; }
        private Action<object, object> objectGenerator;

        private TypeConverter typeConverter;

        public TypeConverterWrapper(TypeConverter converter, Type resultType, Action<object, object> objectGenerator = null) {
            this.typeConverter = converter;
            this.objectGenerator = objectGenerator;

            MethodInfo method = typeof(Enumerable).GetMethod("OfType");
            MethodInfo generic = method.MakeGenericMethod(new Type[] { resultType });

            ConvertEnumerable = o => generic.Invoke(null, new[] { o });
        }

        public object ConvertFromString(string value, object source) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(value));
            Contract.Requires<ArgumentNullException>(source != null);

            try {
                object obj = this.typeConverter.ConvertFromInvariantString(value);
                if (this.objectGenerator != null && source != null) {
                    this.objectGenerator.Invoke(source, obj);
                }

                return obj;
            }
            catch (FormatException ex) {
                throw new XacmlInvalidDataTypeException(ex.Message, ex);
            }
        }

        public bool IsForType(TypeConverter converter) {
            Contract.Requires<ArgumentNullException>(converter != null);

            Type currentConverterType = this.typeConverter.GetType();
            Type paramConverterType = converter.GetType();
            return paramConverterType.IsAssignableFrom(currentConverterType);
        }
    }
}