using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinTimesheet.Models
{
    public class Operation
    {
        public int EmployeeID { get; set; }
        public int? CustomerID { get; set; }
        public int WorkAssidnmentID { get; set; }
        public string OperationType { get; set; }

        public string Comment { get; set; }

        public string Longitude { get; set; }
        public string Latitude { get; set; }
    }
}
