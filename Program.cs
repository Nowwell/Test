using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CheckFieldPopulation.com.salesforce.my.cs92.aecom.epm4;
using System.Threading;
using System.Net;
using System.IO;

namespace CheckFieldPopulation
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            SforceService service = new SforceService();

            LoginResult lr = service.login("sean.fife@consultant.aecom.com.epm4", "Donkey1979!");

            service.SessionHeaderValue = new SessionHeader();
            service.SessionHeaderValue.sessionId = lr.sessionId;
            service.Url = lr.serverUrl;

            DescribeGlobalResult dgr = service.describeGlobal();

            using (StreamWriter sw = new StreamWriter(@"C:\Users\SeanFife\Desktop\ePM_pse\psePopulation.csv"))
            {
                sw.WriteLine("Object, Field, Count");

                foreach (DescribeGlobalSObjectResult s in dgr.sobjects)
                {
                    DescribeSObjectResult sdor = service.describeSObject(s.name);

                    foreach (Field f in sdor.fields)
                    {
                        if (f.name.StartsWith("pse__"))
                        {
                            try
                            {
                                QueryResult qr = service.query("SELECT count(" + f.name + ") FROM " + s.name + " WHERE " + f.name + "!= null");

                                while (qr.done == false)
                                {
                                    Thread.Sleep(2000);
                                    qr = service.queryMore(qr.queryLocator);

                                }

                                AggregateResult ar = ((AggregateResult)qr.records[0]);
                                Console.WriteLine(s.name + "." + f.name + " = " + ar.Any[0].InnerText);
                                sw.WriteLine(string.Format("{0}, {1}, {2}", s.name, f.name, ar.Any[0].InnerText));
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine(s.name + "." + f.name + " = unable to determine from field type");
                                sw.WriteLine(string.Format("{0}, {1}, {2}", s.name, f.name, "Error - \"" + e.Message + "\""));
                            }
                        }
                    }
                }

                sw.Flush();
                sw.Close();
            }
            service.logout();

            Console.ReadKey();
        }
    }
}
