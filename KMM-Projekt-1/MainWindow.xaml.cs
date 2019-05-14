using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Color = System.Drawing.Color;

namespace KMM_Projekt_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        
        private WriteableBitmap loadedImage;
        private BitmapImage bitmapImage;
        private Bitmap bitmapImage3;

        public MainWindow()
        {
            InitializeComponent();
        }

        public static int[] delTab = {
        3,5,7,12,13,14,15,20,
        21,22,23,28,29,30,31,48,
        52,53,54,55,56,60,61,62,
        63,65,67,69,71,77,79,80,
        81,83,84,85,86,87,88,89,
        91,92,93,94,95,97,99,101,
        103,109,111,112,113,115,116,117,
        118,119,120,121,123,124,125,126,
        127,131,133,135,141,143,149,151,
        157,159,181,183,189,191,192,193,
        195,197,199,205,207,208,209,211,
        212,213,214,215,216,217,219,220,
        221,222,223,224,225,227,229,231,
        237,239,240,241,243,244,245,246,
        247,248,249,251,252,253,254,255
        };

        public static readonly int[,] compareTab = {

                       { 128, 1, 2 },
                       { 64, 0, 4 }, // 0 is a middle pixel, the rest are weights for the neighbourhood
                       { 32, 16, 8 } // of this pixel
                       
                                   };
        public static int deletion = 1;


        #region Konwertery bitmap

        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        private WriteableBitmap WriteableBitmapBitmapFromBitmap(Bitmap writeBmp)
        {
            BitmapSource bitmapSource =
                 System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(writeBmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                 System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            WriteableBitmap writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(bitmapSource);
            return writeableBitmap;
        }

        public Bitmap CreateNonIndexedImage(Bitmap src)
        {
            Bitmap newBmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics gfx = Graphics.FromImage(newBmp))
            {
                gfx.DrawImage(src, 0, 0);
            }

            return newBmp;
        }
        #endregion

        #region Wczytywanie
        private void loadImg(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image files (*.png;*.jpeg)|*.jpg;*;|All files(*.*)|*.*)";
            if (open.ShowDialog() == true)
            {
                var uri = new Uri(open.FileName);
                loadedImage = new WriteableBitmap(new BitmapImage(uri));
                bitmapImage = new BitmapImage(uri);
             
                Image1.Source = loadedImage;
                Image2.Source = loadedImage;
                Image3.Source = loadedImage;
            }
        }
        #endregion

        

        #region Binaryzacja

        private int[,] Binarization(int treshold)
        {
          
            int r, g, b;
            int[] border = new int[256];
            for (int i = treshold; i < 256; i++)
            {
                border[i] = 255;
            }

            var bitmap = BitmapFromWriteableBitmap(loadedImage);

            int[,] tab = new int[bitmap.Height,bitmap.Width];

            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    int value;
                    Color pixelColor = bitmap.GetPixel(j, i);

                    value = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;

                    if (value < treshold)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                        tab[i, j] = 1;
                    }
                    else
                    {
                        r = 255;
                        g = 255;
                        b = 255;
                        tab[i, j] = 0;
                    }


                    bitmap.SetPixel(j, i, Color.FromArgb(r, g, b));
                }
            }


            Image2.Source = WriteableBitmapBitmapFromBitmap(bitmap);

            return tab;
        }

        private void BinaryzationClick(object sender, RoutedEventArgs e)
        {
            string a = BinValue.Text;
            Binarization(Convert.ToInt32(a));
        }



        #endregion

        #region Kmm

        public static int[,] SetTwoThree(Bitmap newImage, int[,] pixelArray)
        {
            int yArray, xArray, maskY, maskX;
            int checkDiagonal = 0;
            int checkPlus = 0;

            for (int y = 1; y < newImage.Height - 1; y++)
            {
                for (int x = 1; x < newImage.Width - 1; x++)
                {
                    if (pixelArray[y, x] == 1)
                    {
                        for (maskY = 0; maskY < 3; maskY++)
                        {
                            for (maskX = 0; maskX < 3; maskX++)
                            {
                                yArray = (y + maskY - 1);
                                xArray = (x + maskX - 1);

                                if ((maskY == 0 && maskX == 0) || (maskY == 0 && maskX == 2) || (maskY == 2 && maskX == 0) || (maskY == 2 && maskX == 2))
                                {
                                    if (pixelArray[yArray, xArray] == 0)
                                    {
                                        checkDiagonal = 1;
                                    }
                                }
                                else if (pixelArray[yArray, xArray] == 0)
                                    checkPlus = 1;
                            }
                        }
                        if (checkDiagonal == 1 && checkPlus == 0)
                            pixelArray[y, x] = 3;
                        else
                            pixelArray[y, x] = 2;

                        if (checkDiagonal == 0 && checkPlus == 0)
                            pixelArray[y, x] = 1;

                        checkPlus = 0;
                        checkDiagonal = 0;
                    }
                }
            }
            return pixelArray;
        }


        public static int[,] FindAndDeleteFour(Bitmap newImage, int[,] pixelArray)
        {
            int yArray, xArray, maskY, maskX;
            int neighbourCounter = 0;
            int sum = 0;
            for (int y = 1; y < newImage.Height - 1; y++)
            {
                for (int x = 1; x < newImage.Width - 1; x++)
                {
                    if (pixelArray[y, x] == 2)
                    {
                        for (maskY = 0; maskY < 3; maskY++)
                        {
                            for (maskX = 0; maskX < 3; maskX++)
                            {
                                if (maskX == 1 && maskY == 1)
                                    continue;

                                yArray = (y + maskY - 1);
                                xArray = (x + maskX - 1);

                                if (pixelArray[yArray, xArray] > 0)
                                {
                                    neighbourCounter++; 
                                    sum +=  compareTab[maskY, maskX]; 
                                }

                            }
                        }
                        if (neighbourCounter == 2 || neighbourCounter == 3 || neighbourCounter == 4)
                        {
                            if (delTab.Contains(sum))
                            {
                                deletion++;
                                pixelArray[y, x] = 0; 
                            }
                        }
                        else
                        {
                            pixelArray[y, x] = 2; 
                        }
                    }
                    neighbourCounter = 0;
                    sum = 0;
                }
            }
            return pixelArray ;
        }

        public int[,] DeletingTwoThree(Bitmap newImage, int[,] pixelArray)
        {
            int yArray, xArray, maskY, maskX;
            int sum = 0;
            int N = 2;
            while (N <= 3)
            {
                for (int y = 1; y < newImage.Height - 1; y++)
                {
                    for (int x = 1; x < newImage.Width - 1; x++)
                    {
                        if (pixelArray[y, x] == N)
                        {
                            for (maskY = 0; maskY < 3; maskY++)
                            {
                                for (maskX = 0; maskX < 3; maskX++)
                                {
                                    yArray = (y + maskY - 1);
                                    xArray = (x + maskX - 1);

                                    if (pixelArray[yArray, xArray] > 0)
                                    {
                                        sum += compareTab[maskY, maskX];
                                    }
                                }
                            }

                            if (delTab.Contains(sum))
                            {
                                deletion++;
                                pixelArray[y, x] = 0; 
                            }
                            else
                            {
                                pixelArray[y, x] = 1;
                            }
                        }
                        sum = 0;
                    }
                }
                N++;
            }
           
            return pixelArray;
        }

        #endregion


        private void LaunchClick(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = BitmapFromWriteableBitmap(loadedImage);
            int[,] pixelArray = new int[ bitmap.Height,bitmap.Width];

            pixelArray = Binarization(Int32.Parse(BinValue.Text));

            while (deletion != 0)
            {
                deletion = 0;
                pixelArray = SetTwoThree(bitmap, pixelArray);
                pixelArray = FindAndDeleteFour(bitmap, pixelArray);
                pixelArray = DeletingTwoThree(bitmap, pixelArray);
                RefreshImage3(pixelArray, bitmap);
                Refresh(Image3);
            }
            deletion = 1;
            bitmapImage3 = bitmap;
        }

        private void RefreshImage3(int[,] pixelArray, Bitmap bitmap)
        {
            Image3.Source = WriteableBitmapBitmapFromBitmap(PixelArrayToImage(pixelArray, bitmap));
        }

        private Bitmap PixelArrayToImage(int[,]pixelArray,Bitmap bitmap)
        {
            Bitmap newBitmap = new Bitmap(bitmap);
            for(int i = 0; i < bitmap.Height; i++)
            {
                for(int j = 0; j < bitmap.Width; j++)
                {
                    if (pixelArray[i, j] != 0)
                    {
                        bitmap.SetPixel(j, i, Color.Black);

                    }
                    else
                    {
                        bitmap.SetPixel(j, i, Color.White);
                    }
                }
            }
            return newBitmap;
        }

        private delegate void NoArgDelegate();

        public static void Refresh(DependencyObject obj)
        {
            obj.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                (NoArgDelegate)delegate { });
        }

        int[] minuncje = { 1, 3 };
        private int minuncja(int x, int y, int[,] t)
        {
            int suma = 0;
            suma += Math.Abs(t[x, y + 1] - t[x - 1, y + 1]);
            suma += Math.Abs(t[x - 1, y + 1] - t[x - 1, y]);
            suma += Math.Abs(t[x - 1, y] - t[x - 1, y - 1]);
            suma += Math.Abs(t[x - 1, y - 1] - t[x, y - 1]);
            suma += Math.Abs(t[x, y - 1] - t[x + 1, y - 1]);
            suma += Math.Abs(t[x + 1, y - 1] - t[x + 1, y]);
            suma += Math.Abs(t[x + 1, y] - t[x + 1, y + 1]);
            suma += Math.Abs(t[x + 1, y + 1] - t[x, y + 1]);

            suma /= 2;
            if (suma == minuncje[0]) return 1;
            if (suma == minuncje[1]) return 2;
            return 0;

        }

        private void MinucjeBtn_Click(object sender, EventArgs e)
        {

            Bitmap bitmap = bitmapImage3;
            Bitmap tmp = new Bitmap(bitmap.Width, bitmap.Height);
            int[,] pixelArray = new int[bitmap.Width, bitmap.Height];

            Color color;
            for (int i = 1; i < bitmap.Width - 1; i++) // zwykłe uzupełnienie tabliicy 0 i 1 do liczenia minuncji
            {
                for (int j = 1; j < bitmap.Height - 1; j++)
                {
                    color = bitmap.GetPixel(i, j);
                    tmp.SetPixel(i, j, color);
                    pixelArray[i, j] = 0;
                    if (color.R != 255)
                    {
                        pixelArray[i, j] = 1;
                    }
                }
            }
            for (int i = 1; i < bitmap.Width - 1; i++)   //liczenie minuncji i zaznaczenie
            {
                for (int j = 1; j < bitmap.Height - 1; j++)
                {
                    if (pixelArray[i, j] == 1) //jezeli to czarny pixel
                    {
                        if (minuncja(i, j, pixelArray) == 1) // zakonczenia
                        {
                            for (int k = i - 3; k < i + 4; k++)
                            {
                                for (int l = j - 3; l < j + 4; l++)
                                {
                                    if (k == i - 3 || l == j - 3 || k == i + 3 || l == j + 3)
                                    {
                                        tmp.SetPixel(k, l, Color.Red);
                                    }
                                }
                            }
                        }
                        if (minuncja(i, j, pixelArray) == 2) //rozwidlenia
                        {
                            for (int k = i - 3; k < i + 4; k++)
                            {
                                for (int l = j - 3; l < j + 4; l++)
                                {
                                    if (k == i - 3 || l == j - 3 || k == i + 3 || l == j + 3)
                                    {
                                        tmp.SetPixel(k, l, Color.Blue);
                                    }
                                }
                            }
                        }
                    }


                }
            }
            Image4.Source = WriteableBitmapBitmapFromBitmap(tmp);
           
        }

    }
}
