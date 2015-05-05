﻿using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Examples
{
    public static class PEVerify
    {
        static PEVerify()
        {
            CopyBuildOutputsLocally("Protobuf.Tests");
            CopyBuildOutputsLocally("protobuf_net");
        }
        private static void CopyBuildOutputsLocally(string project)
        {
            var path = Path.Combine("../artifacts/bin", project,
#if DEBUG
                "Debug"
#elif RELEASE
                "Release"
#else
                ""
#error Unknown build
#endif
                ,
#if DNX451
                "dnx451"
#elif DNXCORE50
                "dnxcore50"
#else
                ""
#error Unknown runtime
#endif
                );
            foreach (var file in Directory.GetFiles(path))
            {
                File.Copy(file, Path.Combine("TestFiles", Path.GetFileName(file)), true);
            }
        }
        public static bool AssertValid(string path)
        {
#if DNXCORE50
            Assert.True(true);
            return true;
#else
            // note; PEVerify can be found %ProgramFiles%\Microsoft SDKs\Windows\v6.0A\bin
            const string exePath = @"%ProgramFiles(x86)%\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\PEVerify.exe";
            string fullPath = Environment.ExpandEnvironmentVariables(exePath);
            if (!File.Exists(fullPath))
                Assert.Equal("peverify exists", "peverify does not exist");
            var testPath = Path.Combine(Environment.CurrentDirectory, "TestFiles", path);
            var psi = new ProcessStartInfo(Path.GetFileName(fullPath), testPath);
            psi.WorkingDirectory = Path.GetDirectoryName(fullPath);
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            using (Process proc = Process.Start(psi))
            {
                if (proc.WaitForExit(10000))
                {
                    if (proc.ExitCode != 0)
                    {
                        Console.WriteLine("PEVerify failed on " + testPath);
                    }
                    Assert.Equal(0, proc.ExitCode); //, path);
                    return proc.ExitCode == 0;
                }
                else
                {
                    proc.Kill();
                    throw new TimeoutException();
                }
            }
#endif
        }
    }
}
