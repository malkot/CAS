using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace WindowsService2
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string[] res = System.IO.File.ReadAllLines(@"c:\ConnectionString.txt");
            int kol_devices = (res.Length - 1);            // Количество приборов
            Thread[] threads = new Thread[kol_devices];    // Объявление массива потоков в количестве равном количеству приборов

            for (int i = 0; i < kol_devices; i++)          // Цикл для создания потоков
            {
                threads[i] = new Thread(Opros);            // создание потоков
                threads[i].Name = "Potok " + i.ToString(); // Имя потока
                threads[i].IsBackground = true;            // Свойство потока - фоновый
                threads[i].Start();                        // Запуск потока
            }                      
        }

        private static void Opros()
        {

            string[] res = System.IO.File.ReadAllLines(@"c:\ConnectionString.txt");
            COM COM1 = new COM();                           // Объявление объекта класса 'COM'
            COM1.num_COM = res[1];                          // Применение полю 'num_COM' значения 'COM1'
            COM1.ID = 0x31;                                 // Применение полю ID значения "1"
            COM1.Inic_COM();                                // Инициализация COM порта

            while (true)
            {
                COM1.Opros();                               // Запуска метода 'Opros'
                Thread.Sleep(100);                          // Задержка 0.1сек
            }
        }

        protected override void OnStop()
        {
        }
    }
}
