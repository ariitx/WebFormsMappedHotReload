using System.IO;
using EnvDTE;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

[assembly: InternalsVisibleTo("WebFormsMappedHotReloadTests")]
namespace WebFormsMappedHotReload
{
    internal class Helper
    {
        internal string GetFileType(Window window)
        {
            var documentFullName = window.Document?.FullName;

            if (documentFullName == null)
                documentFullName = window.Project?.FullName;

            if (Path.HasExtension(documentFullName))
                return Path.GetExtension(documentFullName).Replace(".", "");

            return "";
        }

    }
}
