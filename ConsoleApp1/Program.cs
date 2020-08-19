using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QkuangLibrary;


namespace ConsoleApp1
{


    class Program
    {
       
        static void Main(string[] args)
        {
            Console.WriteLine("输入需要整理的路径……");
            string path = Console.ReadLine();
            MDTidy md = new MDTidy();

             Console.WriteLine(string.Format("修改次数：{0}", md.Start(path)));

           

        }

    }
}
