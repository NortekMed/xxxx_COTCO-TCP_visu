using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using nortekmed.network;

namespace visu_cotco
{
    
    public partial class Form1 : Form
    {

        
        SmallServer smallserver = null;
        ProcessingMessageThread processingmessagethread;
        public static UInt16[] aenvoyer = new UInt16[36];
        public static DateTime date_tcpip;
        public static byte slave_adress = 1;
        public static int delay = 0;
        string com_modbus = "COM1";
        int modbus_baudrate = 9600;
        System.IO.Ports.Parity parity_modbus = System.IO.Ports.Parity.None;
        int databit_modbus = 8;
        System.IO.Ports.StopBits stopbit_modbus = System.IO.Ports.StopBits.One;
        Modbus modbus1;
        object loc_aenvoyer;

        string log_path = "";
      
         //public static string databaseconnectionstring = "Server=192.168.127.1;User=SYSDBA;Password=masterkey;Port=3050;Dialect=3;Pooling=false;Charser=NONE;Database=d:\\COTCO.FDB";
         public static string databaseconnectionstring = "Server=localhost;User=SYSDBA;Password=masterkey;Port=3050;Dialect=3;Pooling=false;Charser=NONE;Database=D:\\COTCO.FDB";
         public static string databaseconnectionstring_vent = "Server=localhost;User=SYSDBA;Password=masterkey;Port=3050;Dialect=3;Pooling=true;Charser=NONE;Database=D:\\COTCO_VENT.FDB";

        //public static string databaseconnectionstring = "Server=localhost;User=SYSDBA;Password=masterkey;Pooling=false;Charser=NONE;Database=Cotco_mer-pc:d:\\COTCO.FDB";
        // public static string databaseconnectionstring = "Server=localhost;User=SYSDBA;Password=masterkey;Pooling=false;Charser=NONE;Database= E:\\COTCO.FDB";       
        //public static string databaseconnectionstring = "Server=192.168.1.197;User=SYSDBA;Password=masterkey;Pooling=false;Port=3050;Dialect=3;Charser=NONE;Database= d:\\COTCO.FDB";
        // public static string databaseconnectionstring_vent = "Server=localhost;User=SYSDBA;Password=masterkey;Port=3050;Dialect=3;Pooling=false;Charser=NONE;Database=C:\\Users\\thierryS\\Documents\\COTCO_VENT.FDB";
       //  public static string databaseconnectionstring = "Server=localhost;User=SYSDBA;Password=masterkey;Pooling=false;Charser=NONE;Database= C:\\Users\\thierryS\\Documents\\COTCO.FDB";

        public Form1()
        {
            InitializeComponent();

            string parent_path = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory()).FullName;
            Directory.CreateDirectory(parent_path + "//log");
            log_path = parent_path + "//log";

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string[] token = assembly.FullName.Split(new char[] { ',' });
            this.Text = "Visualisation " + token[1];


            timer1.Interval = 120000;
            timer1.Enabled = false;
            groupBox13.Visible = false;
            groupBox13.Enabled = false;
            comboBox1.SelectedIndex = 0;
            tabControl1.SelectedIndex = 0;
            if (File.Exists("config_visu_cotco.txt") == true)
            {
                using (StreamReader sr = new StreamReader("config_visu_cotco.txt"))
                {
                    string input = sr.ReadLine();
                    //   1;0;COM1;9600;N;8;1
                    string[] split = input.Split(new char[] { ';' });
                    try
                    {
                        slave_adress = Convert.ToByte(split[0]);
                    }
                    catch { }
                    try
                    {
                        delay = Convert.ToInt32(split[1]);
                    }
                    catch { }
                    try
                    {
                        com_modbus = split[2];
                    }
                    catch { }
                    try
                    {
                        modbus_baudrate = Convert.ToInt32(split[3]);
                    }
                    catch { }

                    char parity =  Convert.ToChar(split[4]);
                    switch (parity)
                    {
                        case 'N':
                            parity_modbus = System.IO.Ports.Parity.None;
                            break;
                        case 'O':
                            parity_modbus = System.IO.Ports.Parity.Odd;
                            break;
                        case 'E':
                            parity_modbus = System.IO.Ports.Parity.Even;
                            break;
                        case 'M':
                            parity_modbus = System.IO.Ports.Parity.Mark;
                            break;
                        case 'S':
                            parity_modbus = System.IO.Ports.Parity.Space;
                            break;
                        default:
                            parity_modbus = System.IO.Ports.Parity.None;
                            break;
                    }
                    try
                    {
                        databit_modbus = Convert.ToInt32(split[5]);
                    }
                    catch { }
                    int stopbits = Convert.ToInt32(split[6]);
                    switch (parity)
                    {
                        case '1':
                            stopbit_modbus = System.IO.Ports.StopBits.One;
                            break;
                        case '2':
                            stopbit_modbus = System.IO.Ports.StopBits.Two;
                            break;
                       
                        default:
                            stopbit_modbus = System.IO.Ports.StopBits.One;
                            break;
                    }



                    sr.Close();
                }

            }
            System.IO.Ports.SerialPort sp1 = new System.IO.Ports.SerialPort(com_modbus, modbus_baudrate, parity_modbus, databit_modbus, stopbit_modbus);
            modbus1 = new Modbus(sp1);
            modbus1.reponse_modbus1 += new Modbus.reponse_modbus(TraiteEvent_reponse_modbus1);
            modbus1.logging_info_evt += new Modbus.logging_info(TraiteEvent_MODBUS_log);
            modbus1.start_prog();
           

            loc_aenvoyer = new object();

            processingmessagethread = new ProcessingMessageThread();
            processingmessagethread.affich_param_SBE += new ProcessingMessageThread.mesure_SBE(processingmessagethread_affich_param_SBE);
            processingmessagethread.affich_param_vent += new ProcessingMessageThread.mesure_vent(processingmessagethread_affich_param_vent);
            System.Threading.Thread threadprocess = new System.Threading.Thread(new System.Threading.ThreadStart(processingmessagethread.Start));
            threadprocess.IsBackground = true;
            threadprocess.Start();
             

            smallserver = new SmallServer(60001, 100);

            smallserver.SmallServerEvent += new SmallServer.SmallServerEventHandler(smallserver_SmallServerEvent);
            smallserver.StartListening();


        }

        void processingmessagethread_affich_param_vent(DateTime date_compas, float compas, DateTime date_wind, float ws, float wd, float wm)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ProcessingMessageThread.mesure_vent(processingmessagethread_affich_param_vent), new object[] {  date_compas, compas, date_wind, ws, wd, wm });
                return;
            }

            //vent
            bool alarm = false;
            DateTime test = date_wind.AddMinutes(50);
            if (test < DateTime.Now)
            { alarm = true; }
            else { alarm = false; }
            label17.Text = "Date : " + date_wind.ToString("dd/MM/yy HH:mm:ss");
            label18.Text = "VitMoy : " + string.Format("{0:0.00}", ws) + " knots";
            label19.Text = "DirMoy : " + string.Format("{0:0.0}", wd) + " °";
            label20.Text = "VitMax : " + string.Format("{0:0.00}", wm) + " Knots";
            if (alarm)
            {
                label17.BackColor = Color.Red;
                label18.BackColor = Color.Red;
                label19.BackColor = Color.Red;
                label20.BackColor = Color.Red;
                //LogBox.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarm Wind\r\n");
                log("  Alarm Wind");
            }
            else
            {
                label17.BackColor = Color.Transparent;
                label18.BackColor = Color.Transparent;
                label19.BackColor = Color.Transparent;
                label20.BackColor = Color.Transparent;
            }
            //compas
            test = date_compas.AddMinutes(50);
            if (test < DateTime.Now)
            { alarm = true; }
            else { alarm = false; }
            label21.Text = "Date : " + date_compas.ToString("dd/MM/yy HH:mm:ss");
            label22.Text = "Compas : " + string.Format("{0:0.0}", compas) + " °";
            if (alarm)
            {

                label21.BackColor = Color.Red;
                label22.BackColor = Color.Red;
                //LogBox.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarm compas\r\n");
                log("  Alarm compas");
            }
            else
            {
                label21.BackColor = Color.Transparent;
                label22.BackColor = Color.Transparent;
            }
            lock (loc_aenvoyer)
            {
                int index = 33;
                if (label17.BackColor == Color.Transparent & ws < 100)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(ws * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }
                }
                else
                {
                    aenvoyer[index] = 0;
                }

                index += 1;

                if (label17.BackColor == Color.Transparent & wd < 360)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(wd * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }
                }
                else
                {
                    aenvoyer[index] = 0;
                }

                index += 1;

                if (label17.BackColor == Color.Transparent & wm < 100)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(wm * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }
                }
                else
                {
                    aenvoyer[index] = 0;
                }
            }
        }

     


        ///affichage toutes les minutes
        void processingmessagethread_affich_param_SBE(DateTime timestamplog, DateTime[] date_sbe, float[] temp_sbe, /*DateTime date_compas, float compas, DateTime date_wind, float ws, float wd, float wm,*/ DateTime[] date_courant, float[] spd_cou, float[] dir_cou, DateTime date_houle, float hm0, float tp, float tm02, float hmax, float meandir, float dirtp, DateTime date_pression, float pression)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ProcessingMessageThread.mesure_SBE(processingmessagethread_affich_param_SBE), new object[] { timestamplog, date_sbe, temp_sbe,/* date_compas, compas, date_wind, ws, wd, wm,*/ date_courant, spd_cou, dir_cou, date_houle, hm0, tp, tm02, hmax, meandir, dirtp, date_pression,pression });
                return;
            }

            date_tcpip = DateTime.Now;
            label1.Text = "Date : " + date_tcpip.ToString("dd/MM/yy HH:mm:ss");
            label1.BackColor = Color.Transparent;
          
           

            bool alarm = false;
            //sbe_1
            DateTime test = date_sbe[0].AddMinutes(50);
            if (test < DateTime.Now) alarm = true;
            label3.Text = "Date : " + date_sbe[0].ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_sbe[0]) + " °C";
            if (alarm)
            {
                label3.BackColor = Color.Red;
                //LogBox.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarm SBE_1\r\n");
                log("  Alarm SBE_1");
                temp_sbe[0] = 0;
            }
            else
            {
                label3.BackColor = Color.Transparent;
            }
            //sbe_2
            test = date_sbe[1].AddMinutes(50);
            if (test < DateTime.Now)
            { alarm = true; }
            else { alarm = false; }
            label4.Text = "Date : " + date_sbe[1].ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_sbe[1]) + " °C";
            if (alarm)
            {
                label4.BackColor = Color.Red;
                //LogBox.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarm SBE_2\r\n");
                log("  Alarm SBE_2");
            }
            else
            {
                label4.BackColor = Color.Transparent;
            }
            //sbe3
            test = date_sbe[2].AddMinutes(50);
            if (test < DateTime.Now)
            { alarm = true; }
            else { alarm = false; }
            label5.Text = "Date : " + date_sbe[2].ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_sbe[2]) + " °C";
            if (alarm)
            {
                label5.BackColor = Color.Red;
                //LogBox.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarm SBE_3\r\n");
                log("  Alarm SBE_3");
            }
            else
            {
                label5.BackColor = Color.Transparent;
            }
            //sbe4
            test = date_sbe[3].AddMinutes(50);
            if (test < DateTime.Now)
            { alarm = true; }
            else { alarm = false; }
            label6.Text = "Date : " + date_sbe[3].ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_sbe[3]) + " °C";
            if (alarm)
            {
                label6.BackColor = Color.Red;
                //LogBox.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarm SBE_4\r\n");
                log("  Alarm SBE_4");
            }
            else
            {
                label6.BackColor = Color.Transparent;
            }
            //sbe5
            test = date_sbe[4].AddMinutes(50);
            if (test < DateTime.Now)
            { alarm = true; }
            else { alarm = false; }
            label7.Text = "Date : " + date_sbe[4].ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_sbe[4]) + " °C";
            if (alarm)
            {
                label7.BackColor = Color.Red;
                //LogBox.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarm SBE_5\r\n");
                log("  Alarm SBE_5");
            }
            else
            {
                label7.BackColor = Color.Transparent;
            }
            //sbe6
            test = date_sbe[5].AddMinutes(50);
            if (test < DateTime.Now)
            { alarm = true; }
            else { alarm = false; }
            label8.Text = "Date : " + date_sbe[5].ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_sbe[5]) + " °C";
            if (alarm)
            {
                label8.BackColor = Color.Red;
                //LogBox.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarm SBE_6\r\n");
                log("  Alarm SBE_6");
            }
            else
            {
                label8.BackColor = Color.Transparent;
            }
           
            //courant
            richTextBox1.Clear();
            test = date_courant[0].AddMinutes(50);
            if (test < DateTime.Now)
            { alarm = true; }
            else { alarm = false; }

            label16.Text = "Date : " + date_courant[0].ToString("dd/MM/yy HH:mm:ss");
            for (int i = 0; i < 10; i++)
            {
                int k = 10 - i;
                richTextBox1.AppendText("Cell -" + k.ToString() + "m   Vit = " + string.Format("{0:0.00}", spd_cou[i]) + " knots     Dir = " + string.Format("{0:0.0}", dir_cou[i]) + " ° \r\n");
            }
            richTextBox1.Refresh();

            if (alarm)
            {
                label16.BackColor = Color.Red;
                //LogBox.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarm Current\r\n");
                //log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarm Current\r\n");
                log("  Alarm Current");
            }
            else
            {
                label16.BackColor = Color.Transparent;
            }

            //houle

            test = date_houle.AddMinutes(100);
            if (test < DateTime.Now)
            { alarm = true; }
            else { alarm = false; }
          
            if (hm0 == -9) hm0 = 0;           
            if (hmax == -9) hmax = 0;           
            if (tm02 == -9) tm02 = 0;          
            if (tp == -9) tp = 0;           
            if (dirtp == -9) dirtp = 0;         
            if (meandir == -9) meandir = 0;

            label9.Text = "Date : " + date_houle.ToString("dd/MM/yy HH:mm:ss");
            label10.Text = "HSig : " + string.Format("{0:0.00}", hm0) + " m";
            label11.Text = "TSig : " + string.Format("{0:0.0}", tm02) + " s";
            label12.Text = "TPic : " + string.Format("{0:0.0}", tp) + " s";
            label13.Text = "HMax : " + string.Format("{0:0.00}", hmax) + " m";
            label14.Text = "DirTp : " + string.Format("{0:0.0}", dirtp) + " °";
            label15.Text = "Meandir : " + string.Format("{0:0.0}", meandir) + " °";
            if (alarm)
            {
                label9.BackColor = Color.Red;
                label10.BackColor = Color.Red;
                label11.BackColor = Color.Red;
                label12.BackColor = Color.Red;
                label13.BackColor = Color.Red;
                label14.BackColor = Color.Red;
                label15.BackColor = Color.Red;
                //LogBox.AppendText(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "  Alarme Wave\r\n");
                log("  Alarme Wave");
            }
            else
            {
                label9.BackColor = Color.Transparent;
                label10.BackColor = Color.Transparent;
                label11.BackColor = Color.Transparent;
                label12.BackColor = Color.Transparent;
                label13.BackColor = Color.Transparent;
                label14.BackColor = Color.Transparent;
                label15.BackColor = Color.Transparent;

            }

            lock (loc_aenvoyer)
            {

                int index = 0;

                if (label3.BackColor == Color.Transparent & temp_sbe[0] < 50f)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(temp_sbe[0] * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }

                }
                else
                {
                    aenvoyer[index] = 0;
                }

                index += 1;

                if (label4.BackColor == Color.Transparent & temp_sbe[1] < 50)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(temp_sbe[1] * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }

                }
                else
                {
                    aenvoyer[index] = 0;
                }
               
                index += 1;

                if (label5.BackColor == Color.Transparent & temp_sbe[2] < 50)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(temp_sbe[2] * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }

                }
                else
                {
                    aenvoyer[index] = 0;
                }

                index += 1;

                if (label6.BackColor == Color.Transparent & temp_sbe[3] < 50)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(temp_sbe[3] * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }

                }
                else
                {
                    aenvoyer[index] = 0;
                }

                index += 1;

                if (label7.BackColor == Color.Transparent & temp_sbe[4] < 50)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(temp_sbe[4] * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }

                }
                else
                {
                    aenvoyer[index] = 0;
                }

                index += 1;

                if (label8.BackColor == Color.Transparent & temp_sbe[5] < 50)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(temp_sbe[5] * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }

                }
                else
                {
                    aenvoyer[index] = 0;
                }

                index += 1;

                if ( label9.BackColor == Color.Transparent  & hm0 < 15)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(hm0 * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }
                }
                else
                {
                    aenvoyer[index] = 0; 
                }
                
                index += 1;

                if (label9.BackColor == Color.Transparent & tm02 < 30)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(tm02 * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }
                }
                else
                {
                    aenvoyer[index] = 0;
                }
       
                index += 1;

                if (label9.BackColor == Color.Transparent & hmax < 20)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(hmax * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }
                }
                else
                {
                    aenvoyer[index] = 0;
                }
               
                index += 1;

                if (label9.BackColor == Color.Transparent & tp < 20)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(tp * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }
                }
                else
                {
                    aenvoyer[index] = 0;
                }
               
                index += 1;

                if (label9.BackColor == Color.Transparent & dirtp < 360)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(dirtp * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }
                }
                else
                {
                    aenvoyer[index] = 0;
                }
               
                index += 1;

                if (label9.BackColor == Color.Transparent & meandir < 360)
                {
                    try
                    {
                        aenvoyer[index] = Convert.ToUInt16(Math.Round(meandir * 100F));
                    }
                    catch
                    { aenvoyer[index] = 0; }
                }
                else
                {
                    aenvoyer[index] = 0;
                }
               
                index += 1;

                for (int i = 0; i < 10; i++)
                {
                    if (label16.BackColor == Color.Transparent & spd_cou[i] < 6.5F)
                    {
                        try
                        {
                            aenvoyer[index] = Convert.ToUInt16(Math.Round(spd_cou[i] * 10000F));
                        }
                        catch
                        { aenvoyer[index] = 0; }
                    }
                    else
                    {
                        aenvoyer[index] = 0;
                    }
                   
                    index += 1;
                    if (label16.BackColor == Color.Transparent & dir_cou[i] < 360)
                    {
                        try
                        {
                            aenvoyer[index] = Convert.ToUInt16(Math.Round(dir_cou[i] * 100F));
                        }
                        catch
                        { aenvoyer[index] = 0; }
                    }
                    else
                    {
                        aenvoyer[index] = 0;
                    }
                   
                    index += 1;

                }

                if (label16.BackColor == Color.Transparent)
                {
                    try
                        {
                            aenvoyer[index] = Convert.ToUInt16(Math.Round(pression / 1000F));
                        }
                    catch
                    { aenvoyer[index] = 0; }
                }
                else
                {
                    aenvoyer[index] = 0; ;
                }
               
                //index += 1;

               

            }


        }


        void smallserver_SmallServerEvent(object sender, SmallServerEventArgs e)
        {
            switch (e.Kind)
            {
                case SmallServerEventArgs.kind.newclient:

                    //Console.WriteLine("--> New client incoming @ " + DateTime.Now + "\n" + e.Message);

                    SmallServerModem server = new SmallServerModem();
                    server.SmallServerModemEvent += new SmallServerModem.SmallServerModemEventHandler(server_SmallServerModemEvent);
                    server.NetworkStream = e.SmallClient;
                    System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(server.Start));
                    th.IsBackground = true;
                    th.Start();
                    break;
                case SmallServerEventArgs.kind.startlistening:
                    break;

            }

        }


        void server_SmallServerModemEvent(object sender, SmallServerModemEventArgs e)
        {
            if (e.Kind == SmallServerModemEventArgs.kind.datamessage)
            {


                // connect
                try
                {
                    //Console.WriteLine("Got messsage, write data !!. :");
                    try
                    {
                        // try parsing !
                        processingmessagethread.addMessage(e.Message);
                    }
                    catch (Exception ex2)
                    {
                       // System.Console.WriteLine("cannot parse message");
                    }

                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                }

            }
        }
      

   
        FbConnection myConnection;
     

        void TraiteEvent_reponse_modbus1(DateTime heure_modbus)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Modbus.reponse_modbus(TraiteEvent_reponse_modbus1), new object[] { heure_modbus });
                return;
            }
            label2.Text = "Date : " + heure_modbus.ToString("dd/MM/yy HH:mm:ss");
        }

      

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex > 0 & tabControl1.SelectedIndex < 5)
            {
                groupBox13.Visible = true;
                groupBox13.Enabled = true;
                dateTimePicker1.Format = DateTimePickerFormat.Custom;
                dateTimePicker2.Format = DateTimePickerFormat.Custom;
                dateTimePicker1.CustomFormat = "dd/MM/yyyy HH:mm";
                dateTimePicker2.CustomFormat = "dd/MM/yyyy HH:mm";
                dateTimePicker1.Value = DateTime.Now.AddDays(-1);
                dateTimePicker2.Value = DateTime.Now;
                label25.Visible = false;
                comboBox1.Visible = false;

            }
            else if (tabControl1.SelectedIndex == 0 | tabControl1.SelectedIndex == 5)
            {
                groupBox13.Visible = false;
                groupBox13.Enabled = false;
            }
            if(tabControl1.SelectedIndex == 3)
            {
                label25.Visible = true;
                comboBox1.Visible = true;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // refresh
            
            if (dateTimePicker2.Value > dateTimePicker1.Value)
            {
                
                if (tabControl1.SelectedIndex > 0)
                {
                    switch (tabControl1.SelectedIndex)
                    {
                        case 1: //temperature                           
                            trace_temperature();
                            break;

                        case 2: //houle
                             
                             trace_houle();
                             
                            break;

                        case 3://courant
                           
                           
                            trace_courant();
                            
                            break;

                        case 4:
                         
                              trace_vent();
                            
                            break;

                        default:
                            break;


                    }
                }
            }
        }

        private void trace_temperature()
        {
            string timestampsrequest = " WHERE a.TIME_LOG>='" + dateTimePicker1.Value.ToString("dd.MM.yyyy , HH:mm:ss") + "' and a.TIME_LOG<='" + dateTimePicker2.Value.ToString("dd.MM.yyyy , HH:mm:ss") + "'";
           
            tChart1.Axes.Bottom.Labels.DateTimeFormat = "HH:mm dd/MM/yy";
            tChart1.Axes.Bottom.Labels.MultiLine = false;
           
            DataSet ds1 = new DataSet();
            FbDataAdapter dataadapter1 = new FirebirdSql.Data.FirebirdClient.FbDataAdapter("SELECT TIME_LOG,TEMP  FROM SBE_1 a " + timestampsrequest + " order by a.TIME_LOG", databaseconnectionstring);
            try
            {
                
                //tChart1.Axes.Bottom.Maximum = dateTimePicker2.Value; 
                label1.BackColor = Color.Transparent;
                dataadapter1.Fill(ds1);
                fastLine1.XValues.DateTime = true;
                fastLine1.XValues.DataMember = "TIME_LOG";
                fastLine1.YValues.DataMember = "TEMP";
                fastLine1.DataSource = ds1.Tables[0];
                //recherche min et max
                DataTable myDataTable = ds1.Tables[0];
                double min = 100;
                double max = 0;
                DateTime hmin = DateTime.Parse("01/01/2000 00:00:00");
                DateTime hmax = DateTime.Parse("01/01/2000 00:00:00");
                int compt = 0;
                double temp_moy = 0;
                foreach (DataRow dRow in myDataTable.Rows)
                {
                    double temptemp = Convert.ToDouble(dRow["TEMP"]);
                    temp_moy += temptemp;
                    compt += 1;
                    if (temptemp < min)
                    {
                        hmin = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        min = temptemp;
                    }
                    if (temptemp > max)
                    {
                        hmax = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        max = temptemp;
                    }

                }
                temp_moy /= compt;
                label28.Text = "Date : " + hmin.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", min) + " °C";
                label34.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", max) + " °C";
                label40.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_moy) + " °C";
               

            }
            catch {
                label1.BackColor = Color.Red;
                log("Problème de base données sur SBE1.");
            }

            DataSet ds2 = new DataSet();
            FbDataAdapter dataadapter2 = new FirebirdSql.Data.FirebirdClient.FbDataAdapter("SELECT TIME_LOG,TEMP  FROM SBE_2 a " + timestampsrequest + " order by a.TIME_LOG", databaseconnectionstring);
            try
            {
                label1.BackColor = Color.Transparent;
                dataadapter2.Fill(ds2);
                fastLine2.XValues.DateTime = true;
                fastLine2.XValues.DataMember = "TIME_LOG";
                fastLine2.YValues.DataMember = "TEMP";
                fastLine2.DataSource = ds2.Tables[0];
                //recherche min et max
                DataTable myDataTable = ds2.Tables[0];
                double min = 100;
                double max = 0;
                DateTime hmin = DateTime.Parse("01/01/2000 00:00:00");
                DateTime hmax = DateTime.Parse("01/01/2000 00:00:00");
                int compt = 0;
                double temp_moy = 0;
                foreach (DataRow dRow in myDataTable.Rows)
                {
                    double temptemp = Convert.ToDouble(dRow["TEMP"]);
                    temp_moy += temptemp;
                    compt += 1;
                    if (temptemp < min)
                    {
                        hmin = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        min = temptemp;
                    }
                    if (temptemp > max)
                    {
                        hmax = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        max = temptemp;
                    }

                }
                temp_moy /= compt;
                label31.Text = "Date : " + hmin.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", min) + " °C";
                label37.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", max) + " °C";
                label43.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_moy) + " °C";
            }
            catch {
                label1.BackColor = Color.Red;
                log("Problème de base données sur SBE2.");
            }

            DataSet ds3 = new DataSet();
            FbDataAdapter dataadapter3 = new FirebirdSql.Data.FirebirdClient.FbDataAdapter("SELECT TIME_LOG,TEMP  FROM SBE_3 a " + timestampsrequest + " order by a.TIME_LOG", databaseconnectionstring);
            try
            {
                label1.BackColor = Color.Transparent;
                dataadapter3.Fill(ds3);
                fastLine3.XValues.DateTime = true;
                fastLine3.XValues.DataMember = "TIME_LOG";
                fastLine3.YValues.DataMember = "TEMP";               
                fastLine3.DataSource = ds3.Tables[0];
                //recherche min et max
                DataTable myDataTable = ds3.Tables[0];
                double min = 100;
                double max = 0;
                DateTime hmin = DateTime.Parse("01/01/2000 00:00:00");
                DateTime hmax = DateTime.Parse("01/01/2000 00:00:00");
                int compt = 0;
                double temp_moy = 0;
                foreach (DataRow dRow in myDataTable.Rows)
                {
                    double temptemp = Convert.ToDouble(dRow["TEMP"]);
                    temp_moy += temptemp;
                    compt += 1;
                    if (temptemp < min)
                    {
                        hmin = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        min = temptemp;
                    }
                    if (temptemp > max)
                    {
                        hmax = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        max = temptemp;
                    }

                }
                temp_moy /= compt;
                label30.Text = "Date : " + hmin.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", min) + " °C";
                label36.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", max) + " °C";
                label42.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_moy) + " °C";
            }
            catch {
                label1.BackColor = Color.Red;
                log("Problème de base données sur SBE3.");
            }

            DataSet ds4 = new DataSet();
            FbDataAdapter dataadapter4 = new FirebirdSql.Data.FirebirdClient.FbDataAdapter("SELECT TIME_LOG,TEMP  FROM SBE_4 a " + timestampsrequest + " order by a.TIME_LOG", databaseconnectionstring);
            try
            {
                label1.BackColor = Color.Transparent;
                dataadapter4.Fill(ds4);
                fastLine4.XValues.DateTime = true;
                fastLine4.XValues.DataMember = "TIME_LOG";
                fastLine4.YValues.DataMember = "TEMP";
                fastLine4.DataSource = ds4.Tables[0];
                //recherche min et max
                DataTable myDataTable = ds4.Tables[0];
                double min = 100;
                double max = 0;
                DateTime hmin = DateTime.Parse("01/01/2000 00:00:00");
                DateTime hmax = DateTime.Parse("01/01/2000 00:00:00");
                int compt = 0;
                double temp_moy = 0;
                foreach (DataRow dRow in myDataTable.Rows)
                {
                    double temptemp = Convert.ToDouble(dRow["TEMP"]);
                    temp_moy += temptemp;
                    compt += 1;
                    if (temptemp < min)
                    {
                        hmin = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        min = temptemp;
                    }
                    if (temptemp > max)
                    {
                        hmax = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        max = temptemp;
                    }

                }
                temp_moy /= compt;
                label29.Text = "Date : " + hmin.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", min) + " °C";
                label35.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", max) + " °C";
                label41.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_moy) + " °C";
            }
            catch {
                label1.BackColor = Color.Red;
                log("Problème de base données sur SBE4.");
            }

            DataSet ds5 = new DataSet();
            FbDataAdapter dataadapter5 = new FirebirdSql.Data.FirebirdClient.FbDataAdapter("SELECT TIME_LOG,TEMP  FROM SBE_5 a " + timestampsrequest + " order by a.TIME_LOG", databaseconnectionstring);
            try
            {
                label1.BackColor = Color.Transparent;
                dataadapter5.Fill(ds5);
                fastLine5.XValues.DateTime = true;
                fastLine5.XValues.DataMember = "TIME_LOG";
                fastLine5.YValues.DataMember = "TEMP";
                fastLine5.DataSource = ds5.Tables[0];
                //recherche min et max
                DataTable myDataTable = ds5.Tables[0];
                double min = 100;
                double max = 0;
                DateTime hmin = DateTime.Parse("01/01/2000 00:00:00");
                DateTime hmax = DateTime.Parse("01/01/2000 00:00:00");
                int compt = 0;
                double temp_moy = 0;
                foreach (DataRow dRow in myDataTable.Rows)
                {
                    double temptemp = Convert.ToDouble(dRow["TEMP"]);
                    temp_moy += temptemp;
                    compt += 1;
                    if (temptemp < min)
                    {
                        hmin = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        min = temptemp;
                    }
                    if (temptemp > max)
                    {
                        hmax = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        max = temptemp;
                    }

                }
                temp_moy /= compt;
                label27.Text = "Date : " + hmin.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", min) + " °C";
                label33.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", max) + " °C";
                label39.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_moy) + " °C";
            }
            catch {
                label1.BackColor = Color.Red;
                log("Problème de base données sur SBE5.");
            }

            DataSet ds6 = new DataSet();
            FbDataAdapter dataadapter6 = new FirebirdSql.Data.FirebirdClient.FbDataAdapter("SELECT TIME_LOG,TEMP  FROM SBE_6 a " + timestampsrequest + " order by a.TIME_LOG", databaseconnectionstring);
            try
            {
                label1.BackColor = Color.Transparent;
                dataadapter6.Fill(ds6);
                fastLine6.XValues.DateTime = true;
                fastLine6.XValues.DataMember = "TIME_LOG";
                fastLine6.YValues.DataMember = "TEMP";
                fastLine6.DataSource = ds6.Tables[0];
                //recherche min et max
                DataTable myDataTable = ds6.Tables[0];
                double min = 100;
                double max = 0;
                DateTime hmin = DateTime.Parse("01/01/2000 00:00:00");
                DateTime hmax = DateTime.Parse("01/01/2000 00:00:00");
                int compt = 0;
                double temp_moy = 0;
                foreach (DataRow dRow in myDataTable.Rows)
                {
                    double temptemp = Convert.ToDouble(dRow["TEMP"]);
                    temp_moy += temptemp;
                    compt += 1;
                    if (temptemp < min)
                    {
                        hmin = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        min = temptemp;
                    }
                    if (temptemp > max)
                    {
                        hmax = Convert.ToDateTime(dRow["TIME_LOG"].ToString());
                        max = temptemp;
                    }

                }
                temp_moy /= compt;
                label26.Text = "Date : " + hmin.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", min) + " °C";
                label32.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", max) + " °C";
                label38.Text = "Date : " + hmax.ToString("dd/MM/yy HH:mm:ss") + "  - Temperature : " + string.Format("{0:00.00}", temp_moy) + " °C";
            }
            catch {
                label1.BackColor = Color.Red;
                log("Problème de base données sur SBE6.");
            }

            groupBox14.Text = "TEMPERATURE MINI du " + dateTimePicker1.Value.ToString("dd/MM/yy HH:mm") + " au " + dateTimePicker2.Value.ToString("dd/MM/yy HH:mm");
            groupBox21.Text = "TEMPERATURE MAXI du " + dateTimePicker1.Value.ToString("dd/MM/yy HH:mm") + " au " + dateTimePicker2.Value.ToString("dd/MM/yy HH:mm");
            
           
        }

        private void trace_houle()
        {
            string timestampsrequest = " WHERE a.TIME_LOG>='" + dateTimePicker1.Value.ToString("dd.MM.yyyy , HH:mm:ss") + "' and a.TIME_LOG<='" + dateTimePicker2.Value.ToString("dd.MM.yyyy , HH:mm:ss") + "'";
            
            tChart2.Axes.Bottom.Labels.DateTimeFormat = "HH:mm dd/MM/yy";
            tChart2.Axes.Bottom.Labels.MultiLine = false;

            DataSet ds1 = new DataSet();
            FbDataAdapter dataadapter1 = new FirebirdSql.Data.FirebirdClient.FbDataAdapter("SELECT TIME_LOG,HM0,HMAX,TM02,TP,DIRTP,MEANDIR  FROM AWAC_WAVE a " + timestampsrequest + " order by a.TIME_LOG", databaseconnectionstring);
            try
            {
                label1.BackColor = Color.Transparent;
                dataadapter1.Fill(ds1);
                fastLine7.XValues.DateTime = true;
                fastLine7.XValues.DataMember = "TIME_LOG";
                fastLine7.YValues.DataMember = "HM0";
                fastLine7.DataSource = ds1.Tables[0];
                fastLine8.XValues.DateTime = true;
                fastLine8.XValues.DataMember = "TIME_LOG";
                fastLine8.YValues.DataMember = "HMAX";
                fastLine8.DataSource = ds1.Tables[0];
                fastLine9.XValues.DateTime = true;
                fastLine9.XValues.DataMember = "TIME_LOG";
                fastLine9.YValues.DataMember = "TM02";
                fastLine9.DataSource = ds1.Tables[0];
                fastLine10.XValues.DateTime = true;
                fastLine10.XValues.DataMember = "TIME_LOG";
                fastLine10.YValues.DataMember = "TP";
                fastLine10.DataSource = ds1.Tables[0];
                fastLine11.XValues.DateTime = true;
                fastLine11.XValues.DataMember = "TIME_LOG";
                fastLine11.YValues.DataMember = "DIRTP";
                fastLine11.DataSource = ds1.Tables[0];
                fastLine12.XValues.DateTime = true;
                fastLine12.XValues.DataMember = "TIME_LOG";
                fastLine12.YValues.DataMember = "MEANDIR";
                fastLine12.DataSource = ds1.Tables[0];
            }
            catch {
                label1.BackColor = Color.Red;
                log("Problème de lecture de la base de données AWAC-WAVE.");
            }


        }

        private void trace_courant()
        {
            int nb_couche = comboBox1.SelectedIndex + 1;
            string timestampsrequest = " WHERE a.TIME_LOG>='" + dateTimePicker1.Value.ToString("dd.MM.yyyy , HH:mm:ss") + "' and a.TIME_LOG<='" + dateTimePicker2.Value.ToString("dd.MM.yyyy , HH:mm:ss") + "'" + "and a.CELLNUM ='" + nb_couche.ToString() + "'";

            tChart3.Axes.Bottom.Labels.DateTimeFormat = "HH:mm dd/MM/yy";
            tChart3.Axes.Bottom.Labels.MultiLine = false;
            tChart3.Text = "Courant couche N° " + nb_couche.ToString(); 

            DataSet ds1 = new DataSet();
            FbDataAdapter dataadapter1 = new FirebirdSql.Data.FirebirdClient.FbDataAdapter("SELECT TIME_LOG,SPD,DIR  FROM AWAC_COURANT a " + timestampsrequest + " order by a.TIME_LOG", databaseconnectionstring);
            try
            {
                label1.BackColor = Color.Transparent;
                dataadapter1.Fill(ds1);
                fastLine13.XValues.DateTime = true;
                fastLine13.XValues.DataMember = "TIME_LOG";
                fastLine13.YValues.DataMember = "SPD";
                fastLine13.DataSource = ds1.Tables[0];
                fastLine14.XValues.DateTime = true;
                fastLine14.XValues.DataMember = "TIME_LOG";
                fastLine14.YValues.DataMember = "DIR";
                fastLine14.DataSource = ds1.Tables[0];
            }
            catch {
                label1.BackColor = Color.Red;
                log("Problème de lecture de la base de données AWAC_COURANT.");
            }

        }

        private void trace_vent()
        {
            int nb_couche = comboBox1.SelectedIndex + 1;
            string timestampsrequest = " WHERE a.TIME_REC>='" + dateTimePicker1.Value.ToString("dd.MM.yyyy , HH:mm:ss") + "' and a.TIME_REC<='" + dateTimePicker2.Value.ToString("dd.MM.yyyy , HH:mm:ss") + "'";

            tChart4.Axes.Bottom.Labels.DateTimeFormat = "HH:mm dd/MM/yy";
            tChart4.Axes.Bottom.Labels.MultiLine = false;
           

            DataSet ds1 = new DataSet();
            FbDataAdapter dataadapter1 = new FirebirdSql.Data.FirebirdClient.FbDataAdapter("SELECT TIME_REC,VITMOY,DIRMOY,VITMAX  FROM VENT a " + timestampsrequest + " order by a.TIME_REC", databaseconnectionstring_vent);
            try
            {
                label1.BackColor = Color.Transparent;
                dataadapter1.Fill(ds1);
                fastLine15.XValues.DateTime = true;
                fastLine15.XValues.DataMember = "TIME_REC";
                fastLine15.YValues.DataMember = "VITMOY";
                fastLine15.DataSource = ds1.Tables[0];
                fastLine16.XValues.DateTime = true;
                fastLine16.XValues.DataMember = "TIME_REC";
                fastLine16.YValues.DataMember = "VITMAX";
                fastLine16.DataSource = ds1.Tables[0];
                fastLine17.XValues.DateTime = true;
                fastLine17.XValues.DataMember = "TIME_REC";
                fastLine17.YValues.DataMember = "DIRMOY";
                fastLine17.DataSource = ds1.Tables[0];
            }
            catch {
                label1.BackColor = Color.Red;
                log("Problème de lecture de la base de données VENT.");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //export format excel
            switch (tabControl1.SelectedIndex)
            {
                case 1: //temperature
                    for (int i = 0; i < 6; i++)
                    {
                        tChart1[i].DateTimeFormat = "dd/MM/yyyy HH:mm:ss";
                        tChart1[i].ValueFormat = "##0.##";
                    }
                    saveFileDialog1.DefaultExt = tChart1.Export.Data.Text.FileExtension;
			        saveFileDialog1.FileName = tChart1.Name+ "."+saveFileDialog1.DefaultExt;
                    saveFileDialog1.Filter = "Text files (*.csv;*.txt)|*.csv;*.txt";
			        if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
			            {
				            tChart1.Export.Data.Text.Series = null; // export all series
				            tChart1.Export.Data.Text.IncludeLabels = true;
				            tChart1.Export.Data.Text.IncludeIndex = false;
				            tChart1.Export.Data.Text.IncludeHeader = true;
                            tChart1.Export.Data.Text.IncludeSeriesTitle = true;
                            tChart1.Export.Data.Text.TextDelimiter = ";";
                            tChart1.Export.Data.Text.TextLineSeparator = "\r\n";
				            tChart1.Export.Data.Text.Save(this.saveFileDialog1.FileName);
			            }		

                    break;

                case 2:
                     for (int i = 0; i < 6; i++)
                    {
                        tChart2[i].DateTimeFormat = "dd/MM/yyyy HH:mm:ss";
                        tChart2[i].ValueFormat = "##0.##";
                    }
                     saveFileDialog1.DefaultExt = tChart1.Export.Data.Text.FileExtension;
			        saveFileDialog1.FileName = tChart1.Name+ "."+saveFileDialog1.DefaultExt;
                    saveFileDialog1.Filter = "Text files (*.csv;*.txt)|*.csv;*.txt";
			        if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
			            {
                            tChart2.Export.Data.Text.Series = null; // export all series
                            tChart2.Export.Data.Text.IncludeLabels = true;
                            tChart2.Export.Data.Text.IncludeIndex = false;
                            tChart2.Export.Data.Text.IncludeHeader = true;
                            tChart2.Export.Data.Text.IncludeSeriesTitle = true;
                            tChart2.Export.Data.Text.TextDelimiter = ";";
                            tChart2.Export.Data.Text.TextLineSeparator = "\r\n";
                            tChart2.Export.Data.Text.Save(this.saveFileDialog1.FileName);
			            }		

                    break;

                case 3:
                     for (int i = 0; i < 2; i++)
                    {
                        tChart3[i].DateTimeFormat = "dd/MM/yyyy HH:mm:ss";
                        tChart3[i].ValueFormat = "##0.##";
                    }

                    saveFileDialog1.DefaultExt = tChart1.Export.Data.Text.FileExtension;
			        saveFileDialog1.FileName = tChart1.Name+ "."+saveFileDialog1.DefaultExt;
                    saveFileDialog1.Filter = "Text files (*.csv;*.txt)|*.csv;*.txt";
			        if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
			            {
                            tChart3.Export.Data.Text.Series = null; // export all series
                            tChart3.Export.Data.Text.IncludeLabels = true;
                            tChart3.Export.Data.Text.IncludeIndex = false;
                            tChart3.Export.Data.Text.IncludeHeader = true;
                            tChart3.Export.Data.Text.IncludeSeriesTitle = true;
                            tChart3.Export.Data.Text.TextDelimiter = ";";
                            tChart3.Export.Data.Text.TextLineSeparator = "\r\n";
                            tChart3.Export.Data.Text.Save(this.saveFileDialog1.FileName);
			            }		

                    break;

                case 4:
                     for (int i = 0; i < 3; i++)
                    {
                        tChart4[i].DateTimeFormat = "dd/MM/yyyy HH:mm:ss";
                        tChart4[i].ValueFormat = "##0.##";
                    }

                    saveFileDialog1.DefaultExt = tChart1.Export.Data.Text.FileExtension;
			        saveFileDialog1.FileName = tChart1.Name+ "."+saveFileDialog1.DefaultExt;
                    saveFileDialog1.Filter = "Text files (*.csv;*.txt)|*.csv;*.txt";
			        if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
			            {
                            tChart4.Export.Data.Text.Series = null; // export all series
                            tChart4.Export.Data.Text.IncludeLabels = true;
                            tChart4.Export.Data.Text.IncludeIndex = false;
                            tChart4.Export.Data.Text.IncludeHeader = true;
                            tChart4.Export.Data.Text.IncludeSeriesTitle = true;
                            tChart4.Export.Data.Text.TextDelimiter = ";";
                            tChart4.Export.Data.Text.TextLineSeparator = "\r\n";
                            tChart4.Export.Data.Text.Save(this.saveFileDialog1.FileName);
			            }		
                    break;

                default:
                    break;


            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //change couche de courant
            trace_courant();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime test = date_tcpip.AddMinutes(15);
            if (test < DateTime.Now)
            {
                label1.BackColor = Color.Red;
            }


        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ///fermeture
            ///

            
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
           // threadprocess.Abort();
           // th.Abort();
        }

        private void log(string str)
        {
            string s_log = DateTime.Now.ToString("HH:mm:ss") + ":\t" + str + "\n";
            LogBox.AppendText(s_log);
            LogBox.ScrollToCaret();

            log_file("log", s_log);
        }

        private void log_file(string name, string s_log)
        {
            DateTime date = DateTime.Now;
            //string parent_path = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory()).FullName;
            string logfile = name + "-" + date.ToString("yy-MM-dd") + ".txt";
            if (File.Exists(log_path + "//" + logfile) == false)
            {
                using (StreamWriter swritter = File.CreateText(log_path + "//" + logfile))
                {
                    swritter.Close();
                }
            }

            StreamWriter f = File.AppendText(log_path + "//" + logfile);
            f.Write(s_log);
            f.Close();

        }

        void TraiteEvent_MODBUS_log(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Modbus.logging_info(log), new object[] { msg });
                return;
            }
            log(msg);
        }


    }

    class ProcessingMessageThread
    {
        object stacklock;
        System.Threading.AutoResetEvent newmessagesignal;
        System.Collections.Generic.Queue<byte[]> stack;
        public delegate void mesure_SBE (DateTime timestamplog, DateTime[] date_sbe, float[] temp_sbe,/* DateTime date_compas, float compas, DateTime date_wind, float ws, float wd, float wm,*/ DateTime[] date_courant, float [] spd_cou, float [] dir_cou, DateTime date_houle,  float hm0, float tp,float tm02,float hmax,float meandir,float dirtp, DateTime date_pression, float pression);
        public event mesure_SBE affich_param_SBE;
        public delegate void mesure_vent( DateTime date_compas, float compas, DateTime date_wind, float ws, float wd, float wm);
        public event mesure_vent affich_param_vent;
        public ProcessingMessageThread()
        {
            stack = new Queue<byte[]>();
            stacklock = new object();
            newmessagesignal = new System.Threading.AutoResetEvent(false);

        }


        public void Start()
        {
            while (true)
            {
                bool finished = false;
                newmessagesignal.WaitOne();
                while (!finished)
                {
                    byte[] message = null;
                    lock (stacklock)
                    {
                        try
                        {
                            message = stack.Dequeue();
                        }
                        catch (Exception)
                        {

                        }
                    }
                    if (message != null)
                    {
                        BoueeDirectionnelle waves = null;
                        wind wind = null;

                        try
                        {
                            waves = BoueeDirectionnelle.ParseTrame(message);
                            if (affich_param_SBE != null ) affich_param_SBE(waves.Timestamplog,waves.Date_sbe,waves.Temp_sbe,/*waves.Date_compas,waves.Compas,waves.Date_wind,waves.Ws,waves.Wd,waves.Wm,*/waves.Date_courant,waves.Spd_cou,waves.Dir_cou,waves.Date_houle,waves.Hm0,waves.Tp,waves.Tm02,waves.Hmax,waves.Meandir,waves.Dirtp,waves.Date_pression,waves.Pression);
                        }
                        catch { }

                        if (waves == null)
                        {
                            try
                            {
                                wind = wind.ParseTrame(message);
                                if (affich_param_vent != null) affich_param_vent(wind.Date_compas,wind.Compas,wind.Date_wind,wind.Ws,wind.Wd,wind.Wm);
                            }
                            catch { }
                        }

                    }
                    else
                    {
                        finished = true;
                    }
                }
            }
        }
                           

       //
        public void addMessage(byte[] message)
        {
            lock (stacklock)
            {
                stack.Enqueue(message);
                newmessagesignal.Set();
            }
        }

    }
}
