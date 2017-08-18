using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;
using System.IO;

namespace BKSB2Obj
{
    class Program
    {
        
        [STAThread]
        static void Main(string[] args)
        {
            

            Console.WriteLine("Temple Run *.BKSB Converter");
            Console.WriteLine("Written By Eric V. At the VG-Resource.");
            Console.WriteLine("http://www.vg-resource.com");
            Console.WriteLine();

            Console.WriteLine("Type \"s\" for singular file extraction...");
            Console.WriteLine();
            Console.WriteLine("Type \"b\" for singular file extraction...");
            Console.WriteLine();
            ConsoleKey read = Console.ReadKey().Key;

            if (read == ConsoleKey.S)
            {
                Console.Clear();
                Console.WriteLine("Select the input BKSB File...");
                Console.WriteLine();
                OpenFileDialog openfile = new OpenFileDialog();
                openfile.DefaultExt = "*.BKSB";

                openfile.Filter = "Temple Run BKSB (*.bksb)|*.BKSB*";




                if (openfile.ShowDialog() == DialogResult.OK)
                {

                    try
                    {
                        string filedir = openfile.FileName;
                        string BKSBName = Path.GetFileNameWithoutExtension(filedir);

                        Console.WriteLine("Choose the output directory...");
                        VistaFolderBrowserDialog foldersaveloc = new VistaFolderBrowserDialog();
                        foldersaveloc.ShowDialog();
                        string folderloc = foldersaveloc.SelectedPath;
                        Console.Clear();
                        ConvertModel(filedir, folderloc, BKSBName);
                        Console.Out.WriteLine();
                        Console.Out.WriteLine("Conversion complete! Press any key to exit...");
                        Console.ReadKey();

                    }
                    catch (Exception e)
                    {
                        Console.Write(e);
                    }

                }

                else Console.WriteLine("No input file selected!");



                Console.ReadKey();



            } //Singular Extraction


            else if(read == ConsoleKey.B) //Implement Batch Extraction here
            {
                Console.Clear();
                Console.Out.WriteLine("Choose the directory of the BKSB files...");
                Console.WriteLine();
                try
                {

                    VistaFolderBrowserDialog opendir = new VistaFolderBrowserDialog();
                    opendir.ShowDialog();
                    string ChoosenOpenPath = opendir.SelectedPath;

                    string[] files = Directory.GetFiles(ChoosenOpenPath); 


                    VistaFolderBrowserDialog closedir = new VistaFolderBrowserDialog();
                    Console.WriteLine("Choose the output directory...");
                    closedir.ShowDialog();
                    string OutputDir = closedir.SelectedPath;
                    Console.Clear();
                    

                    Boolean bksbfound = false;

                    

                    for(int i = 0; i < files.Length; i++)
                    {
                        string BKSBName = Path.GetFileNameWithoutExtension(files[i]);

                        if (Path.GetExtension(files[i]) != ".bksb") continue;

                        else
                        {
                            ConvertModel(files[i], OutputDir, BKSBName);
                            bksbfound = true;
                        }


                    }

                    if (bksbfound)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Conversion complete! Press any key to exit...");
                        Console.ReadKey();
                        
                    }
                    else
                    {
                       
                        Console.WriteLine("No BKSB files found in this directory! Press any key to exit...");
                        Console.ReadKey();
                    }

                





                }
                catch(Exception e)
                {
                    Console.Write(e);
                }
                Console.ReadKey();

            }
            else
            {
                Console.Clear();
                Console.WriteLine("Invalid conversion method selected!!!");
                Console.ReadKey();
            }
            
        }

        public static void ConvertModel(string filelink, string outdir, string BKSBName)
        {
            BinaryReader bksb = new BinaryReader(new FileStream(filelink, FileMode.Open));

            int MagicNumber = (int)bksb.ReadUInt32();
            bksb.BaseStream.Seek(5, SeekOrigin.Begin);
            int VertHeader = ( (int) bksb.ReadUInt32() * 16) + 9;
            int VertBuffer = VertHeader + 0x2F;
            if (MagicNumber == 0x476) VertBuffer += 0x13;
            bksb.BaseStream.Seek(VertHeader + 4, SeekOrigin.Begin);
            int VertCount = (int)bksb.ReadUInt32();
            int VertStride = (int)bksb.ReadUInt32();


            Vector3[] coordinates = new Vector3[VertCount];
            Vector2[] UVs = new Vector2[VertCount];



            float[] uvx = new float[VertCount];
            float[] uvy = new float[VertCount];


            for(int i = 0; i < VertCount; i++)
            {
                bksb.BaseStream.Seek(VertBuffer + i * VertStride, SeekOrigin.Begin);
                coordinates[i] = new Vector3(bksb.ReadSingle(), bksb.ReadSingle(), bksb.ReadSingle());
                UVs[i] = new Vector2(bksb.ReadSingle(), bksb.ReadSingle());
            }

            bksb.BaseStream.Seek(VertBuffer + VertCount * VertStride, SeekOrigin.Begin);

            int IdxCount = (int) bksb.ReadUInt32();
            bksb.BaseStream.Seek(0xc, SeekOrigin.Current);

            UInt16[] faceArray = new UInt16[IdxCount];

            for(int i = 0; i < IdxCount; i++)
            {
                faceArray[i] = bksb.ReadUInt16();
            }
            bksb.Close();

            ConvertOBJ(coordinates, UVs, faceArray, BKSBName, outdir);
            Console.WriteLine("Converted " + BKSBName + ".bksb" + " to " + outdir + "\\" + BKSBName + ".obj");
        }


        public static void ConvertOBJ(Vector3[] coordinates, Vector2[] UVs, UInt16[] faceArray, string BKSBName, string OutDir)
        {
            string SaveDirectory = OutDir + "\\"  + BKSBName + ".obj";
            StringBuilder Output = new StringBuilder();
            Output.AppendLine("# BKSB to OBJ Exporter By Eric Van Hoven");
            Output.AppendLine(null);

            for(int i = 0; i < coordinates.Length; i++)
            {
                Output.AppendLine(string.Format("v {0} {1} {2}", coordinates[i].X, coordinates[i].Y, coordinates[i].Z));
            }
            Output.AppendLine(null);
            for (int i = 0; i < UVs.Length; i++)
            {
                Output.AppendLine(string.Format("vt {0} {1} 0.0000", UVs[i].X, UVs[i].Y));
            }
            Output.AppendLine(null);
            for (int i = 0; i < faceArray.Length; i+= 3)
            {

                int p1 = (int)faceArray[i] + 1;
                int p2 = (int)faceArray[i + 1] + 1;
                int p3 = (int)faceArray[i + 2] + 1;
                Output.AppendLine(string.Format(string.Format("f {0}//{1} {2}//{3} {4}//{5} ", p1, p1, p2, p2, p3, p3)));
            }

            File.WriteAllText(SaveDirectory, Output.ToString());




        }
        
        

    }

    class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }

    class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }


}
