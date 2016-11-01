using System.Collections.Generic;

namespace ConecctorOneC
{
    public class Connection
    {
        static public dynamic Result;

        static public dynamic ConnectionString()
        {

            return null;

            //var com1S = new V83.COMConnector
            //{
            //    PoolCapacity = 10,
            //    PoolTimeout = 60,
            //    MaxConnections = 2
            //};

            ////Result = com1S.Connect(@"File='\\srvkb\SolidWorks Admin\TEMP\1C_Test';Usr='Админенко (администратор)';pwd='';");
            ////Result = com1S.Connect(@"File='C:\1C_test_base';Usr='Админенко (администратор)';pwd='';");
            //Result = com1S.Connect("srvr='Srvprog'; ref='Yurchenko_new'; usr='com-user'; pwd='1'");
            //return Result;
        }

        public void CreateNomenclature(string description)
        {
            dynamic refer = Result.Справочники.Номенклатура.СоздатьЭлемент();
            refer.Наименование = description;
            refer.Записать();
        }

        public class ClassifierMeasure
        {
            public string Code { get; set; }
            public string DescriptionFull { get; set; }
            public string Description { get; set; }

        }

        public List<ClassifierMeasure> ClassifierMeasureList()
        {
            var list = new List<ClassifierMeasure>();
            dynamic qr = Result.NewObject("Запрос");
            qr.Текст = "ВЫБРАТЬ * ИЗ Справочник.КлассификаторЕдиницИзмерения";
            dynamic выборка = qr.Выполнить().Выбрать();
            while (выборка.Следующий())
            {
                var value = new ClassifierMeasure
                {
                    Code = выборка.Код,
                    DescriptionFull = выборка.НаименованиеПолное,
                    Description = выборка.Наименование
                };
                list.Add(value);
            }
            return list;
        }

        public class GroupnOfNomenclature
        {
           // public string Code { get; set; }
           // public string DescriptionFull { get; set; }
            public string Description { get; set; }
        }

        public List<GroupnOfNomenclature> GroupnOfNomenclatureList()
        {
            var list = new List<GroupnOfNomenclature>();
            dynamic qr = Result.NewObject("Запрос");
            qr.Текст = "ВЫБРАТЬ * ИЗ Справочник.НоменклатурныеГруппы";
            dynamic выборка = qr.Выполнить().Выбрать();
            while (выборка.Следующий())
            {
                var value = new GroupnOfNomenclature
                {
                    //Code = выборка.Код,
                   // DescriptionFull = выборка.НаименованиеПолное,
                    Description = выборка.Наименование
                };
                list.Add(value);
            }
            return list;
        }

        //public List<Nomenclature> SearchNomenclatureByCode(string name)
        //{
        //    var list = new List<Nomenclature>();
        //    dynamic meta = Result.Справочники.Номенклатура.НайтиПоКоду(name);
        //    var value = new Nomenclature
        //    {
        //        Code = meta.Код,
        //        Description = meta.Наименование
        //    };
        //    list.Add(value);
        //    return list;
        //}

        public class Nomenclature
        {
            public string Code { get; set; }
            public string Description { get; set; }

        }

        public Nomenclature SearchNomenclatureByCode(string name)
        {
            dynamic meta = Result.Справочники.Номенклатура.НайтиПоКоду(name);
            var value = new Nomenclature
            {
                Code = meta.Код,
                Description = meta.Наименование
            };
            return value;
        }

        public List<Nomenclature> SearchNomenclatureByName(string name)
        {
            var list = new List<Nomenclature>();
            dynamic qr = Result.NewObject("Запрос");
            qr.Текст = "ВЫБРАТЬ первые 50 * ИЗ Справочник.Номенклатура КАК Спр ГДЕ (Спр.Наименование ПОДОБНО &Наименование)";
            qr.УстановитьПараметр("Наименование", "%" + name + "%");

            dynamic выборка = qr.Выполнить().Выбрать();

            while (выборка.Следующий())
            {
                var value = new Nomenclature
                {
                    Code = выборка.Код,
                    Description = выборка.Наименование
                };
                //MessageBox.Show(выборка.Наименование);
             list.Add(value);
            }
            return list;
        }

        public void Nomen()
        {
            //Получим ссылку на номенклатуру по её коду
            dynamic meta = Result.GetMeasureID_ByGoodsCode("0000228245");

            //А теперь из подчиненного Справочника - ЕдиницыИзмерения, запросом получим "Единицу хранения
            System.Windows.Forms.MessageBox.Show(meta);

            //dynamic query = Result.NewObject("Query");


            //query.Text =@"ВЫБРАТЬ ЕдиницыИзмерения.Ссылка КАК СсылкаНаЕдИзм, ЕдиницыИзмерения.Наименование КАК Наименование ИЗ	Справочник.ЕдиницыИзмерения КАК ЕдиницыИзмерения ГДЕ	ЕдиницыИзмерения.Владелец = &Владелец";

            ////query.Text = @"ВЫБРАТЬ * ИЗ Справочник.Номенклатура";

            //// Установим параметр - ВЛАДЕЛЕЦ, который используется как условие в Запросе.
            //query.SetParameter("Владелец", meta);


            // Выполним текст запроса и сразу выгрузим результат запроса в Таблицу Значений
            //dynamic execute = query.Execute().Unload();

            //if (execute.Количество() == 0)
            //{
            //    MessageBox.Show("Пусто");
            //}

            //    execute[0].СсылкаНаЕдИзм
            //    foreach (dynamic test in execute.СсылкаНаЕдИзм)
            //    {
            //       MessageBox.Show(test.Наименование);
            //    }

            //MessageBox.Show(execute.ВыбратьСтроку().СсылкаНаЕдИзм.ToString());

            //dynamic fe = execute.ВыбратьСтроку();



            //for (int i = 0; i < execute.Количество(); i++)
            //{
            //    dynamic resaltq = execute[i].Наименование;
            //    MessageBox.Show(resaltq);
            //}
            //dynamic resaltq = execute[0].СсылкаНаЕдИзм;
            //MessageBox.Show(resaltq);

            //while (execute.Количество())
            //{
            //    MessageBox.Show(execute.СсылкаНаЕдИзм);
            //}



            //MessageBox.Show("Для номенклатуры: " + meta.Наименование + ", Единица измерения: " + resaltq);



            // MessageBox.Show(execute);

            // var test = execute.СсылкаНаЕдИзм;
            //Type type = test.GetType();
            //var str = (string)type.InvokeMember("СсылкаНаЕдИзм", BindingFlags.GetProperty, null, test, null); // 1

            //MessageBox.Show(test);

            // int rowcount = выборка.Количество();

            //          Type type = execute.GetType();
            //          string i = (string)type.InvokeMember("СсылкаНаЕдИзм", BindingFlags.GetProperty, null, execute, null); // 1

            //foreach (dynamic test in выборка)
            //{
            //dynamic nomenk = i.test;



            //}

        }
    }
}