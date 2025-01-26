using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Labelme.Entities
{

    public class LabelmeJson
    {
        public string version { get; set; }
        //public Flags flags { get; set; }
        public List<Shape> shapes { get; set; }
        public string imagePath { get; set; }
        public string imageData { get; set; }
        public int imageHeight { get; set; }
        public int imageWidth { get; set; }
    }


    public class Shape
    {
        public string label { get; set; }
        public List<List<double>> points { get; set; }
        public string group_id { get; set; }
        public string description { get; set; }
        public string shape_type { get; set; }
        //public Flags flags { get; set; }
        public string mask { get; set; }
    }
}
