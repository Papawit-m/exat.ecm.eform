using Aspose.Words.Replacing;
using Aspose.Words;
using EXAT.ECM.Business.Models;
using EXAT.ECM.Business.Configurations;
using System;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace EXAT.ECM.Business.Helper
{
    public class AsposeHelper : Aspose.Words.Document
    {
        private readonly AsposeOption _asposeOption;
        private readonly IWebHostEnvironment _environment;

        #region 

        public AsposeHelper(IWebHostEnvironment environment)
        {
            _environment = environment;
            SetLicense();
        }

        public AsposeHelper(string fileName
                                , AsposeOption asposeOption
                                , IWebHostEnvironment environment) : base(fileName)
        {
            _asposeOption = asposeOption;
            _environment = environment;
            SetLicense();

        }
        public AsposeHelper(Stream stream
                                , AsposeOption asposeOption
                                , IWebHostEnvironment environment) : base(stream)
        {
            _asposeOption = asposeOption;
            _environment = environment;
            SetLicense();
        }

        public AsposeHelper(string fileName
                                , AsposeOption asposeOption
                                , IWebHostEnvironment environment
                                , Aspose.Words.Loading.LoadOptions loadOptions = null) : base(fileName, loadOptions)
        {
            _asposeOption = asposeOption;
            _environment = environment;
            SetLicense();
        }
        public AsposeHelper(Stream stream
                                , AsposeOption asposeOption
                                , IWebHostEnvironment environment
                                , Aspose.Words.Loading.LoadOptions loadOptions = null) : base(stream, loadOptions)
        {
            _asposeOption = asposeOption;
            _environment = environment;
            SetLicense();
        }

        public int ReplaceHtml(Node node, string pattern, string replacement, Aspose.Words.Replacing.FindReplaceOptions options = null)
        {
            if (options == null) return base.Document.Range.Replace(pattern, replacement);
            return base.Document.Range.Replace(pattern, replacement, options);
        }

        public Aspose.Words.Saving.SaveOutputParameters SaveWord(Stream stream, Aspose.Words.SaveFormat saveFormat)
        {
            return base.Save(stream, saveFormat);
        }

        public Aspose.Words.Saving.SaveOutputParameters Save(Stream stream, string fileName)
        {
            Aspose.Words.SaveFormat saveFormat = new Aspose.Words.SaveFormat();
            switch (Path.GetExtension(fileName).Trim().Replace(".", "").ToLower())
            {
                case "doc": saveFormat = SaveFormat.Doc; break;
                case "docx": saveFormat = SaveFormat.Docx; break;

                case "xls":
                case "xlsx": saveFormat = SaveFormat.Xlsx; break;

                case "pdf": saveFormat = SaveFormat.Pdf; break;
            }
            return base.Save(stream, saveFormat);
        }
        #endregion

        public void MergeWord(System.IO.MemoryStream stream, Aspose.Words.Document document, SaveFormat saveFormat)
        {
            Aspose.Words.Document sorce = new Document(stream);
            sorce.AppendDocument(document, ImportFormatMode.KeepSourceFormatting);
            sorce.Save(stream, saveFormat);
        }

        private void SetLicense()
        {
            /// TODO:: Load Fonts custom
            string licensePath = string.Format("{0}/{1}", this._environment.ContentRootPath, _asposeOption.LicensePath);

            Aspose.Words.License lic = new Aspose.Words.License();
            using (System.IO.FileStream fs = System.IO.File.OpenRead(licensePath))
            { lic.SetLicense(fs); }
        }

        #region ReplaceWithHtml
        public class ReplaceWithHtml : IReplacingCallback
        {
            internal ReplaceWithHtml(FindReplaceOptions options)
            {
                mOptions = options;
            }

            ReplaceAction IReplacingCallback.Replacing(ReplacingArgs args)
            {
                DocumentBuilder builder = new DocumentBuilder((Document)args.MatchNode.Document);
                builder.MoveTo(args.MatchNode);
                builder.InsertHtml(args.Replacement);
                args.Replacement = "";
                return ReplaceAction.Replace;
            }

            private readonly FindReplaceOptions mOptions;
        }
        #endregion
    }

    public class AsposeHelperOption : Aspose.Words.Loading.LoadOptions
    {
        private readonly AsposeOption _asposeOption;
        private readonly IWebHostEnvironment _environment;

        public AsposeHelperOption(AsposeOption asposeOption
                                 , IWebHostEnvironment environment)
        {
            _asposeOption = asposeOption;
            _environment = environment;
        }

        public Aspose.Words.Loading.LoadOptions option()
        {
            /// TODO:: Load Fonts custom
            string fontsPath = string.Format("{0}/{1}", this._environment.ContentRootPath, _asposeOption.FontsPath);

            Aspose.Words.Fonts.FontSettings fontSettings = new Aspose.Words.Fonts.FontSettings();
            fontSettings.SetFontsSources(new Aspose.Words.Fonts.FontSourceBase[]{
                        new Aspose.Words.Fonts.FolderFontSource(fontsPath, true, 1),
                        new Aspose.Words.Fonts.SystemFontSource(0),}
                    );
            Aspose.Words.Loading.LoadOptions loadOptions = new Aspose.Words.Loading.LoadOptions();
            loadOptions.FontSettings = fontSettings;
            return loadOptions;
        }

    }
}
