using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using System.Collections.Generic;
#pragma warning disable VSTHRD010
namespace RolexDevstudio
{
    using Process = System.Diagnostics.Process;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("Rolex", "Generate C# lexers", ".4")]
    [Guid("C220FAA7-CA4A-48C9-BADB-E9A9AD87FDDE")]
    [ComVisible(true)]
    [ProvideObject(typeof(Rolex))]
    [CodeGeneratorRegistration(typeof(Rolex), "Gplex", "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}", GeneratesDesignTimeSource = true)]
    // if we supported VB for this we'd add the following, but Gplex only emits C#
    //[CodeGeneratorRegistration(typeof(Gplex),"Gplex", "{164b10b9-b200-11d0-8c61-00a0c91e29d5}",GeneratesDesignTimeSource =true)]
    public sealed class Rolex : IVsSingleFileGenerator, IObjectWithSite
    {
        object _site;
        Array _projects;
        ServiceProvider _serviceProvider;

        public Rolex()
        {
            EnvDTE.DTE dte;
            try
            {
                dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
                _projects = (Array)dte.ActiveSolutionProjects;
            }
            catch
            {
                dte = null;
                _projects = null;
            }

        }
        ProjectItem _FindItem(string path)
        {
            int iFound = 0;
            uint itemId = 0;
            EnvDTE.ProjectItem item;
            Microsoft.VisualStudio.Shell.Interop.VSDOCUMENTPRIORITY[] pdwPriority = new Microsoft.VisualStudio.Shell.Interop.VSDOCUMENTPRIORITY[1];
            for (var i = 0; i < _projects.Length; i++)
            {
                Microsoft.VisualStudio.Shell.Interop.IVsProject vsProject = VSUtility.ToVsProject(_projects.GetValue(i) as EnvDTE.Project);
                vsProject.IsDocumentInProject(path, out iFound, pdwPriority, out itemId);
                if (iFound != 0 && itemId != 0)
                {
                    Microsoft.VisualStudio.OLE.Interop.IServiceProvider oleSp = null;
                    vsProject.GetItemContext(itemId, out oleSp);
                    if (null != oleSp)
                    {
                        ServiceProvider sp = new ServiceProvider(oleSp);
                        // convert our handle to a ProjectItem
                        item = sp.GetService(typeof(EnvDTE.ProjectItem)) as EnvDTE.ProjectItem;
                        return item;
                    }

                }
            }
            return null;


        }
        #region IVsSingleFileGenerator Members

        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = ".cs";
            return pbstrDefaultExtension.Length;
        }
        ServiceProvider SiteServiceProvider {
            get {
                if (null == _site)
                    return null;
                if (null == _serviceProvider)
                {
                    var oleServiceProvider = _site as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
                    _serviceProvider = new ServiceProvider(oleServiceProvider);
                }
                return _serviceProvider;
            }
        }

        public int Generate(string wszInputFilePath, string bstrInputFileContents,
          string wszDefaultNamespace, IntPtr[] rgbOutputFileContents,
          out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            string outp = "";
            string errp = "";
            try
            {
                if (null == _site)
                    throw new InvalidOperationException("The Rolex custom tool can only be used in a design time environment. Consider using Rolex as a pre-build step instead.");
                wszInputFilePath = Path.GetFullPath(wszInputFilePath);
                var item = _FindItem(wszInputFilePath);
                if (null == item)
                    throw new ApplicationException("Design time environment project item fetch failed.");
                if (0 != string.Compare("cs", VSUtility.GetProjectLanguageFromItem(item), StringComparison.InvariantCultureIgnoreCase))
                    throw new NotSupportedException("The Rolex generator only supports C# projects");
                var dir = Path.GetDirectoryName(wszInputFilePath);
                var scannerFile = Path.GetFileNameWithoutExtension(wszInputFilePath) + ".cs";
                var proj = item.ContainingProject;
                var pil = new List<object>(proj.ProjectItems.Count);
                foreach(EnvDTE.ProjectItem pi in proj.ProjectItems)
                {
                    if (pi != item)
                        pil.Add(pi);
                    
                }
                var genShared = !VSUtility.HasClassOrStruct( pil, wszDefaultNamespace, "Token");
                pGenerateProgress.Progress(0, 2);
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "rolex";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.Arguments = "\"" + wszInputFilePath.Replace("\"", "\"\"") + "\"";
                psi.Arguments += " /output -";//\"" + Path.Combine(dir, scannerFile).Replace("\"", "\"\"") + "\"";
                psi.Arguments += " /namespace \"" + wszDefaultNamespace + "\"";
                if (!genShared)
                    psi.Arguments += " /noshared";
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                var isSuccess = false;
                using (var proc = new Process())
                {
                    proc.StartInfo = psi;
                    proc.Start();
                    outp = proc.StandardOutput.ReadToEnd().TrimEnd();
                    errp = proc.StandardError.ReadToEnd().TrimEnd();
                    if (!proc.HasExited)
                        proc.WaitForExit();
                    isSuccess = 0 == proc.ExitCode;
                }
                
                var outputPath = Path.Combine(dir, scannerFile);
                pGenerateProgress.Progress(1, 2);
                if (!isSuccess)
                {
                    _DumpErrors(errp,pGenerateProgress);
                    //pGenerateProgress.GeneratorError(0, 0, "Rolex failed: " + errp, unchecked((uint)-1), unchecked((uint)-1));
                } else
                {

                    // have used streams here in the past to scale, but even for huge items, this is faster!
                    // most likely due to the lack of extra copies (memory/array resizing)
                    byte[] bytes = Encoding.UTF8.GetBytes(outp);
                    int length = bytes.Length;
                    rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(length);
                    Marshal.Copy(bytes, 0, rgbOutputFileContents[0], length);
                    pcbOutput = (uint)length;
                    return VSConstants.S_OK;
                }
            }
            catch (Exception ex)
            {
                pGenerateProgress.GeneratorError(0, 0, "Rolex custom tool failed with: " + ex.Message, unchecked((uint)-1), unchecked((uint)-1));
                errp += "Rolex custom tool failed with: " + ex.Message;
            }
            finally
            {
                try
                {
                    pGenerateProgress.Progress(2, 2);
                }
                catch { }
            }
            
            // have used streams here in the past to scale, but even for huge items, this is faster!
            // most likely due to the lack of extra copies (memory/array resizing)
            pcbOutput = (uint)0;
            return VSConstants.S_OK;
        }

        #endregion

        static void _DumpErrors(string errp,IVsGeneratorProgress progress)
        {
            var tr = new StringReader(errp);
            var found = false;
            string line = null;
            while (null != (line = tr.ReadLine()))
            {
                var i = line.IndexOf('(');
                if (0 > i) continue;
                var fn = line.Substring(0, i);
                if (-1 < fn.IndexOfAny(Path.GetInvalidPathChars()))
                    continue;
                var i2 = line.IndexOf(')', i + 1);
                if (0 > i2) continue;
                if (line.Length <= i2 || ':' != line[i2 + 1])
                    continue;
                var s = line.Substring(i + 1, i2 - i - 2);
                var sa = s.Split(',');
                if (2 != sa.Length)
                    continue;
                int l = 0;
                int c = 0;
                if (!int.TryParse(sa[0].Trim(), out l) || !int.TryParse(sa[1].Trim(), out c))
                    continue;
                var i3 = line.IndexOf(' ', i2 + 1);
                if (0 > i3)
                    continue;
                var el = line.Substring(i2 + 1, i3 - (i2 + 1));
                int isWarning = 0;
                if (el.Equals(":warning", StringComparison.Ordinal))
                {
                    isWarning = 1;
                }
                else if (!el.Equals(":error", StringComparison.Ordinal))
                    continue;
                var i4 = line.IndexOf(':', i3 + 1);
                if (0 > i4)
                    continue;
                var ec = 0;
                s = line.Substring(i3 + 1, i4 - (i3 + 1));
                if (!int.TryParse(s, out ec))
                    continue;
                s = line.Substring(i4 + 1);
                found = true;
                if (null != progress)
                    progress.GeneratorError(isWarning, 0, s, unchecked((uint)l - 1), unchecked((uint)c));
            }
            if (!found)
                progress.GeneratorError(0, 0, errp, 0u, 0u);
        }
        #region IObjectWithSite Members

        public void GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            if (null == this._site)
            {
                throw new Win32Exception(-2147467259);
            }

            IntPtr objectPointer = Marshal.GetIUnknownForObject(this._site);

            try
            {
                Marshal.QueryInterface(objectPointer, ref riid, out ppvSite);
                if (ppvSite == IntPtr.Zero)
                {
                    throw new Win32Exception(-2147467262);
                }
            }
            finally
            {
                if (objectPointer != IntPtr.Zero)
                {
                    Marshal.Release(objectPointer);
                    objectPointer = IntPtr.Zero;
                }
            }
        }

        public void SetSite(object pUnkSite)
        {
            this._site = pUnkSite;
        }

        #endregion

    }
}
#pragma warning restore VSTHRD010