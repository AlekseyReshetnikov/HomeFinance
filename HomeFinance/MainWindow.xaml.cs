using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.Entity;
using System.Data;
using System.Collections.Generic;
using System.Windows.Controls.DataVisualization.Charting;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace HomeFinance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataContext db;
        Motion CurrentMotion;  // Текущая редактируемая запись
        const System.Windows.Input.Key KeyForDelete = Key.Escape;  // Клавиша для удаления записей из вспом. справочников, не очевидная
        bool NewRecord = true;
        const int RecordsInList = 20;
        bool GridInFocus = false;
        ChartFilter CurrentFilter;  // Данные в фильтре

        public MainWindow()
        {
            InitializeComponent();
            db = new DataContext();
            // Первое заполнение типов расходов - если таблица окажется пустой
            db.FillNotions();
            this.Closing += MainWindow_Closing;

            // Настройка интерфейса
            EditUI_Settings();
            // Новая запись
            NewMotion();
            // Настройки фильтра на вкладке "Итоги"
            Filter_Settings();
        }

        // При закрытии окна - освобождаем связь с базой данных
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            db.Dispose();
        }

        private void ShowMessage(string Message)
        {
            MessageBox.Show(Message);
        }

        private bool AskQuestion(string Message)
        {
            return (MessageBox.Show(Message, "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes);
        }

        // Первые настройки элементов управления
        private void EditUI_Settings()
        {
            // по умолчанию - ввод расходов
            cmbDebit.Items.Add("Расход");
            cmbDebit.Items.Add("Доход");
            // Настройка ItemSource, где нет зависимости от debit \ credit
            cmbPerson_UpdateItemSource();
            cmbAsset_UpdateItemSource();
        }

        // Часто используется одна и та же строка
        private void UpdateDataContext()
        {
            gridEditData.DataContext = CurrentMotion;
        }


        #region cmbDebit

        private void SetDebit_forCmbDebit()
        {
            if (CurrentMotion.Debit)
                cmbDebit.SelectedIndex = 1;
            else
                cmbDebit.SelectedIndex = 0;
        }

        private void SetDebit_forOtherControls()
        {
            cmbCategory_UpdateItemSource();
            cmbNotion_UpdateItemSource();
            // В нижнем гриде - либо расходы, либо доходы
            listGrid_UpdateItemSourse();
        }

        // Поменяли или установили значение
        private void cmdDebit_Selected(object sender, SelectionChangedEventArgs e)
        {
            bool Debit = cmbDebit.SelectedIndex == 1;
            // Синхронизируем значение в cmdDebit и CurrentMotion.Debit
            if ((CurrentMotion != null) && (CurrentMotion.Debit != Debit))
            {
                CurrentMotion.Debit = Debit;
                SetDebit_forOtherControls();
            }
        }
        
        private void cmbDebit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                cmbPerson.Focus();
                e.Handled = true;
            }
        }
        #endregion

        #region cmbPerson

        private void cmbPerson_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    bool Ok = false;
                    if (cmbPerson.SelectedIndex >= 0)
                        Ok = true;
                    else   // Если введен текст, добавляем нового пользователя                
                        Ok = AddPerson(cmbPerson.Text);
                    if (Ok)
                        txtDate.Focus();
                    e.Handled = true;
                    break;
                case KeyForDelete:
                    if (cmbPerson.SelectedIndex >= 0)
                    {
                        Person P = (Person)cmbPerson.SelectedItem;
                        if (AskQuestion(String.Format("Вы действительно хотите удалить человека '{0}'?", P.Name)))
                            DeletePerson(P);
                    }
                    e.Handled = true;
                    break;
            }
        }

        // cmbPerson - установка ItemsSource
        private void cmbPerson_UpdateItemSource()
        {
            cmbPerson.ItemsSource = db.Persons.ToList().Where(q => q.ActMk);
            cmbPerson.DisplayMemberPath = "Name";

            // Использовать ObservableCollection не получилось (
            //cmbPerson.ItemsSource = new ObservableCollection<Person>(db.Persons.ToList().Where(q => q.ActMk));
            // cmbPerson.ItemsSource = new ObservableCollection<Person>(db.Persons.Where(q => q.ActMk));
            // cmbPerson.ItemsSource = db.Persons.Local.ToBindingList();
        }

        #endregion

        #region txtDate
        private void txtDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtSumma.Focus();
                e.Handled = true;
            }
        }

        #endregion

        #region txtSumma
        private void txtSumma_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                cmbCategory.Focus();
                e.Handled = true;
            }           
        }

        // Если собираемся ввести сумму - сразу уберем лишний ноль
        private void txtSumma_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSumma.Text == "0")
                txtSumma.Text = "";
        }

        private void txtSumma_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtSumma.Text == "")
                txtSumma.Text = "0";
        }

        #endregion

        #region cmbCategory

        private void cmbCategory_UpdateItemSource()
        {
            cmbCategory.ItemsSource = db.Categories.ToList().Where(q => q.Debit == CurrentMotion.Debit && q.ActMk);
            cmbCategory.DisplayMemberPath = "Name";
          }

        private void cmbCategory_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Category C;
                    if (cmbCategory.SelectedIndex >= 0)
                        C = (Category)cmbCategory.SelectedItem;
                    else
                        C = AddCategory(cmbCategory.Text);
                    if (C != null)
                    {
                        SetNotion(C.Notion);
                        txtNote.Focus();
                    }
                    e.Handled = true;
                    break;
                case KeyForDelete:
                    if (cmbCategory.SelectedIndex >= 0)
                    {
                        Category C1 = (Category)cmbCategory.SelectedItem;
                        string Comm = CurrentMotion.Debit ? "доходов" : "расходов";
                        if (AskQuestion(String.Format("Вы действительно хотите удалить статью {0} '{1}'?", Comm, C1.Name)))
                            DeleteCategory(C1);
                    }
                    e.Handled = true;
                    break;
            }            
        }

        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategory.SelectedIndex >= 0)
            {
                Category C = (Category)cmbCategory.SelectedItem;
                SetNotion(C.Notion);
            }
        }

        #endregion

        #region txtNote

        private void txtNote_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)  // Переходим к элементу, где требуется дальнейший ввод 
            {
                if (CurrentMotion.Notion == null)
                    cmbNotion.Focus();
                else
                {
                    if (CurrentMotion.Notion.NeedAsset)
                        cmbAsset.Focus();
                    else
                        SaveMotion();  // Ничего больше не надо вводить - сразу сохраним
                }
                e.Handled = true;
            }
        }
        #endregion

        #region cmbNotion

        private void cmbNotion_UpdateItemSource()
        {
            cmbNotion.ItemsSource = db.Notions.ToList().Where(q => q.Debit == CurrentMotion.Debit && q.ActMk);
            cmbNotion.DisplayMemberPath = "Name";
        }

        private void cmbNotion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)  // Переходим к элементу, где требуется дальнейший ввод 
            {
                if (cmbNotion.SelectedIndex>=0)
                {
                    Notion N = (Notion)cmbNotion.SelectedItem;
                    if (N.NeedAsset)
                        cmbAsset.Focus();
                    else
                        SaveMotion();
                }
                e.Handled = true;
            }
        }

        // Если тип расходов определился по расходу - не дадим его менять
        private void SetNotion(Notion N)
        {
            if (N == null)                
                cmbNotion.IsReadOnly = true;
            else
            {
                cmbNotion.IsReadOnly = false;
                CurrentMotion.Notion = N;
                cmbNotion.SelectedValue = N;
            }
        }

        #endregion

        #region cmbAsset

        private void cmbAsset_UpdateItemSource()
        {
            cmbAsset.ItemsSource = db.Assets.ToList().Where(q => q.ActMk);
            cmbAsset.DisplayMemberPath = "Name";
        }

        private void cmbAsset_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    bool Ok = false;
                    if (cmbAsset.SelectedIndex >= 0)
                        Ok = true;
                    else
                        Ok = AddAsset(cmbAsset.Text);
                    if (Ok)
                        SaveMotion();
                    e.Handled = true;
                    break;
                case KeyForDelete:
                    if (cmbAsset.SelectedIndex >= 0)
                    {
                        Asset A = (Asset)cmbAsset.SelectedItem;
                        if (AskQuestion(String.Format("Вы действительно хотите удалить актив '{0}'?", A.Name)))
                            DeleteAsset(A);
                    }
                    e.Handled = true;
                    break;
            }
        }
        #endregion

        #region txtStatus

        private void SetStatus(string StatusText)
        {
            txtStatus.Text = StatusText;
            txtStatus.Foreground = Brushes.DarkGreen;
        }

        private void SetErrorStatus(string Error)
        {
            txtStatus.Text = Error;
            txtStatus.Foreground = Brushes.DarkRed;
        }

        #endregion

        #region listGrid

        private void listGrid_UpdateItemSourse()
        {
            listGrid.ItemsSource = db.Motions.ToList().Where(q => q.Debit == CurrentMotion.Debit && q.ActMk).OrderByDescending(m => m.Id).Take(RecordsInList);
        }

        private void listGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (listGrid.SelectedItem == null)
                return;
            // Ориентироваться на фокус listGrid.IsFocused не получилось - в фокусе не сам грид, а ячейка
            if (!GridInFocus)  // вручную устанавливаю / сбрасываю это свойство
                return;
            CurrentMotion = (Motion)listGrid.SelectedItem;
            EditMotion();
        }

        private void listGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            GridInFocus = true;
        }

        private void listGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            GridInFocus = false;
        }

        #endregion

        // StringFormat={}{0:C}}


        #region buttons
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveMotion();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            NewMotion();
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (AskQuestion("Вы действительно хотите удалить текущую строку полностью?"))
                DeleteMotion();
        }

        #endregion

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case (Key.Enter):  // Ctrl+Enter - сохранение записи
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    {
                        e.Handled = true;
                        // Если нажали Ctrl+Enter на txtNote - примечание не сохраняется (фокус не успевает сместиться)
                        if ((txtNote.IsFocused) && (txtNote.Text != ""))
                            CurrentMotion.Note = txtNote.Text;
                        SaveMotion();
                    }
                    break;
                case (Key.Insert):  // Новая запись
                    e.Handled = true;
                    NewMotion();
                    break;
                case (Key.Add):   // Переходим на след. день
                    DateTime D = ((DateTime)txtDate.SelectedDate).AddDays(1);
                    txtDate.SelectedDate = D;
                    e.Handled = true;
                    break;
            }
        }


    }

    public class ToolkPoint
    {
        public int Value { get; set; }
        public DateTime Time { get; set; }
    }
}
