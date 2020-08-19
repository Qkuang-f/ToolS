using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace ConsoleApp1
{
    /// <summary>
    /// MD整理类，对越级标题进行整理修改。
    /// </summary>
    public class MDTidy
    {
        int lateHeadIndex = 0;
        string _INPUTText = string.Empty;
        int modifyNumber=0;
        public int Start(string path)
        {
            
            modifyNumber = 0;           //返回修改的个数
            string OverFilePaht = path.Substring(0, path.LastIndexOf('\\'))+"\\副本.md";

            ReadFileUsingReader(path);
            WriteFileUsingWriter(OverFilePaht, _INPUTText);

            return modifyNumber;
        }

        public void ReadFileUsingReader(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                   line =  OperatLine(line);
                    _INPUTText += line+"\n";
                }
            }
            stream.Close();
        }

        public string OperatLine(string line)
        {
            //操作每一行，若未标题，且不符合规则，则修改。
            if (IsHead(line))
            {
                if (HeadIndex(line) - lateHeadIndex > 1)
                {
                    modifyNumber++;
                    int deltaHead = HeadIndex(line) - lateHeadIndex;
                    Console.WriteLine(string.Format("较大：{0}", deltaHead));
                    //标题大于上一个标题。大于一正常，大于二就要修改，小于正常
                    line = line.Substring(line.IndexOf('#')+ deltaHead-1);
                }
                lateHeadIndex = HeadIndex(line);
            }

            return line;
        }

        public void WriteFileUsingWriter(string fileName, string lines)
        {
            var outputStream = File.OpenWrite(fileName);

            using (var writer = new StreamWriter(outputStream))
            {
                byte[] preamble = Encoding.UTF8.GetPreamble();
                outputStream.Write(preamble, 0, preamble.Length);
                writer.Write(lines);
                writer.Flush();
            }
            
            outputStream.Close();
        }

        private bool IsHead(string str)
        {
            //是否是标题
            string Parrten = @"^#";
            return Regex.IsMatch(str, Parrten);
        }

        public int HeadIndex(string str)
        {
            //标题级别

            var results = from c in str
                          where c == '#'
                          select c;

            return results.Count();
        }
    }
}
