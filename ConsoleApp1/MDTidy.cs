using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace Tool.MD
{

    /// <summary>
    /// MD整理类，对越级标题进行整理修改。
    /// </summary>
    public class MDTidy
    {

        string _INPUTText = string.Empty;       //修改后的字符串
        int modifyNumber = 0;     // 修改次数

        int currentHeadIndex = 0; // 当前标题级别——未修改前
        int RootClamp = 10;
        Subsidiary subsidiary;

        public int Start(string path)
        {
            modifyNumber = 0;           //返回修改的个数
            subsidiary = new Subsidiary();
            string OverFilePaht = path.Substring(0, path.LastIndexOf('\\')) + "\\副本.md";

            ReadFileUsingReader(path);
            WriteFileUsingWriter(OverFilePaht, _INPUTText);

            return modifyNumber;
        }

        private void ReadFileUsingReader(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            using (var reader = new StreamReader(stream))
            {

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    line = OperatLine(line);
                    _INPUTText += line + "\n";
                }
            }
            stream.Close();
        }

        private string OperatLine(string line)
        {
            //操作每一行，若未标题，且不符合规则，则修改。
            if (IsHead(line))
            {

                this.currentHeadIndex = HeadIndex(line);
                if(RootClamp == 10)
                {
                    //说明读取的是第一个个标题
                    SetRootHead();
                    return line;
                }

                // 未修改级别和前一个已修改级别比较。 确定是否修改
                //  未修改级别和前一个未修改级别比较，确定修改成相对程度。
                // 未修改级别和前一个已修改级别比较，确定修改绝对程度。
                if (this.currentHeadIndex - this.RootClamp > 0)
                {
                    int targetHead =  subsidiary.ParentHead(this.currentHeadIndex)+1;
                    int delta =  this.currentHeadIndex- targetHead;

                    line = line.Substring(line.IndexOf('#') +delta);

                    // Console.WriteLine(string.Format("较大：{0}", delta));
                    modifyNumber = delta <= 0 ?modifyNumber: modifyNumber+1;
                  

                    subsidiary.Add(this.currentHeadIndex, targetHead);
                }
                else
                {
                    SetRootHead();
                }
     
            }

            return line;
        }

        private void WriteFileUsingWriter(string fileName, string lines)
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

        private void SetRootHead()
        {
            this.RootClamp = this.currentHeadIndex;
            this.subsidiary.ToRoot(this.currentHeadIndex);
        }
    }

    public class Subsidiary
    {
        // 上一版简单的逻辑处理，让越级标题修改。但是不保证树（标题）的结构
        // 这里结合数据结构后，简单许多
        List<Head> headBuff;    // 用于缓存标题的记录，每一次根读取算一次

        public int lasteIndex;
        public Subsidiary()
        {
            headBuff = new List<Head>();
        }
        /// <summary>
        /// 在每次Root的Buff中，追加节点
        /// </summary>
        /// <param name="oldhead">追加标题修改前标题级别</param>
        /// <param name="target">追加标题修改后级别</param>
        /// <param name="isroot">是否是Root</param>
        /// <returns></returns>
        public Head Add(int oldhead,int target,bool isroot = false)
        {

            Head result = new Head(oldhead, target, isroot); ;
            if (lasteIndex >= headBuff.Count - 1)
            {

                headBuff.Add(result);
                lasteIndex++;
            }
            else
            {
                headBuff[++lasteIndex]=result;
                
            }

            return result;
        }

        /// <summary>
        /// 重置Buff，要求这是新的Root
        /// </summary>
        /// <param name="roothead">Root的标题级别</param>
        public void ToRoot(int roothead)
        {
   
            lasteIndex = -1;
            Add(roothead, roothead, true);
        }
        /// <summary>
        /// 根据当前标题，寻找到标题的父级节点的已修改标题级别
        /// -1 :意料之外的错误
        /// </summary>
        /// <param name="currentHead"></param>
        /// <returns></returns>
        public int ParentHead(int currentHead)
        {

            int result = -1;
            for(int i = lasteIndex; i >-1; i--)
            {
                if (currentHead - headBuff[i].oldHeadIndex > 0||i==0)
                {

                    result = headBuff[i].targetHeadIndex;
                    break;
                }
            }
            return result;
        } 
    }

    public struct Head
    {
        // 这里可以删除掉isRoot属性。因为每次Buff，第一个肯定为root。保留因为方便监视变量
        public  bool isRoot;
        public int oldHeadIndex;
        public int targetHeadIndex;
        public Head(int oldHeadIndex,int targetHeadIndex, bool isRoot = false)
        {
            this.isRoot = isRoot;
            this.oldHeadIndex = oldHeadIndex;
            this.targetHeadIndex = targetHeadIndex;
        }
    }
}
