using System;
using System.Diagnostics;
using System.Linq;
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
        public void DisplayEnumProperty()
        {
            var classWithEnumProperty = AssemblyWeaver.Assembly.GetType("AssemblyToProcess.ClassWithEnumProperty", true);
            var fullName = typeof(DebuggerDisplayAttribute).FullName;

            var debuggerDisplay = classWithEnumProperty.CustomAttributes.FirstOrDefault(t => t.AttributeType.FullName == fullName);
            var debuggerDisplayName = typeof(DebuggerDisplayAttribute).Name;
            Assert.IsNotNull(debuggerDisplay,
                             "'{0}' should be decorated with '{1}'.",
                             classWithEnumProperty.FullName,
                             debuggerDisplayName);

            Assert.AreEqual(1,
                            debuggerDisplay.ConstructorArguments.Count,
                            "'{0}' should have 1 ctor parameter.",
                            debuggerDisplayName);

            Assert.AreEqual("EnumProperty = {EnumProperty}",
                            debuggerDisplay.ConstructorArguments[0].Value);
        }
    }
}