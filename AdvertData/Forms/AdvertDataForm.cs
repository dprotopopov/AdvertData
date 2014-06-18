using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace AdvertData.Forms
{
    /// <summary>
    ///     Форма для отображения отчёта
    ///     Пример использования
    ///     Application.Run(new AdvertDataForm(new Database()));
    /// </summary>
    public partial class AdvertDataForm : Form
    {
        private static readonly Record Record = new Record
        {
            StartTime = DateTime.Now,
            Key = "key",
            User = "user",
            Dll = "dll",
            Url = "url",
            Resource = "resource",
            Result = "result",
            Ad = "ad"
        };

        /// <summary>
        ///     Форма для отображения отчёта
        ///     Пример использования
        ///     Application.Run(new AdvertDataForm(new Database()));
        /// </summary>
        /// <param name="database"></param>
        public AdvertDataForm(Database database)
        {
            InitializeComponent();
            groupBoxTest.Visible = Test;
            Database = database;
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    checkBoxReport.Checked = Report;
                    AddComboBoxes(Database.GetStartDates());
                    DataSource = new BindingList<Record>(Database.GetData(comboBox1.Text, comboBox2.Text).ToList());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    DataSource = null;
                }
                finally
                {
                    Database.Disconnect();
                }
            propertyGrid1.SelectedObject = Record;
            checkBoxReport.CheckedChanged += checkBoxReport_CheckedChanged;
        }

        private Database Database { get; set; }

        /// <summary>
        ///     флаг «тест». Если в True, то отображается интерфейс для тестирования методов библиотеки, иначе таблица занимает все
        ///     свободное пространство.
        /// </summary>
        private bool Test
        {
            get { return testToolStripMenuItem.Checked; }
            set { testToolStripMenuItem.Checked = value; }
        }

        /// <summary>
        ///     флаг «Вести отчет» устанавливает значение в True или False переменной Report отдельной таблицы проекта Settings и
        ///     на данный отчет или методы никак не влияет. Данный фильтр нужен для других механизмов проекта, но управляется из
        ///     этого интерфейса. DLL должна содержать методы управления флагом, а так же метод определения состояния флага.
        ///     Соединение с базой данных должно быть установлено перед использованием свойства.
        /// </summary>
        public bool Report
        {
            get { return Database.Report; }
            set { Database.Report = value; }
        }

        private object DataSource
        {
            get { return gridControl1.DataSource; }
            set { gridControl1.DataSource = value; }
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Test = !Test;
            groupBoxTest.Visible = Test;
        }

        private void checkBoxReport_CheckedChanged(object sender, EventArgs e)
        {
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    Database.Report = checkBoxReport.Checked;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    DataSource = null;
                }
                finally
                {
                    Database.Disconnect();
                }
        }

        /// <summary>
        ///     Событие
        ///     Обновить экран
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkBoxReport.CheckedChanged -= checkBoxReport_CheckedChanged;
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    checkBoxReport.Checked = Report;
                    ClearComboBoxes();
                    AddComboBoxes(Database.GetStartDates());
                    DataSource = new BindingList<Record>(Database.GetData(comboBox1.Text, comboBox2.Text).ToList());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    DataSource = null;
                }
                finally
                {
                    Database.Disconnect();
                }
            checkBoxReport.CheckedChanged += checkBoxReport_CheckedChanged;
        }

        /// <summary>
        ///     Событие
        ///     Сформировать отчёт
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    DataSource = new BindingList<Record>(Database.GetData(comboBox1.Text, comboBox2.Text).ToList());
                }
                catch (Exception)
                {
                    DataSource = null;
                }
                finally
                {
                    Database.Disconnect();
                }
        }

        /// <summary>
        ///     Событие
        ///     Удалить записи с заданным StartDate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(comboBox3.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    Database.RemoveStartTime(dateTime.ToString("s"), dateTime.ToString("s"));
                    ClearComboBoxes();
                    AddComboBoxes(Database.GetStartDates());
                    DataSource = new BindingList<Record>(Database.GetData(comboBox1.Text, comboBox2.Text).ToList());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    DataSource = null;
                }
                finally
                {
                    Database.Disconnect();
                }
        }

        /// <summary>
        ///     Событие
        ///     Удалить все записи за дату
        ///     - возможность удалить все записи StartTime по дате
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            DateTime dateTime;
            try
            {
                dateTime = Convert.ToDateTime(comboBox4.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    Database.RemoveStartTime(
                        new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0).ToString("s"),
                        new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999).ToString("s"));
                    ClearComboBoxes();
                    AddComboBoxes(Database.GetStartDates());
                    DataSource = new BindingList<Record>(Database.GetData(comboBox1.Text, comboBox2.Text).ToList());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    DataSource = null;
                }
                finally
                {
                    Database.Disconnect();
                }
        }

        /// <summary>
        ///     Событие
        ///     Удалить все записи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    Database.ClearAllReport();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    Database.Disconnect();
                }
            ClearComboBoxes();
            DataSource = null;
        }

        private void ClearComboBoxes()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            comboBox4.Items.Clear();
            comboBox5.Items.Clear();
            comboBox6.Items.Clear();
        }

        private void AddComboBoxes(IEnumerable<string> list)
        {
            comboBox1.Items.AddRange(list.Cast<object>().ToArray());
            comboBox2.Items.AddRange(list.Cast<object>().ToArray());
            comboBox3.Items.AddRange(list.Cast<object>().ToArray());
            comboBox5.Items.AddRange(list.Cast<object>().ToArray());
            comboBox6.Items.AddRange(list.Cast<object>().ToArray());
            IEnumerable<string> list1 =
                list.Select(Convert.ToDateTime)
                    .Select(dt => new DateTime(dt.Year, dt.Month, dt.Day))
                    .Select(dt => dt.ToShortDateString()).Distinct();
            comboBox4.Items.AddRange(list1.Cast<object>().ToArray());
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Событие
        ///     Удалить записи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    Database.RemoveStartTime(comboBox5.Text, comboBox6.Text);
                    ClearComboBoxes();
                    AddComboBoxes(Database.GetStartDates());
                    DataSource = new BindingList<Record>(Database.GetData(comboBox1.Text, comboBox2.Text).ToList());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    DataSource = null;
                }
                finally
                {
                    Database.Disconnect();
                }
        }

        /// <summary>
        ///     Событие
        ///     Добавить запись
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    Database.AddStartTime(comboBox7.Text);
                    ClearComboBoxes();
                    AddComboBoxes(Database.GetStartDates());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    Database.Disconnect();
                }
        }

        /// <summary>
        ///     Установить Report = True
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            checkBoxReport.CheckedChanged -= checkBoxReport_CheckedChanged;
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    Database.Report = true;
                    checkBoxReport.Checked = Database.Report;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    Database.Disconnect();
                }
            checkBoxReport.CheckedChanged += checkBoxReport_CheckedChanged;
        }

        /// <summary>
        ///     Установить Report = False
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            checkBoxReport.CheckedChanged -= checkBoxReport_CheckedChanged;
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    Database.Report = false;
                    checkBoxReport.Checked = Database.Report;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    Database.Disconnect();
                }
            checkBoxReport.CheckedChanged += checkBoxReport_CheckedChanged;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            lock (Database.Semaphore)
                try
                {
                    Database.Connect();
                    bool value = Database.SetData(Record.StartTime.ToString("s"), Record.Key, Record.Dll,
                        Record.Resource,
                        Record.Url, Record.User,
                        Record.Result, Record.Ad);
                    MessageBox.Show(value.ToString());
                    ClearComboBoxes();
                    AddComboBoxes(Database.GetStartDates());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    Database.Disconnect();
                }
        }
    }
}