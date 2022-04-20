using System;
using System.Data;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace Data.BaseMySql.Test
{
    [TestFixture]
    public class DBTest
    {
        private DB _dataBase;

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
        public void AbrirFecharConexaoTest()
        {
            _dataBase.OpenConnection();
            _dataBase.CloseConnection();
        }

        [Test]
        [ExpectedException(typeof (ExcecaoAbrirConexao))]
        public void ExcecaoAbrirConexaoTest()
        {
            _dataBase.OpenConnection();
            _dataBase.OpenConnection();
        }

        [Test]
        [ExpectedException(typeof (ExcecaoFecharConexao))]
        public void ExcecaoFecharConexaoTest()
        {
            _dataBase.CloseConnection();
        }

        [Test]
        public void ExecuteSqlTest()
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
        public void ExistValueTest()
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
        public void GetValueTest()
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
        public void GetDataReaderTest()
        {
            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", "1", "texto"));
            _dataBase.CloseConnection();

            //Recupera o valor incluido no bd
            _dataBase.OpenConnection();
            MySqlDataReader dataReader =
                _dataBase.GetDataReader("Select TesteId, Titulo From TesteTB Where TesteId = '1'");
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
        public void GetDataTableTest()
        {
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
                Assert.AreEqual("1", row.ItemArray[0].ToString());
                Assert.AreEqual("texto", row.ItemArray[1].ToString());
            }
        }

        [Test]
        public void DateTest()
        {
            DateTime expectedDate = DateTime.Now;
            
            //Cria o registro no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Insert Into TesteTB (TesteId, Titulo, Data, DataHora) Values ('{0}', '{1}', '{2}', '{3}')", "1", "texto", _dataBase.DateMySql(expectedDate), _dataBase.DateTimeMySql(expectedDate)));
            _dataBase.CloseConnection();

            //Recupera o valor incluido no bd
            _dataBase.OpenConnection();
            string actualDataHora = _dataBase.GetValue("Select DataHora From TesteTB Where TesteId = '1'");
            DateTime actualDateTime = _dataBase.DateMySql(actualDataHora);
            _dataBase.CloseConnection();

            //Verifica se o valor recuperado é igual ao valor incluido no bd
            Assert.AreEqual(expectedDate.ToString(), actualDateTime.ToString());

            //Recupera o valor incluido no bd
            _dataBase.OpenConnection();
            string actualData = _dataBase.GetValue("Select Data From TesteTB Where TesteId = '1'");
            DateTime actualDate = _dataBase.DateMySql(actualData);
            _dataBase.CloseConnection();

            //Verifica se o valor recuperado é igual ao valor incluido no bd
            Assert.AreEqual(expectedDate.ToShortDateString(), actualDate.ToShortDateString());
        }

    }
}
