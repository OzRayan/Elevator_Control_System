using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data;


namespace _1926120_AssignmentOne
{

    public partial class Form1 : Form
    {
        // PictureBox 1 width and height as reference
        public int width, height;
        // Floor where the elevator is currently
        public int floor = 0;
        // Floor to go
        public int toGo = 0;
        // Takes valuse 1 or -1 for door animation, default 1
        public int multiplier = 1;
        // Dictionary for each level Y position of groupBox
        public Dictionary<int, int> controlTopPositions = new Dictionary<int, int> { { 690, 0 }, { 470, 1 }, { 250, 2 }, { 30, 3 } };
        // Thestate of door animation
        public bool open = false;

        // Graphics
        // Level numbering
        public Font floorFont;
        // Line colour
        public Pen linePen;
        // Numbering colour, floor colour, floor margin colour
        public Brush floorNumberBrush, floorBrush, floorMarginBrush;
        // Each level wall colour
        public Dictionary<int, Brush> floorBrushes = new Dictionary<int, Brush> {
            { 0, Brushes.Khaki }, { 1, Brushes.SkyBlue }, { 2, Brushes.LightGreen }, { 3, Brushes.LightPink } };
        // Bitmaps used for Graphics, later on added to pictureBoxes: main ( elevator), left wall with the button and nr
        // right wall
        public Bitmap mainBmp, leftBmp, rightBmp;
        // Graphicss object to draw shapes
        public Graphics g, gLeft, gRight;
        // Timers for control and door animation
        public Timer tmrControl = new Timer();
        public Timer tmrFloor_0 = new Timer();
        public Timer tmrFloor_1 = new Timer();
        public Timer tmrFloor_2 = new Timer();
        public Timer tmrFloor_3 = new Timer();

        //Context context = new Context(new Floor0(), new Form1());

        private string dbconnection = "Provider=Microsoft.ACE.OLEDB.12.0;" + @"data source =Log.accdb";
        private string dbcommand = "Select * from Log;";

        public OleDbDataAdapter adapter;
        public OleDbConnection conn;
        public OleDbCommand comm;
        public DataSet ds;

        public Message msg = new Message();

        public Form1()
        {
            InitializeComponent();
        }

        private DataTable dbConnection()
        {
            try
            {
                conn = new OleDbConnection(dbconnection);
                comm = new OleDbCommand(dbcommand, conn);
                adapter = new OleDbDataAdapter(comm);
                ds = new DataSet();
                adapter.Fill(ds);
            } catch (OleDbException e)
            {
                MessageBox.Show(e.ToString());
            }

            return ds.Tables[0];
        }

        public void createTable()
        {
            DataTable record = new DataTable("Log");

            DataColumn logID = new DataColumn();
            logID.DataType = Type.GetType("System.Int32");
            logID.ColumnName = "LogID";
            logID.AllowDBNull = false;
            logID.Unique = true;
            logID.AutoIncrement = true;

            DataColumn logFrom = new DataColumn();
            logFrom.DataType = Type.GetType("System.Int32");
            logFrom.ColumnName = "LevelFrom";
            logFrom.AllowDBNull = false;

            DataColumn logTo = new DataColumn();
            logTo.DataType = Type.GetType("System.Int32");
            logTo.ColumnName = "LevelTo";
            logTo.AllowDBNull = false;

            DataColumn atDate = new DataColumn();
            atDate.DataType = Type.GetType("System.DateTime");
            atDate.ColumnName = "createdAt";
            atDate.AllowDBNull = false;

            record.Columns.Add(logID);
            record.Columns.Add(logFrom);
            record.Columns.Add(logTo);
            record.Columns.Add(atDate);

            DataColumn[] pk = new DataColumn[1];
            pk[0] = record.Columns["logID"];
            record.PrimaryKey = pk;

            // DataTable dt = dbConnection();
            string cmd = "Create table Log('LogID' integer not null Primary Key, 'LevelFrom' integer, 'LevelTo' integer, 'createAt' datetime);";
            conn = new OleDbConnection(dbconnection);
            //comm = new OleDbCommand(dbcommand, conn);
            //adapter = new OleDbDataAdapter(comm);
            ds = new DataSet();
            OleDbCommand cm = new OleDbCommand(cmd, conn);

            try
            {
                conn.Open();
                int val = (int)cm.ExecuteNonQuery();
            } catch (OleDbException e)
            {
                MessageBox.Show(e.ToString());
            } finally
            {
                conn.Close();
            }

        }

        public void addLog(int start, int end)
        {
            DataTable dt = dbConnection();
            string cmd = "insert into Log (LevelFrom, LevelTo, createAt) values('" + start.ToString() + "', '" + end.ToString() + "', '" + DateTime.Now.ToString() + "');";

            adapter.InsertCommand = new OleDbCommand(cmd, conn);
            DataRow newRow = dt.NewRow();

            //label1.Text = newRow["LogID"].ToString();
            newRow["LevelFrom"] = start;
            newRow["LevelTo"] = end;
            newRow["createAt"] = DateTime.Now;

            ds.Tables[0].Rows.Add(newRow);

            DataSet dsc = ds.GetChanges();
            try
            {
                adapter.Update(dsc);
            } catch (OleDbException e)
            {
                MessageBox.Show(e.ToString());
            }
            ds.AcceptChanges();
            msg.showData(dt);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            floorFont = new Font("Times New Roman", 16, FontStyle.Bold);
            // Adding for each level a colour
            floorNumberBrush = Brushes.Black;

            floorBrush = Brushes.BlanchedAlmond;
            floorMarginBrush = Brushes.RosyBrown;

            linePen = new Pen(Color.Black, 1f);

            // Main picture box width, it's used for element mesure and position
            width = pictureBox1.Width;
            height = pictureBox1.Height;

            // Bitmaps
            mainBmp = new Bitmap(width + 1, height + 1);
            leftBmp = new Bitmap(125, height + 1);
            rightBmp = new Bitmap(125, height + 1);
            // Adding bitmap to Graphics
            g = Graphics.FromImage(mainBmp);
            gLeft = Graphics.FromImage(leftBmp);
            gRight = Graphics.FromImage(rightBmp);
            g.Clear(Color.Transparent);
            gLeft.Clear(Color.Transparent);
            gRight.Clear(Color.Transparent);
            // Removing pixelation 
            g.SmoothingMode = SmoothingMode.AntiAlias;
            gLeft.SmoothingMode = SmoothingMode.AntiAlias;
            gRight.SmoothingMode = SmoothingMode.AntiAlias;
            // Drawing elevator on each level
            for (int i = 0; i <= 3; i++)
            {
                drawElevator(i);
            }
            // Drawing wall on each level
            drawFloors();
            // Disposing graphic object after cycle
            g.Dispose();
            gLeft.Dispose();
            gRight.Dispose();
            // Adding bitmaps to picture boxes
            pictureBox1.Image = mainBmp;
            pictureBox2.Image = leftBmp;
            pictureBox3.Image = rightBmp;
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            /*            try
                        {
                            createTable();
                        }
                        catch (OleDbException exc)
                        {
                            MessageBox.Show(exc.ToString());
                        }*/
            DataTable dt = dbConnection();
            conn.Open();
            msg.Show();
            msg.showData(dt);
            conn.Close();
        }

        private void btn0_Click(object sender, EventArgs e)
        {
            // Setting destination level
            toGo = 0;
            btnControlEvent(toGo);
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            toGo = 1;
            btnControlEvent(toGo);
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            toGo = 2;
            btnControlEvent(toGo);
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            toGo = 3;
            btnControlEvent(toGo);
        }

        private List<Action> animationList()
        {
            List<Action> animatedFloors = new List<Action>
            {
                () => animateFloor_0(),
                () => animateFloor_1(),
                () => animateFloor_2(),
                () => animateFloor_3()
            };
            return animatedFloors;
        }

        private void btnControlEvent(int togo)
        {
            List<Action> animatedFloors = animationList();
            // During animation all buttons are disabled
            enableControlButtons();
            enableCallButtons();
            //toGo = 3;
            int val = floor;
            int val2 = togo;
            addLog(val, val2);
            if (floor == togo)
            {
                // If at level, animate door (open & close)
                animatedFloors[togo].Invoke();
                // Setting all control buttons to default
                setControlDefault();
            }
            else
            {
                // If not at level, still disable all buttons till reaches the wanted level
                enableCallButtons();
                enableControlButtons();
                // Control button colour change
                setControlColor(togo);
                // Moving the groupBox (Control panel)
                moveControl();
            }
        }

        private void btnCallFloor0_Click(object sender, EventArgs e)
        {
            toGo = 0;
            btnCallEvent(toGo);
        }

        private void btnCallFloor1_Click(object sender, EventArgs e)
        {
            toGo = 1;
            btnCallEvent(toGo);
        }

        private void btnCallFloor2_Click(object sender, EventArgs e)
        {
            toGo = 2;
            btnCallEvent(toGo);
        }

        private void btnCallFloor3_Click(object sender, EventArgs e)
        {
            toGo = 3;
            btnCallEvent(toGo);
        }

        private void btnCallEvent(int togo)
        {
            List<Action> animatedFloors = animationList();
            // Disables all buttons
            enableControlButtons();
            enableCallButtons();
            // Setting control colour to default
            setControlDefault();
            //toGo = 0;
            if (floor == togo)
            {
                // If at wanted level, animate doors at that level
                animatedFloors[togo].Invoke();
                // Setting colour for call button
                setCallColor(togo);
            }
            else
            {
                // If not at wanted level, still disable call buttons
                enableCallButtons();
                // Setting call button collour
                setCallColor(togo);
                // Move group Box (Control panel)
                moveControl();
            }
        }

        private void drawElevator(int floor)
        {
            // Creating an image object
            Image image = Image.FromFile("Elevator_Interior.jpg");
            // Drawing image to main picture Box with g
            g.DrawImage(image, new Rectangle(width / 2 - 75, (floor * height / 4) + 30, 150, 182));
            // Adding other lines and shapes to the design
            g.DrawLine(linePen, width / 2 - 75, height / 4 + (floor * height / 4) - 7, width / 2 + 75, height / 4 + (floor * height / 4) - 7);
            g.DrawLine(linePen, width / 2 - 75, height / 4 + (floor * height / 4) - 190, width / 2 + 75, height / 4 + (floor * height / 4) - 190);
            g.FillRectangle(floorBrush, new Rectangle(width / 2 - 75, height / 4 + (floor * height / 4) - 7, 150, 6));
            g.FillRectangle(floorBrushes[floor], new Rectangle(width / 2 - 78, (floor * height / 4) - 1, 154, 30));
        }

        private void drawFloors()
        {
            // The width of picture box 2 and 3
            int newWidth = pictureBox2.Width;
            // Drawing all lines and shapes to left and right side of elevator
            for (int i=3; i>=0; i--)
            {
                // Outside wall conture
                gLeft.DrawLine(linePen, 0, height / 4 + (i * height / 4) - 2, newWidth, height / 4 + (i * height / 4) - 2);
                gRight.DrawLine(linePen, 0, height / 4 + (i * height / 4) - 2, newWidth, height / 4 + (i * height / 4) - 2);

                gLeft.DrawLine(linePen, newWidth-1, height / 4 + (i * height / 4) - 2, newWidth-1, height / 4 + (i * height / 4) - 190);
                gRight.DrawLine(linePen, 0, height / 4 + (i * height / 4) - 2, 0, height / 4 + (i * height / 4) - 190);
                // Outside wall fill
                gLeft.FillRectangle(floorBrushes[i], new Rectangle(0, (i * height / 4) - 2, newWidth-1, 218));
                gLeft.DrawRectangle(linePen, 0, height / 4 + (i * height / 4) - 22, newWidth-2, 20);
                gLeft.FillRectangle(floorMarginBrush, new Rectangle(0, height / 4 + (i * height / 4) - 22, newWidth-2, 20));

                gRight.FillRectangle(floorBrushes[i], new Rectangle(0, (i * height / 4) - 2, newWidth-1, 218));
                gRight.DrawRectangle(linePen, 0, height / 4 + (i * height / 4) - 22, newWidth-2, 20);
                gRight.FillRectangle(floorMarginBrush, new Rectangle(0, height / 4 + (i * height / 4) - 22, newWidth-2, 20));
            }
            // Level number Ex. -> Floor 1
            for (int i=3; i>=0; i--)
            {
                gLeft.DrawString($"Floor {i}", floorFont, floorNumberBrush, new Point(width / 2 - 180, height - (i * height / 4) - 190));
            }
        }

        private void animateFloor_0()
        {
            // Door animation for each level using Timer.Tick
            // The Ticking period
            tmrFloor_0.Interval = 10;
            // Adding new Event for this Timer
            tmrFloor_0.Tick += new EventHandler(tmr_Floor0_Tick);
            // Starting the timer
            tmrFloor_0.Start();
        }

        private void animateFloor_1()
        {
            // All comments are same as animateFloor_0() method
            tmrFloor_1.Interval = 10;
            tmrFloor_1.Tick += new EventHandler(tmr_Floor1_Tick);
            tmrFloor_1.Start();
        }

        public void animateFloor_2()
        {
            // All comments are same as animateFloor_0() method
            tmrFloor_2.Interval = 10;
            tmrFloor_2.Tick += new EventHandler(tmr_Floor2_Tick);
            tmrFloor_2.Start();
        }

        public void animateFloor_3()
        {
            // All comments are same as animateFloor_0() method
            tmrFloor_3.Interval = 10;
            tmrFloor_3.Tick += new EventHandler(tmr_Floor3_Tick);
            tmrFloor_3.Start();
        }

        public void tmr_Floor0_Tick(object sender, EventArgs e)
        {
            // The Tick Event for level: floor as current floor position
            // A list of coordinates for each door on each floor, the left upper corner of each door
            int[] coor = { picBoxLeft_0.Location.X, picBoxRight_0.Location.X, picBoxLeft_0.Location.Y, picBoxRight_0.Location.Y };
            // Disabling all buttons
            enableControlButtons();
            enableCallButtons();
            setCallColor(floor);

            if (coor[0] == 382)
            {
                // If door corner at position, setting state of door to true
                open = true;
                // Multiplier to -1 to reverse the movement
                multiplier = -1;
                // timer stop
                tmrFloor_0.Stop();
                // Waiting for doors to start closing back
                System.Threading.Thread.Sleep(1200);
                // start timer again for closing
                tmrFloor_0.Start();

            }
            else if (coor[0] == 457 && open)
            {
                // If door at closed postion, state to false
                open = false;
                // direction back to default
                multiplier = 1;

                // IMPORTANT //
                /* Removing after each cycle the Event from timer tick
                 prevents animation speed up */
                tmrFloor_0.Tick -= new EventHandler(tmr_Floor0_Tick);
                // Ending animation, 1 cycle of open and close
                tmrFloor_0.Stop();
                // Enabling all buttons
                enableControlButtons(true);
                enableCallButtons(true);
                // Call buttons colour to default
                setCallDefault();

            }
            else if (coor[0] > 382 && open == false)
            {
                // If not at level and it's in "opening" state, set door direction for opening movement
                multiplier = 1;
            }
            else if (coor[0] > 382 && open)
            {
                // If not at level but it's in "closing" state, set door direction for closing movement
                multiplier = -1;
            }
            // Finaly repositioning each door at new coordinates
            picBoxLeft_0.Location = new Point(coor[0] - multiplier, coor[2]);
            picBoxRight_0.Location = new Point(coor[1] + multiplier, coor[3]);
        }

        public void tmr_Floor1_Tick(object sender, EventArgs e)
        {
            // All comments are same as tmr_Floor0_Tick() Event
            int[] coor = { picBoxLeft_1.Location.X, picBoxRight_1.Location.X, picBoxLeft_1.Location.Y, picBoxRight_1.Location.Y };

            enableControlButtons();
            enableCallButtons();
            setCallColor(floor);

            if (coor[0] == 382)
            {
                open = true;
                multiplier = -1;
                tmrFloor_1.Stop();
                System.Threading.Thread.Sleep(1200);
                tmrFloor_1.Start();

            }
            else if (coor[0] == 457 && open)
            {
                open = false;
                multiplier = 1;
                tmrFloor_1.Tick -= new EventHandler(tmr_Floor1_Tick);
                tmrFloor_1.Stop();

                enableControlButtons(true);
                enableCallButtons(true);
                setCallDefault();

            }
            else if (coor[0] > 382 && open == false)
            {
                multiplier = 1;
            }
            else if (coor[0] > 382 && open)
            {
                multiplier = -1;
            }

            picBoxLeft_1.Location = new Point(coor[0] - multiplier, coor[2]);
            picBoxRight_1.Location = new Point(coor[1] + multiplier, coor[3]);
        }

        public void tmr_Floor2_Tick(object sender, EventArgs e)
        {
            // All comments are same as tmr_Floor0_Tick() Event
            int[] coor = { picBoxLeft_2.Location.X, picBoxRight_2.Location.X, picBoxLeft_2.Location.Y, picBoxRight_2.Location.Y };

            enableControlButtons();
            enableCallButtons();
            setCallColor(floor);

            if (coor[0] == 382)
            {
                open = true;
                multiplier = -1;
                tmrFloor_2.Stop();
                System.Threading.Thread.Sleep(1200);
                tmrFloor_2.Start();

            }
            else if (coor[0] == 457 && open)
            {
                open = false;
                multiplier = 1;
                tmrFloor_2.Tick -= new EventHandler(tmr_Floor2_Tick);
                tmrFloor_2.Stop();

                enableControlButtons(true);
                enableCallButtons(true);
                setCallDefault();

            }
            else if (coor[0] > 382 && open == false)
            {
                multiplier = 1;
            }
            else if (coor[0] > 382 && open)
            {
                multiplier = -1;
            }

            picBoxLeft_2.Location = new Point(coor[0] - multiplier, coor[2]);
            picBoxRight_2.Location = new Point(coor[1] + multiplier, coor[3]);
        }

        public void tmr_Floor3_Tick(object sender, EventArgs e)
        {
            // All comments are same as tmr_Floor0_Tick() Event
            int[] coor = { picBoxLeft_3.Location.X, picBoxRight_3.Location.X, picBoxLeft_3.Location.Y, picBoxRight_3.Location.Y };

            enableControlButtons();
            enableCallButtons();
            setCallColor(floor);

            if (coor[0] == 382)
            {
                open = true;
                multiplier = -1;
                tmrFloor_3.Stop();
                System.Threading.Thread.Sleep(1200);
                tmrFloor_3.Start();

            }
            else if (coor[0] == 457 && open)
            {
                open = false;
                multiplier = 1;
                tmrFloor_3.Tick -= new EventHandler(tmr_Floor3_Tick);
                tmrFloor_3.Stop();

                enableControlButtons(true);
                enableCallButtons(true);
                setCallDefault();

            }
            else if (coor[0] > 382 && open == false)
            {
                multiplier = 1;
            }
            else if (coor[0] > 382 && open)
            {
                multiplier = -1;
            }

            picBoxLeft_3.Location = new Point(coor[0] - multiplier, coor[2]);
            picBoxRight_3.Location = new Point(coor[1] + multiplier, coor[3]);
        }

        public void moveControl()
        {
            // Setting ticking interval to 10 milliseconds
            tmrControl.Interval = 10;
            // Adding event to control Tick
            tmrControl.Tick += new EventHandler(tmr_Control_Tick);
            // Start timer
            tmrControl.Start();
        }

        public void tmr_Control_Tick(object sender, EventArgs e)
        {
            // Event for groupBox (Control panel) movement           
            if (toGo > floor)
            {
                // If destination bigger(upper) than current floor level, move groupBox1 up 2 px
                if (groupBox1.Top != 690 - (toGo * 220))
                {
                    groupBox1.Top -= 2;
                    setFloorLabel();
                } else { executeAnimation(); }

            }
            else if (toGo < floor)
            {
                // If destination smaler(lower) than current floor level, move groupBox1 down 2 px
                if (groupBox1.Top != 690 - (toGo * 220))
                {
                    groupBox1.Top += 2;
                    setFloorLabel();
                } else { executeAnimation();}
            }
        }

        public void setFloorLabel()
        {
            // If the groupBox1 Top corner is one of the unwanted level, displays the level number on control panel
            if (controlTopPositions.Keys.Contains(groupBox1.Top))
            {
                lblFloor.Text = controlTopPositions[groupBox1.Top].ToString();
                // Meanwhile keeping control buttons disabled
                enableControlButtons();
            }
        }

        public void executeAnimation()
        {
            // If in the levels, enable buttons and colours
            groupBox1.Top = 690 - (toGo * 220);
            enableCallButtons(true);
            setCallDefault();
            setControlDefault();
            // IMPORTANT //
            // Removing Event from timer tick after each cycle
            tmrControl.Tick -= new EventHandler(tmr_Control_Tick);
            // Setiing current floor at wanted floor level
            floor = toGo;
            // Choosing the right animation for the current level
            getCorrectAnimation(toGo);
        }

        public void getCorrectAnimation(int val)
        {
            // Animates doors on wanted level (val)
            switch (val)
            {
                case 0:
                    animateFloor_0();
                    break;
                case 1:
                    animateFloor_1();
                    break;
                case 2:
                    animateFloor_2();
                    break;
                case 3:
                    animateFloor_3();
                    break;
            }
        }

        public void enableCallButtons(bool flag = false)
        {
            btnCallFloor0.Enabled = flag;
            btnCallFloor1.Enabled = flag;
            btnCallFloor2.Enabled = flag;
            btnCallFloor3.Enabled = flag;
        }

        public void enableControlButtons(bool flag = false)
        {
            btn0.Enabled = flag;
            btn1.Enabled = flag;
            btn2.Enabled = flag;
            btn3.Enabled = flag;
        }

        public void setCallDefault()
        {
            Color foreColor = SystemColors.ControlText;
            Color backColor = SystemColors.Control;
            btnCallFloor0.ForeColor = foreColor;
            btnCallFloor0.BackColor = backColor;
            btnCallFloor1.ForeColor = foreColor;
            btnCallFloor1.BackColor = backColor;
            btnCallFloor2.ForeColor = foreColor;
            btnCallFloor2.BackColor = backColor;
            btnCallFloor3.ForeColor = foreColor;
            btnCallFloor3.BackColor = backColor;
        }

        public void setControlDefault()
        {
            Color foreColor = SystemColors.ControlText;
            Color backColor = SystemColors.ControlDark;
            btn0.ForeColor = foreColor;
            btn0.BackColor = backColor;
            btn1.ForeColor = foreColor;
            btn1.BackColor = backColor;
            btn2.ForeColor = foreColor;
            btn2.BackColor = backColor;
            btn3.ForeColor = foreColor;
            btn3.BackColor = backColor;
        }

        public void setCallColor(int buttonNr)
        {
            Color foreColor = Color.FromArgb(255, 255, 255);
            Color backColor = Color.FromArgb(255, 210, 210);

            switch (buttonNr)
            {
                case 0:
                    btnCallFloor0.ForeColor = foreColor;
                    btnCallFloor0.BackColor = backColor;
                    break;
                case 1:
                    btnCallFloor1.ForeColor = foreColor;
                    btnCallFloor1.BackColor = backColor;
                    break;
                case 2:
                    btnCallFloor2.ForeColor = foreColor;
                    btnCallFloor2.BackColor = backColor;
                    break;
                case 3:
                    btnCallFloor3.ForeColor = foreColor;
                    btnCallFloor3.BackColor = backColor;
                    break;
            }
        }

        public void setControlColor(int buttonNr)
        {
            Color foreColor = Color.FromArgb(255, 255, 255);
            Color backColor = Color.FromArgb(220, 220, 135);

            switch (buttonNr)
            {
                case 0:
                    btn0.ForeColor = foreColor;
                    btn0.BackColor = backColor;
                    break;
                case 1:
                    btn1.ForeColor = foreColor;
                    btn1.BackColor = backColor;
                    break;
                case 2:
                    btn2.ForeColor = foreColor;
                    btn2.BackColor = backColor;
                    break;
                case 3:
                    btn3.ForeColor = foreColor;
                    btn3.BackColor = backColor;
                    break;
            }
        }
    }

    abstract class ElevatorState
    {
        public abstract void MovingUp(Context context);
        public abstract void MovingDown(Context context);
    }

    class Floor4 : ElevatorState
    {
        public override void MovingUp(Context context)
        {
            MessageBox.Show("Out of move!");
        }
        public override void MovingDown(Context context)
        {
            context._state = new Floor3();
            MessageBox.Show($"YOU REACHED {3}");
        }
    }

    class Floor3 : ElevatorState
    {
        public override void MovingUp(Context context)
        {
            context._state = new Floor4();
            MessageBox.Show($"YOU REACHED {4}");
        }
        public override void MovingDown(Context context)
        {
            context._state = new Floor2();
            MessageBox.Show($"YOU REACHED {2}");
        }
    }

    class Floor2 : ElevatorState
    {
        public override void MovingUp(Context context)
        {
            context._state = new Floor3();
            MessageBox.Show($"YOU REACHED {3}");
        }
        public override void MovingDown(Context context)
        {
            context._state = new Floor1();
            MessageBox.Show($"YOU REACHED {1}");
        }
    }

    class Floor1 : ElevatorState
    {
        public override void MovingUp(Context context)
        {
            context._state = new Floor2();
            MessageBox.Show($"YOU REACHED {2}");
        }
        public override void MovingDown(Context context)
        {
            context._state = new Floor0();
            MessageBox.Show($"YOU REACHED {0}");
        }
    }


    class Floor0 : ElevatorState
    {
        public override void MovingUp(Context context)
        {
            context._state = new Floor1();
            MessageBox.Show($"YOU REACHED {1}");
        }
        public override void MovingDown(Context context)
        {
            MessageBox.Show("Out of move");
        }
    }

    class Context
    {
        public ElevatorState _state;
        public Form1 _foo;

        public Context(ElevatorState state, Form1 foo)
        {
            _state = state;
            _foo = foo;
        }
        public void MovingUp()
        {
            _state.MovingUp(this);
        }
        public void MovingDown()
        {
            _state.MovingDown(this);
        }
        public ElevatorState State
        {
            get { return _state; }
            set { _state = value; }


        }
    }

}
