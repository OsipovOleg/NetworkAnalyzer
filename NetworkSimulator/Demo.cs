using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkDescriptions;
using System.IO;


namespace NetworkSimulator
{
    public static class Demo
    {

        public static void RunModel()
        {
            //Создание описания сети 
            DescriptionOFJQN Description = new DescriptionOFJQN(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/msu.txt");
            OFJQN.CreateNetworkModel(Description, new Random()).Run(1000000); 
            



        }
    }
}
