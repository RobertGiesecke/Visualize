using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using NUnit.Framework;

public static class Decompiler
{
    public static string Decompile(string assemblyPath, string identifier = "")
    {
        var exePath = GetPathToILDasm();

        if (!string.IsNullOrEmpty(identifier))
            identifier = "/item:" + identifier;

        using (var process = Process.Start(new ProcessStartInfo(exePath, String.Format("\"{0}\" /text /linenum {1}", assemblyPath, identifier))
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }))
        {
            var projectFolder = Path.GetFullPath(Path.GetDirectoryName(assemblyPath) + "\\..\\..\\..").Replace("\\", "\\\\");
            projectFolder = String.Format("{0}{1}\\\\", Char.ToLower(projectFolder[0]), projectFolder.Substring(1));

            process.WaitForExit(10000);

            return string.Join(Environment.NewLine, Regex.Split(process.StandardOutput.ReadToEnd(), Environment.NewLine)
                    .Where(l => !l.StartsWith("// ") && !string.IsNullOrEmpty(l))
                    .Select(l => l.Replace(projectFolder, ""))
                    .ToList());
        }
    }

    private static string GetPathToILDasm()
    {
        var pathToDotNetFrameworkSdk = ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.Version40);

        var path = (from fileName in Directory.GetFiles(pathToDotNetFrameworkSdk, "ildasm.exe", SearchOption.AllDirectories)
                    let fileInfo = new FileInfo(fileName)
                    let fileVersion = FileVersionInfo.GetVersionInfo(fileName)
                    orderby !fileInfo.FullName.ToLowerInvariant().Contains("netfx 4.0"),
                            fileVersion.ProductVersion descending,
                            !fileInfo.FullName.ToLowerInvariant().Contains("x64"),
                            fileInfo.CreationTimeUtc descending
                    select fileName).FirstOrDefault();
        if (path == null)
            Assert.Ignore("ILDasm could not be found");
        return path;
    }
}