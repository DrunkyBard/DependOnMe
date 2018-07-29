using CodeJam;
using DependOnMe.VsExtension.ContentTypeDefinition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DependOnMe.VsExtension.CodeGeneration
{
    [Guid("0FECB64A-8779-4A7B-B7CD-226DD6531FB1")] // copied from VSLangProj80.dll
    public abstract class VsContextGuids
    {
        public const string VsContextGuidVcsProject = "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}";
    }

    [ComVisible(true)]
    [CodeGeneratorRegistration(typeof(DrtGenerator), "DRT Class Generator", VsContextGuids.VsContextGuidVcsProject, GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(DrtGenerator))]
    public sealed class DrtGenerator : IVsSingleFileGenerator, IObjectWithSite
    {
        private const uint UndefinedLineOrColumn = 0xFFFFFFFF;

        private object _site;

        public void SetSite(object pUnkSite) => _site = pUnkSite;

        public void GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            if (_site == null)
            {
                throw new COMException("object is not sited", VSConstants.E_FAIL);
            }

            IntPtr pUnknownPointer = Marshal.GetIUnknownForObject(_site);
            Marshal.QueryInterface(pUnknownPointer, ref riid, out var intPointer);

            if (intPointer == IntPtr.Zero)
            {
                throw new COMException("site does not support requested interface", VSConstants.E_NOINTERFACE);
            }

            ppvSite = intPointer;
        }

        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = ".g.cs";
            
            return VSConstants.S_OK;
        }

        public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            Code.NotNull(wszDefaultNamespace, nameof(wszDefaultNamespace));
            Code.NotNull(wszInputFilePath, nameof(wszInputFilePath));
            Code.NotNull(bstrInputFileContents, nameof(bstrInputFileContents));

            var fileExtension = Path.GetExtension(wszInputFilePath);

            if (!fileExtension.Equals(ContentType.DotTest, StringComparison.OrdinalIgnoreCase) && 
                !fileExtension.Equals(ContentType.DotModule, StringComparison.OrdinalIgnoreCase))
            {
                pcbOutput = 0;
                GeneratorError(4, "DrtGenerator can be applied to .drt or .drm files", UndefinedLineOrColumn, UndefinedLineOrColumn, pGenerateProgress);

                return VSConstants.E_FAIL;
            }

            GenerationUnit genUnit;

            if (fileExtension.Equals(ContentType.DotTest, StringComparison.OrdinalIgnoreCase))
            {
                genUnit = CodeGenerator.GenerateDrt(bstrInputFileContents, wszDefaultNamespace, wszInputFilePath);
            }
            else
            {
                genUnit = CodeGenerator.GenerateDrm(bstrInputFileContents, wszDefaultNamespace, wszInputFilePath);
            }
            
            if (genUnit.HasErrors())
            {
                pcbOutput = 0;

                foreach (var error in genUnit.Errors)
                {
                    GeneratorError(4, error.Message, (uint)error.From.Line-1, (uint)error.From.Column, pGenerateProgress);
                }

                return VSConstants.E_FAIL;
            }
            
            // The contract between IVsSingleFileGenerator implementors and consumers is that 
            // any output returned from IVsSingleFileGenerator.Generate() is returned through  
            // memory allocated via CoTaskMemAlloc(). Therefore, we have to convert the 
            // byte[] array returned from GenerateCode() into an unmanaged blob.  

            var bytes                = genUnit.Src;
            int outputLength         = bytes.Length;
            rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(outputLength);
            Marshal.Copy(bytes, 0, rgbOutputFileContents[0], outputLength);
            pcbOutput = (uint)outputLength;
            
            return VSConstants.S_OK;
        }

        private void GeneratorError(uint level, string message, uint line, uint column, IVsGeneratorProgress pGenerateProgress) 
            => pGenerateProgress?.GeneratorError(0, level, message, line, column);
    }
}
