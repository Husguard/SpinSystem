using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpinSystem
{
    public class Animation
    {
        public int Frames { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<List<List<byte>>> Data { get; set; }
        public void ExportToData(Spin[,] spins)
        {
            this.Data.Add(new List<List<byte>>());
            // i - номер текущего добавленного кадра
            foreach (Spin obj in spins)
            {
                if (obj.GetSpin() == 1) this.Data[this.Data.Count - 1].Add(new List<byte> { 255, 255, 255 });
                else this.Data[this.Data.Count - 1].Add(new List<byte> { 0, 0, 0 });
            }
        }
        public void ExportToFile()
        {
            using (FileStream re = File.Create("C:/Users/Админ/Desktop/file1.json"))
            {
                using (BufferedStream stream = new BufferedStream(re, 24000))
                    JsonSerializer.SerializeToStream(this, stream);
            }
        }
    }
}
