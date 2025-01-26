using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Azure;
using System.Threading;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Microsoft.Rest;
using System.Linq;
using ConsoleApp1.Labelme;

namespace ConsoleApp1
{
    internal class Program
    {

        static void Main(string[] args)
        {
            //(new Labelme_Main()).run(); //Build Project; Upload images; Train model; prediction
            (new Labelme_Main()).predict(); //prediction
            return;
        }
    }
}
