﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace visu_cotco
{
    class Modbus
    {
        public delegate void logging_info(string msg);
        public event logging_info logging_info_evt;

        SerialPort serial1 ;

         object loc_aenvoyer;
         private byte[] _messageReceived = new byte[8];
         private byte _slaveAddress = 1;
         private int _slaveDelay = 0;
         public delegate void reponse_modbus(DateTime heure_mesure_SBE);
         public event reponse_modbus reponse_modbus1;


        public void start_prog()
        {

            try
            {

                serial1.ReadTimeout = 500;
                serial1.DiscardNull = false; 
                serial1.RtsEnable = true;
                serial1.DtrEnable = true;
                serial1.ReadBufferSize = 2048;
                serial1.DataReceived += new SerialDataReceivedEventHandler(serial1_DataReceived);
                serial1.Open();
                if (logging_info_evt != null) logging_info_evt("Port série Modbus ouvert.");
            }
            catch {
                if (logging_info_evt != null) logging_info_evt("Problème démarrage Modbus.");
            }
           
             
         

        }

        

        public void Stop_sbe()
        {
            try
            {
                serial1.Close();
                if (logging_info_evt != null) logging_info_evt("Fermeture port série Modbus.");
            }
            catch { }

        }

        private byte[] createRespondMessage()
        {
            int numberOfPoints = 0;
            int bytesToSend = 0;
            int startAddress = 0;
            numberOfPoints = (_messageReceived[4] << 8) | _messageReceived[5];
            bytesToSend = 5 + numberOfPoints * 2;
            byte[] respondMessage = new byte[bytesToSend];
            respondMessage[0] = _slaveAddress;
            respondMessage[1] = 3;
            respondMessage[2] = Convert.ToByte(2 * numberOfPoints);
            startAddress = (_messageReceived[2] << 8) | _messageReceived[3];
            lock (loc_aenvoyer)
            {
                
                int j = 0;
                for (int i = 0; i < numberOfPoints; i++)
                {
                    respondMessage[i + j + 3] = Convert.ToByte((Form1.aenvoyer[startAddress + i] >> 8) & 0xff);
                    respondMessage[i + j + 4] = Convert.ToByte(Form1.aenvoyer[startAddress + i] & 0xff);
                    j++;
                }
            }
            byte[] crcCalculation = CRCStuff.calculateCRC(ref respondMessage, bytesToSend - 2);
            respondMessage[bytesToSend - 2] = crcCalculation[0];
            respondMessage[bytesToSend - 1] = crcCalculation[1];
            return respondMessage;
        }

        public Modbus(SerialPort p)
        {
            //
            serial1 = p;
            loc_aenvoyer = new object();
               
        }

         string mot;
         void serial1_DataReceived(object sender, SerialDataReceivedEventArgs e)
         {

             while (serial1.BytesToRead >= 8)
             {
                 serial1.Read(_messageReceived, 0, 8);
                 //addLog(createLogStr(ref _messageReceived), LogType.RX);
                 if (_messageReceived[0] == _slaveAddress && _messageReceived[1] == 3)
                 {
                     if (CRCStuff.checkCRC(ref _messageReceived, 8))
                     {
                         DateTime test = Form1.date_tcpip.AddMinutes(15);
                         if (test > DateTime.Now)
                         {
                             byte[] messageToSend = createRespondMessage();
                             System.Threading.Thread.Sleep(_slaveDelay);
                             serial1.Write(messageToSend, 0, messageToSend.Length);
                             if (reponse_modbus1 != null) reponse_modbus1(DateTime.Now);
                         }

                     }
                     else
                     {
                        if ( logging_info_evt!=null) logging_info_evt("Modbus, CRC erreur.");
                         // on vide le buffer
                         if (serial1.BytesToRead > 0)
                         {
                             byte[] buf_1 = new byte[serial1.BytesToRead];
                             serial1.Read(buf_1, 0, serial1.BytesToRead);
                         }
                     }
                 }
                 else
                 {
                    if (logging_info_evt != null) logging_info_evt("Modbus, adresse erreur.");
                    // on vide le buffer
                    if (serial1.BytesToRead > 0)
                     {
                         byte[] buf_1 = new byte[serial1.BytesToRead];
                         serial1.Read(buf_1, 0, serial1.BytesToRead);
                     }
                 }
             }

         }

    }
    public static class CRCStuff
    {
        static byte[] crcHi = {
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
        0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
        0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81,
        0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
        0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
        0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
        0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
        0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
        0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
        0x40
        };

        static byte[] crcLo = {
        0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7, 0x05, 0xC5, 0xC4,
        0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
        0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD,
        0x1D, 0x1C, 0xDC, 0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
        0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32, 0x36, 0xF6, 0xF7,
        0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
        0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE,
        0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
        0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1, 0x63, 0xA3, 0xA2,
        0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
        0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB,
        0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
        0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0, 0x50, 0x90, 0x91,
        0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
        0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88,
        0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
        0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83, 0x41, 0x81, 0x80,
        0x40
        };

        public static byte[] calculateCRC(ref byte[] messageArray, int dataLength)
        {
            byte usCRCHi = 0xFF;
            byte usCRCLo = 0xFF;
            byte[] returnResult = { 0x00, 0x00, 0x00 };
            int index = 0;
            int messageIndex = 0;
            while (dataLength > 0)
            {
                index = usCRCLo ^ messageArray[messageIndex];
                usCRCLo = Convert.ToByte(usCRCHi ^ crcHi[index]);
                usCRCHi = crcLo[index];
                messageIndex++;
                dataLength--;
            }
            //0th item is crcLo
            returnResult[0] = usCRCLo;
            //1st item is crcHi
            returnResult[1] = usCRCHi;
            //2nd item is the total CRC16.
            //returnResult[2] = Convert.ToByte((usCRCHi << 8 | usCRCLo));
            return returnResult;
        }

        public static bool checkCRC(ref byte[] messageToCheck, int numberOfBytes)
        {
            byte[] calculatedCRC;
            calculatedCRC = calculateCRC(ref messageToCheck, numberOfBytes - 2);
            if (calculatedCRC[0] == messageToCheck[numberOfBytes - 2] && calculatedCRC[1] == messageToCheck[numberOfBytes - 1]) return true;
            return false;
        }


    }
}
