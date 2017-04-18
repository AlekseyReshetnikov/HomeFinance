using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HomeFinance
{
    public partial class MainWindow : Window
    {
        #region Person
        private bool AddPerson(string PersonName)
        {
            if (PersonName == "")
                return false;
            try
            {
                Person P = db.AddPerson(PersonName);
                CurrentMotion.Person = P;
                cmbPerson_UpdateItemSource();
            }
            catch (Exception E)
            {
                ShowMessage(E.Message);
                return false;
            }
            return true;
        }

        private void DeletePerson(Person P)
        {
            try
            {
                db.DeletePerson(P);
                CurrentMotion.Person = null;
                cmbPerson_UpdateItemSource();
            }
            catch (Exception E)
            {
                ShowMessage(E.Message);
            }
        }
        #endregion

        #region Category
        private Category AddCategory(string CategoryName)
        {
            if (CategoryName == "")
                return null;
            Category C = null;
            try
            {                
                C = db.AddCategory(CategoryName, CurrentMotion.Debit);
                CurrentMotion.Category = C;
                cmbCategory_UpdateItemSource();
            }
            catch (Exception E)
            {
                ShowMessage(E.Message);                
            }
            return C;
        }

        private void DeleteCategory(Category C)
        {
            try
            {
                db.DeleteCategory(C);
                CurrentMotion.Category= null;
                cmbCategory_UpdateItemSource();
            }
            catch (Exception E)
            {
                ShowMessage(E.Message);
            }
        }
        #endregion

        #region Asset
        private bool AddAsset(string AssetName)
        {
            if (AssetName == "")
                return false;
            try
            {
                Asset A = db.AddAsset(AssetName);
                CurrentMotion.Asset = A;
                cmbAsset_UpdateItemSource();
            }
            catch (Exception E)
            {
                ShowMessage(E.Message);
                return false;
            }
            return true;
        }

        private void DeleteAsset(Asset A)
        {
            try
            {
                db.DeleteAsset(A);
                CurrentMotion.Asset = null;
                cmbAsset_UpdateItemSource();
            }
            catch (Exception E)
            {
                ShowMessage(E.Message);
            }
        }
        #endregion

        #region Motion

        // Приступаем ко вводу первой новой записи
        private void NewMotion()
        {
            bool OldDebit = false;
            if (CurrentMotion != null)
                OldDebit = CurrentMotion.Debit;
            CurrentMotion = new Motion();
            CurrentMotion.ActMk = true;
            // Продолжим ввод либо расходов, либо доходов. Если ничего не было раньше - то расход
            CurrentMotion.Debit = OldDebit;
            SetDebit_forCmbDebit();   // Для cmbDebit не использую Binding
            SetDebit_forOtherControls();

            // По умолчанию - продолжим ввод за ту же дату
            Motion M = db.GetLastMotion();
            if (M != null)
            {
                CurrentMotion.Date = M.Date;
                CurrentMotion.Person = M.Person;
            }
            SetNotion(null);

            UpdateDataContext();
            SetStatus("Новая запись");
            SetNewRecord(true);
            txtSumma.Focus();
            // cmbDebit.Focus();
        }

        // Одну запись сохранили, сразу приступаем ко вводу новой
        private void NewNextMotion()
        {
            Motion M = new Motion();
            M.ActMk = true;
            M.Debit = CurrentMotion.Debit;
            M.Date = CurrentMotion.Date;
            M.Person = CurrentMotion.Person;
            SetNotion(null);
            CurrentMotion = M;

            listGrid_UpdateItemSourse();
            UpdateDataContext();
            SetStatus("Сохранили");  // И создали новую запись
            SetNewRecord(true);
            txtSumma.Focus();
        }

        private void EditMotion()
        {
            UpdateDataContext();
            SetStatus("Редактирование");
            SetNewRecord(false);
        }

        private void DeleteMotion()
        {
            db.DeleteMotion(CurrentMotion);
            NewMotion();
        }

        private void SetNewRecord(bool newRecord)
        {
            NewRecord = newRecord;
            btnNew.IsEnabled = !NewRecord;
            btnDel.IsEnabled = !NewRecord;
        }

        private void SaveMotion()
        {
            try
            {
                if (CurrentMotion.Person == null)
                {
                    SetErrorStatus("Ошибка: человек");
                    cmbPerson.Focus();
                    return;
                }
                if (CurrentMotion.Summa == 0)
                {
                    SetErrorStatus("Ошибка: сумма");
                    txtSumma.Focus();
                    return;
                }
                if (CurrentMotion.Category == null)
                {
                    SetErrorStatus("Ошибка: статья");
                    cmbCategory.Focus();
                    return;
                }
                if (CurrentMotion.Notion == null)
                {
                    SetErrorStatus("Ошибка: тип");
                    cmbNotion.Focus();
                    return;
                }
                if ((CurrentMotion.Notion.NeedAsset) && (CurrentMotion.Asset == null))
                {
                    SetErrorStatus("Ошибка: актив");
                    cmbAsset.Focus();
                    return;
                }
                // Обеспечим автозаполнение ссылок Category.Notion
                if (CurrentMotion.Category.Notion == null)
                    db.Category_FillNotion(CurrentMotion.Category, CurrentMotion.Notion);

                if (NewRecord)
                {
                    db.AddMotion(CurrentMotion);
                    NewNextMotion();  // Сразу перейдем ко вводу след. записи
                }
                else
                {
                    db.UpdateMotion(CurrentMotion);
                    listGrid_UpdateItemSourse();
                    SetStatus("Сохранили");
                }
            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message);
            }
        }

        #endregion

    }
}
