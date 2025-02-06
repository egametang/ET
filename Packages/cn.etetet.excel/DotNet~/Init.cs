using System;

namespace ET
{
    [EnableClass]
    internal static class Init
    {
        private static int Main(string[] args)
        {
            try
            {
                NoCut.Run();
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