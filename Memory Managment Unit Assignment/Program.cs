using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMU
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
    class Process
    {

        public int segmentnum;
        public string name;
        public Segment[] segment;
        public int counter;
        public void segmentallocation()
        {
            segment = new Segment[segmentnum];
            counter = 0;
        }

        public void AddSegment(Segment newSegment)
        {
            segment[counter] = newSegment;
            counter = counter + 1;
        }
    }
}
