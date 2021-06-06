using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public struct Segment
{
    public string segment_name;
    public int segment_size;

};//defines Segment
struct Hole
{
    public int num;
    public int start;
    public int size;
};


namespace MMU
{
    public partial class Form1 : Form
    {
        int memory_size = 0;
        private readonly static Stack<int> HolesGone = new Stack<int>();
        int size, Holesum = 0, Holenum = 0, Oldnum = 0, flag = 0, holeflag = 1, pflag = 1, Deallocate = 0;
        Hole Holes;
        Segment SG;
        Process PS;
        List<Hole> Hole_List = new List<Hole>();
        List<Process> PS_List = new List<Process>();
        SortedList<int, Segment> Memory = new SortedList<int, Segment>();
        SortedList<int, Segment> Memory_External_Compaction= new SortedList<int, Segment>();

        public Form1()
        {
            InitializeComponent();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
       

        private void button_Enter_Click(object sender, EventArgs e)
        {
            memory_size = int.Parse(textMsize.Text);
            size = memory_size;
            if (size < 0)
            {
                label10.Visible = true;
                label10.Text = "Invalid Memory Size";
                label10.ForeColor = Color.Red;
                return;
            }
            else
            {
                label10.Visible = false;
                label10.Text = "";
            }

            enable_hole();

        }

        private void memory_display(ref SortedList<int, Segment> Memory)
        {
            flowLayoutPanel1.Visible = true;
            flowLayoutPanel2.Visible = true;
            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel2.Controls.Clear();
            int memory_ratio = memory_size;

            if (memory_size > 1500)
                memory_ratio = 1000;
            else if (memory_size > 500)
                memory_ratio = 500;

            for (int i = 0; i < Memory.Count; i++) 
            {
                // Memory.Keys[i] -> starting address of segment
                // Memory.Values[i].segment_name > segment name in memory
                Label label = new Label();
                Label num = new Label();
                label.ForeColor = Color.Beige;
               
                label.Text = Memory.Values[i].segment_name;
                num.Text = Memory.Keys[i].ToString();
                if (Memory.Values[i].segment_name.Contains("Hole"))
                {
                    label.BackColor = Color.Blue;
                }
                else if (Memory.Values[i].segment_name.Contains("Old"))
                {
                    label.BackColor = Color.Black;
                }
                else
                    label.BackColor = Color.DarkRed;
                
                if (i < Memory.Count - 1)
                {
                    label.Height = (Memory.Keys[i + 1] - Memory.Keys[i]) * 200 / memory_ratio;
                    if (label.Height < 20)
                        label.Height = 20;
                    num.Height = label.Height;
                }
                else if (Memory.Keys[i] < size - 20)
                {
                    label.Height = (size - Memory.Keys[i]) * 200 / memory_ratio;
                    if (label.Height < 20)
                        label.Height = 20;
                    num.Height = label.Height;
                }
                else
                {
                    label.Height = 20;
                    num.Height = label.Height;
                }
                flowLayoutPanel1.Controls.Add(label);
                flowLayoutPanel2.Controls.Add(num);
            }
            if(Memory.Keys[Memory.Count() - 1] != size)
            {
                Label num = new Label();
                num.Text = size.ToString();
                num.Height = 30;
                flowLayoutPanel2.Controls.Add(num);
            }
        }

        private void button_Addhole_Click(object sender, EventArgs e)
        {
            Oldnum = 0;

            Hole hole1 = new Hole();
            hole1.start = int.Parse(textBox1.Text);
            hole1.size = int.Parse(textBox2.Text);
            Holesum += hole1.size;
            hole1.num = Holenum;
            if ((hole1.start < 0) || (hole1.start > size) || (Hole_List.Any(hole => hole.start == hole1.start)))
            {
                label8.Visible = true;
                label8.Text = "Invalid Starting Address";
                label8.ForeColor = Color.Red;
                return;
            }
            else
            {
                label8.Visible = false;
                label8.Text = "";
            }
            if (hole1.size < 0 || (hole1.start + hole1.size) > size || Holesum > size)
            {
                if (Holes.size >= 0)
                {
                    Holesum = Holesum - Holes.size;
                }
                label11.Visible = true;
                label11.Text = "Invalid Hole Size";
                label11.ForeColor = Color.Red;
                return;
            }
            else
            {
                label11.Visible = false;
                label11.Text = "";
            }
            
            if (Hole_List.Count > 0)
            {
                for(int a = 0; a < Hole_List.Count; a++)
                {
                    bool flag = false;
                    if (a+1 < Hole_List.Count)
                    {
                        if (hole1.start == Hole_List[a].start + Hole_List[a].size && hole1.start + hole1.size == Hole_List[a + 1].start)
                        {
                            for (int k = 0; k < Memory.Count; k++)
                            {
                                if (Memory.Keys[k] == Hole_List[a].start)
                                {
                                    int old_address = Memory.Keys[k];
                                    int size = Memory.Values[k].segment_size + hole1.size + Memory.Values[k + 2].segment_size;
                                    Segment SG = new Segment();
                                    SG.segment_name = Memory.Values[k].segment_name;
                                    SG.segment_size = size;
                                    Memory.Remove(Memory.Keys[k]);
                                    Memory.Remove(Memory.Keys[k]);
                                    Memory.Remove(Memory.Keys[k]);
                                    Memory.Add(old_address, SG);
                                    flag = true;
                                }
                            }
                            hole1.start = Hole_List[a].start;
                            hole1.size += Hole_List[a].size + Hole_List[a+1].size;
                            Hole_List.Remove(Hole_List[a]);
                            Hole_List.Remove(Hole_List[a]);
                            Hole_List.Add(hole1);
                            Holenum--;
                            memory_display(ref Memory);
                            disable_hole();
                            return;
                        }

                    }
                    if (hole1.start == Hole_List[a].start + Hole_List[a].size && !flag) //fo2ya hole
                    {
                        int old_size = Hole_List[a].size;
                        int old_starting_size = Hole_List[a].start;
                        for (int k = 0; k < Memory.Count; k++)
                        {
                            if (Memory.Values[k].segment_name.Contains("Hole"))
                            {
                                if (Memory.Keys[k] + Memory.Values[k].segment_size == hole1.start)
                                {
                                    Segment SG = new Segment();
                                    Segment SG2 = new Segment();
                                    SG.segment_name = Memory.Values[k].segment_name;
                                    SG.segment_size = hole1.size + old_size;
                                    int key = Memory.Keys[k];

                                    if (k < Memory.Count)
                                    {
                                        SG2.segment_name = Memory.Values[k + 1].segment_name;
                                        SG2.segment_size = Memory.Values[k + 1].segment_size - hole1.size;
                                        int new_address = Memory.Keys[k + 1] + hole1.size;
                                        Memory.Remove(Memory.Keys[k]); // hole
                                        Memory.Remove(Memory.Keys[k]); // old process 

                                        Memory.Add(key, SG);
                                        if(new_address != memory_size)
                                            Memory.Add(new_address, SG2);
                                    }
                                    else
                                    {
                                        Memory.Remove(Memory.Keys[k]);
                                        Memory.Remove(Memory.Keys[k]);
                                        Memory.Add(key, SG);
                                    }
                                }
                            }
                        }

                        // Adjusting Hole_list
                        hole1.start = old_starting_size;
                        hole1.size += old_size;
                        Hole_List.Remove(Hole_List[a]);
                        Hole_List.Add(hole1);


                        memory_display(ref Memory);
                        disable_hole();
                        return;

                    }
                    else if (hole1.start + hole1.size == Hole_List[a].start && !flag)
                    {
                        int old_size = Hole_List[a].size;
                        for (int k = 0; k < Memory.Count; k++)
                        {
                            if (Memory.Values[k].segment_name.Contains("Hole"))
                            {
                                if (Memory.Keys[k] == hole1.start + hole1.size)
                                {
                                    Segment SG = new Segment();
                                    Segment SG2 = new Segment();
                                    SG.segment_name = Memory.Values[k].segment_name;
                                    SG.segment_size = hole1.size + old_size;

                                    if ( k >= 1)
                                    {
                                        SG2.segment_name = Memory.Values[k - 1].segment_name;
                                        SG2.segment_size = Memory.Values[k - 1].segment_size - hole1.size;
                                        int new_address = Memory.Keys[k - 1];
                                        Memory.Remove(Memory.Keys[k]); // hole
                                        Memory.Remove(Memory.Keys[k - 1]); // old process 

                                        Memory.Add(hole1.start, SG);
                                        if(SG2.segment_size != 0)
                                            Memory.Add(new_address, SG2);
                                    }
                                    else
                                    {
                                        Memory.Remove(Memory.Keys[k]);
                                        Memory.Remove(Memory.Keys[k - 1]);
                                        Memory.Add(hole1.start, SG);
                                    }
                                }
                            }
                        }

                        // Adjusting Hole_list
                        hole1.size += old_size;
                        Hole_List.Remove(Hole_List[a]);
                        Hole_List.Add(hole1);


                        memory_display(ref Memory);
                        disable_hole();
                        return;
                    }
                }
            }
            
            Holenum++;
            Hole_List.Add(hole1);

            Memory.Clear();
            PS_List.Clear();
            Hole_List.Sort((h1, h2) => h1.start.CompareTo(h2.start));

            int i = 0, memory_ratio = memory_size ;

            for (int k = 0; k < Holenum; k++)
            {
                if(Hole_List[0].start == 0 && k == 0)
                {
                    
                }
                else if (k == 0)
                {
                    PS = new Process();
                    SG.segment_name = "Old Process " + Oldnum.ToString();
                    SG.segment_size = Hole_List[k].start;
                    Memory.Add(0, SG);
                    PS.name = SG.segment_name;
                    PS.segmentnum = 1;
                    PS.segmentallocation();
                    PS.AddSegment(SG);
                    PS_List.Add(PS);
                    Oldnum = Oldnum + 1;

                }
                else if (Hole_List[k].start > (Hole_List[k - 1].start + Hole_List[k - 1].size))
                {
                    PS = new Process();
                    SG.segment_name = "Old Process " + Oldnum.ToString();
                    SG.segment_size = Hole_List[k].start - (Hole_List[k - 1].start + Hole_List[k - 1].size);
                    Memory.Add((Hole_List[k - 1].start + Hole_List[k - 1].size), SG);
                    PS.name = SG.segment_name;
                    PS.segmentnum = 1;
                    PS.segmentallocation();
                    PS.AddSegment(SG);
                    PS_List.Add(PS);
                    Oldnum = Oldnum + 1;
                }

                SG.segment_name = "Hole " + Hole_List[k].num.ToString();
                SG.segment_size = Hole_List[k].size;
                Memory.Add(Hole_List[k].start, SG);
                if (k == Holenum - 1)
                {
                    if ((Hole_List[k].start + Hole_List[k].size) < size)
                    {
                        PS = new Process();
                        SG.segment_name = "Old Process " + Oldnum.ToString();
                        SG.segment_size = size - (Hole_List[k].start + Hole_List[k].size);
                        Memory.Add((Hole_List[k].start + Hole_List[k].size), SG);
                        PS.name = SG.segment_name;
                        PS.segmentnum = 1;
                        PS.segmentallocation();
                        PS.AddSegment(SG);
                        PS_List.Add(PS);
                        Oldnum = Oldnum + 1;
                    }
                }
            }

            comboBox1.Items.Clear();
            foreach(Process p in PS_List)
                comboBox1.Items.Add(p.name);
            disable_hole();
            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel2.Controls.Clear();
            textBox1.Text = "";
            textBox2.Text = "";
            memory_display(ref Memory);

        }

        private void button1_Click(object sender, EventArgs e)  //Deallocate button
        {
            label12.Visible = false;
            label12.Text = "";
            int index = comboBox1.SelectedIndex;
            Deallocte(PS_List[index],ref Hole_List, Memory);
            PS_List.RemoveAt(index);
            comboBox1.SelectedIndex = -1;
            comboBox1.Items.Clear();
            foreach (Process p in PS_List)
            {
                comboBox1.Items.Add(p.name);
            }
                
            memory_display(ref Memory);
        }

        int segments_num = 0, processes_num = 0;
        string value = "";

        private void button2_Click(object sender, EventArgs e)
        {
            enable_hole();
            button1.Enabled = false;
            button2.Enabled = false;
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            textMsize.Clear();
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            comboBox1.SelectedIndex = -1;
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;

            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            button_Addhole.Enabled = false;
            button_Addprocess.Enabled = false;
            comboBox1.Enabled = false;
            deallocate_btn.Enabled = false;
            label9.Visible = true;
            button2.Visible = true;
            button1.Visible = true;
            button1.Enabled = false;
            button2.Enabled = false;

            textMsize.Enabled = true;
            button_Enter.Enabled = true;

            label8.Visible = false;
            label10.Visible = false;
            label11.Visible = false;
            label12.Visible = false;
            label13.Visible = false;
            label14.Visible = false;
            label15.Visible = false;


            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel2.Controls.Clear();
            PS_List.Clear();
            Hole_List.Clear();
            Memory.Clear();

            processes_num = 0;
            Holenum = 0;
            Oldnum = 0;
            Holesum = 0;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            disable_hole_enable_process();
        }

        private void button_Addprocess_Click(object sender, EventArgs e)
        {
            groupBox1.Enabled = false;
            if (!Hole_List.Any())
            {
                label12.Visible = true;
                label12.Text = "Memory Full please deallocate processes";
                label12.ForeColor = Color.Red;
                return;
            }

            Process PS = new Process();
            PS.name = "P" + processes_num;

            processes_num++;        

            segments_num = int.Parse(textBox3.Text);
            if (segments_num <= 0)
            {
                label13.Visible = true;
                label13.Text = "Invalid segments number";
                label13.ForeColor = Color.Red;
                return;
            }
            else
            {
                label13.Visible = false;
                label13.Text = "";
            }
            PS.segmentnum = segments_num;
            PS.segmentallocation();

            string textbox_string = textBox4.Text;
            string[] strings = textbox_string.Split(',');

            foreach (string x in strings)
            {
                string[] row_string = x.Split(':');

                Segment segmenti = new Segment();
                segmenti.segment_name =  "P" + (processes_num - 1) + ": " + row_string[0].Replace(" ", String.Empty);
                segmenti.segment_size = int.Parse(row_string[1]);
                if (segmenti.segment_size <= 0)
                {
                    label14.Visible = true;
                    label14.Text = "Invalid segments size";
                    label14.ForeColor = Color.Red;
                    return;
                }
                else
                {
                    label14.Visible = false;
                    label14.Text = "";
                }
                PS.AddSegment(segmenti);
            }


            if (radioButton1.Checked)
                value = radioButton1.Text;
            else if(radioButton2.Checked)
                value = radioButton2.Text;
            else if(radioButton3.Checked)
                value = radioButton3.Text;
            char c = value[0];
            if (!MemoryAllocation(PS, ref Hole_List, c, Memory))
            {
                Deallocte(PS, ref Hole_List, Memory);
                label15.Visible = true;
                label15.Text = "Sorry but process does not fit";
                label15.ForeColor = Color.Red;
                return;
            }
            else
            {
                PS_List.Add(PS);
                label15.Visible = false;
                label15.Text = "";
            }

            memory_display(ref Memory);
            comboBox1.Items.Clear();
            foreach (Process p in PS_List)
                comboBox1.Items.Add(p.name);

            textBox3.Text = "";
            textBox4.Text = "";

        }



        private void button3_Click(object sender, EventArgs e)
        {
            Memory_External_Compaction.Clear();
            
            bool flag = false;
            Hole_List.Sort((h1, h2) => h1.start.CompareTo(h2.start));
            int address = Hole_List[0].start;
            for (int i = 0, j = 0; i < Memory.Count; i++)
            {
                if (Memory.Values[i].segment_name.Contains("Hole"))
                {
                    // do nothing
                }
                else if (Memory.Keys[i] < Hole_List[0].start)
                {
                    SG.segment_name = Memory.Values[i].segment_name;
                    SG.segment_size = Memory.Values[i].segment_size;
                    Memory_External_Compaction.Add(Memory.Keys[i], SG);
                    j++;
                }
                else
                {
                    SG.segment_name = Memory.Values[i].segment_name;
                    SG.segment_size = Memory.Values[i].segment_size;
                    Memory_External_Compaction.Add(address, SG);
                    address += Memory_External_Compaction.Values[j].segment_size;
                    j++;
                }
            }

            int holes_size = 0;
            for (int i = 0; i < Hole_List.Count; i++)
            {
                holes_size += Hole_List[i].size;
            }
            SG.segment_name = "Hole 0";
            SG.segment_size = holes_size;
            Memory_External_Compaction.Add(address, SG);
            Memory.Clear();
            for(int i = 0; i < Memory_External_Compaction.Count; i++)
            {
                Segment SG = new Segment();
                SG.segment_name = Memory_External_Compaction.Values[i].segment_name;
                SG.segment_size = Memory_External_Compaction.Values[i].segment_size;
                Memory.Add(Memory_External_Compaction.Keys[i], SG);
            }
            Hole h = new Hole();
            h.start = address;
            h.size = holes_size;

            Hole_List.Clear();
            Hole_List.Add(h);

            memory_display(ref Memory_External_Compaction);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private bool MemoryAllocation(Process PS,ref List<Hole> Hole_List, char choice, SortedList<int, Segment> Mem)
         {
            bool isfit;
            Segment segmenttemp1;
            Hole holetemp1;

            if (choice == 'F')
                Hole_List = Hole_List.OrderBy(a => a.start).ToList();
            for (int i = 0; i < PS.segmentnum; i++)
            {
                isfit = false;
                if (choice == 'B')
                    Hole_List = Hole_List.OrderBy(a => a.size).ToList();
                else if (choice == 'W')
                    Hole_List = Hole_List.OrderByDescending(a => a.size).ToList();
                for (int j = 0; j < Hole_List.Count; j++)
                {
                    if (Hole_List[j].size >= PS.segment[i].segment_size)
                    {
                        isfit = true;
                        segmenttemp1.segment_name = PS.segment[i].segment_name;
                        segmenttemp1.segment_size = PS.segment[i].segment_size;
                        Mem[Hole_List[j].start] = segmenttemp1;
                        segmenttemp1.segment_size = Hole_List[j].size - PS.segment[i].segment_size;
                        if (segmenttemp1.segment_size == 0)
                        {
                            HolesGone.Push(Hole_List[j].num);
                            Hole_List.RemoveAt(j);
                        }
                        else
                        {
                            segmenttemp1.segment_name = "Hole " + Hole_List[j].num.ToString();
                            holetemp1.num = Hole_List[j].num;
                            holetemp1.start = Hole_List[j].start + PS.segment[i].segment_size;
                            holetemp1.size = segmenttemp1.segment_size;

                            Hole_List[j] = holetemp1;
                            Mem.Add(holetemp1.start, segmenttemp1);
                        }
                        break;
                    }
                }
                if (!isfit)
                    return false;
            }
            return true;
        }


        private void Deallocte(Process PS,ref List<Hole> Hole_List, SortedList<int, Segment> Mem)
        {
            Process lklk = new Process();
            int i;
            int Memindex;
            Segment segmenttemp1;
            Hole holetemp1;
            Hole_List.Sort((h1, h2) => h1.start.CompareTo(h2.start));
            for (i = (PS.segmentnum - 1); i >= 0; i = i - 1)
            {
                if (Mem.ContainsValue(PS.segment[i]))
                {
                    Memindex = Mem.IndexOfValue(PS.segment[i]); 
                    if (Memindex != 0 && Mem.Values[Memindex - 1].segment_name.Contains("Hole"))
                    {
                        var number = Hole_List.FindIndex(p => p.start == Mem.Keys[Memindex - 1]);

                        holetemp1.num = Hole_List[number].num;
                        holetemp1.start = Hole_List[number].start;
                        holetemp1.size = Hole_List[number].size + Mem.Values[Memindex].segment_size;
                        if ((Memindex + 1) != Mem.Count && Mem.Values[Memindex + 1].segment_name.Contains("Hole"))
                        {
                            holetemp1.size += Hole_List[number + 1].size;
                            Hole_List.RemoveAt(number + 1);
                            Mem.RemoveAt(Memindex + 1);
                        }
                        Hole_List[number] = holetemp1;
                        segmenttemp1.segment_name = Mem.Values[Memindex - 1].segment_name;
                        segmenttemp1.segment_size = holetemp1.size;
                        Mem[Mem.Keys[Memindex - 1]] = segmenttemp1;
                        Mem.RemoveAt(Memindex);
                    }
                    else if ((Memindex + 1) != Mem.Count && Mem.Values[Memindex + 1].segment_name.Contains("Hole"))
                    {
                        var number = Hole_List.FindIndex(p => p.start == Mem.Keys[Memindex + 1]);
                        holetemp1.num = Hole_List[number].num;
                        holetemp1.start = Mem.Keys[Memindex];
                        holetemp1.size = Hole_List[number].size + Mem.Values[Memindex].segment_size;
                        Hole_List[number] = holetemp1;
                        segmenttemp1.segment_name = Mem.Values[Memindex + 1].segment_name;
                        segmenttemp1.segment_size = holetemp1.size;
                        Mem[Mem.Keys[Memindex]] = segmenttemp1;
                        Mem.RemoveAt(Memindex + 1);
                    }
                    else
                    {
                        if (HolesGone.Any())
                        { holetemp1.num = HolesGone.Pop(); }
                        else
                            holetemp1.num = Hole_List.OrderByDescending(n => n.num).First().num + 1;
                        holetemp1.start = Mem.Keys[Memindex];
                        holetemp1.size = Mem.Values[Memindex].segment_size;
                        Hole_List.Add(holetemp1);
                        segmenttemp1.segment_name = "Hole " + holetemp1.num.ToString();
                        segmenttemp1.segment_size = Mem.Values[Memindex].segment_size;
                        Mem[Mem.Keys[Memindex]] = segmenttemp1;
                    }
                }
            }
            Hole_List.Sort((h1, h2) => h1.start.CompareTo(h2.start));
        }


        private void enable_hole()
        {
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            button_Addhole.Enabled = true;
            button_Clear.Enabled = true;
            textMsize.Enabled = false;
            button_Enter.Enabled = false;
        }

        private void disable_hole()
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            button_Addhole.Enabled = false;
            button1.Enabled = true;
            button2.Enabled = true; 
        }

        private void disable_hole_enable_process()
        {
            label9.Visible = false;
            button1.Visible = false;
            button2.Visible = false;
            groupBox1.Enabled = true;

            textBox3.Enabled = true;
            textBox4.Enabled = true;
            button_Addprocess.Enabled = true;
            comboBox1.Enabled = true;
            deallocate_btn.Enabled = true;
            button3.Enabled = true;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
 