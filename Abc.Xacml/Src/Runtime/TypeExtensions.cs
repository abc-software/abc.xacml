// ----------------------------------------------------------------------------
// <copyright file="TypeExtensions.cs" company="ABC Software Ltd">
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
    using System.Reflection;

    /// <summary>
    /// Extension methods <see cref="Type" /> type.
    /// </summary>
    public static class TypeExtensions {
#if NETSTANDARD1_6
        /// <summary>Determines whether an instance of the current <see cref="T:System.Type" /> can be assigned from an instance of the specified Type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>true if <paramref name="c" /> and the current Type represent the same type, or if the current Type is in the inheritance hierarchy of <paramref name="c" />, or if the current Type is an interface that <paramref name="c" /> implements, or if <paramref name="c" /> is a generic type parameter and the current Type represents one of the constraints of <paramref name="c" />. false if none of these conditions are true, or if <paramref name="c" /> is null.</returns>
        /// <param name="c">The type to compare with the current type. </param>
        public static bool IsAssignableFrom(this Type type, Type c) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetTypeInfo().IsAssignableFrom(c);
        }

        /// <summary>
        /// Searches for the specified public method whose parameters match the specified argument types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The string containing the name of the public method to get.</param>
        /// <returns>A <see cref="MethodInfo"/> object representing the public method whose parameters match the specified argument types, if found; otherwise, a null reference </returns>
        public static MethodInfo GetMethod(this Type type, string name) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetTypeInfo().GetMethod(name);
        }

        /// <summary>
        /// Searches for the specified public method whose parameters match the specified argument types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The string containing the name of the public method to get.</param>
        /// <param name="types">An array of Type objects representing the number, order, and type of the parameters for the method to get. </param>
        /// <returns>A <see cref="MethodInfo"/> object representing the public method whose parameters match the specified argument types, if found; otherwise, a null reference </returns>
        public static MethodInfo GetMethod(this Type type, string name, Type[] types) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetTypeInfo().GetMethod(name, types);
        }

        /// <summary>
        /// Searches for the specified public method whose parameters match the specified argument types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The string containing the name of the public method to get.</param>
        /// <param name="bindingFlags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted. </param>
        /// <returns>A <see cref="MethodInfo"/> object representing the public method whth specified name, if found; otherwise, a null reference </returns>
        public static MethodInfo GetMethod(this Type type, string name, BindingFlags bindingFlags) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetTypeInfo().GetMethod(name, bindingFlags);
        }

        /// <summary>
        /// Searches for the specified members, using the specified binding constraints.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The string containing the name of the members to get.</param>
        /// <param name="bindingFlags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted. </param>
        /// <returns>An array of <see cref="MemberInfo"/> objects representing the public members with the specified name, if found; otherwise, an empty array.</returns>
        public static MemberInfo[] GetMember(this Type type, string name, BindingFlags bindingFlags) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetTypeInfo().GetMember(name, bindingFlags);
        }
#endif

        /// <summary>
        /// Searches for the specified public method whose parameters match the specified argument types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The string containing the name of the public method to get.</param>
        /// <param name="genericType">Type of the generic.</param>
        /// <param name="typeGetter">The type getter.</param>
        /// <returns>A <see cref="MethodInfo"/> object representing the public method whose parameters match the specified argument types, if found; otherwise, a null reference </returns>
        public static MethodInfo GetGenericMethodInfo(this Type type, string name, Type genericType, Func<Type, Type[]> typeGetter) {
            if (type == null) {
                throw new ArgumentNullException(nameof(type));
            }

            MemberInfo[] mis = type.GetMember(name + "*", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);
            if (mis.Length == 0) {
                throw new NotImplementedException("Unknown function");
            }

            Type u = ((MethodInfo)mis[0]).GetGenericArguments()[0]; // assume we know the class structure above, for simplicity.
            MethodInfo mInfo = type.GetMethod(name, typeGetter(u));
            if (!mInfo.IsGenericMethod) {
                throw new NotImplementedException("Unknown generic method");
            }

            mInfo = mInfo.MakeGenericMethod(genericType);
            return mInfo;
        }
    }
}
