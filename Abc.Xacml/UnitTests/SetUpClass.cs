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
            var dir = Path.GetDirectoryName(new Uri(typeof(SetUpClass).Assembly.CodeBase).LocalPath);
            Environment.CurrentDirectory = dir;

            // or
            //Directory.SetCurrentDirectory(dir);
        }
    }
}
