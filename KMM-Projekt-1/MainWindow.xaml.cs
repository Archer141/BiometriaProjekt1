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
        public static int deletion = 0;


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

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    int value;
                    Color pixelColor = bitmap.GetPixel(i, j);

                    value = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;

                    r = border[value];
                    g = border[value];
                    b = border[value];


                    bitmap.SetPixel(i, j, Color.FromArgb(r, g, b));
                    if (pixelColor.R==0)
                    {
                        tab[i, j] = 1;

                    }
                    else
                    {
                        tab[i, j] = 0;

                    }
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

        public static int[,] SetOneTwoThree(Bitmap newImage, int[,] pixelArray)
        {
            int yArray, xArray, maskY, maskX;
            int checkStick = 0;
            int checkClose = 0;

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
                                        checkStick = 1;
                                    }
                                }
                                else if (pixelArray[yArray, xArray] == 0)
                                    checkClose = 1;
                            }
                        }
                        if (checkStick == 1 && checkClose == 0)
                            pixelArray[y, x] = 3;
                        else
                            pixelArray[y, x] = 2;

                        if (checkStick == 0 && checkClose == 0)
                            pixelArray[y, x] = 1;

                        checkClose = 0;
                        checkStick = 0;
                    }
                }
            }
            return pixelArray;
        }


        public static int[,] FindAndDeleteFour(Bitmap newImage, int[,] pixelArray)
        {
            int yArray, xArray, maskY, maskX;
            int check = 0;
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
                                    check++; //counting neighbours for that pixel
                                    sum +=  compareTab[maskY, maskX]; //summary according to compareTable
                                }

                            }
                        }
                        if (check == 2 || check == 3 || check == 4)
                        {
                            if (delTab.Contains(sum))
                            {
                                deletion++;
                                pixelArray[y, x] = 0; // we are find "4" and setting it to "0" at the same time
                            }
                        }
                        else
                        {
                            pixelArray[y, x] = 2; // if we not find any "4"
                        }
                    }
                    check = 0;
                    sum = 0;
                }
            }
            return pixelArray ;
        }

        public static int[,] DeletingTwoThree(Bitmap newImage, int[,] pixelArray)
        {
            int deletion = 0;
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

                                    if (pixelArray[yArray, xArray] != 0)
                                    {
                                        sum += compareTab[maskY, maskX]; //summary according to compareTable
                                    }
                                }
                            }

                            if (delTab.Contains(sum))
                            {
                                deletion++;
                                pixelArray[y, x] = 0; //if we find pixel to delete, we are deleting it setting it to 0
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

                pixelArray = SetOneTwoThree(bitmap, pixelArray);
                pixelArray = FindAndDeleteFour(bitmap, pixelArray);
                pixelArray = DeletingTwoThree(bitmap, pixelArray);

                deletion = deletionFirst > deletionSecond ? deletionFirst : deletionSecond;
            }

        }

    }
}
