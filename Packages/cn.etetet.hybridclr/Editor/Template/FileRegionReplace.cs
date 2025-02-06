using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Editor.Template
{
    public class FileRegionReplace
    {
        private readonly string _tplCode;

        private readonly Dictionary<string, string> _regionReplaceContents = new Dictionary<string, string>();

        public FileRegionReplace(string tplCode)
        {
            _tplCode = tplCode;
        }

        public void Replace(string regionName, string regionContent)
        {
            _regionReplaceContents.Add(regionName, regionContent);
        }

        public string GenFinalString()
        {
            string originContent = _tplCode;

            string resultContent = originContent;

            foreach (var c in _regionReplaceContents)
            {
                resultContent = ReplaceRegion(resultContent, c.Key, c.Value);
            }
            return resultContent;
        }

        public void Commit(string outputFile)
        {
            string dir = Path.GetDirectoryName(outputFile);
            Directory.CreateDirectory(dir);
            string resultContent = GenFinalString();
            var utf8WithoutBOM = new System.Text.UTF8Encoding(false);
            File.WriteAllText(outputFile, resultContent, utf8WithoutBOM);
        }

        public static string ReplaceRegion(string resultText, string region, string replaceContent)
        {
            int startIndex = resultText.IndexOf("//!!!{{" + region);
            if (startIndex == -1)
            {
                throw new Exception($"region:{region} start not find");
            }
            int endIndex = resultText.IndexOf("//!!!}}" + region);
            if (endIndex == -1)
            {
                throw new Exception($"region:{region} end not find");
            }
            int replaceStart = resultText.IndexOf('\n', startIndex);
            int replaceEnd = resultText.LastIndexOf('\n', endIndex);
            if (replaceStart == -1 || replaceEnd == -1)
            {
                throw new Exception($"region:{region} not find");
            }
            if (resultText.Substring(replaceStart, replaceEnd - replaceStart) == replaceContent)
            {
                return resultText;
            }
            resultText = resultText.Substring(0, replaceStart) + "\n" + replaceContent + "\n" + resultText.Substring(replaceEnd);
            return resultText;
        }
    }
}
