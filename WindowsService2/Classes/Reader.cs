using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsService2.Classes
{
    internal class Reader
    {
        private int mDeviceId;
        private string mPortName;
        private SerialPort mPort;

        public event EventHandler<ValueReadedEventArgs> ValueReaded;

        public Reader(int deviceId, string portname)
        {
            this.mDeviceId = deviceId;
            this.mPortName = portname;
            this.mPort = new System.IO.Ports.SerialPort(this.mPortName, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
        }

        public void Connect()
        {
            if (this.mPort.IsOpen == true)
            {
                this.mPort.Close();
            }

            this.mPort.PortName = this.mPortName;
            this.mPort.Open();
        }

        public void Disconnect()
        {
            if (this.mPort.IsOpen)
            {
                this.mPort.Close();
            }
        }

        public void Query(CancellationToken token)
        {
            byte id1 = Convert.ToByte((this.mDeviceId / 10) + 0x30);
            byte id2 = Convert.ToByte((this.mDeviceId % 10) + 0x30);

            // Посылка согласно протокола
            byte[] data = new byte[7] { 0x44, id1, id2, 0x4B, 0x57, 0x0D, 0x0A };

            this.mPort.DiscardInBuffer();

            //Запись посылки в порт
            this.mPort.Write(data, 0, 7);

            //Задержка, ожидание ответа от прибора
            token.WaitHandle.WaitOne(100);

            string readmsg;

            // Определение длины посылки
            if (this.mPort.BytesToRead == 22)
            {
                // Массив для ответа от прибора
                byte[] answer = new byte[this.mPort.BytesToRead];
                int readed = this.mPort.Read(answer, 0, answer.Length);

                // Перекодировка отвера в ASCII
                readmsg = Encoding.ASCII.GetString(answer);
                string stab, num, weight, kg;

                // Отделение признака стабильности из общей посылки
                stab = (readmsg.Remove(2));

                // Если вес стабилен - идём проверять значение веса
                if (String.Equals(stab, "ST", StringComparison.CurrentCulture))
                {
                    // Отделение номера прибора из общей посылки 
                    num = (readmsg.Remove(0, 6));
                    num = (num.Remove(1));

                    //Отделение значения веса из общей посылки 
                    weight = (readmsg.Remove(0, 10));
                    weight = (weight.Remove(7));
                    weight = weight.Replace(" ", "");
                    weight = weight.Replace(".", ",");

                    //Конвертация значения веса из String в Double
                    double weight_double = System.Convert.ToDouble(weight);

                    //Если вес больше 15кг пишем в базу и тормозим поток на 10 сек.
                    if (weight_double > 15)
                    {
                        kg = (readmsg.Remove(0, 18));
                        kg = (kg.Remove(2));

                        if (this.ValueReaded != null)
                        {
                            this.ValueReaded(this, new ValueReadedEventArgs(this.mDeviceId, weight_double));
                        }

                        token.WaitHandle.WaitOne(10000);
                    }
                }
            }
        }

        public class ValueReadedEventArgs : EventArgs
        {
            public double WeightValue { get; private set; }
            public int DeviceId { get; private set; }

            public ValueReadedEventArgs(int deviceId, double weightValue)
            {
                this.DeviceId = deviceId;
                this.WeightValue = weightValue;
            }
        }
    }
}
