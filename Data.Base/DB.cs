using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Data.Base
{
    public class ExcecaoAbrirConexao : Exception
    {
        public ExcecaoAbrirConexao(string mensagem)
            : base(mensagem)
        {
        }
    }

    public class ExcecaoFecharConexao : Exception
    {
        public ExcecaoFecharConexao(string mensagem)
            : base(mensagem)
        {
        }
    }

    public class ExcecaoExecutarSQL : Exception
    {
        public ExcecaoExecutarSQL(string mensagem)
            : base(mensagem)
        {
        }
    }

    public class DB : IDisposable
    {
        private SqlConnection _connection;
        private SqlTransaction _transaction;

        #region Construtores

        /// <summary>
        /// Instancia o objeto DB.
        /// </summary>
        public DB()
        {
            _connection = null;
            _transaction = null;
        }

        /// <summary>
        /// Instancia o objeto DB e abre a conexão com o banco de dados.
        /// </summary>
        public DB(bool openConnection):this()
        {
            if(!openConnection) 
                throw new ArgumentException("Este construtor é utilizado para abrir a conexão.");
            
            OpenConnection();
        }

        /// <summary>
        /// Instancia o objeto DB, abre a conexão com o banco de dados e inicia a transação.
        /// </summary>
        public DB(bool openConnection, bool beginTransaction): this()
        {
            if (!openConnection || !beginTransaction) 
                throw new ArgumentException("Este construtor é utilizado para abrir a conexão e inicar a transação.");

            OpenConnection();
            BeginTransaction();
        }

        #endregion

        #region Connection

        /// <summary>
        /// Obtém a string de conexão do arquivo App.config.
        /// </summary>
        private string GetConnectionString()
        {
            try
            {
                return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
            catch (Exception ex)
            {
                throw new Exception("Descrição: Erro ao obter a string de conexão: \"ConnectionString\"." + "\n" +
                                    "Verifique se o arquivo App.config possui uma string de conexão como esta:" + "\n" +
                                    "<connectionStrings>" + "\n" +
                                    "<add name=\"ConnectionString\"" + "\n" +
                                    "connectionString=\"Data Source=.\\SQLEXPRESS; Initial Catalog=master; Integrated Security=True;\"/>" + "\n" +
                                    "</connectionStrings>" + "\n" + "Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Abre a conexão com o banco de dados.
        /// </summary>
        public void OpenConnection()
        {
            try
            {
                if (_connection == null || _connection.ConnectionString == "")
                {
                    _connection = new SqlConnection(GetConnectionString());
                }

                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                    return;
                }

                throw new Exception("Conexão não estava fechada.");
            }
            catch (Exception ex)
            {
                throw new ExcecaoAbrirConexao("Descrição: Erro ao abrir conexão." + "\n" + "Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Fecha a conexão com o banco de dados.
        /// </summary>
        public void CloseConnection()
        {
            try
            {
                if (_connection.State == ConnectionState.Open)
                {
                    if(_transaction != null)
                        throw new Exception("Transação não esta finalizada.");

                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                    return;
                }

                throw new Exception("Conexão não estava aberta.");
            }
            catch (Exception ex)
            {
                throw new ExcecaoFecharConexao("Descrição: Erro ao fechar conexão." + "\n" + "Erro : " + ex.Message);
            }
        }

        #endregion

        #region Transaction

        /// <summary>
        /// Iniciar a transação.
        /// </summary>
        public void BeginTransaction()
        {
            if (_transaction != null)
                throw new Exception("Descrição: Erro ao Iniciar transação. Transação não esta finalizada.");

            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// Consolida a transação.
        /// </summary>
        public void CommitTransaction()
        {
            if (_transaction == null)
                throw new Exception("Descrição: Erro ao Consolidar transação. Transação esta finalizada.");

            try
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
            catch (Exception ex)
            {
                throw new Exception("Uma exceção do tipo: " + ex.GetType() + " foi encontrada durante a tentativa de consolidar a transação.");
            }
        }

        /// <summary>
        /// Cancela a transação.
        /// </summary>
        public void RollBackTransaction()
        {
            if (_transaction == null)
                throw new Exception("Descrição: Erro ao Cancelar transação. Transação esta finalizada.");

            try
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
            catch (Exception ex)
            {
                throw new Exception("Uma exceção do tipo: " + ex.GetType() + " foi encontrada durante a tentativa de cancelar a transação.");
            }
        }

        #endregion

        #region GetCommand

        /// <summary>
        /// Retorna um SqlCommand baseado no texto da query passada pelo parâmetro command.
        /// </summary>
        /// <param name="command">Query ou nome de algum procedimento armazenado no banco.</param>
        public SqlCommand GetCommand(string command)
        {
            var sqlCommand = new SqlCommand(command, _connection);
            if(_transaction != null) sqlCommand.Transaction = _transaction;
            return sqlCommand;
        }

        #endregion

        //public bool ExistsValue(string sql)
        //public bool ExistsValue(SqlCommand sqlCommand)
        //public bool ExistsValue(IStoredProcedureContext context)
        //public bool ExistsValue(ISqlCommandContext context)
        #region ExistsValue

        /// <summary>
        /// Retorna true se a Stored Procedure passado pelo parâmetro context retornar uma linha.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa IStoredProcedureContext</param>
        public bool ExistsValue(IStoredProcedureContext context)
        {
            using (var command = GetCommand(context.NAME))
            {
                command.CommandType = CommandType.StoredProcedure;
                context.AddParameters(command);
                return ExistsValue(command);
            }
        }

        /// <summary>
        /// Retorna true se o SQL passado pelo parâmetro sql retornar uma linha.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa ISqlCommandContext</param>
        public bool ExistsValue(ISqlCommandContext context)
        {
            using (var command = GetCommand(context.SQL))
            {
                context.AddParameters(command);
                return ExistsValue(command);
            }
        }

        /// <summary>
        /// Retorna true se a query passado pelo parâmetro sql retornar uma linha.
        /// </summary>
        /// <param name="sql">string</param>
        public bool ExistsValue(string sql)
        {
            using (var command = GetCommand(sql))
            {
                return ExistsValue(command);
            }
        }

        /// <summary>
        /// Retorna true se o SqlCommand passado pelo parâmetro sqlCommand retornar uma linha.
        /// </summary>
        /// <param name="sqlCommand">SqlCommand</param>
        public bool ExistsValue(SqlCommand sqlCommand)
        {
            try
            {
                bool exists;

                using (var dataReader = sqlCommand.ExecuteReader())
                {
                    exists = dataReader.HasRows;
                    dataReader.Close();
                }

                return exists;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (SqlException ex)
            {
                var errorMessages = new StringBuilder();
                for (var i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append(" Index # " + i + "\n" +
                                         " Message: " + ex.Errors[i].Message + "\n" +
                                         " LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                         " Source: " + ex.Errors[i].Source + "\n" +
                                         " Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                throw new ExcecaoExecutarSQL(errorMessages.ToString());
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        #endregion

        //public int Execute(IStoredProcedureContext context)
        //public int Execute(string sql)
        //public int Execute(SqlCommand sqlCommand)
        //public int Execute(ISqlCommandContext context)
        #region Execute

        /// <summary>
        /// Retorna o número de linhas afetadas pelo SqlCommand passado pelo parâmetro sqlCommand.
        /// </summary>
        /// <param name="sqlCommand">SqlCommand</param>
        public int Execute(SqlCommand sqlCommand)
        {
            try
            {
                return sqlCommand.ExecuteNonQuery();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (SqlException ex)
            {
                var errorMessages = new StringBuilder();
                for (var i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append(" Index # " + i + "\n" +
                                         " Message: " + ex.Errors[i].Message + "\n" +
                                         " LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                         " Source: " + ex.Errors[i].Source + "\n" +
                                         " Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                throw new ExcecaoExecutarSQL(errorMessages.ToString());
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Retorna o número de linhas afetadas pela Stored Procedure passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa IStoredProcedureContext</param>
        public int Execute(IStoredProcedureContext context)
        {
            using (var command = GetCommand(context.NAME))
            {
                command.CommandType = CommandType.StoredProcedure;
                context.AddParameters(command);
                return Execute(command);
            }
        }

        /// <summary>
        /// Retorna o número de linhas afetadas pelo SQL passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa ISqlCommandContext</param>
        public int Execute(ISqlCommandContext context)
        {
            using (var command = GetCommand(context.SQL))
            {
                context.AddParameters(command);
                return Execute(command);
            }
        }

        /// <summary>
        /// Retorna o número de linhas afetadas pela query passada por parâmetro.
        /// </summary>
        /// <param name="sql">Query</param>
        public int Execute(string sql)
        {
            using (var command = GetCommand(sql))
            {
                return Execute(command);
            }
        }

        #endregion

        //public SqlDataReader GetDataReader(string sql)
        //public SqlDataReader GetDataReader(SqlCommand sqlCommand)
        //public SqlDataReader GetDataReader(IStoredProcedureContext context)
        //public SqlDataReader GetDataReader(ISqlCommandContext context)
        #region GetDataReader

        /// <summary>
        /// Retorna um SqlDataReader baseado no contexto da Stored Procedure passado pelo parâmetro context.
        /// DataReader = Conectado ao banco de dados, a conexão não pode ser fechada.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa IStoredProcedureContext</param>
        public SqlDataReader GetDataReader(IStoredProcedureContext context)
        {
            using (var command = GetCommand(context.NAME))
            {
                command.CommandType = CommandType.StoredProcedure;
                context.AddParameters(command);
                return GetDataReader(command);
            }
        }

        /// <summary>
        /// Retorna um SqlDataReader baseado no contexto do SQL passado pelo parâmetro context.
        /// DataReader = Conectado ao banco de dados, a conexão não pode ser fechada.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa ISqlCommandContext</param>
        public SqlDataReader GetDataReader(ISqlCommandContext context)
        {
            using (var command = GetCommand(context.SQL))
            {
                context.AddParameters(command);
                return GetDataReader(command);
            }
        }

        /// <summary>
        /// Retorna o SqlDataReader baseado no texto da query passada pelo parâmetro sql.
        /// DataReader = Conectado ao banco de dados, a conexão não pode ser fechada.
        /// </summary>
        /// <param name="sql">string</param>
        /// <returns></returns>
        public SqlDataReader GetDataReader(string sql)
        {
            using (SqlCommand command = GetCommand(sql))
            {
                return GetDataReader(command);
            }
        }

        /// <summary>
        /// Retorna o SqlDataReader baseado SqlCommand passado pelo parâmetro sqlCommand.
        /// DataReader = Conectado ao banco de dados, a conexão não pode ser fechada.
        /// </summary>
        /// <param name="sqlCommand">SqlCommand</param>
        /// <returns></returns>
        public SqlDataReader GetDataReader(SqlCommand sqlCommand)
        {
            try
            {
                return sqlCommand.ExecuteReader();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (SqlException ex)
            {
                var errorMessages = new StringBuilder();
                for (var i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append(" Index # " + i + "\n" +
                                         " Message: " + ex.Errors[i].Message + "\n" +
                                         " LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                         " Source: " + ex.Errors[i].Source + "\n" +
                                         " Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                throw new ExcecaoExecutarSQL(errorMessages.ToString());
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        #endregion

        //public DataSet GetDataSet(string sql)
        #region GetDataSet

        /// <summary>
        /// Retorna o DataSet baseado no texto da query passada pelo parâmetro sql.
        /// DataSet = Desconectado ao banco de dados, a conexão pode ser fechada.
        /// </summary>
        /// <param name="sql">Query</param>
        public DataSet GetDataSet(string sql)
        {
            try
            {
                var dataSet = new DataSet();

                using (var dataAdapter = new SqlDataAdapter(GetCommand(sql)))
                {
                    dataAdapter.Fill(dataSet, "0");
                }

                return dataSet;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (SqlException ex)
            {
                var errorMessages = new StringBuilder();
                for (var i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append(" Index # " + i + "\n" +
                                         " Message: " + ex.Errors[i].Message + "\n" +
                                         " LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                         " Source: " + ex.Errors[i].Source + "\n" +
                                         " Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                throw new ExcecaoExecutarSQL(errorMessages.ToString());
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        //Percorrendo um DataSet
        //if (datatable1 !=null)
        //{
        //    foreach (DataRow dr i datatable1.Rows)
        //    {
        //        string text1 = dr["Column1Name Or Number"].ToString();
        //        string text2 = dr["Column1Name Or Number"].ToString();
        //    }
        //}

        #endregion

        //public DataTable GetDataTable(string sql)
        //public DataTable GetDataTable(SqlCommand sqlCommand)
        //public DataTable GetDataTable(IStoredProcedureContext context)
        //public DataTable GetDataTable(ISqlCommandContext context)
        #region GetDataTable

        /// <summary>
        /// Retorna o DataTable baseado no texto da query passada pelo parâmetro sql.
        /// DataTable = Desconectado ao banco de dados, a conexão pode ser fechada.
        /// </summary>
        /// <param name="sql">Query</param>
        public DataTable GetDataTable(string sql)
        {
            return GetDataTable(GetCommand(sql));
        }

        /// <summary>
        /// Retorna o DataTable baseado no SqlCommand passado pelo parâmetro sqlCommand.
        /// DataTable = Desconectado ao banco de dados, a conexão pode ser fechada.
        /// </summary>
        /// <param name="sqlCommand">SqlCommand</param>
        public DataTable GetDataTable(SqlCommand sqlCommand)
        {
            try
            {
                var dataTable = new DataTable();

                using (var dataAdapter = new SqlDataAdapter(sqlCommand))
                {
                    dataAdapter.Fill(dataTable);
                }

                return dataTable;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (SqlException ex)
            {
                var errorMessages = new StringBuilder();
                for (var i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append(" Index # " + i + "\n" +
                                         " Message: " + ex.Errors[i].Message + "\n" +
                                         " LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                         " Source: " + ex.Errors[i].Source + "\n" +
                                         " Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                throw new ExcecaoExecutarSQL(errorMessages.ToString());
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        /// <summary>
        /// Retorna um DataTable baseado no contexto da Stored Procedure passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa IStoredProcedureContext</param>
        public DataTable GetDataTable(IStoredProcedureContext context)
        {
            using (var command = GetCommand(context.NAME))
            {
                command.CommandType = CommandType.StoredProcedure;
                context.AddParameters(command);
                return GetDataTable(command);
            }
        }

        /// <summary>
        /// Retorna um DataTable baseado no contexto da Stored Procedure passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa ISqlCommandContext</param>
        public DataTable GetDataTable(ISqlCommandContext context)
        {
            using (var command = GetCommand(context.SQL))
            {
                context.AddParameters(command);
                return GetDataTable(command);
            }
        }

        //Percorrendo um DataTable
        //static void Main()
        //{
        //    DataTable table = GetTable(); // Get the data table.
        //    foreach (DataRow row in table.Rows) // Loop over the rows.
        //    {
        //        Console.WriteLine("--- Row ---"); // Print separator.
        //        foreach (var item in row.ItemArray) // Loop over the items.
        //        {
        //            Console.Write("Item: "); // Print label.
        //            Console.WriteLine(item); // Invokes ToString abstract method.
        //        }
        //    }
        //    Console.Read(); // Pause.
        //}

#endregion

        //public string GetValue(IStoredProcedureContext context)
        //public string GetValue(ISqlCommandContext context)
        //public string GetValue(string sql)
        //public string GetValue(SqlCommand sqlCommand)
        #region GetValue

        /// <summary>
        /// Retorna o valor da primeira coluna da primeira linha do resultado da Stored Procedure passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa IStoredProcedureContext</param>
        public string GetValue(IStoredProcedureContext context)
        {
            using (var command = GetCommand(context.NAME))
            {
                command.CommandType = CommandType.StoredProcedure;
                context.AddParameters(command);
                return GetValue(command);
            }
        }

        /// <summary>
        /// Retorna o valor da primeira coluna da primeira linha do resultado do SQL passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa ISqlCommandContext</param>
        public string GetValue(ISqlCommandContext context)
        {
            using (var command = GetCommand(context.SQL))
            {
                context.AddParameters(command);
                return GetValue(command);
            }
        }

        /// <summary>
        /// Retorna o valor da primeira coluna da primeira linha do resultado da query passada por parâmetro.
        /// </summary>
        /// <param name="sql">Query</param>
        public string GetValue(string sql)
        {
            return GetValue(GetCommand(sql));
        }

        /// <summary>
        /// Retorna o valor da primeira coluna da primeira linha do resultado do SqlCommand passado por parâmetro.
        /// </summary>
        /// <param name="sqlCommand">SqlCommand</param>
        public string GetValue(SqlCommand sqlCommand)
        {
            try
            {
                return sqlCommand.ExecuteScalar().ToString();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (SqlException ex)
            {
                var errorMessages = new StringBuilder();
                for (var i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append(" Index # " + i + "\n" +
                                         " Message: " + ex.Errors[i].Message + "\n" +
                                         " LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                         " Source: " + ex.Errors[i].Source + "\n" +
                                         " Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                throw new ExcecaoExecutarSQL(errorMessages.ToString());
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        #endregion

        //public string GetResult(IStoredProcedureContext context)
        #region GetResult

        /// <summary>
        /// Retorna o valor do resultado da Stored Procedure passado pelo parâmetro context.
        /// </summary>
        /// <param name="context">Classe de contexto que implementa IStoredProcedureContext</param>
        public string GetResult(IStoredProcedureContext context)
        {
            try
            {
                string result;

                using (var command = GetCommand(context.NAME))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    context.AddParameters(command);

                    var returnValue = command.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;

                    command.ExecuteNonQuery();

                    result = returnValue.Value.ToString();
                }

                return result;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Erro : " + ex.Message);
            }
            catch (SqlException ex)
            {
                var errorMessages = new StringBuilder();
                for (var i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append(" Index # " + i + "\n" +
                                         " Message: " + ex.Errors[i].Message + "\n" +
                                         " LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                         " Source: " + ex.Errors[i].Source + "\n" +
                                         " Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                throw new ExcecaoExecutarSQL(errorMessages.ToString());
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Descrição: A operação não gerou ou encontrou a fonte do retorno esperado." + "\n" + "Erro : " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro : " + ex.Message);
            }
        }

        #endregion

        #region Dispose

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera os recursos alocados pela instância da classe.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if(_transaction != null)
                        RollBackTransaction();
                    if(_connection != null)
                        CloseConnection();
                }
                disposed = true;
            }
        }

        ~DB()
        {
            Dispose(false);
        }

        #endregion
    }
}