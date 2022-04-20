using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Data.Base.Test
{
    public class Entidade
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
    }

    public class P_ObterPorId_Contexto : IStoredProcedureContext
    {
        public string Id { get; set; }

        public string NAME
        {
            get { return "P_ObterPorId"; }
        }

        public void AddParameters(SqlCommand command)
        {
            command.Parameters.Add("@Id", Id);
        }
    }

    public class P_Lista_Teste_Context : IStoredProcedureContext
    {
        public string NAME
        {
            get { return "p_ListaTeste"; }
        }

        public void AddParameters(SqlCommand command)
        {
            return;
        }
    }

    public class EntidadeDAO : BaseDao<Entidade>
    {
        protected override string GetSelectCommand()
        {
            throw new NotImplementedException();
        }

        public Entidade ObterPorSQL(string id)
        {
            return GetBySql(string.Format("Select TesteId, Titulo From TesteTB Where TesteId = '{0}'", id));
        }

        protected override string GetSelectCommand(string id)
        {
            return string.Format("Select TesteId, Titulo From TesteTB Where TesteId = '{0}'", id);
        }

        protected override string GetSelectCommandWithJoin(string foreignKey)
        {
            throw new NotImplementedException();
        }

        protected override string GetInsertCommand(Entidade entidade)
        {
            return string.Format("Insert Into TesteTB (TesteId, Titulo) Values ('{0}', '{1}')", entidade.Id, entidade.Titulo);
        }

        protected override string GetDeleteCommand(Entidade entidade)
        {
            return string.Format("Delete From TesteTB Where TesteId = '{0}'", entidade.Id);
        }

        protected override string GetUpdateCommand(Entidade entidade)
        {
            return string.Format("UPDATE TesteTB SET Titulo = '{0}' WHERE (TesteId = '{1}')", entidade.Titulo, entidade.Id);
        }

        protected override string GetExistsCommand(Entidade entidade)
        {
            return string.Format("Select TesteId, Titulo From TesteTB Where TesteId = '{0}'", entidade.Id);
        }

        protected override Entidade Hydrate(SqlDataReader reader)
        {
            return new Entidade()
            {
                Id = reader[0].ToString(),
                Titulo = reader[1].ToString()
            };
        }
    }

    [TestFixture]
    public class BaseDAOTest
    {
        EntidadeDAO _entidadeDAO;

        [SetUp]
        public void TestInitialize()
        {
            var _dataBase = new DB();
            //Exclui os registros no bd
            _dataBase.OpenConnection();
            _dataBase.Execute(string.Format("Delete From TesteTB"));
            _dataBase.CloseConnection();

            _entidadeDAO = new EntidadeDAO();
        }

        [TearDown]
        public void TestCleanup()
        {
            _entidadeDAO = null;
        }

        [Test]
        public void InsertTest()
        {
            Entidade entidade = new Entidade()
            {
                Id = "77",
                Titulo = "texto77"
            };

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Insert(entidade);
            _entidadeDAO.CloseConnection();

            _entidadeDAO.OpenConnection();
            Entidade entidadeRecuperada = _entidadeDAO.Get(entidade.Id);
            _entidadeDAO.CloseConnection();

            Assert.AreEqual(entidadeRecuperada.Id, entidade.Id);
            Assert.AreEqual(entidadeRecuperada.Titulo, entidade.Titulo);

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Delete(entidade);
            _entidadeDAO.CloseConnection();
        }

        [Test]
        public void ObterPorSqlTest()
        {
            Entidade entidade = new Entidade()
            {
                Id = "77",
                Titulo = "texto77"
            };

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Insert(entidade);
            _entidadeDAO.CloseConnection();

            _entidadeDAO.OpenConnection();
            Entidade entidadeRecuperada = _entidadeDAO.ObterPorSQL("77");
            _entidadeDAO.CloseConnection();

            Assert.AreEqual(entidadeRecuperada.Id, entidade.Id);
            Assert.AreEqual(entidadeRecuperada.Titulo, entidade.Titulo);

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Delete(entidade);
            _entidadeDAO.CloseConnection();
        }

        [Test]
        public void ExistsTest()
        {
            Entidade entidade = new Entidade()
            {
                Id = "77",
                Titulo = "texto77"
            };

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Insert(entidade);
            _entidadeDAO.CloseConnection();

            _entidadeDAO.OpenConnection();
            bool resultado = _entidadeDAO.Exists(entidade);
            _entidadeDAO.CloseConnection();

            Assert.AreEqual(resultado, true);

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Delete(entidade);
            _entidadeDAO.CloseConnection();
        }

        [Test]
        public void UpdateTest()
        {
            Entidade entidade = new Entidade()
            {
                Id = "77",
                Titulo = "texto77"
            };
            _entidadeDAO.OpenConnection();
            _entidadeDAO.Insert(entidade);
            _entidadeDAO.CloseConnection();

            entidade.Titulo = "Alterado";

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Update(entidade);
            _entidadeDAO.CloseConnection();

            _entidadeDAO.OpenConnection();
            bool resultado = _entidadeDAO.Exists(entidade);
            _entidadeDAO.CloseConnection();

            Assert.AreEqual(resultado, true);

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Delete(entidade);
            _entidadeDAO.CloseConnection();
        }

        [Test]
        public void GetFromProcedureTest()
        {
            Entidade entidade = new Entidade()
            {
                Id = "77",
                Titulo = "texto77"
            };

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Insert(entidade);
            _entidadeDAO.CloseConnection();

            _entidadeDAO.OpenConnection();
            Entidade entidadeRecuperada = _entidadeDAO.Get(new P_ObterPorId_Contexto(){Id = "77"});
            _entidadeDAO.CloseConnection();

            Assert.AreEqual("77", entidadeRecuperada.Id);
            Assert.AreEqual("texto77", entidadeRecuperada.Titulo);

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Delete(entidade);
            _entidadeDAO.CloseConnection();
        }

        [Test]
        public void GetAllFromProcedureTest()
        {
            Entidade entidade = new Entidade()
            {
                Id = "77",
                Titulo = "texto77"
            };

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Insert(entidade);
            _entidadeDAO.CloseConnection();

            _entidadeDAO.OpenConnection();
            List<Entidade> entidades = _entidadeDAO.GetAll(new P_Lista_Teste_Context());
            _entidadeDAO.CloseConnection();

            Assert.AreEqual(1, entidades.Count);
            Assert.AreEqual("77", entidades[0].Id);
            Assert.AreEqual("texto77", entidades[0].Titulo);

            _entidadeDAO.OpenConnection();
            _entidadeDAO.Delete(entidade);
            _entidadeDAO.CloseConnection();
        }
    }
}
