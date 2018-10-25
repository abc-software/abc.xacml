using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Abc.Xacml.UnitTests {
    [SetUpFixture]
    public class SetUpClass {
        [OneTimeSetUp]
        public void RunBeforeAnyTests() {
            Directory.SetCurrentDirectory(BaseDirectory);
        }

        public static string BaseDirectory {
            get {
#if NETSTANDARD1_6
                var dir = AppContext.BaseDirectory;
#else
                var dir = Path.GetDirectoryName(new Uri(typeof(Xacml30TestsCases).Assembly.CodeBase).LocalPath);
#endif
                return dir;
            }
        }
    }
}
