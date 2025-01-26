using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Labelme.Entities
{
    public class LabelmeBBoxJson
    {
        public List<Category> categories { get; set; }
        public List<Image> images { get; set; }
        public List<Annotation> annotations { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Image
    {
        public int id { get; set; }
        public string file_name { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Annotation
    {
        public int image_id { get; set; }
        public List<double> bbox { get; set; }
        public int category_id { get; set; }
        public int id { get; set; }
        public int iscrowd { get; set; }
        public double area { get; set; }
        public int ignore { get; set; }
    }
}
