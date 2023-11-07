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
#if NET40_OR_GREATER
    using System.ComponentModel.Composition.Hosting;
#endif
#if NETSTANDARD || NET5_0_OR_GREATER
    using System.IO;
    using System.Runtime.Loader;
    using System.Composition.Hosting;
    using System.Reflection;
#endif

    internal static class ExtensibilityManager {
        private const string FileMask = "Abc.Xacml.*.dll";
#if NET40_OR_GREATER
        private static CompositionContainer container;
#endif
#if NETSTANDARD || NET5_0_OR_GREATER
        private static CompositionHost container;
#endif

        public static IEnumerable<T> GetExportedTypes<T>() {
            EnsureInitializedContainer();
#if NET40_OR_GREATER
            return container.GetExportedValues<T>();
#endif
#if NETSTANDARD || NET5_0_OR_GREATER
            return container.GetExports<T>();
#endif
        }

        private static void EnsureInitializedContainer() {
            if (container == null) {
#if NET40_OR_GREATER
                var catalog = new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, FileMask);
                container = new CompositionContainer(catalog);
#endif
#if NETSTANDARD || NET5_0_OR_GREATER
                // HACK: do not load UnitTest assemblies
                var assemblies = Directory.GetFiles(AppContext.BaseDirectory, FileMask)
                        .Where(p => !Path.GetFileNameWithoutExtension(p).Contains("Test"))
                        .Select(p => AssemblyLoadContext.Default.LoadFromAssemblyName(AssemblyLoadContext.GetAssemblyName(p)));
                var configuration = new ContainerConfiguration().WithAssemblies(assemblies);
                container = configuration.CreateContainer();
#endif
            }
        }
    }
}
