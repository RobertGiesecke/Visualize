using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class DebuggerDisplayInjectorTests
    {
        [Test]
        public void EnumShouldNotGetDebuggerDisplay()
        {
            var simpleEnumType = AssemblyWeaver.Assembly.GetType("AssemblyToProcess.SimpleEnum", true);
            var fullName = typeof(DebuggerDisplayAttribute).FullName;
            Assert.IsFalse(simpleEnumType.CustomAttributes.Any(t => t.AttributeType.FullName == fullName),
                           "Enums should not get decorated with '{0}'.",
                           typeof(DebuggerDisplayAttribute).Name);
        }

        [Test]
        public void DisplayInheritedProperties()
        {
            var asm = AssemblyDefinition.ReadAssembly(AssemblyWeaver.AfterAssemblyPath,
                                                      new ReaderParameters(ReadingMode.Deferred));
            var classWithPropertiesDescendant = asm.MainModule.GetType("AssemblyToProcess.ClassWithPropertiesDescendant");
            var fullName = typeof(DebuggerDisplayAttribute).FullName;

            var debuggerDisplay = classWithPropertiesDescendant.CustomAttributes.FirstOrDefault(t => t.AttributeType.FullName == fullName);
            var debuggerDisplayName = typeof(DebuggerDisplayAttribute).Name;
            Assert.IsNotNull(debuggerDisplay,
                             "'{0}' should be decorated with '{1}'.",
                             classWithPropertiesDescendant.FullName,
                             debuggerDisplayName);

            Assert.AreEqual(1,
                            debuggerDisplay.ConstructorArguments.Count,
                            "'{0}' should have 1 ctor parameter.",
                            debuggerDisplayName);

            Assert.AreEqual("Boolean = {Boolean} | NewProperty = {NewProperty} | Nullable = {Nullable} | Number = {Number} | String = {String}", 
                            debuggerDisplay.ConstructorArguments[0].Value);
        }
    }
}
