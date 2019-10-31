using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace nortekmed.network
{
    class SmallServerModem
    {
        //private byte ack_command = (byte)'A';
        //private byte ack_message_command = (byte)'$';
        //private byte ack_clock_command = (byte)'#';

        public delegate void SmallServerModemEventHandler(object sender, SmallServerModemEventArgs e);
        public event SmallServerModemEventHandler SmallServerModemEvent;
        
        private enum MachineState { idle, connectiontest1 ,messagesizeanddata};
        public NetworkStream NetworkStream { 
            get { return s; }

            set { if (value == null) throw new ArgumentException("No NetworkStream input in the parser"); s = value; }
        }


        private NetworkStream s;
 
        public SmallServerModem()
        {
           

        }
        
        // start the parser (to use for entry point in a thread)
        public void Start()
        {

            //Console.WriteLine("Thread start : " + Thread.CurrentThread.Name );

            StringBuilder sb = new StringBuilder(); ;

            if (s == null) throw new ArgumentException("No NetworkStream input in the parser");
            
            s.ReadTimeout = 200000;
            byte[] buffer=new byte[1024];
            bool running = true ;
            while (running)
            {
                try
                {

                    if (s.Read(buffer, 0, 2) == 2)
                    {
                        UInt16 length = (UInt16)(buffer[1] * 256 + buffer[0]);
                        byte[] buffer2 = new byte[length];

                        s.ReadTimeout = 200000;
                        int nbread = s.Read(buffer2, 0, length);
                        if (nbread < length) nbread += s.Read(buffer2, nbread, length - nbread);
                        if (nbread == length)
                        {
                            //s.WriteByte(ack_message_command); // @ todo check good number of bytes written

                            if (SmallServerModemEvent != null) SmallServerModemEvent(this, new SmallServerModemEventArgs(SmallServerModemEventArgs.kind.datamessage, buffer2));

                        }
                        else
                        {
                            running = false;
                        }
                    }
                    else
                    {
                        // error
                        running = false;
                    }
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Source + " : " + e.Message);
                    running = false;
                }
                catch (ObjectDisposedException e)
                {
                    Console.WriteLine(e.Source + " : " + e.Message);
                    running = false;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine(e.Source + " : " + e.Message);
                    running = false;
                }
               

            }
            if (SmallServerModemEvent != null)
                SmallServerModemEvent(this, new SmallServerModemEventArgs(SmallServerModemEventArgs.kind.disconnection));
            try
            {
                s.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Source + " : " + e.Message);
            }
            Console.WriteLine("Thread stop : " + Thread.CurrentThread.Name);
        }



    }

    class SmallServerModemEventArgs : EventArgs
    {
        public enum kind {connectiontest,protocolerror,disconnection,networkerror, datamessage, asciimessage };
        kind ekind;
        public kind Kind { get { return ekind; } }
        byte[] message;
        public Byte[] Message { get { return message; } }

        public SmallServerModemEventArgs(kind kind)
        {
            this.ekind = kind;

        }
        public SmallServerModemEventArgs(kind kind, byte[] message)
        {
            this.ekind = kind;
            this.message = message;            
        }
    }
}
