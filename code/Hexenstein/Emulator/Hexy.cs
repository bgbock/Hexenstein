﻿using System;
using System.Diagnostics;
using System.IO.Ports;

namespace Hexenstein.Emulator
{
    internal class Hexy : IDisposable
    {
        private SerialPort port;
        private int number;
        private int? inServo;
        private int? inPos;
        private bool servoCount;
        private bool posCount;

        public void ConnectToPort(string portName)
        {
            if (port != null)
            {
                if (port.IsOpen)
                    port.Close();
                port.Dispose();
            }

            port = new SerialPort(portName);

            port.DataReceived += PortDataReceived;

            port.Open();
        }

        private void PortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (port.BytesToRead != 0)
            {
                var data = (char)port.ReadChar();

                //if (data == '\r')
                //    Debug.WriteLine("\\r");
                //else if (data == '\n')
                //    Debug.WriteLine("\\n");
                //else
                //    Debug.Write(data);

                if (data == '\r' || data == '\n')
                {
                    if (posCount)
                    {
                        inPos = number;
                        posCount = false;
                    }
                    number = 0;

                    if (inServo.HasValue && inServo.Value < 32 &&
                        inPos.HasValue && (inPos.Value >= 500 && inPos.Value <= 2500))
                        SetServo(inServo.Value, inPos.Value);

                    inServo = null;
                    inPos = null;
                }
                if (data == 'V')
                    port.WriteLine("SERVOTOR_EMULATOR");
                if (data == 'C')
                    for (int i = 0; i < 32; i++)
                        SetServo(i, 1500);
                if (data == 'K')
                    for (int i = 0; i < 32; i++)
                        SetServo(i, 0);
                if (data == '#')
                {
                    number = 0;
                    servoCount = true;
                }
                if (data == 'P')
                {
                    if (servoCount)
                    {
                        inServo = number;
                        servoCount = false;
                    }
                    number = 0;
                    posCount = true;
                }
                if (data == 'L')
                {
                    if (servoCount)
                    {
                        inServo = number;
                        servoCount = false;
                    }
                    number = 0;
                    SetServo(inServo.Value, -1);
                }
                if (data == 'T')
                {
                    if (posCount)
                    {
                        inPos = number;
                        posCount = false;
                    }
                    number = 0;
                }
                if (char.IsDigit(data))
                {
                    number = number * 10 + (data - '0');
                }
            }
        }

        private void SetServo(int servo, int value)
        {
            Debug.WriteLine("Servo: {0} Value: {1}", servo, value);
        }

        #region IDisposable Members

        private bool disposed;

        ~Hexy()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Clean up managed resources.

                    if (port != null)
                        port.Dispose();
                }

                // Clean up unmanaged resources.

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }
}