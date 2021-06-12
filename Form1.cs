using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.FreeGlut;
using Tao.OpenGl;

namespace lab1
{
    public partial class Form1 : Form
    {
        //<---------------ABOBA------------------>

        Bitmap bmp;
        string dir = "./2.jpg";

        Color col;

        int height = 0;
        int width = 0;

        int n = 800;
        int m = 945;

        double[,] newM;
        double[,] matr;

        float[] color1 = new float[4] { 1, 0, 0, 1 };
        float[] shininess = new float[1] { 30 };

        //<--------------------------------->
        public Form1()
        {
            InitializeComponent();
            AnT.InitializeContexts();

            //initMAN
           
        }

        // вспомогательные переменные - в них будут храниться обработанные значения,
        // полученные при перетаскивании ползунков пользователем
        double a = 0, b = 0, c = -5, d = 0, zoom = 1; // выбранные оси

        private void AnT_Load(object sender, EventArgs e)
        {

        }

        int os_x = 1, os_y = 0, os_z = 0;
        // режим сеточной визуализации
       // bool Wire = false;
        private void Form1_Load(object sender, EventArgs e)
        {

            // инициализация библиотеки glut
            Glut.glutInit();
            // инициализация режима экрана
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE);
            // установка цвета очистки экрана (RGBA)
            Gl.glClearColor(255, 255, 255, 1);
            // установка порта вывода
            Gl.glViewport(0, 0, AnT.Width, AnT.Height);
            // активация проекционной матрицы
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            // очистка матрицы
            Gl.glLoadIdentity();
            // установка перспективы
            Glu.gluPerspective(45, (float)AnT.Width / (float)AnT.Height, 0.1, 200);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            // начальная настройка параметров openGL (тест глубины, освещение и первый источник света)
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);
            // установка первых элементов в списках combobox
            comboBox1.SelectedIndex = 0;
            // comboBox2.SelectedIndex = 0;
            // активация таймера, вызывающего функцию для визуализации
            image_to_bmp();
            height = get_height(bmp);
            width = get_width(bmp);


            RenderTimer.Start();


        }



        private void image_to_bmp()
        {
            bmp = new Bitmap(dir);
            matr = generate_matrix(bmp.Width, bmp.Height);

        }

        private int get_width(Bitmap b)
        {
            return b.Size.Width;
        }

        private int get_height(Bitmap b)
        {
            return b.Size.Height;
        }
        private double calc_L(int x, int y)
        {
            Color color = bmp.GetPixel(x, y);
            return 1 / (1 + Math.Exp(-Math.Abs(color.GetHashCode() / 5000000)));
        }

        private int get_color(int x = 0, int y = 0)
        {
            col = bmp.GetPixel(x, y);

            //pictureBox1.BackColor = col;
            return col.GetHashCode();
        }


        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            // вызываем функцию отрисовки сцены
            Draw();
        }
        // событие изменения значения
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            // переводим значение, установившееся в элементе trackBar в необходимый нам формат
            a = (double)trackBar1.Value / 1000.0;
            // подписываем это значение в label элементе под данным ползунком
            label4.Text = a.ToString();
        }
        // событие изменения значения
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            // переводим значение, установившееся в элементе trackBar в необходимый нам формат
            b = (double)trackBar2.Value / 1000.0;
            // подписываем это значение в label элементе под данным ползунком
            label5.Text = b.ToString();
        }
        // событие изменения значения
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            // переводим значение, установившееся в элементе trackBar в необходимый нам формат
            c = (double)trackBar3.Value / 1000.0;
            // подписываем это значение в label элементе под данным ползунком
            label6.Text = c.ToString();
        }
        // событие изменения значения
        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            // переводим значение, установившееся в элементе trackBar в необходимый нам формат
            d = (double)trackBar4.Value;
            // подписываем это значение в label элементе под данным ползунком
            //label6.Text = d.ToString();
        }
        // событие изменения значения
        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            // переводим значение, установившееся в элементе trackBar в необходимый нам формат
            zoom = (double)trackBar5.Value / 1000.0;
            // подписываем это значение в label элементе под данным ползунком
            //label6.Text = zoom.ToString();
        }
        // изменения значения чекбокса

        // изменение в элементах comboBox
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // в зависимости от выбранного режима
            switch (comboBox1.SelectedIndex)
            {
                // устанавливаем необходимую ось (будет использована в функции glRotate**)
                case 0:
                    {
                        os_x = 1;
                        os_y = 0;
                        os_z = 0;
                        break;
                    }
                case 1:
                    {
                        os_x = 0;
                        os_y = 1;
                        os_z = 0;
                        break;
                    }
                case 2:
                    {
                        os_x = 0;
                        os_y = 0;
                        os_z = 1;
                        break;
                    }
            }
        }


        bool f = true;

        // функция отрисовки
        private void Draw()
        {
            // очистка буфера цвета и буфера глубины
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(255, 255, 255, 1);
            // очищение текущей матрицы
            Gl.glLoadIdentity();
            // помещаем состояние матрицы в стек матриц, дальнейшие трансформации затронут только визуализацию объекта
            Gl.glPushMatrix();
            // производим перемещение в зависимости от значений, полученных при перемещении ползунков
            Gl.glTranslated(a, b, c);
            // поворот по установленной оси
            Gl.glRotated(d, os_x, os_y, os_z);
            // и масштабирование объекта
            Gl.glScaled(zoom, zoom, zoom);
            // в зависимости от установленного типа объекта
            //Glut.glutSolidCube(0.20);

            
            // Glut.glutWireCube(2.8);

            if (f)
            {
                if (n > width)
                {
                    n = width;
                }

                if (m > height)
                {
                    m = height;
                }

                newM = squeeze_matr(n, m, matr);
                f = false;
            }


            PointF l_up = new PointF(-1.4f, 1.4f);
            PointF r_down = new PointF(1.4f, -1.4f);


            double size = (r_down.X - l_up.X) / n;
            //MessageBox.Show(Convert.ToString(Convert.ToInt32(5 / 2)));
            //double[,] matrix = new double[width,height];

            bool first = true;



            Gl.glTranslated(-size * n / 2, -size * m / 2, 0);
            for (int i = 0; i < n; i++)
            {
                if (!first)
                {
                    Gl.glTranslated(size, 0, 0);
                }
                else
                {
                    first = false;
                }

                for (int j = 0; j < m; j++)
                {

                    color1 = new float[4] {(float)bmp.GetPixel(n-1-i, m-1-j).R/255, (float)bmp.GetPixel(n-1-i, m-1-j).R/255, (float)bmp.GetPixel(n-1-i, m-1-j).R/255, 1};
                    //(bmp.GetPixel(i-1,j-1).R, bmp.GetPixel(i - 1, j - 1).G, bmp.GetPixel(i - 1, j - 1).B);
                    Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, color1);
                    Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SPECULAR, color1);
                    Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SHININESS, shininess);

                    Glut.glutSolidCylinder(size / 2, newM[i,j], 15, 15);
                    Gl.glTranslated(0,size,0);
                }
                Gl.glTranslated(0, -size * m, 0);
            }/**/


            // возвращаем состояние матрицы
            Gl.glPopMatrix();
            // завершаем рисование
            Gl.glFlush();
            // обновляем элемент AnT 
            AnT.Invalidate();
            //MessageBox.Show(Convert.ToString(size)+" "+Convert.ToString(newM[0,0]));
            //RenderTimer.Stop();

        }

        private double[,] generate_matrix(int width, int height)
        {
            double[,] matrix = new double[width, height];
            for (int i = width; i > 0; i--)
            {
                for (int j = height; j > 0; j--)
                {
                    matrix[width - i, height - j] = calc_L(i - 1, j - 1);
                }

            }

            return matrix;
        }

        private double[,] squeeze_matr(int n, int m, double[,]  old)
        {
            
            int coefX = Convert.ToInt32(width/n);
            int coefY = Convert.ToInt32(height/m);
            if  ((coefX > 1) && (coefY > 1)) { 
                double[,] new_matr = new double[n,m];
                int halfX = coefX/2;
                int halfY = coefY/2;
                for (int i = 0; i < n; i += 1)
                {
                    for (int j = 0; j < m; j += 1)
                    {
                        new_matr[i, j] = get_midle_length(i*coefX, j*coefX, halfX, halfY, width, height, old);
                    }
                }
                //MessageBox.Show("Ready");
                return new_matr;
            }
           
            return old;
        }

        private double get_midle_length(int x, int y, int hx, int hy, int xmax, int ymax, double[,] old) 
        {
            double sum = 0;
            int count = 0;
            for (int i = x-hx; i < x+hx; i++)
            {
                for (int j = y-hy; j < y+hy; j++)
                {
                    if (((i > 0) && (i < xmax)) && ((j > 0) && (j < ymax)))
                    {
                        count++;
                        sum += old[i, j];
                    }
                }
            }
            //MessageBox.Show(Convert.ToString(sum / count));
            //return sum / count;
            return (sum/count)/(Math.Sqrt(1+Convert.ToDouble(sum/count)));
        }
    }

    // обрабатываем отклик таймера

}
