using System;
using System.Collections.Generic;
using System.Text;

namespace visu_cotco
{


    class Trame_COTCO
    {

        private DateTime[] date_sbe;
        float[] temp_sbe;
       // DateTime date_compas;
       // float compas;
       // DateTime date_wind;
       // float ws, wd, wm;
        DateTime [] date_courant;
        float[] spd_cou;
        float[] dir_cou;
        DateTime date_houle;
        float hm0;
        float tp;
        float tm02;
        float hmax;
        float meandir;
        float dirtp;
        DateTime date_pression;
        float pression;

        public DateTime Date_pression
        {
            get { return date_pression; }
            set { date_pression = value; }
        }

        public float Pression
        {
            get { return pression; }
            set { pression = value; }
        }
        private DateTime timestamplog;

        public DateTime Timestamplog
        {
            get { return timestamplog; }
            set { timestamplog = value; }
        }
       

        public DateTime[] Date_sbe
        {
            get { return date_sbe; }
            set { date_sbe = value; }
        }

        public float[] Temp_sbe
        {
            get { return temp_sbe; }
            set { temp_sbe = value; }
        }

      /*  public DateTime Date_compas
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
        }*/

        public DateTime[] Date_courant
        {
            get { return date_courant; }
            set { date_courant = value; }
        }

        public float[] Spd_cou
        {
            get { return spd_cou; }
            set { spd_cou = value; }
        }

        public float[] Dir_cou
        {
            get { return dir_cou; }
            set { dir_cou = value; }
        }

        public DateTime Date_houle
        {
            get { return date_houle; }
            set { date_houle = value; }
        }

        public float Meandir
        {
            get { return meandir; }
            set { meandir = value; }
        }
       

        public float Dirtp
        {
            get { return dirtp; }
            set { dirtp = value; }
        }

        public float Tm02
        {
            get { return tm02; }
            set { tm02 = value; }
        }

        public float Tp
        {
            get { return tp; }
            set { tp = value; }
        }

        public float Hm0
        {
            get { return hm0; }
            set { hm0 = value; }
        }

        public float Hmax
        {
            get { return hmax; }
            set { hmax = value; }
        }

        
        public Trame_COTCO( DateTime timestamplog, DateTime[] date_sbe, float[] temp_sbe,/* DateTime date_compas, float compas, DateTime date_wind, float ws, float wd, float wm,*/ DateTime[] date_courant, float [] spd_cou, float [] dir_cou, DateTime date_houle,  float hm0, float tp,float tm02,float hmax,float meandir,float dirtp,DateTime date_pression, float pression)
        {
            this.timestamplog = timestamplog;
            this.date_sbe = date_sbe;
            this.temp_sbe = temp_sbe;
           /* this.date_compas = date_compas;
            this.compas = compas;
            this.date_wind = date_wind;
            this.ws = ws;
            this.wd = wd;
            this.wm = wm;*/
            this.date_courant = date_courant;
            this.spd_cou = spd_cou;
            this.dir_cou = dir_cou;
            this.date_houle = date_houle;
            this.tp = tp;
            this.hm0 = hm0;
            this.meandir = meandir;
            this.tm02 = tm02;
            this.dirtp = dirtp;
            this.hmax = hmax;
            this.date_pression = date_pression;
            this.pression = pression;
            
        }

        public static Trame_COTCO ParseTrame(Byte[] bytes)
        {
            DateTime[] date_sbe = new DateTime[6];
            float[] temp_sbe = new float[6];
            DateTime date_compas;
            float compas;
            DateTime date_wind;
            float ws,wd,wm;
            DateTime[]date_courant = new DateTime[10];
            float[] spd_cou = new float[10];
            float[] dir_cou = new float[10];
            DateTime date_houle;
            float hm0, tp, tm02, hmax, meandir, dirtp;
            DateTime date_pression;
            float pression;

            int index = 0;
            Int16 word = -19060; ;
           
            for (int i = 0; i < 154; i++)
            {
                word = (Int16)(word + BitConverter.ToInt16(bytes, i * 2));
            }
            Int16 word_1 = BitConverter.ToInt16(bytes, 308);

            if (word != word_1) throw new Exception();

            for (int i = 0 ; i < 6 ; i++)
            {
                date_sbe[i] = DateTime.FromOADate(BitConverter.ToDouble(bytes,index));
                index += 8;
                temp_sbe[i] = BitConverter.ToSingle(bytes, index);
                index += 4;
            }

            //date_compas = DateTime.FromOADate(BitConverter.ToDouble(bytes, index));
            index += 8;
            //compas = BitConverter.ToSingle(bytes, index);
            index += 4;

            //date_wind = DateTime.FromOADate(BitConverter.ToDouble(bytes, index));
            index += 8;
            //ws = BitConverter.ToSingle(bytes, index);
            index += 4;
            //wd = BitConverter.ToSingle(bytes, index);
            index += 4;
            //wm = BitConverter.ToSingle(bytes, index);
            index += 4;

           
            for (int i = 0; i < 10; i++)
            {
                 date_courant[i] = DateTime.FromOADate(BitConverter.ToDouble(bytes, index));
                 index += 8;
                spd_cou[i] = BitConverter.ToSingle(bytes, index);
                index += 4;
                dir_cou[i] = BitConverter.ToSingle(bytes, index);
                index += 4;
            }
            
            date_houle = DateTime.FromOADate(BitConverter.ToDouble(bytes, index));
            index += 8;
            hm0 = BitConverter.ToSingle(bytes, index);
            index += 4;
            tm02 = BitConverter.ToSingle(bytes, index);
            index += 4;
            tp = BitConverter.ToSingle(bytes, index);
            index += 4;
            hmax = BitConverter.ToSingle(bytes, index);
            index += 4;
            dirtp = BitConverter.ToSingle(bytes, index);
            index += 4;
            meandir = BitConverter.ToSingle(bytes, index);
            index += 4;
            date_pression = DateTime.FromOADate(BitConverter.ToDouble(bytes, index));
            index += 8;
            pression = BitConverter.ToSingle(bytes, index);
            index += 4;

            DateTime timestamplog = DateTime.Now;

            return new Trame_COTCO( timestamplog, date_sbe, temp_sbe, /* date_compas,  compas, date_wind,  ws,  wd,  wm,*/  date_courant,  spd_cou, dir_cou,  date_houle,   hm0,  tp, tm02, hmax,  meandir, dirtp,date_pression,pression);
           
        }


    }
}
