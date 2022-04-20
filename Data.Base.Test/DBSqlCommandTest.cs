using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Data.Base.Test
{
    public class SqlSelect : ISqlCommandContext
    {
        public string TesteId { get; set; }
        public string Titulo { get; set; }
        public string SQL { get { return "Select TesteId, Titulo From TesteTB Where TesteId = @TestId And Titulo = @Titulo"; } }
        public void AddParameters(SqlCommand command)
        {
            command.Parameters.Add("@TestId", TesteId);
            command.Parameters.Add("@Titulo", Titulo);
        }
    }

    public class SqlInsert : ISqlCommandContext
    {
        public string TesteId { get; set; }
        public string Titulo { get; set; }
        public string SQL { get { return "Insert Into TesteTB (TesteId, Titulo) Values (@TesteId, @Titulo)"; } }
        public void AddParameters(SqlCommand command)
        {
            command.Parameters.Add("@TesteId", TesteId);
            command.Parameters.Add("@Titulo", Titulo);
        }
    }

    [TestFixture]
    public class DBSqlCommandTest
    {
        [SetUp]
        public void TestInitialize()
        {
            using (var db = new DB(true))
            {
                db.Execute("Delete From TesteTB");
            }
        }

        [Test]
        public void Exists_Value_SqlCommand_Test()
        {
            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            }

            //Recupera o valor incluido no bd
            bool actual;
            using (var db = new DB(true))
            {
                actual = db.ExistsValue(new SqlSelect() { TesteId = "1", Titulo = "texto"});
            }

            bool expected = true;

            //Verifica se o valor recuperado é igual ao valor incluido no bd
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Execute_SqlCommand_Test()
        {
            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(new SqlInsert(){TesteId = "2", Titulo = "texto"});
            }

            //Recupera o valor incluido no bd
            bool actual;
            using (var db = new DB(true))
            {
                actual = db.ExistsValue(new SqlSelect() { TesteId = "2", Titulo = "texto" });
            }

            bool expected = true;

            //Verifica se o valor recuperado é igual ao valor incluido no bd
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Get_Data_Reader_From_Procedure_Test()
        {
            string testeIdActual = "", testeIdExpected = "1";
            string tituloActual = "", tituloExpedted = "texto";

            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            }

            //Recupera o valor incluido no bd
            using (var db = new DB(true))
            {
                var dr = db.GetDataReader(new SqlSelect() { TesteId = "1", Titulo = "texto" });

                while (dr.Read())
                {
                    testeIdActual = dr[0].ToString();
                    tituloActual = dr[1].ToString();
                }
            }

            Assert.AreEqual(testeIdExpected, testeIdActual);
            Assert.AreEqual(tituloExpedted, tituloActual);
        }

        [Test]
        public void Get_Data_Table_From_Procedure_Test()
        {
            string testeIdActual = "", testeIdExpected = "1";
            string tituloActual = "", tituloExpedted = "texto";

            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            }

            //Recupera o valor incluido no bd
            DataTable dataTable;
            using (var db = new DB(true))
            {
                dataTable = db.GetDataTable(new SqlSelect() { TesteId = "1", Titulo = "texto" });
            }

            if (dataTable == null) Assert.Fail();
            if (dataTable.Rows.Count == 0) Assert.Fail();

            foreach (DataRow row in dataTable.Rows)
            {
                testeIdActual = row.ItemArray[0].ToString();
                tituloActual = row.ItemArray[1].ToString();
            }

            Assert.AreEqual(testeIdExpected, testeIdActual);
            Assert.AreEqual(tituloExpedted, tituloActual);
        }

        [Test]
        public void Get_Value_From_SqlCommand_Test()
        {
            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "3", "texto"));
            }

            //Recupera o valor incluido no bd
            string actual;
            using (var db = new DB(true))
            {
                actual = db.GetValue(new SqlSelect() { TesteId = "3", Titulo = "texto" });
            }

            string expected = "3";

            //Verifica se o valor recuperado é igual ao valor incluido no bd
            Assert.AreEqual(expected, actual);
        }
    }
}
