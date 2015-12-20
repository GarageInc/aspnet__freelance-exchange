using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Security.Cryptography;

namespace ApplicationMain
{
    public partial class Form1 : Form
    {
        // RSA
        BigInteger p;
        BigInteger q;
        BigInteger n;
        BigInteger E;
        BigInteger d;

        string message = string.Empty;
        string sign = string.Empty;

        // Pollard's method
        BigInteger pE;
        BigInteger pN;
        BigInteger pSW;

        public Form1()
        {
            InitializeComponent();

            // 1
            p = BigInteger.Parse(pTextBox.Text);
            q = BigInteger.Parse(qTextBox.Text);
            n = p * q;
            nTextBox.Text = n.ToString();

            // 2
            pE = BigInteger.Parse(textBoxPe.Text);
            pN = BigInteger.Parse(textBoxPN.Text);
            pSW = BigInteger.Parse(textBoxPSW.Text);
        }

        // Расширенный алгоритм Евклида
        private void РасширенныйАлгоритмЕвклида(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y, out BigInteger d)
        {
            BigInteger q, r, x1, x2, y1, y2;

            if (b == 0)
            {
                d = a;
                x = 1;
                y = 0;
                return;
            }

            x2 = 1;
            x1 = 0;
            y2 = 0;
            y1 = 1;

            while (b > 0)
            {
                q = a / b;
                r = a - q * b;
                x = x2 - q * x1;
                y = y2 - q * y1;
                a = b;
                b = r;
                x2 = x1;
                x1 = x;
                y2 = y1;
                y1 = y;
            }

            d = a;
            x = x2;
            y = y2;
        }

        BigInteger НайтиОбратныйПоМодулю(BigInteger a, BigInteger n)
        {
            BigInteger x, y, d;
            РасширенныйАлгоритмЕвклида(a, n, out x, out y, out d);

            if (d == 1) return x;

            return 0;

        }

        // Тест Миллера-Рабина
        bool ТестМиллераРабина(BigInteger n, BigInteger a)
        {
            // Отсеиваем тривиальные случаи(самые простые)
            if (n <= 1)
                return false;
            if (n == 2)
                return true;
            if (n % 2 == 0)
                return false;

            BigInteger s = 0, d = n - 1;
            while (d % 2 == 0)
            {
                d /= 2;
                s++;
            }

            // Реализация проверок по возведению в степень по модулю 'n'
            BigInteger r = 1;
            BigInteger x = (BigInteger)BigInteger.ModPow(a, d, n);
            if (x == 1 || x == n - 1)
                return true;

            x = (long)BigInteger.ModPow(BigInteger.Pow(a, (int)d), BigInteger.Pow(2, (int)r), n);

            if (x == 1)
                return false;

            if (x != n - 1)
                return false;

            return true;
        }


        // Функция проверки, использует тест Миллера-Рабина
        bool ПроверитьЧислоТестомМиллераРабина(BigInteger i)
        {
            // ПРОВЕРКА НЕ НА 2х ОСНОВАНИЯХ 'a', а на 10! Т.е. чем больше оснований - тем точнее проверка числа на простоту long[] massA = new long [11];
            // Разные основания 'а' от 2 до 10
            for (long a = 2; a < i && a < 11; a++)
            {
                // Запускаем тест
                bool b = ТестМиллераРабина((long)i, (long)a);// Передаем число и проверяем
                if (!b)
                {
                    return false;
                }
            }
            return true;
        }

        // Получить простое число
        public long ПолучитьПростоеЧисло(ref int i)
        {
            Random r = new Random();
            long число = 0;
            bool isCorrect = false;

            for (; i < 10 * i && !isCorrect; i++)
            {
                число = i; //(long)r.Next(3, 15);
                if (ПроверитьЧислоТестомМиллераРабина(число))
                {
                    break;
                }
                //else
                //{
                //    Console.WriteLine("Введенное число НЕ простое!");
                //}
            }

            return число;
        }

        // Функция для умножения двух чисел x,y по модулю m
        BigInteger mulmod(BigInteger x, BigInteger y, BigInteger m)
        {
            return (x * y) % m;
        }

        // Функция возведения числа x в степен а по модулю n
        BigInteger powmod(BigInteger x, BigInteger a, BigInteger m)
        {
            BigInteger r = 1;
            while (a > 0)
            {
                if (a % 2 != 0)
                    r = mulmod(r, x, m);
                a = a >> 1;
                x = mulmod(x, x, m);
            }
            return r;
        }


        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            // Считаем хеш сообщения
            // Попросим ввести зашифровываемое сообщение 'm'
            message = messageTextBox.Text;

            // Установим константу 'e'
            Random r = new Random();

            E = (BigInteger)new long[] { 17, 257, 65537 }[r.Next(0, 3)];// Одно из 3х простых чисел Ферма
            eTextBox.Text = E.ToString();

            // Найдем константу 'd' - закрытый ключ Боба
            BigInteger fn = (p - 1) * (q - 1);
            d = НайтиОбратныйПоМодулю(E, fn);
            if (d < 0)
                d = fn + d;

            dTextBox.Text = d.ToString();

            // Получим хеш сообщения
            string hash;
            using (MD5 md5Hash = MD5.Create())
            {
                hash = GetMd5Hash(md5Hash, message);
            }
            hTextBox.Text = hash;


            // Шифруем хеш
            char[] mass = hash.ToCharArray(0, hash.Length);
            sign = "";
            for (int i = 0; i < mass.Length; i++)
            {
                var a = powmod((BigInteger)mass[i], d, n);
                sign += (char)((int)a);
            }

            // Выведем результат:
            signTextBox.Text = sign;


            messageTextBox2.Text = message;
            signTextBox2.Text = sign;

            Clear();
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            message = messageTextBox2.Text;
            sign = signTextBox2.Text;
            // Получим хеш сообщения
            string hash;
            using (MD5 md5Hash = MD5.Create())
            {
                hash = GetMd5Hash(md5Hash, message);
            }
            hTextBox3.Text = hash;

            // Переводим подпись в массив символов
            char[] mass = sign.ToCharArray(0, sign.Length);
            // Получаем хэш сообщения
            string newHash = "";
            for (int i = 0; i < mass.Length; i++)
            {
                var a = powmod((BigInteger)mass[i], E, n);

                newHash += (char)(((int)a) % 65536);
            }
            // Выводим полученную подпись
            hTextBox2.Text = newHash;

            if (hash != newHash)
            {
                checkTextBox.Text = "НЕВЕРНО";
                checkTextBox.BackColor = Color.Red;
            }

            else
            {
                checkTextBox.Text = "ВЕРНО";
                checkTextBox.BackColor = Color.Green;
            }
        }

        private void generateSimpleButton_Click(object sender, EventArgs e)
        {
            // Ищем первое простое число
            Random r = new Random();

            int i = r.Next(1, 256);
            p = ПолучитьПростоеЧисло(ref i);
            // Ищем второе простое число
            i = i + 1;
            q = ПолучитьПростоеЧисло(ref i);

            n = p * q;

            // Выведем сгенерированные простые числа
            pTextBox.Text = p.ToString();
            qTextBox.Text = q.ToString();
            nTextBox.Text = (p * q).ToString();

            Clear();
        }

        private void Clear()
        {
            hTextBox2.Text = string.Empty;
            hTextBox3.Text = string.Empty;
            checkTextBox.Text = string.Empty;

            checkTextBox.BackColor = Color.White;
        }

        private void getKeysButton_Click(object sender, EventArgs e)
        {
            p = BigInteger.Parse(pTextBox.Text);
            q = BigInteger.Parse(qTextBox.Text);
            n = p * q;
            nTextBox.Text = n.ToString();

            Clear();
        }


        // Метод Полларда
        private void startButton_Click(object sender, EventArgs e)
        {
            pE = BigInteger.Parse(textBoxPe.Text);
            pN = BigInteger.Parse(textBoxPN.Text);
            pSW = BigInteger.Parse(textBoxPSW.Text);
            MessageBox.Show(pN.ToString());
            textBoxPp.Text = string.Empty;
            textBoxPq.Text = string.Empty;

            richTextBox1.Text = "";

            string str = "";
            List<BigInteger> mass = new List<BigInteger>();
            BigInteger sw = BigInteger.Parse(pSW.ToString());
            while (sw > 0)
            {
                BigInteger a = sw % 100;
                mass.Add(a);
                str += (((char)(a)).ToString());
                sw = sw / 100;
            }

            richTextBox1.Text = "Зашифровано:" + str;

            richTextBox1.Text += '\n';

            DateTime start = DateTime.Now;
            BigInteger p = P(pN);// 1292698383713369
            BigInteger q = pN / p;
            DateTime finish = DateTime.Now;

            richTextBox1.Text += '\n';
            richTextBox1.Text += (finish - start).ToString();
            richTextBox1.Text += '\n';

            richTextBox1.Text += " p = " + p;
            richTextBox1.Text += " q = " + q;
            richTextBox1.Text += '\n';
            //
            richTextBox1.Text += "\n Полученный:";
            richTextBox1.Text +=  p * q;
            richTextBox1.Text += "\n Оригинальный:";
            richTextBox1.Text +=  pN;
            richTextBox1.Text += '\n';

            // Найдем константу 'd' - закрытый ключ Боба
            BigInteger fn = (p - 1) * (q - 1);
            d = НайтиОбратныйПоМодулю(pE, fn);
            if (d < 0)
                d = fn + d;

            richTextBox1.Text += "\nd = " + d;

            BigInteger res = powmod((BigInteger)pSW, d, pN);

            richTextBox1.Text += '\n';
            richTextBox1.Text += res;
            richTextBox1.Text += '\n';
            //
            sw = BigInteger.Parse(res.ToString());
            str = "";
            richTextBox1.Text += res + "\n";

            while (sw > 0)
            {
                BigInteger a = sw % 100;
                str += Encoding.ASCII.GetString(new[] { (byte)((int)a) });
                sw = sw / 100;
            }

            richTextBox1.Text += "\n РЕЗУЛЬТАТ 1: \n";
            richTextBox1.Text += str;

            //
            sw = BigInteger.Parse(revers(res).ToString());
            str = "";
            
            richTextBox1.Text += res + "\n";

            while (sw > 0)
            {
                BigInteger a = sw % 100;
                //str += (((char)(a)).ToString());
                str += Encoding.ASCII.GetString(new[] { (byte)((int)a) });
                sw = sw / 100;
            }

            richTextBox1.Text += "\n РЕЗУЛЬТАТ 2: \n";
            richTextBox1.Text += str;
        }

        public BigInteger revers(BigInteger x)
        {
            char[] arr = x.ToString().ToCharArray();
            Array.Reverse(arr);
            var res = new string(arr);

            BigInteger res2;
            BigInteger.TryParse(res, out res2);

            return res2;
        }

        BigInteger P(BigInteger N)
        {
            Random r = new Random();
            BigInteger x = N / 2;
            BigInteger y = 1, i = 0, stage = 2;
            BigInteger index = 0;
            while (gcd(N, x - y > 0 ? x - y : y - x) == 1)
            {
                if (i == stage)
                {
                    y = x;
                    stage = stage * 2;
                }
                x = (x * x + 1) % N;
                i = i + 1;
                index = index + 1;
            }
            richTextBox1.Text += "\n ИТЕРАЦИЙ: " + index;
            return gcd(N, x - y > 0 ? x - y : y - x);
        }

        BigInteger gcd(BigInteger a, BigInteger b)
        {
            while (b != 0)
                b = a % (a = b);
            return a;
        }
    }
}
