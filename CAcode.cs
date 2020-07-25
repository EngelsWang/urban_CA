using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace CA
{
    class Program
    {
        static void Main()
        {
            int row=0, col=0, noDataValue=0,Ks=5;
            int Rfa = 100, Pthreshold = 20;
            string outPath =@"out.txt",maskPath= @"pgfile.txt";
            string beginFile = @"urban2001.txt";
            string pgFile = @"pgfile.txt", unsuitableFile = "unsuitable.txt";
            Program n = new Program();
            n.LoadMaskData(ref row, ref col,ref noDataValue,maskPath, outPath);
            n.CaCore(row,col, noDataValue,Ks, Rfa, Pthreshold,beginFile, pgFile, unsuitableFile);
        }
        public void LoadMaskData(ref int row,ref int col,ref int noDataValue,string maskPath,string outPath)
        {
            string Text;
            Text = File.ReadAllText(maskPath, Encoding.ASCII);
            string[] Text1 = Regex.Split(Text, "\n");
            File.WriteAllText(outPath, Text1[0]);
            StreamWriter sw = File.AppendText(outPath);
            for (int i = 1; i <= 5; i++)
            {
                sw.Write(Text1[i]);
            }

            sw.Flush();
            sw.Close();
            sw.Dispose();
            //读取行号
            string[] Text2 = Regex.Split(Text1[0], " +");
            col = int.Parse(Text2[1]);
            //读取列号
            Text2 = Regex.Split(Text1[1], " +");
            row = int.Parse(Text2[1]);
            Text2 = Regex.Split(Text1[5], " +");
            noDataValue = int.Parse(Text2[1]);
        }
        public void GetFig(ref int[] figdata,string path)
        {
            Console.WriteLine("Loading " + path+ "...");
            string Text;
            Text = File.ReadAllText(path, Encoding.ASCII);

            string[] Text1 = Regex.Split(Text, "\n");
            //读取行号
            string[] Text2 = Regex.Split(Text1[0], " +");
            int col = int.Parse(Text2[1]);
            //读取列号
            Text2 = Regex.Split(Text1[1], " +");
            int row = int.Parse(Text2[1]);
            for (int i = 0; i < row; i++)
            {
                string[] Text3 = Regex.Split(Text1[6+i], " +");
                for (int j = 0; j < col; j++)
                {
                    int num = i * col + j;
                    figdata[num] = int.Parse(Text3[j]);
                }
            }
        }

        public void GetFig(ref double[] figdata, string path)
        {
            Console.WriteLine("Loading " + path + "...");
            string Text;
            Text = File.ReadAllText(path, Encoding.ASCII);

            string[] Text1 = Regex.Split(Text, "\n");
            //读取行号
            string[] Text2 = Regex.Split(Text1[0], " +");
            int col = int.Parse(Text2[1]);
            //读取列号
            Text2 = Regex.Split(Text1[1], " +");
            int row = int.Parse(Text2[1]);
            for (int i = 0; i < row; i++)
            {
                string[] Text3 = Regex.Split(Text1[6 + i], " +");
                for (int j = 0; j < col; j++)
                {
                    int num = i * col + j;
                    figdata[num] = double.Parse(Text3[j]);
                }
            }
        }

        public void CaCore(int row, int col,int noDataValue,int Ks, int Rfa, 
            int Pthreshold,string beginFile, string pgFile, string unsuitableFile)
        {
            //文件路径
            string Path1 = beginFile;
            //string Path2 = "urban2005.txt";
            string Path3 = pgFile;
            string Path4 = unsuitableFile;
            //声明变量
            int[] dataBegin = new int[row * col];
            //int[] dataFinal = new int[row * col];
            int[] dataTemp = new int[row * col];
            double[] dataPg = new double[row * col];
            double[] dataUnsuitable = new double[row * col];
            //读取数据
            GetFig(ref dataBegin, Path1);
            //GetFig(ref dataFinal, Path2);
            GetFig(ref dataPg, Path3);
            GetFig(ref dataUnsuitable, Path4);
            for (int i=0;i<Ks;i++)
            {
                CaCoreOne(row, col, noDataValue, Rfa, Pthreshold, ref dataBegin,ref dataTemp,ref dataPg,ref dataUnsuitable);
                dataTemp.CopyTo(dataBegin, 0);
            }
            string[] dataout = new string[col];
            Console.WriteLine("Saving...");
            StreamWriter sw = File.AppendText("out.txt");
            for (int i = 0; i < row; i++)
            {
                int j;
                for (j = 0; j < col; j++)
                {
                    dataout[j] = dataTemp[i * col + j].ToString();
                }
                string outline=String.Join(" ", dataout);
               
                //Console.WriteLine("write"+i.ToString()+" row");
                sw.WriteLine(outline);
            }
            
            
            sw.Flush();
            sw.Close();
            sw.Dispose();


            Console.WriteLine("Finshed!");
        }
        public void CaCoreOne(int row, int col,int noDataValue,int Rfa,int Pthreshold,ref int[] dataBegin, 
            ref int[] dataTemp, ref double[] dataPg, ref double[] dataUnsuitable)
        {
            Random rdm = new Random();
            for(int i=0;i<row;i++)
                for(int j=0;j<col;j++)
                {
                    //计算数据的一维坐标；
                    int num = col * i + j;
                    if (dataBegin[num] == noDataValue || dataBegin[num] == 1 || dataBegin[num] == 2)
                        dataTemp[num] = dataBegin[num];
                    else
                    {
                        //计算领域影响
                        double con = 0;
                        int tempI, tempJ;
                        tempI = i - 1;
                        if (tempI >= 0)
                        { 
                            tempJ = j - 1;
                            if(tempJ>=0)
                            {
                                if (dataBegin[tempI * col + tempJ] == 1)
                                    con++;
                            }
                            tempJ = j;
                            if (dataBegin[tempI * col + tempJ] == 1)
                                con++;
                            tempJ = j+1;
                            if (tempJ < col)
                            {
                                if (dataBegin[tempI * col + tempJ] == 1)
                                    con++;
                            }
                            
                        }
                        tempI = i;
                        tempJ = j - 1;
                        if (tempJ >= 0)
                        {
                            if (dataBegin[tempI * col + tempJ] == 1)
                                con++;
                        }
                        tempJ = j + 1;
                        if (tempJ < col)
                        {
                            if (dataBegin[tempI * col + tempJ] == 1)
                                con++;
                        }
                        tempI = i + 1;
                        if (tempI < row)
                        {
                            tempJ = j - 1;
                            if (tempJ >= 0)
                            {
                                if (dataBegin[tempI * col + tempJ] == 1)
                                    con++;
                            }
                            tempJ = j;
                            if (dataBegin[tempI * col + tempJ] == 1)
                                con++;
                            tempJ = j + 1;
                            if (tempJ < col)
                            {
                                if (dataBegin[tempI * col + tempJ] == 1)
                                    con++;
                            }
                        }
                        con = con / 8.0;
                        //计算随机影响因子
                        double rdmData;
                        double rungDa = rdm.NextDouble() + 0.00001;
                        if (rungDa >= 1)
                            rungDa = rungDa - 0.00001;
                        rdmData = Math.Pow(-Math.Log(rungDa), Rfa) + 1;
                        //读取城市限制开发数据
                        double unsuitable = dataUnsuitable[num];
                        //读取空间变量发展概率Pg
                        double Pg = dataPg[num];
                        //计算城市开发概率
                        double p = Pg * con * unsuitable * rdmData;
                        if (p > Pthreshold)
                            dataTemp[num] = 1;
                        else
                            dataTemp[num] = dataTemp[num];
                    }
                }
        }
        
    }
}
