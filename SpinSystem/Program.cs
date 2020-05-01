﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace SpinSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamWriter gnuplot = InitGnuplot(@"C:\Program Files\gnuplot\bin");
            List<SpinSystem> systems = new List<SpinSystem>();
            for (int i = 0; i < 4; i++)
            {
                systems.Add(new SpinSystem(1, 600));
            }
            MonteCarlo(systems,2000000);
            gnuplot.WriteLine("plot \"Capacity.txt\" using 1:2 with lines\n");
            Console.ReadLine();
        }
        static public void MonteCarlo(List<SpinSystem> systems, int NumberOfTries)
        {
            Animation data = InitData(systems[0].Length);
            Random random = new Random();
            double TempStep = 0.4;
            string path = "Capacity.txt";
            StreamWriter stream = new StreamWriter(path);
            for (double TempCounter = 0.001; TempCounter < 5; TempCounter += TempStep)
            {
                List<double> AverageEnergy = new List<double>() { 0, 0, 0, 0 };
                List<double> AverageSquareEnergy = new List<double>() { 0, 0, 0, 0 };
                for (int i = 0; i < 4; i++)
                {
                    systems[i].SetTemp(TempCounter + (0.1 * i));
               //     data.ExportToData(systems[i].spins);
                }
                
                for (int n = 0; n <= NumberOfTries; n++) // нужно чтобы каждый поток делал number итераций
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int EnergyNow = systems[i].SwapSpin();
                        AverageEnergy[i] += EnergyNow / NumberOfTries; // неправильные расчеты?
                        AverageSquareEnergy[i] += EnergyNow * EnergyNow / NumberOfTries;
                        if ((n % 1000000) == 0) data.ExportToData(systems[i].spins);
                    }
                    if ((n % 100000) == 0)
                    {
                        for (int i = 0; i <= 2; i++)
                        {
                        //    Console.WriteLine("{0}", CheckSwap(systems[i], systems[i - 1]));
                            if (random.NextDouble() < CheckSwap(systems[i], systems[i + 1])) // checkswap большие значения
                            {
                                Spin[,] tmp;
                                tmp = systems[i].spins;
                                systems[i].spins = systems[i + 1].spins;
                                systems[i + 1].spins = tmp;
                        //        Console.WriteLine("Swapped {0} and {1}", i, i + 1);
                            }
                        }
                    }
                }
                Console.WriteLine("{0} : {1}", systems[0].Temp, systems[0].OldEnergy);
                //      double result = (CalculateHeatCapacity(systems[3], AverageEnergy[3], AverageSquareEnergy[3]));
                //     Console.WriteLine("{0} : {1}", systems[3].Temp, result);
                //   stream.Write("{0} {1}\n", systems[3].Temp.ToString().Insert(TempCounter.ToString().Length, ".000000"), Math.Round(result, 6).ToString().Replace(',', '.'));
            }
            data.ExportToFile();
            stream.Flush();
            stream.Close();
        }
        static public double CheckSwap(SpinSystem first, SpinSystem second)
        {
            double test = (first.OldEnergy - second.OldEnergy) * ((1 / first.Temp) - (1 / second.Temp));
            return Math.Pow(1,test);
        }
        static public Animation InitData(int length)
        {
            Animation export = new Animation();
            export.Frames = 150;
            export.Width = length;
            export.Height = length;
            export.Data = new List<List<List<byte>>>();
            return export;
        }
        static void TaskAsync(object system)
        {
            SpinSystem test = (SpinSystem)system;
            test.SwapSpin();
        }
        static double CalculateHeatCapacity(SpinSystem system, double Energy, double SquareEnergy)
        {
            return (SquareEnergy - Math.Pow(Energy, 2)) / (system.Temp * system.Temp);
            
        }
        static StreamWriter InitGnuplot(string PathToGnuplot)
        {
            StreamWriter GnupStWr;
            Process ExtPro;
            ExtPro = new Process();
            if (PathToGnuplot[PathToGnuplot.Length - 1].ToString() != @"\")
                PathToGnuplot += @"\";
            ExtPro.StartInfo.FileName = PathToGnuplot + "gnuplot.exe";
            ExtPro.StartInfo.UseShellExecute = false;
            ExtPro.StartInfo.RedirectStandardInput = true;
            ExtPro.Start();
            GnupStWr = ExtPro.StandardInput;
            return GnupStWr;
        }
    }
    class SpinSystem
    {
        public Spin[,] spins;
        public int Length;
        public double Temp;
        public Spin J;
        Random random;
        public int OldEnergy, NewEnergy;
        public SpinSystem(int jay, int length)
        {
            random = new Random();
            this.Length = length;
            spins = new Spin[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    spins[i, j] = new Spin(random.Next(1, 2));
                }
            }
            J = new Spin(jay);
            OldEnergy = this.GetEnergySystem();
        }
        public void SetTemp(double temp)
        {
            Temp = temp;
        }
        public int GetEnergySystem()
        {
            int sum = 0;
            for(int i = 0; i < this.Length; i++)
            {
                for(int j = 0; j < this.Length; j++)
                {
                    sum += Neighbours(i,j);
                }
            }
            return -J.GetSpin() * sum;
        }
        public void PrintSystem()
        {
            for (int i = 0; i < this.Length; i++)
            {
                for (int j = 0; j < this.Length; j++)
                {
                    Console.Write("{0} ", spins[i, j].GetSpin());
                }
                Console.WriteLine();
            }
        }
        public int Neighbours(int i, int j)
        {
            Spin center = spins[i, j];
            int sum = center.GetSpin() * 
                (spins[Overflow(i + 1), Overflow(j)].GetSpin() +
                spins[Overflow(i - 1), Overflow(j)].GetSpin() +
                spins[Overflow(i), Overflow(j + 1)].GetSpin() +
                spins[Overflow(i), Overflow(j - 1)].GetSpin());
            return sum;
        }
        private int Overflow(int obj)
        {
            return (this.Length + obj) % this.Length;
        }
        // так к сведению - операции сохранения завязаны на главном классе, что неверно
        public int SwapSpin()
        {
            Random random = new Random();
            int i, j, RotatedSpin, SumEnergy = 0;
            i = random.Next(0, this.Length);
            j = random.Next(0, this.Length);
            RotatedSpin = this.spins[i, j].GetSpin();
            //  EnergyBefore = Neighbours(i, j);
            this.spins[i, j].SetSpin(-RotatedSpin);
            this.NewEnergy = this.OldEnergy + 2 * this.J.GetSpin() * this.Neighbours(i, j);
            double dE = (this.NewEnergy - this.OldEnergy) / this.Temp;
            if (random.NextDouble() > Math.Pow(Math.E, dE))
            {
                spins[i, j].SetSpin(RotatedSpin);
                SumEnergy += OldEnergy;
            }
            else
            {
                SumEnergy += NewEnergy;
                OldEnergy = NewEnergy;
            }
            return SumEnergy;
        }
    }
}