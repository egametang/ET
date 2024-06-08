using System;

namespace ET
{
    internal static class Init
    {
        private static int Main(string[] args)
        {
            try
            {
                ExcelExporter.Export();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("excelexporter ok!");
            return 1;
        }
    }
}