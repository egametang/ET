using System;

namespace ET
{
    internal static class Init
    {
        private static int Main(string[] args)
        {
            try
            {
                NoCut.Run();
                Proto2CS.Export();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("proto2cs ok!");
            return 1;
        }
    }
}