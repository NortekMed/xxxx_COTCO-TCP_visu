using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace visu_cotco
{
    class wind
    {
        
        DateTime date_compas;
        float compas;
        DateTime date_wind;
        float ws, wd, wm;
      
        private DateTime timestamplog;

        public DateTime Timestamplog
        {
            get { return timestamplog; }
            set { timestamplog = value; }
        }
       

      
        public DateTime Date_compas
        {
            get { return date_compas; }
            set { date_compas = value; }
        }

        public float Compas
        {
            get { return compas; }
            set { compas = value; }
        }

        public DateTime Date_wind
        {
            get { return date_wind; }
            set { date_wind = value; }
        }

        public float Ws
        {
            get { return ws; }
            set { ws = value; }
        }

        public float Wd
        {
            get { return wd; }
            set { wd = value; }
        }

        public float Wm
        {
            get { return wm; }
            set { wm = value; }
        }

      

        
        public wind( DateTime timestamplog,  DateTime date_compas, float compas, DateTime date_wind, float ws, float wd, float wm)
        {
            this.timestamplog = timestamplog;          
            this.date_compas = date_compas;
            this.compas = compas;
            this.date_wind = date_wind;
            this.ws = ws;
            this.wd = wd;
            this.wm = wm;            
        }

        public static wind ParseTrame(Byte[] bytes)
        {
          
            DateTime date_compas;
            float compas;
            DateTime date_wind;
            float ws,wd,wm;
          

            int index = 0;
            Int16 word = -19060; ;
           
            for (int i = 0; i < 16; i++)
            {
                word = (Int16)(word + BitConverter.ToInt16(bytes, i * 2));
            }
            Int16 word_1 = BitConverter.ToInt16(bytes, 32);

            if (word != word_1) throw new Exception();

           

            date_compas = DateTime.FromOADate(BitConverter.ToDouble(bytes, index));
            index += 8;
            compas = BitConverter.ToSingle(bytes, index);
            index += 4;

            date_wind = DateTime.FromOADate(BitConverter.ToDouble(bytes, index));
            index += 8;
            ws = BitConverter.ToSingle(bytes, index);
            index += 4;
            wd = BitConverter.ToSingle(bytes, index);
            index += 4;
            wm = BitConverter.ToSingle(bytes, index);
            index += 4;

           

            DateTime timestamplog = DateTime.Now;

            return new wind( timestamplog,  date_compas,  compas, date_wind,  ws,  wd,  wm);
           
        }
    }
}
