using System;
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Data.Base.Test
{
    [TestFixture]
    public class DBTest
    {
        DB _dataBase;

        [SetUp]
        public void TestInitialize()
        {
            _dataBase = new DB();

            //Apaga os registros no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Delete From TesteTB"));
            _dataBase.CloseConnection();  
        }

        [TearDown]
        public void TestCleanup()
        {
            _dataBase = null;
        }

        [Test]
        public void Abrir_Fechar_Conexao_Test()
        {
            _dataBase.OpenConnection();
            _dataBase.CloseConnection();
        }

        [Test]
        [ExpectedException(typeof(ExcecaoAbrirConexao))]
        public void Excecao_Ao_Abrir_Conexao_Aberta_Test()
        {
            _dataBase.OpenConnection();
            _dataBase.OpenConnection();
        }

        [Test]
        [ExpectedException(typeof(ExcecaoFecharConexao))]
        public void Excecao_Ao_Fechar_Conexao_Fechada_Test()
        {
            _dataBase.CloseConnection();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Excecao_Construir_Sem_Abrir_Conexao_Test()
        {
            var db = new DB(openConnection: false);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Excecao_Construir_Sem_Abrir_Conexao_Sem_Inicar_Transacao_Test()
        {
            var db = new DB(openConnection: false, beginTransaction: false);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Excecao_Construir_Sem_Abrir_Conexao_Com_Inicar_Transacao_Test()
        {
            var db = new DB(openConnection: false, beginTransaction: true);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Excecao_Construir_Com_Abrir_Conexao_Sem_Inicar_Transacao_Test()
        {
            var db = new DB(openConnection: true, beginTransaction: false);
        }

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void Excecao_Get_Value_Com_Transacao_Test()
        {
            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.BeginTransaction();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.RollBackTransaction();
            _dataBase.CloseConnection();

            //Recupera o valor incluido no bd
            _dataBase.OpenConnection();
            string actual = _dataBase.GetValue("Select Titulo From TesteTB Where TesteId = '1'");
            _dataBase.CloseConnection();
            string expected = "texto";

            //Verifica se o valor recuperado é igual ao valor incluido no bd
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Execute_Sql_Test()
        {
            //Cria o registro no bd
            _dataBase.OpenConnection();
            int actual = _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CloseConnection();

            int expected = 1;

            //Verifica se o número de linhas afetadas é 1, equivalente ao Insert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Execute_Sql_Com_Transacao_Test()
        {
            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.BeginTransaction();
            int actual = _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CommitTransaction();
            _dataBase.CloseConnection();

            int expected = 1;

            //Verifica se o número de linhas afetadas é 1, equivalente ao Insert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Execute_Sql_Com_Dispose_Test()
        {
            //Cria o registro no bd
            int actual;

            using (var db = new DB(true))
            {
                actual = db.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            }
                
            int expected = 1;

            //Verifica se o número de linhas afetadas é 1, equivalente ao Insert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Exist_Value_Test()
        {
            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CloseConnection();

            //Verifica se existe o registro no bd
            _dataBase.OpenConnection();
            bool actual = _dataBase.ExistsValue("Select 1 From TesteTB Where TesteId = '1'");
            _dataBase.CloseConnection();
            bool expected = true;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Exist_Value_Com_Transacao_Test()
        {
            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CloseConnection();

            //Verifica se existe o registro no bd
            _dataBase.OpenConnection();
            _dataBase.BeginTransaction();
            bool actual = _dataBase.ExistsValue("Select 1 From TesteTB Where TesteId = '1'");
            _dataBase.CommitTransaction();
            _dataBase.CloseConnection();
            bool expected = true;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Exist_Value_Com_Dispose_Test()
        {
            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            }

            bool actual;
            //Verifica se existe o registro no bd
            using (var db = new DB(true))
            {
                 actual = db.ExistsValue("Select 1 From TesteTB Where TesteId = '1'");
            }

            bool expected = true;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Get_Value_Test()
        {
            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CloseConnection();

            //Recupera o valor incluido no bd
            _dataBase.OpenConnection();
            string actual = _dataBase.GetValue("Select Titulo From TesteTB Where TesteId = '1'");
            _dataBase.CloseConnection();
            string expected = "texto";

            //Verifica se o valor recuperado é igual ao valor incluido no bd
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Get_Value_Com_Transacao_Test()
        {
            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CloseConnection();

            //Recupera o valor incluido no bd
            _dataBase.OpenConnection();
            _dataBase.BeginTransaction();
            string actual = _dataBase.GetValue("Select Titulo From TesteTB Where TesteId = '1'");
            _dataBase.CommitTransaction();
            _dataBase.CloseConnection();
            string expected = "texto";

            //Verifica se o valor recuperado é igual ao valor incluido no bd
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Get_Value_Com_Dispose_Test()
        {
            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            }

            //Recupera o valor incluido no bd
            string actual;
            using (var db = new DB(true))
            {
                actual = db.GetValue("Select Titulo From TesteTB Where TesteId = '1'");
            }

            string expected = "texto";

            //Verifica se o valor recuperado é igual ao valor incluido no bd
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetDataReaderTest()
        {
            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CloseConnection();

            //Recupera o valor incluido no bd
            _dataBase.OpenConnection();
            SqlDataReader dataReader = _dataBase.GetDataReader("Select TesteId, Titulo From TesteTB Where TesteId = '1'");
            while (dataReader.Read())
            {
                Assert.AreEqual("1", dataReader[0].ToString());
                Assert.AreEqual("texto", dataReader[1].ToString());
            }
            _dataBase.CloseConnection();
        }

        [Test]
        public void GetDataSetTest()
        {
            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CloseConnection();

            //Recupera o valor incluido no bd
            _dataBase.OpenConnection();
            DataSet dataSet = _dataBase.GetDataSet("Select TesteId, Titulo From TesteTB Where TesteId = '1'");
            _dataBase.CloseConnection();

            if (dataSet == null) Assert.Fail();
            if (dataSet.Tables[0].Rows.Count == 0) Assert.Fail();

            foreach (DataRow dr in dataSet.Tables[0].Rows)
            {
                Assert.AreEqual("1", dr[0].ToString());
                Assert.AreEqual("texto", dr[1].ToString());
            }
        }

        [Test]
        public void Get_Data_Table_Test()
        {
            string testeIdActual = "", testeIdExpected = "1";
            string tituloActual = "", tituloExpected = "texto";

            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CloseConnection();

            //Recupera o valor incluido no bd
            _dataBase.OpenConnection();
            DataTable dataTable = _dataBase.GetDataTable("Select TesteId, Titulo From TesteTB Where TesteId = '1'");
            _dataBase.CloseConnection();

            if (dataTable == null) Assert.Fail();
            if (dataTable.Rows.Count == 0) Assert.Fail();

            foreach (DataRow row in dataTable.Rows)
            {
                testeIdActual = row.ItemArray[0].ToString();
                tituloActual = row.ItemArray[1].ToString();
            }

            Assert.AreEqual(testeIdExpected, testeIdActual);
            Assert.AreEqual(tituloExpected, tituloActual);
        }

        [Test]
        public void Get_Data_Table_Com_Transaction_Test()
        {
            string testeIdActual = "", testeIdExpected = "1";
            string tituloActual = "", tituloExpected = "texto";

            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CloseConnection();

            //Recupera o valor incluido no bd
            _dataBase.OpenConnection();
            _dataBase.BeginTransaction();
            DataTable dataTable = _dataBase.GetDataTable("Select TesteId, Titulo From TesteTB Where TesteId = '1'");
            _dataBase.CommitTransaction();
            _dataBase.CloseConnection();

            if (dataTable == null) Assert.Fail();
            if (dataTable.Rows.Count == 0) Assert.Fail();

            foreach (DataRow row in dataTable.Rows)
            {
                testeIdActual = row.ItemArray[0].ToString();
                tituloActual = row.ItemArray[1].ToString();
            }

            Assert.AreEqual(testeIdExpected, testeIdActual);
            Assert.AreEqual(tituloExpected, tituloActual);
        }

        [Test]
        public void Get_Data_Table_Com_Dispose_Test()
        {
            string testeIdActual = "", testeIdExpected = "1";
            string tituloActual = "", tituloExpected = "texto";

            //Cria o registro no bd
            using (var db = new DB(true))
            {
                db.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            }

            //Recupera o valor incluido no bd
            DataTable dataTable;
            using (var db = new DB(true))
            {
                dataTable = db.GetDataTable("Select TesteId, Titulo From TesteTB Where TesteId = '1'");
            }

            if (dataTable == null) Assert.Fail();
            if (dataTable.Rows.Count == 0) Assert.Fail();

            foreach (DataRow row in dataTable.Rows)
            {
                testeIdActual = row.ItemArray[0].ToString();
                tituloActual = row.ItemArray[1].ToString();
            }

            Assert.AreEqual(testeIdExpected, testeIdActual);
            Assert.AreEqual(tituloExpected, tituloActual);
        }

    }
}
