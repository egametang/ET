using System;

namespace ET
{
    internal static class Init
    {
        private static int Main(string[] args)
        {
            try
            {
                Proto2CS.Export();
            }
            catch (Exception e)
            {
                Log.Console(e.ToString());
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("proto2cs ok!");
            return 1;
        }
    }
}