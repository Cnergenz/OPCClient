using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Opc.Ua;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace OPCCllientExample
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            OPC _OPC = new OPC();
            await _OPC.ConnectOPC("192.168.1.5", "4840");
            int num1 = _OPC.ReadMode("ns=4;s=|var|AX-364ELA0MA1T.Application.GVL.num1");
            Console.WriteLine(num1);
            _OPC.AddMonitoredItem("ns=4;s=|var|AX-364ELA0MA1T.Application.GVL.num1", "num1");
            _OPC.AddMonitoredItem("ns=4;s=|var|AX-364ELA0MA1T.Application.GVL.num2", "num2");
            _OPC.Subscribe();
            Console.ReadKey();
        }
    }
}
