using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public static class DebuggerDisplayInjector
{
    public static void AddDebuggerDisplayAttributes(ModuleDefinition moduleDefinition, TypeDefinition type)
    {
        if (type.IsEnum || type.CustomAttributes.Any(c => c.AttributeType.Name == "CompilerGeneratedAttribute" || c.AttributeType.Name == "DebuggerDisplayAttribute"))
            return;

        var members = (from t in EnumerateBaseTypes(type)
                       let fields = t.Fields.Where(f => f.IsPublic && f.DeclaringType == t && CanPrint(f.FieldType))
                       let props = from p in t.Properties
                                   where p.DeclaringType == t
                                   && p.GetMethod != null && p.GetMethod.IsPublic
                                   && !p.HasParameters
                                   && CanPrint(p.PropertyType)
                                   select p
                       from m in fields.Cast<MemberReference>().Concat(props)
                       group m by m.Name into g
                       select g.First()).ToList();

        var displayBits = members
            .OrderBy(m => m.Name)
            .Select(m => string.Format("{0} = {{{0}}}", m.Name));

        if (displayBits.Any())
        {
            var debuggerDisplay = new CustomAttribute(ReferenceFinder.DebuggerDisplayAttributeCtor);
            debuggerDisplay.ConstructorArguments.Add(new CustomAttributeArgument(moduleDefinition.TypeSystem.String, string.Join(" | ", displayBits)));
            type.CustomAttributes.Add(debuggerDisplay);
        }
    }

    private static IEnumerable<TypeDefinition> EnumerateBaseTypes(TypeDefinition typeDefinition)
    {
        if (IsDelegate(typeDefinition) || typeDefinition.IsEnum || typeDefinition.IsInterface)
            yield break;

        while (typeDefinition.BaseType != null)
        {
            yield return typeDefinition;
            typeDefinition = typeDefinition.BaseType.Resolve();
        }
    }

    private static bool IsDelegate(TypeDefinition typeDefinition)
    {
        return typeDefinition.BaseType != null && typeDefinition.BaseType.FullName == typeof(MulticastDelegate).FullName;
    }

    private readonly static HashSet<string> basicNames = new HashSet<string>
    {
        typeof (short).Name,
        typeof (ushort).Name,
        typeof (int).Name,
        typeof (uint).Name,
        typeof (long).Name,
        typeof (ulong).Name,
        typeof (float).Name,
        typeof (double).Name,
        typeof (bool).Name,
        typeof (byte).Name,
        typeof (sbyte).Name,
        typeof (char).Name,
        typeof (string).Name,
    };

    private static bool CanPrint(TypeReference typeReference)
    {
        if (basicNames.Contains(typeReference.Name))
            return true;

        if (typeReference.IsArray)
            return false;

        //var typeDefinition = typeReference.Resolve();

        //if (typeDefinition.IsEnum)
        //    return true;

        if (typeReference.IsGenericInstance && typeReference.Name == "Nullable`1")
        {
            var genericType = (GenericInstanceType)typeReference;
            return basicNames.Contains(genericType.GenericArguments[0].Name);
        }

        return false;
    }
}