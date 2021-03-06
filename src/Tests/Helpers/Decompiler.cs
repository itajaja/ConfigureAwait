﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace Tests.Helpers
{
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
                var projectFolder = Path.GetFullPath(Path.GetDirectoryName(assemblyPath) + "\\..\\..\\..\\..").Replace("\\", "\\\\") + "\\\\";

                process.WaitForExit(10000);

                return string.Join(Environment.NewLine, Regex.Split(process.StandardOutput.ReadToEnd(), Environment.NewLine)
                        .Where(l => !l.StartsWith("// ") && !string.IsNullOrEmpty(l)) // Ignore comments and blank lines
                        .Select(l => l.Replace(projectFolder, ""))
                        .ToList());
            }
        }

        private static string GetPathToILDasm()
        {
            var path = Path.Combine(ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.Version40), @"bin\NETFX 4.0 Tools\ildasm.exe");
            if (!File.Exists(path))
                path = path.Replace("v7.0", "v8.0");
            if (!File.Exists(path))
                Assert.Ignore("ILDasm could not be found");
            return path;
        }
    }
}