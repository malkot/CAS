using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Threading;
using System.IO;

namespace WindowsService2
{
    class COM
    {
        System.IO.Ports.SerialPort rs_port;
        public string num_COM;
        public byte ID;
        public void Inic_COM()
        {
            rs_port = new System.IO.Ports.SerialPort(num_COM, 9600,                    //Настройка COM-порта {Скорость - 9600, Чётность - НЕТ, Бит данных - 8, Стоповый бит - 1}
            System.IO.Ports.Parity.None, 8,
            System.IO.Ports.StopBits.One);

            if (rs_port.IsOpen == true)
            {
                rs_port.Close();
            }
            rs_port.Open();
        }

        public void Opros(string mysqlConnectionString)
        {                                                                                    
            SqlConnection conn = new SqlConnection(mysqlConnectionString);                            //Connection String
            byte[] data = new byte[7] { 0x44, 0x30, ID, 0x4B, 0x57, 0x0D, 0x0A };                     //Посылка согласно протокола
            rs_port.Write(data, 0, 7);                                                                //Запись посылки в порт
            System.Threading.Thread.Sleep(100);                                                       //Задержка, ожидание ответа от прибора            
            string readmsg;

            if (rs_port.BytesToRead == 22)                                                            //Определение длины посылки
            {
                byte[] answer = new byte[(int)rs_port.BytesToRead];                                   //Массив для ответа от прибора
                rs_port.Read(answer, 0, rs_port.BytesToRead);
                readmsg = Encoding.ASCII.GetString(answer);                                           //Перекодировка отвера в ASCII
                string stab, num, weight, kg;               
                stab = (readmsg.Remove(2));                                                           //Отделение признака стабильности из общей посылки 

                if (String.Equals(stab, "ST", StringComparison.CurrentCulture))                       //Если вес стабилен - идём проверять значение веса
                {
                    num = (readmsg.Remove(0, 6));                                                     //Отделение номера прибора из общей посылки 
                    num = (num.Remove(1));
                    weight = (readmsg.Remove(0, 10));                                                 //Отделение значения веса из общей посылки 
                    weight = (weight.Remove(7));
                    weight = weight.Replace(" ", "");
                    weight = weight.Replace(".", ",");
                    double weight_double = System.Convert.ToDouble(weight);                           //Конвертация значения веса из String в Double
                    if (weight_double > 15)                                                           //Если вес больше 15кг пишем в базу и тормозим поток на 10 сек.
                    {
                        kg = (readmsg.Remove(0, 18));
                        kg = (kg.Remove(2));
                        DateTime date;
                        date = DateTime.Now;
                        conn.Open();
                        string Insquery = "INSERT INTO Arhiv (date,value,id) VALUES ('" + date + "','" + weight_double + "', '" + num + "')";
                        SqlCommand cmd = new SqlCommand(Insquery, conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        Thread.Sleep(10000);
                    }
                }
            }
        }
    }
}