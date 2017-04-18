using System;
using System.Data.Entity;
using System.Linq;

namespace HomeFinance
{
    class DataContext : DbContext
    {
        public DataContext() : base("DbConnection")
        {
        }

        public DbSet<Person> Persons { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Notion> Notions { get; set; }

        public DbSet<Asset> Assets { get; set; }

        public DbSet<Motion> Motions { get; set; }

        // Добавим запись, если такой еще нет - Person
        public Person AddPerson(string name)
        {
            Person P = Persons.FirstOrDefault((p => p.Name == name));
            if (P == null)  // Имя не нашли
            {
                P = new Person(name);
                Persons.Add(P);
                SaveChanges();
            }
            else
            {
                if (!P.ActMk)
                {
                    P.ActMk = true;
                    SaveChanges();
                }
            }
            return P;
        }

        public void DeletePerson(Person P)
        {
            // не настоящее удаление - выставляем признак не используемой записи
            Person A = Persons.Single(p => p.Id == P.Id);
            A.ActMk = false;
            // Persons.Remove(P);
            SaveChanges();
        }

        // Добавим запись, если такой еще нет - Category
        public Category AddCategory(string name, bool debit)
        {
            Category C = Categories.FirstOrDefault((p => p.Name == name && p.Debit == debit));
            if (C == null)  // Имя не нашли
            {
                C = new Category(name, debit);
                Categories.Add(C);
                SaveChanges();
            }
            else
            {
                if (!C.ActMk)
                {
                    C.ActMk = true;
                    SaveChanges();
                }
            }
            return C;
        }

        public void DeleteCategory(Category C)
        {
            // не настоящее удаление - выставляем признак не используемой записи
            Category A = Categories.Single(p => p.Id == C.Id);
            A.ActMk = false;
            A.Notion = null;  // отвяжем ссылку
            SaveChanges();
        }

        public void Category_FillNotion(Category C, Notion N)
        {
            Category A = Categories.Single(p => p.Id == C.Id);
            A.Notion = N;
            SaveChanges();
        }

        // Добавим запись, если такой еще нет - Asset
        public Asset AddAsset(string name)
        {
            Asset A = Assets.FirstOrDefault((p => p.Name == name));
            if (A == null)  // Имя не нашли
            {
                A = new Asset(name);
                Assets.Add(A);
                SaveChanges();
            }
            else
            {
                if (!A.ActMk)
                {
                    A.ActMk = true;
                    SaveChanges();
                }
            }
            return A;
        }

        public void DeleteAsset(Asset A)
        {
            // не настоящее удаление - выставляем признак не используемой записи
            Asset B = Assets.Single(p => p.Id == A.Id);
            B.ActMk = false;
            SaveChanges();
        }

        public void AddMotion(Motion M)
        {
            Motions.Add(M);
            SaveChanges();
        }

        public void UpdateMotion(Motion M)
        {
            Motion A = Motions.Single(p => p.Id == M.Id);
            A.Debit = M.Debit;
            A.Person = M.Person;
            A.Date = M.Date;
            A.Summa = M.Summa;
            A.Category = M.Category;
            A.Note = M.Note;
            A.Notion = M.Notion;
            A.Asset = M.Asset;
            A.ActMk = true;
            SaveChanges();
        }

        public void DeleteMotion(Motion M)
        {
            // не настоящее удаление - выставляем признак не используемой записи
            Motion A = Motions.Single(p => p.Id == M.Id);
            A.ActMk = false;
            SaveChanges();
        }

        // Если уже был ввод данных - берем дату, за которую вводили последний раз. Если нет - текущую дату
        public DateTime GetLastDate()
        {
            DateTime D = DateTime.Now;
            var M = Motions.Where(m => m.ActMk).OrderByDescending(m => m.Id).First();
            if (M != null)
                D = M.Date;
            return D;
        }

        // Последний ввод данных
        public Motion GetLastMotion()
        {
            var M = Motions.Where(m => m.ActMk).OrderByDescending(m => m.Id).First();
            return M;
        }

        public void FillNotions()
        { 
            // Заполним справочник типов расходов / доходов по Кийосаки
            if (Notions.Count()<1)
            {
                Notion N = new Notion("Заработанный доход", true, false);
                Notions.Add(N);
                N = new Notion("Пассивный доход", true, true);
                Notions.Add(N);
                N = new Notion("Социальный доход", true, false);
                Notions.Add(N);
                N = new Notion("Жизнеобеспечение", false, false);
                Notions.Add(N);
                N = new Notion("На имущество", false, true);
                Notions.Add(N);
                N = new Notion("Долги и кредиты", false, false);
                Notions.Add(N);
                N = new Notion("Социальный расход", false, false);
                Notions.Add(N);
                SaveChanges();
            }
        }
        

    }

}
