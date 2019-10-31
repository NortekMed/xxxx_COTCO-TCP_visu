using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;



namespace nortekmed.network

{
    class SmallServer
    {


        public delegate void SmallServerEventHandler(object sender, SmallServerEventArgs e);
        public event SmallServerEventHandler SmallServerEvent;
       
        object lockacceptedsmallclients;
        List<SmallClient> acceptedsmallclients;
        TcpListener tcplistener;
        private int maxConnections;

        private bool listening;
        public bool Listening { get { return listening; } }

        private int port;
        public int Port { get { return port; } }
        public int MaxConnections
        {
            get { return maxConnections; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port">Port</param>
        /// <param name="maxConnections">Max Connections</param>
        public SmallServer(int port, int maxConnections)
        {
            lockacceptedsmallclients = new object();
            acceptedsmallclients = new List<SmallClient>();
            this.port = port;
            IPAddress localAddr = IPAddress.Any;
            tcplistener = new TcpListener(localAddr,port);
            this.maxConnections = maxConnections;
            if (maxConnections < 0) throw new Exception("Wrong maxConnections parameter");
        }


        public Thread  StartListening()
        {
            tcplistener.Start();
            Thread t = new Thread(new ThreadStart(ServerListenThread));
            t.IsBackground = true;
            t.Start();
            
            listening = true;
            if (SmallServerEvent !=null)
            SmallServerEvent(this, new SmallServerEventArgs(SmallServerEventArgs.kind.startlistening,"Starts listening on : "+ port,null));
            return t;
        }
        private void ServerListenThread()
        {
            bool fDone = false;
            while (!fDone)
            {
                try
                {
                    bool accept=false;
                    lock (lockacceptedsmallclients) {
                        if (acceptedsmallclients.Count < maxConnections)
                        { accept = true; }
                    }


                    if (accept)
                    {
                        Socket sockToClient = tcplistener.AcceptSocket();
                        
                        if (sockToClient.Connected)
                        {
                            SmallClient cl = new SmallClient(sockToClient);
                            cl.SmallClientEvent += new SmallClient.SmallClientEventHandler(SmallClientEventHandler);
                            lock (lockacceptedsmallclients) { acceptedsmallclients.Add(cl); }
                            // server event
                            if (SmallServerEvent != null)
                            SmallServerEvent(this,new SmallServerEventArgs(SmallServerEventArgs.kind.newclient,"New Connection "+cl.Socket.RemoteEndPoint.ToString(),cl));
                            // check again if socket still running
                            // if not eject it from the list
                            if (!cl.Socket.Connected) { RemoveClient(cl); }
                            
                            //Console.WriteLine("[tid {0}] Client connected", GetCurrentThreadId());
                            
                            //ClientHandler Handler = new ClientHandler(sockToClient);
                            //Handler.StartRead();
                        }

                    }
                }
                catch (Exception e)
                {
                  //  Console.WriteLine("[tid {0}] Exception in I/O thread - exiting.",
                    //                   GetCurrentThreadId());
                    Console.WriteLine(e.Source+" : "+e.Message);
                    fDone = true;
                }
            }

        }
        private void RemoveClient(SmallClient cl)
        {
            lock (lockacceptedsmallclients) { 
                
                acceptedsmallclients.Remove(cl);
                
            }
            if (SmallServerEvent != null)
            SmallServerEvent(this,new SmallServerEventArgs(SmallServerEventArgs.kind.stopclient,"New Connection "+cl.Socket.RemoteEndPoint.ToString(),cl));
                            
        }

        private void SmallClientEventHandler(object s, SmallClientEventArgs evts)
        {
            if (evts.Kind == SmallClientEventArgs.kind.disconnect)
            {
                RemoveClient((SmallClient)s);
            }
        }

        public void StopListening()
        {
            // stop listener
            tcplistener.Stop();
            // remove clients
            lock (lockacceptedsmallclients)
            {
                
                SmallClient[] tmp=new SmallClient[acceptedsmallclients.Count];
                acceptedsmallclients.CopyTo(tmp);
                foreach (SmallClient i in tmp)
                {
                    i.Dispose();
                }   
                

            }
            listening = false;
            // event
            if (SmallServerEvent != null)
            SmallServerEvent(this, new SmallServerEventArgs(SmallServerEventArgs.kind.stoplistening, "Server stop listen", null));
        }
        



    }



    class SmallClient : NetworkStream
    {
        public delegate void SmallClientEventHandler(object sender, SmallClientEventArgs e);
        public event SmallClientEventHandler SmallClientEvent;
        private Socket s;
        public new Socket Socket { get { return s; } }
        public SmallClient(Socket s) : base(s)
        {
            if (s == null) throw new NullReferenceException();
            this.s = s;
        }

        public int ReadBytes(Byte[] b,int timeout)
        {

            return -1;
        }

        
      
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (SmallClientEvent != null)
                    SmallClientEvent(this, new SmallClientEventArgs(SmallClientEventArgs.kind.disconnect, "Disconnect from :" + s.RemoteEndPoint.ToString()));
                base.Dispose(disposing);
            }

        }


       
    }


    class SmallServerEventArgs : EventArgs
    {
        public enum kind {startlistening, newclient,stopclient,stoplistening };
        kind ekind;
        public kind Kind { get { return ekind; } }
        String message;
        public String Message {get {return message;}}
        SmallClient smallclient;
        public SmallClient SmallClient { get { return smallclient; } }

        public SmallServerEventArgs(kind kind,string message,SmallClient client)
        {
            this.ekind = kind;
            this.message=message;
            this.smallclient = client;
        }
        
    }

    class SmallClientEventArgs : EventArgs
    {
        public enum kind { disconnect };
        kind ekind;
        public kind Kind { get { return ekind; } }
        String message;
        String Message { get { return message; } }
        public SmallClientEventArgs(kind kind,string message)
        {
            this.ekind = kind;
            this.message=message;
        }
    }
}
