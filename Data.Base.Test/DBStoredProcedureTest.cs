using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Data.Base.Test
{
    public class P_Incluir : IStoredProcedureContext
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string NAME { get { return "P_Incluir"; } }
        public void AddParameters(SqlCommand command)
        {
            command.Parameters.Add("@Id", Id);
            command.Parameters.Add("@Titulo", Titulo);
        }
    }

    public class P_ListaTeste_Contexto : IStoredProcedureContext
    {
        public string NAME { get { return "p_ListaTeste"; } }
        public void AddParameters(SqlCommand command) { return; }
    }

    public class P_Teste : IStoredProcedureContext
    {
        public string NAME { get { return "p_Teste"; } }
        public void AddParameters(SqlCommand command) { return; }
    }

    public class P_ObterPorId2_Contexto : IStoredProcedureContext
    {
        public string Id { get; set; }
        public string NAME { get { return "P_ObterPorId"; } }

        public void AddParameters(SqlCommand command)
        {
            command.Parameters.Add("@Id", Id);
        }
    }

    [TestFixture]
    public class DBStoredProcedureTest
    {
        [SetUp]
        public void TestInitialize()
        {
            var _dataBase = new DB();
            //Exclui os registros no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Delete From TesteTB"));
            _dataBase.CloseConnection();            
        }

        [Test]
        public void Exists_Value_Procedure_Test()
        {
            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(new P_Incluir() { Id = "1234", Titulo = "titulo" });
            }

            bool expected = true;
            bool actual;

            using (var db = new DB(true))
            {
                actual = db.ExistsValue(new P_ListaTeste_Contexto());
            }

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Execute_Procedure_Test()
        {
            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(new P_Incluir() {Id = "1234", Titulo = "titulo"});
            }

            //Verifica se existe o registro no bd
            bool actual;
            using (var db = new DB(true))
            {
                actual = db.ExistsValue("Select 1 From TesteTB Where TesteId = '1234' And Titulo = 'titulo'");
            }
            bool expected = true;

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
                var dr = db.GetDataReader(new P_ListaTeste_Contexto());
                
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
                dataTable = db.GetDataTable(new P_ListaTeste_Contexto());
            }

            if (dataTable == null) Assert.Fail();
            if (dataTable.Rows.Count == 0) Assert.Fail();

            foreach (DataRow row in dataTable.Rows)
            {
                testeIdActual = row.ItemArray[0].ToString();
                tituloActual = row.ItemArray[1].ToString();
            }

            Assert.AreEqual(testeIdExpected, testeIdActual);
            Assert.AreEqual(tituloExpedted ,tituloActual);
        }

        [Test]
        public void Get_Value_From_Procedure_Test()
        {
            string actual;
            string expected = "4";

            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "4", "quatro"));
            }

            //Recupera o valor incluido no bd
            using (var db = new DB(true))
            {
                actual = db.GetValue(new P_ObterPorId2_Contexto { Id = "4" });
            }

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Get_Result_From_Procedure_Test()
        {
            string actual;
            string expected = "24";

            using (var db = new DB(true))
            {
                actual = db.GetResult(new P_Teste());
            }

            Assert.AreEqual(expected, actual);
        }
    }
}
