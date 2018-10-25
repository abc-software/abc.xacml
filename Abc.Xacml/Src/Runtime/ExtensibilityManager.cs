// ----------------------------------------------------------------------------
// <copyright file="ExtensibilityManager.cs" company="ABC Software Ltd">
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
    using System.Collections.Generic;
    using System.Linq;
#if NET40 || NET45
    using System.ComponentModel.Composition.Hosting;
#endif
#if NETSTANDARD1_6 || NETSTANDARD2_0
    using System.IO;
    using System.Runtime.Loader;
    using System.Composition.Hosting;
#endif

    internal static class ExtensibilityManager {
        private const string FileMask = "Abc.Xacml.*.dll";
#if NET40 || NET45
        private static CompositionContainer container;
#endif
#if NETSTANDARD1_6 || NETSTANDARD2_0
        private static CompositionHost container;
#endif

        public static IEnumerable<T> GetExportedTypes<T>() {
            EnsureInitializedContainer();
#if NET40 || NET45
            return container.GetExportedValues<T>();
#endif
#if NETSTANDARD1_6 || NETSTANDARD2_0
            return container.GetExports<T>();
#endif
        }

        private static void EnsureInitializedContainer() {
            if (container == null) {
#if NET40 || NET45
                var catalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, FileMask);
                container = new CompositionContainer(catalog);
#endif
#if NETSTANDARD1_6 || NETSTANDARD2_0
                var assemblies = Directory.GetFiles(AppContext.BaseDirectory, FileMask)
                                    .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath);
                var configuration = new ContainerConfiguration().WithAssemblies(assemblies);
                container = configuration.CreateContainer();
#endif
            }
        }
    }
}
