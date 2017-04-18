using System;

namespace HomeFinance
{
    // Человек
    class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool ActMk { get; set; }

        // Автоматическое создание - работает без параметров
        // Без этого конструктора обращение к контексту "падало"
        public Person()
        {
        }

        // Быстрый способ создать и присвоить значение
        public Person(string name)
        {
            Name = name;
            ActMk = true;
        }

        public override string ToString()
        {
            // return Id.ToString() + ". " + Name;
            return Name;
        }
    }


    // Типы расходов / доходов
    class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Debit { get; set; }
        public bool ActMk { get; set; }
        public int? NotionId { get; set; }  // внешний ключ
        public Notion Notion { get; set; }   // навигационное свойство


        // Основной конструктор
        public Category()
        { }

        public Category(string name, bool debit)
        {
            Name = name;
            Debit = debit;
            ActMk = true;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    // Типы доходов и расходов по Кийосаки, фиксированные значения в справочнике
    public class Notion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Debit { get; set; }
        public bool NeedAsset { get; set; }
        public bool ActMk { get; set; }

        public Notion()
        { }

        public Notion(string name, bool debit, bool needAsset)
        {
            Name = name;
            Debit = debit;
            NeedAsset = needAsset;
            ActMk = true;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    // Активы и имущество
    public class Asset
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool ActMk { get; set; }

        public Asset()
        { }

        public Asset(string name)
        {
            Name = name;
            ActMk = true;
            // Пустые даты заполняю значениями наугад
            BeginDate = DateTime.Now.AddYears(-10);
            EndDate = DateTime.Now.AddYears(30);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    // Движение, т.е. расходы и доходы
    class Motion
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int? PersonId { get; set; }   // внешний ключ
        public Person Person {get; set; }   // навигационное свойство
        public bool Debit { get; set; }
        public int Summa { get; set; }
        public int? CategoryId { get; set; }  // внешний ключ
        public Category Category { get; set; }   // навигационное свойство
        public int? NotionId { get; set; }  // внешний ключ
        public Notion Notion { get; set; }   // навигационное свойство
        public int? AssetId { get; set; }  // внешний ключ
        public Asset Asset { get; set; }   // навигационное свойство
        public string Note { get; set; }
        public bool ActMk { get; set; }
        public Motion()
        { }

        public Motion(DateTime date, Person person, bool debit, int summa, Category category, string note)
        {
            Date = date;
            Person = person;
            PersonId = person.Id;
            Debit = debit;
            Summa = summa;
            Category = category;
            CategoryId = category.Id;
            Note = note;
            ActMk = true;
        }
    }

    // Пример CashDesk работает на таком соединении:
  //<connectionStrings>
  //  <add name = "DefaultConnection"
  //      connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\sales.mdf;Integrated Security=True;Connect Timeout=30"
  //      providerName="System.Data.SqlClient" />
  //</connectionStrings>

    // не работает
    //<add name = "DBConnection" connectionString="data source=(localdb)\MSSQLLocalDB;Initial Catalog=homefinance.mdf;Integrated Security=True;" providerName="System.Data.SqlClient"/>
}
