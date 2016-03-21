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
using WindowsService2.Classes;

using ComParameters = System.Tuple<string, string>;

namespace WindowsService2
{
    public partial class Service1 : ServiceBase
    {
        private ServiceOptions mOptions;

        public Service1()
        {
            InitializeComponent();
            this.mOptions = new ServiceOptions();
        }

        protected override void OnStart(string[] args)
        {
            string filename = @"c:\ConnectionString.txt";

            if (args.Length == 1)
            {
                filename = args[0];
            }

            this.mOptions = new ServiceOptions(filename);

            int portsCount = this.mOptions.ComPortNames.Count(); // Количество портов
            Thread[] threads = new Thread[portsCount]; // Объявление массива потоков в количестве равном количеству приборов

            for (int i = 0; i < portsCount; i++) // Цикл для создания потоков
            {
                threads[i] = new Thread(new ParameterizedThreadStart(this.ReadInThread)); // создание потоков
                threads[i].Name = "Potok " + i.ToString(); // Имя потока
                threads[i].IsBackground = true; // Свойство потока - фоновый
                threads[i].Start(
                    new ComParameters(
                        this.mOptions.ComPortNames.ElementAt(i),
                        this.mOptions.ConnectionString)); // Запуск потока
            }
        }

        private void ReadInThread(object o)
        {
            ComParameters parameters = o as ComParameters;
            string portname = parameters.Item1;
            string connection = parameters.Item2;

            COM COM1 = new COM();                           // Объявление объекта класса 'COM'
            COM1.num_COM = portname;                        // Применение полю 'num_COM' значения 'COM1'
            COM1.ID = 0x31;                                 // Применение полю ID значения "1"
            COM1.Inic_COM();                                // Инициализация COM порта

            while (true)
            {
                COM1.Opros(connection);                     // Запуска метода 'Opros'
                Thread.Sleep(100);                          // Задержка 0.1сек
            }
        }

        protected override void OnStop()
        {
        }
    }
}
